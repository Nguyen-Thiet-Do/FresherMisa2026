# Tài liệu API — Module Thành Phần Lương (TPL)

> **Base URL:** `http://localhost:5237/api`  
> **Content-Type:** `application/json`  
> **Tác giả:** Nguyen Thiet Do — 2026-05-28

---

## Mục lục

1. [Quy ước chung](#1-quy-ước-chung)
2. [Enum reference](#2-enum-reference)
3. [Model reference](#3-model-reference)
4. [Loại TPL — SalaryComponentTypes](#4-loại-tpl--salarycomponenttypes)
5. [TPL Hệ thống — SalaryCompositionSystems](#5-tpl-hệ-thống--salarycompositionsystems)
6. [TPL Đơn vị — SalaryCompositions](#6-tpl-đơn-vị--salarycompositions)
7. [Tùy chỉnh cột — GridConfigs](#7-tùy-chỉnh-cột--gridconfigs)
8. [FilterCondition — operators reference](#8-lọc-nâng-cao--advancedfilter)
9. [Mã lỗi HTTP](#9-mã-lỗi-http)

---

## 1. Quy ước chung

### 1.1 Cấu trúc response

Mọi API đều trả về cùng một wrapper `ServiceResponse`:

```json
{
  "isSuccess": true,
  "code": 200,
  "data": <payload>,
  "userMessage": null,
  "devMessage": null
}
```

| Field | Type | Mô tả |
|---|---|---|
| `isSuccess` | `bool` | `true` = thành công |
| `code` | `int` | HTTP status code |
| `data` | `any` | Payload thực sự |
| `userMessage` | `string \| null` | Thông báo hiển thị cho người dùng khi lỗi |
| `devMessage` | `string \| null` | Thông tin debug |

### 1.2 Cấu trúc response phân trang

Khi `data` là danh sách có phân trang:

```json
{
  "isSuccess": true,
  "code": 200,
  "data": {
    "data": [...],
    "total": 150,
    "pageSize": 10,
    "currentPage": 1,
    "pageCount": 15
  }
}
```

### 1.3 Response lỗi validation (400)

```json
{
  "isSuccess": false,
  "code": 400,
  "data": [
    { "field": "Code", "message": "Mã thành phần không được là một số thực" },
    { "field": "Name", "message": "Tên thành phần không được vượt quá 255 ký tự" }
  ],
  "devMessage": "Validate thất bại"
}
```

### 1.4 Định dạng ID

Tất cả ID đều là **UUID v4** dạng string: `"3fa85f64-5717-4562-b3fc-2c963f66afa6"`

### 1.5 Sort parameter

Dùng prefix `-` cho DESC, không prefix hoặc `+` cho ASC. Nhiều field cách nhau bằng dấu phẩy:

```
sort=-CreateDate,+Name   →  ORDER BY CreateDate DESC, Name ASC
sort=Code                →  ORDER BY Code ASC
```

---

## 2. Enum reference

### SalaryNature — Tính chất

| Giá trị | Tên | Mô tả |
|---|---|---|
| `1` | Income | Thu nhập |
| `2` | Deduction | Khấu trừ |
| `3` | Information | Thông tin (căn cứ tính lương) |
| `4` | Other | Khác |

### SalaryTaxType — Loại thuế TNCN

> Chỉ có ý nghĩa khi `nature = 1` (Income). Để `null` với các tính chất khác.

| Giá trị | Tên | Mô tả |
|---|---|---|
| `1` | Taxable | Chịu thuế TNCN |
| `2` | FullyExempt | Miễn toàn phần |
| `3` | PartiallyExempt | Miễn một phần |

### SalaryValueType — Kiểu giá trị

| Giá trị | Tên | Mô tả |
|---|---|---|
| `1` | Currency | Tiền tệ (VNĐ) |
| `2` | Number | Số |
| `3` | Percentage | Phần trăm (%) |

### SalaryValueMode — Chế độ tính

| Giá trị | Tên | Mô tả |
|---|---|---|
| `1` | AutoSum | Tự động cộng tổng |
| `2` | Formula | Theo công thức tự đặt |

### SalaryCompositionSource — Nguồn gốc

| Giá trị | Tên | Mô tả |
|---|---|---|
| `1` | Custom | Tự thêm mới |
| `2` | InheritedFromSystem | Kế thừa từ hệ thống |

### SalaryCompositionStatus — Trạng thái theo dõi

| Giá trị | Tên | Mô tả |
|---|---|---|
| `0` | Inactive | Bỏ theo dõi |
| `1` | Active | Đang theo dõi |

---

## 3. Model reference

### 3.1 SalaryComponentType — Loại TPL

```json
{
  "componentTypeID": "uuid",
  "code": "LUONG",
  "name": "Lương",
  "sortOrder": 6,
  "isActive": true,
  "createdBy": null,
  "createDate": "2026-05-27T20:33:44",
  "modifiedBy": null,
  "modifiedDate": null
}
```

### 3.2 SalaryCompositionSystem — TPL Hệ thống

```json
{
  "systemCompositionID": "uuid",
  "code": "LUONG_CO_BAN",
  "name": "Lương cơ bản",
  "componentTypeID": "uuid | null",
  "componentTypeName": "Lương",
  "organizationID": "uuid | null",
  "organizationName": "Công ty TNHH ABC",
  "nature": 1,
  "taxType": 1,
  "taxDeductible": false,
  "valueType": 1,
  "valueFormula": null,
  "defaultValue": 5000000,
  "normFormula": null,
  "description": "Lương theo hợp đồng lao động",
  "showOnPayslip": true
}
```

### 3.3 SalaryComposition — TPL Đơn vị

```json
{
  "salaryCompositionID": "uuid",
  "code": "LUONG_CO_BAN",
  "name": "Lương cơ bản",
  "organizationID": "uuid | null",
  "organizationName": "Công ty TNHH ABC",
  "componentTypeID": "uuid",
  "componentTypeName": "Lương",
  "systemCompositionID": "uuid | null",
  "nature": 1,
  "taxType": 1,
  "taxDeductible": false,
  "valueType": 1,
  "valueMode": 1,
  "valueFormula": null,
  "valueScope": null,
  "normFormula": null,
  "allowExceedNorm": false,
  "description": "Lương theo hợp đồng lao động",
  "showOnPayslip": true,
  "hideWhenZero": false,
  "source": 2,
  "status": 1,
  "createdBy": null,
  "createDate": "2026-05-27T10:00:00",
  "modifiedBy": null,
  "modifiedDate": null
}
```

### 3.4 Quy tắc validation khi tạo / cập nhật TPL

| Field | Bắt buộc | Ràng buộc |
|---|---|---|
| `code` | ✅ | Tối đa 255 ký tự; chỉ `A-Z a-z 0-9 _`; không được là số thuần túy; không được thay đổi sau khi lưu |
| `name` | ✅ | Tối đa 255 ký tự |
| `componentTypeID` | ✅ | Phải tồn tại trong hệ thống |
| `nature` | ✅ | Giá trị hợp lệ: 1–4 |
| `taxType` | ❌ | Chỉ truyền khi `nature = 1`; để `null` với tính chất khác |
| `taxDeductible` | ❌ | Mặc định `false` — giá trị khoản được khấu trừ khi tính thuế TNCN |
| `valueFormula` | ❌ | Xem quy tắc công thức bên dưới |
| `normFormula` | ❌ | Xem quy tắc công thức bên dưới |

### 3.5 Quy tắc công thức (ValueFormula / NormFormula)

Công thức hỗ trợ các hàm sau (phân biệt hoa thường):

| Hàm | Cú pháp | Mô tả |
|---|---|---|
| `SUM` | `SUM(X1, X2, ...)` | Tổng các tham số — ít nhất 1 tham số |
| `IF` | `IF(điều_kiện, giá_trị_đúng, giá_trị_sai)` | Điều kiện |
| `AND` | `AND(đk1, đk2, ...)` | Và — chỉ dùng trong điều kiện của `IF`, ít nhất 2 tham số |
| `OR` | `OR(đk1, đk2, ...)` | Hoặc — chỉ dùng trong điều kiện của `IF` |
| `INT` | `INT(số)` | Làm tròn xuống số nguyên gần nhất |
| `TODAY` | `TODAY()` | Ngày hiện tại — không có tham số |

**Tham số** hợp lệ: số (ví dụ `1000000`, `0.5`, `-5.2`) hoặc **mã TPL đang theo dõi** (ví dụ `LUONG_CO_BAN`).

**Toán tử số học:** `+`, `-`, `*`, `/` và nhóm bằng `( )`.

**Toán tử so sánh** (chỉ dùng trong điều kiện `IF`/`AND`/`OR`): `>`, `<`, `>=`, `<=`, `=`, `<>`.

**Ví dụ hợp lệ:**
```
LUONG_CO_BAN * 0.08
SUM(LUONG_CO_BAN, PHU_CAP_AN_TRUA, PHU_CAP_DI_LAI)
IF(KPI_PERCENT >= 100, THUONG_KPI, 0)
IF(AND(DOANH_SO > 1000000000, SO_KHACH_HANG > 50), 0.5, 0)
SUM(LUONG_CO_BAN, PHU_CAP) * 0.105 + INT(BHXH_NLD / 30)
```

---

## 4. Loại TPL — SalaryComponentTypes

> **Base path:** `/api/SalaryComponentTypes`  
> CRUD đầy đủ — không có endpoint nghiệp vụ riêng.

---

### 4.1 Lấy danh sách (phân trang)

```
GET /api/SalaryComponentTypes/Paging
```

**Query params:**

| Param | Type | Mặc định | Mô tả |
|---|---|---|---|
| `search` | `string` | — | Tìm trong Code, Name |
| `pageSize` | `int` | `10` | Số bản ghi mỗi trang |
| `pageIndex` | `int` | `1` | Trang hiện tại |
| `sort` | `string` | — | Ví dụ: `SortOrder`, `-Name` |
| `searchFields` | `string` | — | Tên field cách nhau bằng `;` |

---

### 4.2 Lấy toàn bộ (không phân trang)

```
GET /api/SalaryComponentTypes
```

---

### 4.3 Lấy chi tiết một loại TPL

```
GET /api/SalaryComponentTypes/{id}
```

**Response 200:** `data` là object `SalaryComponentType`.  
**Response 404:** Không tìm thấy.

---

### 4.4 Tạo mới loại TPL

```
POST /api/SalaryComponentTypes
```

**Request body:**
```json
{
  "code": "PHU_CAP",
  "name": "Phụ cấp",
  "sortOrder": 10,
  "isActive": true
}
```

**Response 201:** `data` là `1`.  
**Response 400:** Validation lỗi.  
**Response 409:** Mã đã tồn tại.

---

### 4.5 Cập nhật loại TPL

```
PUT /api/SalaryComponentTypes/{id}
```

**Request body:** Tương tự tạo mới.

**Response 200:** `data` là số bản ghi bị ảnh hưởng.  
**Response 400 / 404:** Validation lỗi hoặc không tìm thấy.

---

### 4.6 Xóa một loại TPL

```
DELETE /api/SalaryComponentTypes/{id}
```

> ⚠️ Không thể xóa nếu còn TPL nào đang dùng loại này.

**Response 200:** Xóa thành công.  
**Response 400:** Đang được tham chiếu bởi TPL khác.  
**Response 404:** Không tìm thấy.

---

### 4.7 Xóa nhiều — fail-fast

```
POST /api/SalaryComponentTypes/bulk-delete
```

**Request body:** `["uuid-1", "uuid-2"]`

---

### 4.8 Xóa nhiều — partial

```
POST /api/SalaryComponentTypes/bulk-delete/partial
```

**Request body:** `["uuid-1", "uuid-2"]`

**Response 200:**
```json
{
  "data": {
    "succeeded": ["uuid-1"],
    "failed": [{ "id": "uuid-2", "reason": "Không thể xóa vì dữ liệu đang được sử dụng ở nơi khác" }]
  }
}
```

---

### 4.9 Cập nhật một field (PATCH)

```
PATCH /api/SalaryComponentTypes/{id}/{fieldName}
```

Body là **JSON value trực tiếp**:

```json
// PATCH /api/SalaryComponentTypes/{id}/Name
"Phụ cấp lương"

// PATCH /api/SalaryComponentTypes/{id}/SortOrder
5
```

---

## 5. TPL Hệ thống — SalaryCompositionSystems

> **Base path:** `/api/SalaryCompositionSystems`  
> ⚠️ Chỉ đọc — POST / PUT / DELETE / PATCH đều trả `405 Method Not Allowed`.

---

### 5.1 Lấy danh sách (phân trang)

```
GET /api/SalaryCompositionSystems/Paging
```

**Query params:**

| Param | Type | Mặc định | Mô tả |
|---|---|---|---|
| `search` | `string` | — | Tìm trong Code, Name |
| `pageSize` | `int` | `10` | Số bản ghi mỗi trang |
| `pageIndex` | `int` | `1` | Trang hiện tại |
| `sort` | `string` | — | Ví dụ: `-CreateDate` |
| `searchFields` | `string` | — | Tên field cách nhau bằng `;` |

> `componentTypeName` được trả về trong mỗi phần tử.

---

### 5.2 Lấy toàn bộ (không phân trang)

```
GET /api/SalaryCompositionSystems
```

---

### 5.3 Lấy chi tiết một TPL hệ thống

```
GET /api/SalaryCompositionSystems/{id}
```

**Response 200:** `data` là object `SalaryCompositionSystem`.  
**Response 404:** Không tìm thấy.

---

### 5.4 Lọc nâng cao theo nghiệp vụ

```
GET /api/SalaryCompositionSystems/filter
```

**Query params:**

| Param | Type | Mô tả |
|---|---|---|
| `search` | `string` | Tìm kiếm trong Code và Name |
| `organizationID` | `uuid` | Lọc theo đơn vị áp dụng |
| `componentTypeID` | `uuid` | Lọc theo loại TPL |
| `nature` | `int` | Lọc theo tính chất (1–4) |
| `pageSize` | `int` | Mặc định `10` |
| `pageIndex` | `int` | Mặc định `1` |

**Ví dụ:** Lấy TPL hệ thống tính chất Thu nhập:
```
GET /api/SalaryCompositionSystems/filter?nature=1
```

**Response 200:** Cấu trúc phân trang, `data` là mảng `SalaryCompositionSystem` kèm `componentTypeName`.

---

### 5.5 Lọc nâng cao — AdvancedFilter

```
POST /api/SalaryCompositionSystems/AdvancedFilter
POST /api/SalaryCompositionSystems/AdvancedFilterProc
```

> ⚠️ Chỉ trả về TPL hệ thống **chưa được kế thừa** sang TPL đơn vị. TPL đã có bản ghi tương ứng trong `pa_salary_composition` với `Source = 2` sẽ không xuất hiện trong kết quả.

**Request body — 3 phần lọc:**

```json
{
  "pageIndex": 1,
  "pageSize": 20,
  "sort": "-CreateDate",
  "search": "lương",
  "componentTypeID": "00000000-0000-0000-0000-000000000006",
  "filters": [
    { "field": "Nature", "operator": "Eq", "value": 1 }
  ],
  "filterLogic": 0
}
```

| Field | Type | Mô tả |
|---|---|---|
| `pageIndex` | `int` | Trang hiện tại (mặc định `1`) |
| `pageSize` | `int` | Số bản ghi mỗi trang (mặc định `10`) |
| `sort` | `string?` | Xem quy ước sort mục 1.5 |
| `search` | `string?` | **Phần 1** — Tìm trong Code **hoặc** Name (OR) |
| `componentTypeID` | `uuid?` | **Phần 2** — Lọc theo loại TPL. `null` = không lọc |
| `filters` | `FilterCondition[]?` | **Phần 3** — Lọc nâng cao theo trường, xem mục 8 |
| `filterLogic` | `int` | `0` = AND (mặc định), `1` = OR — áp dụng cho `filters` |

**Response 200:** Cấu trúc phân trang, `data` là mảng `SalaryCompositionSystem` kèm `componentTypeName`.

---

## 6. TPL Đơn vị — SalaryCompositions

> **Base path:** `/api/SalaryCompositions`

---

### 6.1 Lấy danh sách (phân trang + tìm kiếm cơ bản)

```
GET /api/SalaryCompositions/Paging
```

**Query params:** tương tự mục 5.1. `componentTypeName` được trả về trong mỗi phần tử.

---

### 6.2 Lấy toàn bộ (không phân trang)

```
GET /api/SalaryCompositions
```

---

### 6.3 Lấy chi tiết một TPL

```
GET /api/SalaryCompositions/{id}
```

**Response 200:** `data` là object `SalaryComposition`.  
**Response 404:** Không tìm thấy.

---

### 6.4 Lọc nâng cao theo nghiệp vụ

```
GET /api/SalaryCompositions/filter
```

**Query params:**

| Param | Type | Mô tả |
|---|---|---|
| `search` | `string` | Tìm kiếm trong Code và Name |
| `organizationIDs` | `uuid[]` | Lọc theo một hoặc nhiều đơn vị — truyền lặp lại param |
| `componentTypeID` | `uuid` | Lọc theo loại TPL |
| `nature` | `int` | Lọc theo tính chất (1–4) |
| `status` | `int` | `0` = Bỏ theo dõi, `1` = Đang theo dõi |
| `source` | `int` | `1` = Tự thêm, `2` = Kế thừa hệ thống |
| `pageSize` | `int` | Mặc định `10` |
| `pageIndex` | `int` | Mặc định `1` |

> ⚠️ `organizationIDs` là **số nhiều**. Để lọc nhiều đơn vị, truyền lặp lại param:
> ```
> ?organizationIDs=uuid-1&organizationIDs=uuid-2&organizationIDs=uuid-3
> ```
> Bỏ trống = lấy tất cả đơn vị.

**Ví dụ:** Lấy TPL đang theo dõi, tính chất Thu nhập, trang 1:
```
GET /api/SalaryCompositions/filter?status=1&nature=1&pageIndex=1&pageSize=20
```

**Ví dụ:** Lọc theo 2 đơn vị cụ thể:
```
GET /api/SalaryCompositions/filter?organizationIDs=00000000-0000-0000-0001-000000000001&organizationIDs=00000000-0000-0000-0001-000000000002
```

**Response 200:** Cấu trúc phân trang, `data` là mảng `SalaryComposition` kèm `componentTypeName`.

---

### 6.5 Tạo mới TPL

```
POST /api/SalaryCompositions
```

**Request body:**
```json
{
  "code": "PHU_CAP_AN_TRUA",
  "name": "Phụ cấp ăn trưa",
  "componentTypeID": "uuid-loai-phu-cap",
  "nature": 1,
  "taxType": 2,
  "valueType": 1,
  "valueMode": 1,
  "valueFormula": null,
  "normFormula": null,
  "allowExceedNorm": false,
  "description": "Phụ cấp ăn trưa hàng tháng",
  "showOnPayslip": true,
  "hideWhenZero": false,
  "organizationID": null
}
```

> FE không cần truyền: `salaryCompositionID` (auto-generate), `componentTypeName` (read-only), `source` (mặc định `Custom`), `status` (mặc định `Active`), `systemCompositionID`, `state`, `isDeleted`, `createDate`, `modifiedDate`.

**Response 201:** `data` là số bản ghi được thêm (`1`).  
**Response 400:** Validation lỗi — `data` là mảng `[{ field, message }]`.  
**Response 409:** Mã đã tồn tại.

---

### 6.6 Cập nhật TPL

```
PUT /api/SalaryCompositions/{id}
```

**Request body:** Tương tự tạo mới.

> ⚠️ `code` **không được thay đổi** sau khi lưu — backend sẽ trả lỗi 400 nếu gửi code khác.

**Response 200:** `data` là số bản ghi bị ảnh hưởng.  
**Response 400:** Validation lỗi hoặc cố đổi `code`.  
**Response 404:** Không tìm thấy.

---

### 6.7 Xóa một TPL

```
DELETE /api/SalaryCompositions/{id}
```

> ⚠️ TPL kế thừa từ hệ thống (`source = 2`) không được phép xóa.

**Response 200:** Xóa thành công.  
**Response 400:** Không thể xóa (TPL hệ thống).  
**Response 404:** Không tìm thấy.

---

### 6.8 Xóa nhiều TPL — fail-fast

```
POST /api/SalaryCompositions/bulk-delete
```

**Request body:**
```json
["uuid-1", "uuid-2", "uuid-3"]
```

Nếu **bất kỳ** ID nào thất bại → rollback toàn bộ, không xóa gì cả.

**Response 200:** Xóa thành công tất cả.  
**Response 400 / 404:** Một ID thất bại → toàn bộ bị rollback.

---

### 6.9 Xóa nhiều TPL — partial

```
POST /api/SalaryCompositions/bulk-delete/partial
```

**Request body:** Tương tự 6.8.

**Response 200:**
```json
{
  "isSuccess": true,
  "code": 200,
  "data": {
    "succeeded": ["uuid-1", "uuid-3"],
    "failed": [
      { "id": "uuid-2", "reason": "Không thể xóa thành phần lương mặc định của hệ thống" }
    ]
  }
}
```

---

### 6.10 Chuyển sang đang theo dõi

```
PATCH /api/SalaryCompositions/{id}/activate
```

Không có request body.

**Response 200:** Cập nhật thành công.  
**Response 404:** Không tìm thấy.

---

### 6.11 Chuyển sang bỏ theo dõi

```
PATCH /api/SalaryCompositions/{id}/deactivate
```

Không có request body.

**Response 200:** Cập nhật thành công.  
**Response 404:** Không tìm thấy.

---

### 6.12 Chuyển nhiều TPL sang đang theo dõi — partial

```
PATCH /api/SalaryCompositions/bulk-activate
```

**Request body:** `["uuid-1", "uuid-2"]`

**Response 200:**
```json
{
  "data": {
    "succeeded": ["uuid-1"],
    "failed": [{ "id": "uuid-2", "reason": "Không tìm thấy thành phần lương" }]
  }
}
```

---

### 6.13 Chuyển nhiều TPL sang bỏ theo dõi — partial

```
PATCH /api/SalaryCompositions/bulk-deactivate
```

**Request body:** `["uuid-1", "uuid-2"]`

**Response 200:** Tương tự 6.12.

---

### 6.14 Kế thừa một TPL hệ thống

```
POST /api/SalaryCompositions/inherit/{systemCompositionId}
```

Không có request body — backend tự lấy thông tin từ `systemCompositionId` và tạo TPL đơn vị với `source = InheritedFromSystem`.

**Response 201:** Tạo thành công.  
**Response 400:** Validation lỗi (ví dụ: `ComponentTypeID` null trong TPL hệ thống).  
**Response 404:** Không tìm thấy TPL hệ thống.  
**Response 409:** Mã TPL đã tồn tại trong đơn vị.

---

### 6.15 Kế thừa nhiều TPL hệ thống cùng lúc — partial

```
POST /api/SalaryCompositions/inherit/batch
```

**Request body:**
```json
["uuid-system-1", "uuid-system-2", "uuid-system-3"]
```

Tiếp tục kể cả khi một số ID thất bại (partial result).

**Response 200:**
```json
{
  "isSuccess": true,
  "code": 200,
  "data": {
    "succeeded": ["uuid-system-1", "uuid-system-3"],
    "failed": [
      { "id": "uuid-system-2", "reason": "Mã thành phần lương đã tồn tại" }
    ]
  }
}
```

---

### 6.16 Gợi ý TPL cho ô nhập công thức

```
GET /api/SalaryCompositions/suggestions?search={keyword}
```

> Chỉ trả về TPL có `status = Active`. Dùng cho autocomplete khi nhập công thức.

**Query params:**

| Param | Type | Mô tả |
|---|---|---|
| `search` | `string` | Tùy chọn — lọc theo Code hoặc Name. Bỏ trống = trả hết |

**Response 200:**
```json
{
  "isSuccess": true,
  "code": 200,
  "data": [
    { "code": "LUONG_CO_BAN", "name": "Lương cơ bản", "description": "Lương theo hợp đồng lao động" },
    { "code": "PHU_CAP_AN_TRUA", "name": "Phụ cấp ăn trưa", "description": null }
  ]
}
```

> Kết quả sắp xếp theo `Code` ASC.

---

### 6.17 Cập nhật một field (PATCH)

```
PATCH /api/SalaryCompositions/{id}/{fieldName}
```

Body là **JSON value trực tiếp** (không wrap object):

```json
// PATCH /api/SalaryCompositions/{id}/Description
"Mô tả mới cho thành phần lương"

// PATCH /api/SalaryCompositions/{id}/ShowOnPayslip
true

// PATCH /api/SalaryCompositions/{id}/NormFormula
"LUONG_CO_BAN * 3"
```

> ⚠️ Không được PATCH các field: `Code` (`[NotPatchable]`), `ComponentTypeName` (read-only), `SalaryCompositionID`, `CreatedBy`, `CreateDate`, `ModifiedBy`, `ModifiedDate`, `State`, `IsDeleted`.

**Response 200:** `data` là `1`.  
**Response 400:** Field không tồn tại hoặc không được phép sửa.  
**Response 404:** Bản ghi không tìm thấy.

---

### 6.18 Lọc nâng cao — AdvancedFilter

```
POST /api/SalaryCompositions/AdvancedFilter
POST /api/SalaryCompositions/AdvancedFilterProc
```

**Request body — 4 phần lọc:**

```json
{
  "pageIndex": 1,
  "pageSize": 20,
  "sort": "-CreateDate",
  "search": "lương",
  "status": 1,
  "organizationIDs": ["uuid-org-1", "uuid-org-2"],
  "filters": [
    { "field": "Nature", "operator": "Eq", "value": 1 }
  ],
  "filterLogic": 0
}
```

| Field | Type | Mô tả |
|---|---|---|
| `pageIndex` | `int` | Trang hiện tại (mặc định `1`) |
| `pageSize` | `int` | Số bản ghi mỗi trang (mặc định `10`) |
| `sort` | `string?` | Xem quy ước sort mục 1.5 |
| `search` | `string?` | **Phần 1** — Tìm trong Code **hoặc** Name (OR) |
| `status` | `int?` | **Phần 2** — `0` = Bỏ theo dõi, `1` = Đang theo dõi. `null` = không lọc |
| `organizationIDs` | `uuid[]?` | **Phần 3** — Lọc theo đơn vị. `null` hoặc `[]` = không lọc |
| `filters` | `FilterCondition[]?` | **Phần 4** — Lọc nâng cao theo trường, xem mục 8 |
| `filterLogic` | `int` | `0` = AND (mặc định), `1` = OR — áp dụng cho `filters` |

**Response 200:** Cấu trúc phân trang, `data` là mảng `SalaryComposition` kèm `componentTypeName` và `organizationName`.

---

## 7. Tùy chỉnh cột — GridConfigs

> **Base path:** `/api/GridConfigs`  
> Lưu trữ cấu hình cột (thứ tự, độ rộng, ẩn/hiện, ghim) theo từng user và từng lưới.  
> Mỗi lưới dùng một `gridCode` riêng biệt — config TPL Hệ thống và TPL Đơn vị là độc lập.

### GridCode conventions

| Lưới | `gridCode` |
|---|---|
| TPL Hệ thống | `SALARY_COMPOSITION_SYSTEM_LIST` |
| TPL Đơn vị | `SALARY_COMPOSITION_LIST` |

---

### 7.1 Lấy config cột của 1 user cho 1 lưới

```
GET /api/GridConfigs/by-grid?userID={userID}&gridCode={gridCode}
```

**Query params:**

| Param | Type | Bắt buộc | Mô tả |
|---|---|---|---|
| `userID` | `string` | ✅ | ID người dùng |
| `gridCode` | `string` | ✅ | Mã lưới — xem bảng conventions |

**Ví dụ:**
```
GET /api/GridConfigs/by-grid?userID=user123&gridCode=SALARY_COMPOSITION_LIST
```

**Response 200:**
```json
{
  "isSuccess": true,
  "code": 200,
  "data": [
    {
      "gridConfigID": "uuid",
      "userID": "user123",
      "gridCode": "SALARY_COMPOSITION_LIST",
      "columnKey": "code",
      "caption": "Mã",
      "orderIndex": 0,
      "width": 120,
      "isPinned": true,
      "pinPosition": 1,
      "isVisible": true
    },
    {
      "gridConfigID": "uuid",
      "userID": "user123",
      "gridCode": "SALARY_COMPOSITION_LIST",
      "columnKey": "name",
      "caption": "Tên",
      "orderIndex": 1,
      "width": 200,
      "isPinned": false,
      "pinPosition": null,
      "isVisible": true
    }
  ]
}
```

> Nếu user chưa có config → trả về mảng rỗng `[]`. FE tự áp dụng cấu hình mặc định.

---

### 7.2 Lưu lại toàn bộ config cột (batch upsert)

```
PUT /api/GridConfigs/batch
```

**Hành vi:** Xóa toàn bộ config cũ của `(userID, gridCode)` rồi ghi lại mới. Gọi mỗi lần user lưu cấu hình.

**Request body:**
```json
{
  "userID": "user123",
  "gridCode": "SALARY_COMPOSITION_LIST",
  "columns": [
    {
      "columnKey": "code",
      "caption": "Mã",
      "orderIndex": 0,
      "width": 120,
      "isPinned": true,
      "pinPosition": 1,
      "isVisible": true
    },
    {
      "columnKey": "name",
      "caption": "Tên",
      "orderIndex": 1,
      "width": 200,
      "isPinned": false,
      "pinPosition": null,
      "isVisible": true
    },
    {
      "columnKey": "status",
      "caption": "Trạng thái",
      "orderIndex": 2,
      "width": null,
      "isPinned": false,
      "pinPosition": null,
      "isVisible": false
    }
  ]
}
```

**GridConfigColumnDto fields:**

| Field | Type | Mô tả |
|---|---|---|
| `columnKey` | `string` | Tên cột — camelCase, khớp với key trong lưới FE (ví dụ: `code`, `name`, `componentType`, `taxType`, `showOnPayslip`) |
| `caption` | `string?` | Tiêu đề cột tùy chỉnh — `null` = dùng tiêu đề mặc định |
| `orderIndex` | `int` | Thứ tự hiển thị (bắt đầu từ `0`) |
| `width` | `int?` | Độ rộng cột (px) — `null` = độ rộng tự động |
| `isPinned` | `bool` | `true` = cột đang được ghim |
| `pinPosition` | `int?` | `1` = ghim trái, `2` = ghim phải, `null` = không ghim |
| `isVisible` | `bool` | `true` = hiển thị, `false` = ẩn |

**Response 200:** `data` là số cột đã ghi.  
**Response 400:** `userID` hoặc `gridCode` rỗng, hoặc `columns` rỗng.

---

## 8. Lọc nâng cao — AdvancedFilter

> Dùng khi cần lọc theo bất kỳ field nào của entity với nhiều điều kiện kết hợp.  
> Có 2 endpoint cho mỗi entity: **Dynamic SQL** (C# build) và **Stored Procedure** — cùng request body, cùng kết quả.

### 8.1 SalaryCompositions — request body

Xem chi tiết tại mục [6.18](#618-lọc-nâng-cao--advancedfilter).

### 8.2 SalaryCompositionSystems — request body

Xem chi tiết tại mục [5.5](#55-lọc-nâng-cao--advancedfilter).

### 8.3 FilterCondition object

| Field | Type | Mô tả |
|---|---|---|
| `field` | `string` | Tên property của entity (PascalCase theo C#) |
| `operator` | `string` | Xem bảng operators |
| `value` | `any` | Giá trị so sánh (dùng cho hầu hết operators) |
| `valueTo` | `any` | Giá trị thứ hai — chỉ dùng cho `Between` / `NotBetween` |
| `values` | `any[]` | Mảng giá trị — chỉ dùng cho `In` / `NotIn` |

### 8.4 Danh sách operators

| Operator | Mô tả | Cần `value` | Cần `valueTo` | Cần `values` |
|---|---|---|---|---|
| `Eq` | Bằng | ✅ | | |
| `Neq` | Khác | ✅ | | |
| `Contains` | Chứa chuỗi | ✅ | | |
| `NotContains` | Không chứa chuỗi | ✅ | | |
| `StartsWith` | Bắt đầu bằng | ✅ | | |
| `EndsWith` | Kết thúc bằng | ✅ | | |
| `Empty` | Rỗng / null | | | |
| `NotEmpty` | Không rỗng | | | |
| `Gt` | Lớn hơn | ✅ | | |
| `Lt` | Nhỏ hơn | ✅ | | |
| `Gte` | Lớn hơn hoặc bằng | ✅ | | |
| `Lte` | Nhỏ hơn hoặc bằng | ✅ | | |
| `Between` | Trong khoảng | ✅ | ✅ | |
| `NotBetween` | Ngoài khoảng | ✅ | ✅ | |
| `Today` | Hôm nay | | | |
| `ThisWeek` | Tuần này | | | |
| `ThisMonth` | Tháng này | | | |
| `ThisYear` | Năm này | | | |
| `LastNDays` | N ngày trước đến hôm nay | ✅ (số nguyên) | | |
| `NextNDays` | Hôm nay đến N ngày sau | ✅ (số nguyên) | | |
| `In` | Trong danh sách | | | ✅ |
| `NotIn` | Ngoài danh sách | | | ✅ |

### 8.5 Ví dụ thực tế

**Lọc TPL kế thừa hệ thống, đang theo dõi, tính chất Thu nhập hoặc Khấu trừ:**
```json
{
  "pageIndex": 1,
  "pageSize": 10,
  "sort": "Code",
  "filters": [
    { "field": "Source", "operator": "Eq", "value": 2 },
    { "field": "Status", "operator": "Eq", "value": 1 },
    { "field": "Nature", "operator": "In", "values": [1, 2] }
  ]
}
```

**Tìm TPL có ValueFormula chứa mã LUONG_CO_BAN:**
```json
{
  "filters": [
    { "field": "ValueFormula", "operator": "Contains", "value": "LUONG_CO_BAN" }
  ]
}
```

**Tìm TPL được tạo trong 30 ngày gần đây:**
```json
{
  "filters": [
    { "field": "CreateDate", "operator": "LastNDays", "value": 30 }
  ]
}
```

---

## 9. Mã lỗi HTTP

| Code | Ý nghĩa | Trường hợp |
|---|---|---|
| `200` | OK | Thành công (GET, PUT, PATCH, DELETE) |
| `201` | Created | Tạo mới thành công (POST) |
| `400` | Bad Request | Validation lỗi; cố sửa `Code`; xóa TPL hệ thống |
| `404` | Not Found | Không tìm thấy bản ghi theo ID |
| `405` | Method Not Allowed | Gọi POST/PUT/DELETE/PATCH trên SalaryCompositionSystems |
| `409` | Conflict | Mã (`Code`) đã tồn tại trong hệ thống |
| `500` | Server Error | Lỗi server — kiểm tra `devMessage` |

---

## Phụ lục — Field name reference (dùng cho AdvancedFilter)

### SalaryComponentType fields

| Field (PascalCase) | Kiểu | Ghi chú |
|---|---|---|
| `ComponentTypeID` | `uuid` | Khóa chính |
| `Code` | `string` | Mã duy nhất |
| `Name` | `string` | |
| `SortOrder` | `int?` | |
| `IsActive` | `bool` | |
| `CreateDate` | `datetime` | |

### SalaryCompositionSystem fields

| Field (PascalCase) | Kiểu | Ghi chú |
|---|---|---|
| `SystemCompositionID` | `uuid` | Khóa chính |
| `Code` | `string` | Mã duy nhất |
| `Name` | `string` | |
| `ComponentTypeID` | `uuid?` | |
| `ComponentTypeName` | `string?` | Read-only — không dùng làm filter field |
| `OrganizationID` | `uuid?` | |
| `OrganizationName` | `string?` | Read-only — không dùng làm filter field |
| `Nature` | `int` | Enum SalaryNature |
| `TaxType` | `int?` | Enum SalaryTaxType |
| `TaxDeductible` | `bool` | Giảm trừ khi tính thuế TNCN |
| `ValueType` | `int` | Enum SalaryValueType |
| `ValueFormula` | `string?` | |
| `DefaultValue` | `decimal?` | Giá trị số — dùng khi không có công thức |
| `NormFormula` | `string?` | |
| `ShowOnPayslip` | `bool` | |
| `CreateDate` | `datetime` | |

### SalaryComposition fields

| Field (PascalCase) | Kiểu | Ghi chú |
|---|---|---|
| `SalaryCompositionID` | `uuid` | Khóa chính |
| `Code` | `string` | Mã duy nhất — không sửa được sau khi lưu |
| `Name` | `string` | |
| `OrganizationID` | `uuid?` | |
| `OrganizationName` | `string?` | Read-only — không dùng làm filter field |
| `ComponentTypeID` | `uuid` | |
| `ComponentTypeName` | `string?` | Read-only — không dùng làm filter field |
| `SystemCompositionID` | `uuid?` | |
| `Nature` | `int` | Enum SalaryNature |
| `TaxType` | `int?` | Enum SalaryTaxType |
| `TaxDeductible` | `bool` | Giảm trừ khi tính thuế TNCN |
| `ValueType` | `int` | Enum SalaryValueType |
| `ValueMode` | `int` | Enum SalaryValueMode |
| `ValueFormula` | `string?` | |
| `NormFormula` | `string?` | |
| `AllowExceedNorm` | `bool` | |
| `Description` | `string?` | |
| `ShowOnPayslip` | `bool` | |
| `HideWhenZero` | `bool` | |
| `Source` | `int` | Enum SalaryCompositionSource |
| `Status` | `int` | Enum SalaryCompositionStatus |
| `CreateDate` | `datetime` | |
| `ModifiedDate` | `datetime?` | |
