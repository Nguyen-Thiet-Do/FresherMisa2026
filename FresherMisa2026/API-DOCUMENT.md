# API Document — FresherMisa2026

Tài liệu tham khảo cho Frontend. Mọi response đều wrap qua `ServiceResponse`.

---

## Mục lục

- [Base URL & Headers](#base-url--headers)
- [Cấu trúc Response chung](#cấu-trúc-response-chung)
- [Mã lỗi HTTP](#mã-lỗi-http)
- [Departments](#departments)
- [Positions](#positions)
- [Employees](#employees)
- [Candidates](#candidates)
- [Files](#files)
- [Advanced Filter](#advanced-filter)
- [Schemas đầy đủ](#schemas-đầy-đủ)

---

## Base URL & Headers

| Môi trường | URL |
|---|---|
| HTTP (dev) | `http://localhost:5237` |
| HTTPS (dev) | `https://localhost:7173` |
| Swagger UI | `http://localhost:5237/swagger` |

**Content-Type mặc định:** `application/json`  
**Upload file:** `multipart/form-data`

---

## Cấu trúc Response chung

Mọi endpoint đều trả về `ServiceResponse`:

```json
{
  "isSuccess": true,
  "code": 200,
  "data": {},
  "userMessage": null,
  "devMessage": null
}
```

| Field | Kiểu | Mô tả |
|---|---|---|
| `isSuccess` | `boolean` | `true` khi thành công |
| `code` | `number` | HTTP status code |
| `data` | `any` | Payload — xem từng endpoint |
| `userMessage` | `string \| null` | Thông báo lỗi cho người dùng (tiếng Việt) |
| `devMessage` | `string \| null` | Chi tiết lỗi cho dev |

**Khi validation lỗi**, `data` là chuỗi các lỗi nối bằng `"; "`:
```json
{
  "isSuccess": false,
  "code": 400,
  "data": "Mã nhân viên không được để trống; Tên nhân viên không được để trống",
  "userMessage": "...",
  "devMessage": null
}
```

**Dữ liệu phân trang** (`PagingResponse`):
```json
{
  "isSuccess": true,
  "code": 200,
  "data": {
    "total": 100,
    "pageSize": 10,
    "currentPage": 1,
    "pageCount": 10,
    "data": [...]
  }
}
```

---

## Mã lỗi HTTP

| Code | Nguyên nhân |
|---|---|
| `200` | Thành công |
| `201` | Tạo mới thành công |
| `400` | Validation lỗi hoặc dữ liệu không hợp lệ |
| `404` | Không tìm thấy bản ghi |
| `409` | Trùng dữ liệu unique (ví dụ: mã đã tồn tại) |
| `500` | Lỗi server |

---

## Departments

Base route: `/api/Departments`

### GET `/api/Departments` — Lấy tất cả phòng ban

**Response `data`:** `Department[]`

---

### GET `/api/Departments/{id}` — Lấy phòng ban theo ID

**Path params:**
| Param | Kiểu | Bắt buộc |
|---|---|---|
| `id` | `guid` | Có |

**Response `data`:** `Department`

---

### GET `/api/Departments/Paging` — Phân trang phòng ban

**Query params:**
| Param | Kiểu | Mặc định | Mô tả |
|---|---|---|---|
| `pageIndex` | `number` | `1` | Trang hiện tại |
| `pageSize` | `number` | `10` | Số bản ghi mỗi trang |
| `search` | `string` | `""` | Từ khóa tìm kiếm |
| `sort` | `string` | `""` | Trường sắp xếp, prefix `-` cho DESC (ví dụ: `-DepartmentName`) |
| `searchFields` | `string` | `""` | Các trường tìm kiếm, cách nhau bằng `;` (ví dụ: `DepartmentCode;DepartmentName`) |

**Response `data`:** `PagingResponse<Department>`

**Ví dụ:**
```
GET /api/Departments/Paging?pageIndex=1&pageSize=10&search=kế toán&sort=-DepartmentName&searchFields=DepartmentCode;DepartmentName
```

---

### GET `/api/Departments/Code/{code}` — Lấy phòng ban theo mã

**Path params:**
| Param | Kiểu | Bắt buộc |
|---|---|---|
| `code` | `string` | Có |

**Response `data`:** `Department`

---

### GET `/api/Departments/{code}/employees` — Lấy nhân viên theo mã phòng ban

**Path params:**
| Param | Kiểu | Bắt buộc |
|---|---|---|
| `code` | `string` | Có |

**Response `data`:** `Employee[]`

---

### GET `/api/Departments/{code}/employee-count` — Đếm nhân viên theo mã phòng ban

**Path params:**
| Param | Kiểu | Bắt buộc |
|---|---|---|
| `code` | `string` | Có |

**Response `data`:** `number`

---

### POST `/api/Departments` — Tạo phòng ban mới

**Request body:** `Department` (JSON)

**Trường bắt buộc:** `DepartmentCode`

**Response `data`:** `Department` (bản ghi vừa tạo)  
**HTTP:** `201 Created`

**Ví dụ:**
```json
{
  "departmentCode": "PB001",
  "departmentName": "Phòng Kế toán",
  "description": "Phụ trách kế toán tài chính"
}
```

---

### PUT `/api/Departments/{id}` — Cập nhật phòng ban

**Path params:**
| Param | Kiểu | Bắt buộc |
|---|---|---|
| `id` | `guid` | Có |

**Request body:** `Department` (JSON) — truyền toàn bộ object

**Response `data`:** `Department`

---

### DELETE `/api/Departments/{id}` — Xóa phòng ban

**Path params:**
| Param | Kiểu | Bắt buộc |
|---|---|---|
| `id` | `guid` | Có |

> Sẽ trả `400` nếu phòng ban còn nhân viên (FK constraint).

---

### POST `/api/Departments/AdvancedFilter` — Lọc nâng cao (dynamic SQL)

Xem mục [Advanced Filter](#advanced-filter).

---

### POST `/api/Departments/AdvancedFilterProc` — Lọc nâng cao (stored procedure)

Xem mục [Advanced Filter](#advanced-filter).

---

## Positions

Base route: `/api/Positions`

### GET `/api/Positions` — Lấy tất cả chức vụ

**Response `data`:** `Position[]`

---

### GET `/api/Positions/{id}` — Lấy chức vụ theo ID

**Path params:**
| Param | Kiểu | Bắt buộc |
|---|---|---|
| `id` | `guid` | Có |

**Response `data`:** `Position`

---

### GET `/api/Positions/Paging` — Phân trang chức vụ

**Query params:** Giống [Departments/Paging](#get-apidepartmentspaging--phân-trang-phòng-ban)

Các trường có thể search: `PositionCode`, `PositionName`

**Response `data`:** `PagingResponse<Position>`

---

### GET `/api/Positions/Code/{code}` — Lấy chức vụ theo mã

**Path params:**
| Param | Kiểu | Bắt buộc |
|---|---|---|
| `code` | `string` | Có |

**Response `data`:** `Position`

---

### POST `/api/Positions` — Tạo chức vụ mới

**Trường bắt buộc:** `PositionCode`, `PositionName`

**Ví dụ:**
```json
{
  "positionCode": "CV001",
  "positionName": "Lập trình viên"
}
```

**HTTP:** `201 Created`

---

### PUT `/api/Positions/{id}` — Cập nhật chức vụ

**Path params:** `id` (guid)  
**Request body:** `Position` (JSON)

---

### DELETE `/api/Positions/{id}` — Xóa chức vụ

> Trả `400` nếu chức vụ còn nhân viên đang giữ.

---

### POST `/api/Positions/AdvancedFilter` | `AdvancedFilterProc`

Xem mục [Advanced Filter](#advanced-filter).

---

## Employees

Base route: `/api/Employees`

### GET `/api/Employees` — Lấy tất cả nhân viên

**Response `data`:** `Employee[]`

---

### GET `/api/Employees/{id}` — Lấy nhân viên theo ID

**Path params:** `id` (guid)

**Response `data`:** `Employee`

---

### GET `/api/Employees/Paging` — Phân trang nhân viên

**Query params:** Giống Departments/Paging

Các trường có thể search: `EmployeeCode`, `EmployeeName`, `Email`, `PhoneNumber`

**Response `data`:** `PagingResponse<Employee>`

---

### GET `/api/Employees/Code/{code}` — Lấy nhân viên theo mã

**Path params:** `code` (string)

**Response `data`:** `Employee`

---

### GET `/api/Employees/Department/{departmentId}` — Lấy nhân viên theo phòng ban

**Path params:** `departmentId` (guid)

**Response `data`:** `Employee[]`

---

### GET `/api/Employees/Position/{positionId}` — Lấy nhân viên theo chức vụ

**Path params:** `positionId` (guid)

**Response `data`:** `Employee[]`

---

### GET `/api/Employees/filter` — Lọc nhân viên có phân trang

**Query params:**
| Param | Kiểu | Mô tả |
|---|---|---|
| `departmentId` | `guid?` | Lọc theo phòng ban |
| `positionId` | `guid?` | Lọc theo chức vụ |
| `salaryFrom` | `number?` | Lương tối thiểu |
| `salaryTo` | `number?` | Lương tối đa |
| `gender` | `number?` | Giới tính (`0` = Nam, `1` = Nữ) |
| `hireDateFrom` | `datetime?` | Ngày vào làm từ (ISO 8601) |
| `hireDateTo` | `datetime?` | Ngày vào làm đến (ISO 8601) |
| `pageSize` | `number` | Mặc định `10` |
| `pageIndex` | `number` | Mặc định `1` |

**Response `data`:** `PagingResponse<Employee>`

**Ví dụ:**
```
GET /api/Employees/filter?salaryFrom=10000000&salaryTo=30000000&gender=0&pageSize=10&pageIndex=1
```

---

### POST `/api/Employees` — Tạo nhân viên mới

**Trường bắt buộc:** `EmployeeCode`, `EmployeeName`, `DepartmentID`, `PositionID`

**Ví dụ:**
```json
{
  "employeeCode": "NV001",
  "employeeName": "Nguyễn Văn A",
  "gender": 0,
  "dateOfBirth": "1995-03-15T00:00:00",
  "phoneNumber": "0901234567",
  "email": "nva@example.com",
  "address": "123 Đường ABC, Hà Nội",
  "departmentID": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "positionID": "3fa85f64-5717-4562-b3fc-2c963f66afb7",
  "salary": 15000000,
  "hireDate": "2024-01-01T00:00:00"
}
```

**HTTP:** `201 Created`

---

### PUT `/api/Employees/{id}` — Cập nhật nhân viên

**Path params:** `id` (guid)  
**Request body:** `Employee` (JSON) — truyền toàn bộ object

---

### DELETE `/api/Employees/{id}` — Xóa nhân viên

**Path params:** `id` (guid)

---

### POST `/api/Employees/AdvancedFilter` | `AdvancedFilterProc`

Xem mục [Advanced Filter](#advanced-filter).

---

## Candidates

Base route: `/api/Candidates`

### GET `/api/Candidates` — Lấy tất cả ứng viên

**Response `data`:** `Candidate[]`

---

### GET `/api/Candidates/{id}` — Lấy ứng viên theo ID

**Path params:** `id` (guid)

**Response `data`:** `Candidate`

---

### GET `/api/Candidates/Paging` — Phân trang ứng viên

**Query params:** Giống Departments/Paging

Các trường có thể search: `FullName`, `Email`, `PhoneNumber`

**Response `data`:** `PagingResponse<Candidate>`

---

### GET `/api/Candidates/filter` — Lọc ứng viên có phân trang

**Query params:**
| Param | Kiểu | Mô tả |
|---|---|---|
| `search` | `string?` | Tìm theo tên, email, số điện thoại |
| `gender` | `string?` | Giới tính (ví dụ: `"Nam"`, `"Nữ"`) |
| `level` | `string?` | Cấp độ (ví dụ: `"Junior"`, `"Senior"`) |
| `city` | `string?` | Thành phố |
| `jobPosition` | `string?` | Vị trí ứng tuyển |
| `department` | `string?` | Phòng ban |
| `candidateSource` | `string?` | Nguồn ứng viên (ví dụ: `"LinkedIn"`) |
| `isEmployee` | `boolean?` | `true` = đã là nhân viên |
| `hiringDateFrom` | `datetime?` | Ngày ứng tuyển từ (ISO 8601) |
| `hiringDateTo` | `datetime?` | Ngày ứng tuyển đến (ISO 8601) |
| `pageSize` | `number` | Mặc định `10` |
| `pageIndex` | `number` | Mặc định `1` |

**Response `data`:** `PagingResponse<Candidate>`

---

### POST `/api/Candidates` — Tạo ứng viên mới

**Trường bắt buộc:** `FullName`

**Ví dụ:**
```json
{
  "fullName": "Trần Thị B",
  "dateOfBirth": "1998-07-20T00:00:00",
  "gender": "Nữ",
  "city": "Hồ Chí Minh",
  "phoneNumber": "0912345678",
  "email": "ttb@example.com",
  "level": "Junior",
  "educationPlace": "Đại học Bách Khoa",
  "major": "Công nghệ thông tin",
  "hiringDate": "2024-05-01T00:00:00",
  "candidateSource": "LinkedIn",
  "jobPosition": "Frontend Developer",
  "department": "Phòng Công nghệ",
  "cvFile": "/uploads/candidates/cv_ttb.pdf",
  "avatar": "/uploads/candidates/avatar_ttb.jpg"
}
```

**HTTP:** `201 Created`

---

### PUT `/api/Candidates/{id}` — Cập nhật ứng viên

**Path params:** `id` (guid)  
**Request body:** `Candidate` (JSON)

---

### DELETE `/api/Candidates/{id}` — Xóa ứng viên

> Tự động xóa file `CVFile` và `Avatar` khỏi server khi xóa ứng viên.

---

### POST `/api/Candidates/AdvancedFilter` | `AdvancedFilterProc`

Xem mục [Advanced Filter](#advanced-filter).

---

## Files

Base route: `/api/Files`

### POST `/api/Files/upload` — Upload file

**Content-Type:** `multipart/form-data`

**Form params:**
| Param | Kiểu | Bắt buộc | Mô tả |
|---|---|---|---|
| `file` | `File` | Có | File cần upload |

**Query params:**
| Param | Kiểu | Mặc định | Mô tả |
|---|---|---|---|
| `folder` | `string` | `"general"` | Thư mục lưu (`"candidates"`, `"employees"`, ...) |

**Response `data`:**
```json
{
  "url": "/uploads/candidates/abc_def.pdf"
}
```

**Ví dụ (JavaScript):**
```js
const formData = new FormData();
formData.append('file', fileInput.files[0]);

const res = await fetch('/api/Files/upload?folder=candidates', {
  method: 'POST',
  body: formData
});
const { data } = await res.json();
// data.url = "/uploads/candidates/abc_def.pdf"
```

---

### DELETE `/api/Files?relativeUrl={url}` — Xóa file

**Query params:**
| Param | Kiểu | Bắt buộc | Mô tả |
|---|---|---|---|
| `relativeUrl` | `string` | Có | URL tương đối của file |

**Ví dụ:**
```
DELETE /api/Files?relativeUrl=/uploads/candidates/abc_def.pdf
```

**Response:**
```json
{
  "isSuccess": true,
  "code": 200
}
```

---

## Advanced Filter

Hai endpoint trên mỗi resource cho phép lọc linh hoạt với nhiều điều kiện:

- **`POST /api/{Resource}/AdvancedFilter`** — C# build WHERE động (an toàn SQL injection)
- **`POST /api/{Resource}/AdvancedFilterProc`** — Stored Procedure nhận JSON tự build WHERE

Cả hai nhận cùng `AdvancedFilterRequest` và trả cùng cấu trúc.

### Request body: `AdvancedFilterRequest`

```json
{
  "pageIndex": 1,
  "pageSize": 10,
  "sort": "-Salary,+EmployeeName",
  "filters": [
    {
      "field": "EmployeeName",
      "operator": "Contains",
      "value": "Nguyễn"
    },
    {
      "field": "Salary",
      "operator": "Between",
      "value": 10000000,
      "valueTo": 30000000
    },
    {
      "field": "Gender",
      "operator": "In",
      "values": [0, 1]
    }
  ]
}
```

| Field | Kiểu | Mô tả |
|---|---|---|
| `pageIndex` | `number` | Mặc định `1` |
| `pageSize` | `number` | Mặc định `10` |
| `sort` | `string?` | Prefix `-` DESC, `+` ASC, nhiều trường cách nhau bằng `,` |
| `filters` | `FilterCondition[]?` | `null` hoặc `[]` = trả toàn bộ có phân trang |

### FilterCondition

| Field | Kiểu | Mô tả |
|---|---|---|
| `field` | `string` | Tên property của entity (PascalCase) |
| `operator` | `FilterOperator` | Loại so sánh (xem bảng bên dưới) |
| `value` | `any?` | Giá trị so sánh |
| `valueTo` | `any?` | Giá trị thứ hai — chỉ dùng cho `Between`, `NotBetween` |
| `values` | `any[]?` | Mảng giá trị — chỉ dùng cho `In`, `NotIn` |

### FilterOperator

| Operator | Dùng cho | Cần `value` | Cần `valueTo` | Cần `values` |
|---|---|---|---|---|
| `Eq` | String, Number, Date | `value` | | |
| `Neq` | String, Number, Date | `value` | | |
| `Contains` | String | `value` | | |
| `NotContains` | String | `value` | | |
| `StartsWith` | String | `value` | | |
| `EndsWith` | String | `value` | | |
| `Empty` | String | | | |
| `NotEmpty` | String | | | |
| `Gt` | Number, Date | `value` | | |
| `Lt` | Number, Date | `value` | | |
| `Gte` | Number, Date | `value` | | |
| `Lte` | Number, Date | `value` | | |
| `Between` | Number, Date | `value` (from) | `valueTo` (to) | |
| `NotBetween` | Number, Date | `value` (from) | `valueTo` (to) | |
| `Today` | Date | | | |
| `ThisWeek` | Date | | | |
| `ThisMonth` | Date | | | |
| `ThisYear` | Date | | | |
| `LastNDays` | Date | `value` (số ngày) | | |
| `NextNDays` | Date | `value` (số ngày) | | |
| `In` | String, Number | | | `values` |
| `NotIn` | String, Number | | | `values` |

> **Lưu ý Date presets:** Backend tự tính khoảng ngày, FE chỉ cần truyền operator.

**Ví dụ — lọc nhân viên vào làm trong 30 ngày gần nhất, lương ≥ 10 triệu:**
```json
{
  "pageIndex": 1,
  "pageSize": 20,
  "sort": "-HireDate",
  "filters": [
    { "field": "HireDate", "operator": "LastNDays", "value": 30 },
    { "field": "Salary", "operator": "Gte", "value": 10000000 }
  ]
}
```

**Response `data`:** `PagingResponse<T>`

---

## Schemas đầy đủ

### Department

```json
{
  "departmentID": "guid",
  "departmentCode": "string",        // Bắt buộc, duy nhất
  "departmentName": "string?",
  "description": "string?",
  "createdBy": "string?",            // Hệ thống tự set
  "createDate": "datetime?",         // Hệ thống tự set
  "modifiedBy": "string?",           // Hệ thống tự set
  "modifiedDate": "datetime?",       // Hệ thống tự set
  "isDeleted": "boolean"
}
```

### Position

```json
{
  "positionID": "guid",
  "positionCode": "string",          // Bắt buộc, duy nhất
  "positionName": "string",          // Bắt buộc
  "createdBy": "string?",
  "createDate": "datetime?",
  "modifiedBy": "string?",
  "modifiedDate": "datetime?",
  "isDeleted": "boolean"
}
```

### Employee

```json
{
  "employeeID": "guid",
  "employeeCode": "string",          // Bắt buộc, duy nhất
  "employeeName": "string",          // Bắt buộc
  "gender": "number?",               // 0 = Nam, 1 = Nữ
  "dateOfBirth": "datetime?",
  "phoneNumber": "string?",
  "email": "string?",
  "address": "string?",
  "departmentID": "guid",            // Bắt buộc
  "positionID": "guid",              // Bắt buộc
  "salary": "number?",
  "hireDate": "datetime?",
  "createdDate": "datetime?",
  "createdBy": "string?",
  "modifiedBy": "string?",
  "modifiedDate": "datetime?",
  "isDeleted": "boolean"
}
```

### Candidate

```json
{
  "candidateID": "guid",
  "cvFile": "string?",               // URL tương đối, ví dụ: /uploads/candidates/cv.pdf
  "avatar": "string?",               // URL tương đối, ví dụ: /uploads/candidates/avatar.jpg
  "fullName": "string",              // Bắt buộc
  "dateOfBirth": "datetime?",
  "gender": "string?",               // Ví dụ: "Nam", "Nữ"
  "city": "string?",
  "phoneNumber": "string?",
  "email": "string?",
  "country": "string?",
  "province": "string?",
  "ward": "string?",
  "address": "string?",
  "level": "string?",                // Ví dụ: "Junior", "Middle", "Senior"
  "educationPlace": "string?",
  "major": "string?",
  "hiringDate": "datetime?",
  "candidateSource": "string?",      // Ví dụ: "LinkedIn", "Referral", "Website"
  "hrInCharge": "string?",
  "collaborator": "string?",
  "isReferenceAdded": "boolean",     // Mặc định: false
  "lastCompany": "string?",
  "workCompany": "string?",
  "workStartDate": "datetime?",
  "workEndDate": "datetime?",
  "workPosition": "string?",
  "workDescription": "string?",
  "hiringCampaign": "string?",
  "hiringRound": "string?",          // Ví dụ: "Round 1", "Round 2"
  "rating": "string?",
  "jobPosition": "string?",
  "isEmployee": "boolean",           // Mặc định: false
  "department": "string?",
  "createdBy": "string?",
  "createDate": "datetime?",
  "modifiedBy": "string?",
  "modifiedDate": "datetime?",
  "isDeleted": "boolean"
}
```

### PagingResponse\<T\>

```json
{
  "total": "number",       // Tổng số bản ghi thỏa điều kiện
  "pageSize": "number",    // Số bản ghi mỗi trang
  "currentPage": "number", // Trang hiện tại
  "pageCount": "number",   // Tổng số trang
  "data": []               // Mảng bản ghi
}
```
