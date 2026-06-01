# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

REST API backend quản lý nhân sự (Employee, Department, Position, Candidate) xây dựng bằng **.NET 10 / ASP.NET Core**, dùng **Dapper** + **MySQL**. Không có ORM — toàn bộ truy vấn thông qua **stored procedures** và raw SQL.

**Tech stack:** .NET 10, ASP.NET Core, Dapper 2.1.72, MySqlConnector 2.5.0, Microsoft.Extensions.Caching.Memory, Swashbuckle 10.1.7

**Database:** MySQL schema `misaemployee_development` (localhost:3306, user `root`, password `123456` — xem `appsettings.json`)

## Commands

Chạy từ thư mục `FresherMisa2026.WebAPI/`:

```bash
dotnet build                        # Build toàn bộ solution
dotnet run                          # Chạy API (HTTP: localhost:5237)
dotnet run --launch-profile https   # Chạy HTTPS (localhost:7173) + mở Swagger
```

Swagger UI ở: `http://localhost:5237/swagger` (chỉ ở môi trường Development)

## Architecture

4 lớp theo chiều phụ thuộc một chiều:

```
WebAPI → Application → Entities
WebAPI → Infrastructure → Application → Entities
```

| Project | Vai trò |
|---|---|
| `Entities` | Models, DTOs, `BaseModel`, `ConfigTable`, `IRequired` attribute |
| `Application` | `IBaseService<T>`, `BaseService<T>`, interfaces repo, `SQLExtension` |
| `Infrastructure` | `BaseRepository<T>` (Dapper + MySQL), concrete repositories |
| `WebAPI` | `BaseController<T>`, concrete controllers, middlewares, `Program.cs` |

### Request flow

`Controller` → `Service.InsertAsync/UpdateAsync` → validate (`[IRequired]` + `ValidateCustom`) → `Repository.InsertAsync` → `ValidateUniqueColumnsAsync` (pre-check trùng) → gọi stored procedure → MySQL

### BaseController — endpoints tự động

Mọi controller kế thừa `BaseController<TEntity>` đều có sẵn:

| Method | Route | Mô tả |
|---|---|---|
| GET | `/api/{entity}/Paging` | Phân trang. Query params: `search`, `sort`, `pageSize`, `pageIndex`, `searchFields` |
| GET | `/api/{entity}` | Lấy tất cả (có cache) |
| GET | `/api/{entity}/{id:guid}` | Lấy theo ID (có cache) |
| POST | `/api/{entity}` | Tạo mới — trả 201 |
| PUT | `/api/{entity}/{id:guid}` | Cập nhật |
| DELETE | `/api/{entity}/{id:guid}` | Xóa |

**Paging params:**
- `searchFields`: danh sách trường cách nhau bằng `;` (ví dụ: `FullName;Email`)
- `sort`: prefix `-` cho DESC (ví dụ: `HireDate,-EmployeeName`)

### Entity-specific endpoints

**Employees** (`/api/Employees`): `GET /Code/{code}`, `GET /Department/{id}`, `GET /Position/{id}`, `GET /filter`

**Departments** (`/api/Departments`): `GET /Code/{code}`, `GET /{code}/employees`, `GET /{code}/employee-count`

**Positions** (`/api/Positions`): `GET /Code/{code}`

**Candidates** (`/api/Candidates`): `GET /filter`

### BaseRepository — stored procedure convention

`BaseRepository<TEntity>` tự động gọi các stored procedure theo tên:

| Operation | Stored Procedure |
|---|---|
| Insert | `Proc_Insert{TableName}` |
| Update | `Proc_Update{TableName}` |
| Delete | `Proc_Delete{TableName}ById` |
| Generic paging | `Proc_{TableName}_FilterPaging` |

**Tham số truyền vào SP:** `MappingDbType()` duyệt **tất cả properties** của entity (kể cả các trường từ `BaseModel`: `CreatedBy`, `CreateDate`, `ModifiedBy`, `ModifiedDate`, `State`, `IsDeleted`) và tạo `@v_{PropertyName}`. Stored procedure chỉ cần khai báo các tham số nó thực sự dùng — MySQL bỏ qua tham số thừa.

Tham số key khi xóa: `@v_{KeyName}` (ví dụ: `@v_EmployeeID`).

`Proc_{TableName}_FilterPaging` (generic paging) nhận: `v_pageIndex`, `v_pageSize`, `v_search`, `v_sort`, `v_searchFields` (JSON). Phải trả **2 result sets**: data rows trước, COUNT sau.

### ConfigTable attribute và MethodExtensions (`Entities/Extensions/`)

`[ConfigTable(tableName, hasDeletedColumn, uniqueColumns)]` trên entity class điều khiển hành vi của `BaseRepository`:

| Tham số | Kiểu | Mô tả |
|---|---|---|
| `tableName` | `string` | Tên bảng MySQL — **bắt buộc**, throw `ArgumentException` nếu trống |
| `hasDeletedColumn` | `bool` | `true` → tự động thêm `WHERE IsDeleted = FALSE` vào mọi GET query |
| `uniqueColumns` | `string` | Tên cột unique, cách nhau bằng `,` (ví dụ: `"EmployeeCode"`) |

`MethodExtensions` cung cấp extension methods: `GetTableName()`, `GetHasDeletedColumn()`, `GetUniqueColumns()`, `GetKeyName()`, `GetColumnDisplayName()`. Kết quả `GetKeyName()` được cache vào field `_keyName` trong constructor của `BaseRepository` — không gọi reflection lặp lại.

**`ValidateUniqueColumnsAsync(entity, excludeId?)`** — được gọi tự động trong `InsertAsync` và `UpdateAsync` trước khi mở transaction. Đọc `uniqueColumns` từ `[ConfigTable]`, query `SELECT COUNT(*)` cho từng cột, throw `DuplicateEntityException` nếu trùng. MySqlException 1062 handler vẫn giữ như safety net cho race condition.

**Khi thêm entity có UNIQUE constraint:** khai báo cột trong `uniqueColumns` của `[ConfigTable]` và gắn `[Display(Name = "Tên tiếng Việt")]` lên property đó — cả `ValidateUniqueColumnsAsync` và `TranslateMySqlException` đều dùng display name này cho error message.

### Caching

- Cache 5 phút qua `IMemoryCache`
- Cache keys: `{TableName}_all` (danh sách), `{TableName}_{id}` (đơn)
- Cache tự xóa sau Insert/Update/Delete
- `GetEntityByIDAsync` tìm trong cache đơn → cache danh sách → DB

### SQL queries (inline)

`SQLExtension.Initialize()` load file `WebAPI/Queries/Query.json` khi khởi động. Các query inline (không phải stored proc) được tra cứu bằng `SQLExtension.GetQuery("Entity.Operation")`.

### Exception handling (GlobalExceptionMiddleware)

MySqlException được dịch sang domain exception tại `BaseRepository.TranslateMySqlException()` trước khi bubble lên:

| MySqlException | Domain Exception | HTTP |
|---|---|---|
| 1062 (duplicate key) | `DuplicateEntityException` | 409 |
| 1451 (FK — xóa cha còn con) | `InvalidOperationException` | 400 |
| 1452 (FK — insert con thiếu cha) | `ArgumentException` | 400 |
| SQLSTATE 45000 chứa "không tồn tại" | `KeyNotFoundException` | 404 |
| SQLSTATE 45000 khác | `InvalidOperationException` | 400 |

Middleware chỉ xử lý domain exception, không phụ thuộc MySqlConnector.

`TranslateMySqlException` parse tên constraint `UQ_{ColumnName}` → gọi `GetColumnDisplayName()` để lấy tên tiếng Việt từ `[Display(Name = "...")]` trên entity property.

### ServiceResponse

Mọi response đều wrap qua `ServiceResponse`:
```json
{ "isSuccess": true, "code": 200, "data": ..., "userMessage": null, "devMessage": null }
```
Khi validation lỗi: `data` chứa chuỗi lỗi join bằng `"; "`, `code = 400`.

## Thêm entity mới

1. **Entity:** `Entities/{Name}/{Name}.cs` — kế thừa `BaseModel`, gắn `[ConfigTable("TableName", hasDeletedColumn, "uniqueCol")]`, `[Key]` cho PK (Guid), `[IRequired]` cho trường bắt buộc, `[Display(Name = "Tiếng Việt")]`
2. **DTO:** `Entities/{Name}/DTO/{Name}FilterRequest.cs`
3. **Interfaces:** `Application/Interfaces/Repositories/I{Name}Repository.cs` và `Application/Interfaces/Services/I{Name}Service.cs`
4. **Repository:** `Infrastructure/Repositories/{Name}/{Name}Repository.cs` — gọi custom stored procs
5. **Service:** `Application/Services/{Name}/{Name}Service.cs` — override `ValidateCustom`, `ValidateBeforeInsertAsync`, `ValidateBeforeUpdateAsync` nếu cần. Lifecycle hooks: `OnAfterInsert`, `OnAfterUpdate`, `OnAfterDelete`, `AfterDelete` (cleanup file...), `ValidateBeforeDeleteAsync` + `GetDeleteValidationMessageAsync` (block xóa)
6. **Controller:** `WebAPI/Controllers/{Name}sController.cs` — kế thừa `BaseController<{Name}>`
7. **SQL:** `WebAPI/Queries/{name}_migration.sql` — tạo bảng + 5 stored procs (`Proc_Insert`, `Proc_Update`, `Proc_DeleteById`, `Proc_{Name}_FilterPaging`, custom filter proc nếu có)
8. **DI:** Đăng ký trong `Application/ServiceExtensions.cs` và `Infrastructure/ServiceExtensions.cs`
9. **Query.json:** Thêm inline queries nếu cần `SQLExtension.GetQuery(...)`

## Module Tiền Lương (nhánh `amis-tien-luong`)

### Database

Schema riêng: `amis_tien_luong` (localhost:3306, cùng user/password). Script khởi tạo: `docs/thanh_phan_luong_amis.sql`.

### Entities và bảng DB

| Entity (C#) | Bảng MySQL | `hasDeletedColumn` | Unique |
|---|---|---|---|
| `Organization` | `pa_organization` | `true` | `Code` |
| `SalaryComponentType` | `pa_salary_component_type` | `false` | `Code` |
| `SalaryCompositionSystem` | `pa_salary_composition_system` | `false` | `Code` |
| `SalaryComposition` | `pa_salary_composition` | `true` | `Code` |
| `GridConfig` | `pa_grid_config` | `false` | — (composite: UserID+GridCode+ColumnKey) |

### Enums (tất cả trong `Entities/Enums/`)

| Enum | Dùng ở |
|---|---|
| `SalaryNature` | Income=1, Deduction=2, Information=3, Other=4 |
| `SalaryTaxType` | Taxable=1, FullyExempt=2, PartiallyExempt=3 |
| `SalaryValueType` | Currency=1, Number=2, Percentage=3 |
| `SalaryValueMode` | AutoSum=1, Formula=2 |
| `SalaryCompositionSource` | Custom=1, InheritedFromSystem=2 |
| `SalaryCompositionStatus` | Inactive=0, Active=1 |

### Business Rules quan trọng

| BR | Mô tả | Nơi enforce |
|---|---|---|
| BR-01 | `Code` của `SalaryComposition` **không được sửa sau khi lưu** | `ValidateBeforeUpdateAsync` trong Service |
| BR-05 | `TaxType` chỉ có ý nghĩa khi `Nature = Income` | `ValidateCustom` trong Service |
| BR-07 | Dùng `Status = Inactive` thay vì xóa khi không còn dùng | Convention người dùng |
| BR-08 | TPL mặc định của hệ thống (`Source = InheritedFromSystem`) không được xóa | `ValidateBeforeDeleteAsync` trong Service |
| BR-09 | `AllowExceedNorm = true` cho phép giá trị vượt `NormFormula` (định mức) | Logic tính lương |
| BR-10 | TPL kế thừa từ hệ thống (`Source = InheritedFromSystem`) chỉ được sửa: `Name`, `OrganizationIDs`, `Description`, `ShowOnPayslip`, `Status`. Mọi field còn lại bị lock — `ValidateBeforeUpdateAsync` trả 400 kèm tên các field vi phạm | `GetLockedFieldChanges` trong Service |

### Luồng sử dụng thực tế

```
SalaryCompositionSystem (danh mục hệ thống, seed sẵn)
    → SalaryComposition (đơn vị chọn vào + tự tạo thêm)
        → Mẫu bảng lương → Tính lương nhân viên
```

### Entity-specific endpoints (dự kiến)

**SalaryCompositions** (`/api/SalaryCompositions`): `GET /filter` (lọc theo Nature, Status, ComponentTypeID, OrganizationID, Source)

## Conventions

- **Naming:** PascalCase cho class/method/property; camelCase cho biến local; prefix `_` cho private fields
- **Async:** Tất cả DB calls đều async, suffix `Async`
- **Validation chain:** `[IRequired]` (tự động) → `ValidateCustom` (format/logic) → `ValidateBeforeInsertAsync/UpdateAsync` (DB lookup)
- **Error messages:** Tiếng Việt cho `userMessage`; English hoặc tiếng Việt cho `devMessage`
- **XML docs:** Public methods dùng `/// <summary>` tiếng Việt, kèm `/// Created By: {author} ({date})`
- **Regions:** `#region Declare`, `#region Constructer`, `#region Methods`, `#region OVERRIDE METHODS`
- **`State` field:** Không lấy từ client — BaseService set tự động (`ModelSate.Add` khi insert, `ModelSate.Update` khi update)
- **Primary key:** Auto-generate Guid bởi `EnsurePrimaryKeyForInsert` nếu client không truyền
