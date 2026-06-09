# Tài liệu API — Module Thành Phần Lương (TPL)

> **Base URL:** `http://localhost:5237/api`  
> **Content-Type:** `application/json`  
> **Tác giả:** Nguyen Thiet Do — 2026-05-28

---

## Mục lục

1. [Quy ước chung](#1-quy-ước-chung)
2. [Enum reference](#2-enum-reference)
3. [Model reference](#3-model-reference)
   - [3.6 Luồng xác nhận khi công thức có TPL ngừng theo dõi](#36-luồng-xác-nhận-khi-công-thức-có-tpl-ngừng-theo-dõi)
   - [3.7 Response chi tiết khi công thức có mã TPL không tồn tại](#37-response-chi-tiết-khi-công-thức-có-mã-tpl-không-tồn-tại)
4. [Loại TPL — SalaryComponentTypes](#4-loại-tpl--salarycomponenttypes)
5. [TPL Hệ thống — SalaryCompositionSystems](#5-tpl-hệ-thống--salarycompositionsystems)
6. [TPL Đơn vị — SalaryCompositions](#6-tpl-đơn-vị--salarycompositions)
   - [6.18 Cập nhật nhiều field cùng lúc (PATCH fields)](#618-cập-nhật-nhiều-field-cùng-lúc-patch-fields)
   - [6.20 Phân loại TPL trước khi xóa / ngừng theo dõi hàng loạt](#620-phân-loại-tpl-trước-khi-xóa--ngừng-theo-dõi-hàng-loạt)
7. [Tùy chỉnh cột — GridConfigs](#7-tùy-chỉnh-cột--gridconfigs)
8. [FilterCondition — operators reference](#8-lọc-nâng-cao--datapaging)
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

> Với lỗi công thức tham chiếu mã TPL không tồn tại, mỗi entry có thể kèm thêm field `missingCodes: [{ code, position, length }]` để FE highlight chính xác trong input. Các loại lỗi khác **không** có field này. Chi tiết xem mục [3.7](#37-response-chi-tiết-khi-công-thức-có-mã-tpl-không-tồn-tại).

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

### SalaryAutoSumScope — Phạm vi cộng tổng tự động

> Chỉ có ý nghĩa khi `valueMode = 1` (AutoSum).

| Giá trị | Tên | Mô tả |
|---|---|---|
| `1` | SameWorkUnit | Trong cùng đơn vị công tác |
| `2` | Subordinates | Dưới quyền |
| `3` | OrgStructure | Thuộc cơ cấu tổ chức |

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

### TaxFormulaSource — Nguồn công thức thuế

> Chỉ có ý nghĩa khi `taxType = 3` (PartiallyExempt). Backend tự set — FE dùng để render UI.

| Giá trị | Tên | Mô tả |
|---|---|---|
| `0` | None | Cả hai công thức đều do người dùng nhập thủ công |
| `1` | ExemptDerived | `exemptFormula` được backend tự suy = `(valueFormula) - (taxableFormula)` |
| `2` | TaxableDerived | `taxableFormula` được backend tự suy = `(valueFormula) - (exemptFormula)` |

### LockableField — ID các field có thể bị khóa khi kế thừa hệ thống

> Lưu trong `lockedFields` dưới dạng JSON array of int (ví dụ `"[2,5,7]"`).
> `Code` luôn bị khóa cứng nên không có trong enum này. ID là số nguyên cố định — **không được tái sử dụng / đổi giá trị**, chỉ thêm mới ở cuối.

| ID | Tên field | Display name |
|---|---|---|
| `1` | `ComponentTypeID` | Loại thành phần |
| `2` | `Nature` | Tính chất |
| `3` | `TaxType` | Loại thuế TNCN |
| `4` | `TaxDeductible` | Giảm trừ thuế |
| `5` | `ValueType` | Kiểu giá trị |
| `6` | `ValueMode` | Chế độ tính |
| `7` | `ValueFormula` | Công thức giá trị |
| `8` | `ValueScope` | Phạm vi cộng tổng |
| `9` | `ValueScopeLevel` | Cấp phạm vi |
| `10` | `SumSourceCompositionID` | TPL nguồn AutoSum |
| `11` | `NormFormula` | Công thức định mức |
| `12` | `TaxableFormula` | Công thức phần chịu thuế |
| `13` | `ExemptFormula` | Công thức phần miễn thuế |
| `14` | `AllowExceedNorm` | Cho phép vượt định mức |
| `15` | `HideWhenZero` | Ẩn khi bằng 0 |
| `16` | `Name` | Tên thành phần |
| `17` | `Description` | Mô tả |
| `18` | `ShowOnPayslip` | Hiển thị trên phiếu lương |
| `19` | `OrganizationIDs` | Đơn vị áp dụng |
| `20` | `Status` | Trạng thái |

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
  "showOnPayslip": true,
  "lockedFields": "[2,5,7]"
}
```

> **`lockedFields`** — JSON array of int (string), liệt kê ID các field bị khóa khi TPL này được kế thừa sang TPL đơn vị (xem enum `LockableField` mục 2). `null` hoặc `"[]"` = ngoài `Code` không khóa field nào.

### 3.3 SalaryComposition — TPL Đơn vị

```json
{
  "salaryCompositionID": "uuid",
  "code": "LUONG_CO_BAN",
  "name": "Lương cơ bản",
  "organizationIDs": ["uuid-org-1", "uuid-org-2"],
  "organizationNames": "Công ty TNHH ABC, Chi nhánh Hà Nội",
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
  "valueScopeLevel": null,
  "sumSourceCompositionID": null,
  "normFormula": null,
  "taxableFormula": null,
  "exemptFormula": null,
  "taxFormulaSource": 0,
  "allowExceedNorm": false,
  "description": "Lương theo hợp đồng lao động",
  "showOnPayslip": true,
  "hideWhenZero": false,
  "source": 2,
  "status": 1,
  "lockedFields": "[2,5,7]",
  "isSkipUnfollowedComposition": false,
  "createdBy": null,
  "createDate": "2026-05-27T10:00:00",
  "modifiedBy": null,
  "modifiedDate": null
}
```

> **`organizationIDs`** — mảng UUID các đơn vị áp dụng. Backend tự normalize: nếu tất cả con của một đơn vị cha đều có mặt thì gộp lại thành cha.  
> **`organizationNames`** — chuỗi tên đơn vị cách nhau bằng `", "` — read-only, do backend tổng hợp qua `GROUP_CONCAT`.  
> **`valueScope`** — enum `SalaryAutoSumScope` (1/2/3), chỉ có ý nghĩa khi `valueMode = 1`.  
> **`valueScopeLevel`** — số cấp bậc áp dụng (ví dụ: 2 = 2 cấp dưới), dùng kèm `valueScope`.  
> **`sumSourceCompositionID`** — UUID của TPL nguồn khi AutoSum từ TPL cụ thể khác.  
> **`taxableFormula`** — công thức phần **chịu thuế** TNCN — **chỉ dùng khi `taxType = 3` (PartiallyExempt)**. Để `null` với các loại thuế khác.  
> **`exemptFormula`** — công thức phần **miễn thuế** TNCN — **chỉ dùng khi `taxType = 3` (PartiallyExempt)**. Để `null` với các loại thuế khác. Khi `taxType = 3`, chỉ cần truyền **một trong hai**: backend tự suy ra cái còn lại = `(valueFormula) - (cái đã truyền)`. Nếu không có `valueFormula` (AutoSum mode), phải truyền đủ cả hai.  
> **`taxFormulaSource`** — **read-only**, backend tự set. Cho biết công thức nào được tự suy: `0` = cả hai nhập thủ công, `1` = `exemptFormula` được suy từ `(valueFormula) - (taxableFormula)`, `2` = `taxableFormula` được suy từ `(valueFormula) - (exemptFormula)`. FE dùng để ẩn công thức derived khỏi UI (chỉ hiển thị công thức người dùng tự nhập). Khi update, backend tự re-derive dựa theo giá trị này — FE không cần xử lý gì thêm.  
> **`isSkipUnfollowedComposition`** — **không lưu DB**, chỉ dùng khi tạo / cập nhật. Mặc định `false`. Xem mục 3.6.  
> **`lockedFields`** — read-only đối với FE. Khi `source = 2` (InheritedFromSystem), backend snapshot từ `pa_salary_composition_system.LockedFields` tại lúc kế thừa và dùng để chặn sửa các field tương ứng (xem mục 6.6). Khi `source = 1` (Custom) thì luôn `null`.

### 3.4 Quy tắc validation khi tạo / cập nhật TPL

| Field | Bắt buộc | Ràng buộc |
|---|---|---|
| `code` | ✅ | Tối đa 255 ký tự; chỉ `A-Z a-z 0-9 _`; không được là số thuần túy; không được thay đổi sau khi lưu |
| `name` | ✅ | Tối đa 255 ký tự |
| `componentTypeID` | ✅ | Phải tồn tại trong hệ thống |
| `nature` | ✅ | Giá trị hợp lệ: 1–4 |
| `organizationIDs` | ✅ | Không được rỗng; backend normalize: nếu tất cả con của cha đều có → thay bằng cha |
| `taxType` | ❌ | Chỉ truyền khi `nature = 1`; để `null` với tính chất khác |
| `taxDeductible` | ❌ | Mặc định `false` — giá trị khoản được khấu trừ khi tính thuế TNCN |
| `valueFormula` | ❌ | Xem quy tắc công thức bên dưới |
| `normFormula` | ❌ | Xem quy tắc công thức bên dưới |
| `taxableFormula` | ❌ | Chỉ hợp lệ khi `taxType = 3` (PartiallyExempt). Nếu truyền mà `taxType ≠ 3` → 400. Khi `taxType = 3` và có `valueFormula`: chỉ cần một trong hai — backend tự suy ra `exemptFormula = (valueFormula) - (taxableFormula)`. Xem quy tắc công thức bên dưới |
| `exemptFormula` | ❌ | Chỉ hợp lệ khi `taxType = 3` (PartiallyExempt). Nếu truyền mà `taxType ≠ 3` → 400. Khi `taxType = 3` và có `valueFormula`: chỉ cần một trong hai — backend tự suy ra `taxableFormula = (valueFormula) - (exemptFormula)`. Khi **không có** `valueFormula` (AutoSum mode): phải truyền đủ cả hai. Xem quy tắc công thức bên dưới |

### 3.5 Quy tắc công thức (ValueFormula / NormFormula / TaxableFormula / ExemptFormula)

Công thức hỗ trợ các hàm sau (phân biệt hoa thường):

| Hàm | Cú pháp | Mô tả |
|---|---|---|
| `SUM` | `SUM(X1, X2, ...)` | Tổng các tham số — ít nhất 1 tham số |
| `IF` | `IF(điều_kiện, giá_trị_đúng, giá_trị_sai)` | Điều kiện |
| `AND` | `AND(đk1, đk2, ...)` | Và — chỉ dùng trong điều kiện của `IF`, ít nhất 2 tham số |
| `OR` | `OR(đk1, đk2, ...)` | Hoặc — chỉ dùng trong điều kiện của `IF` |
| `INT` | `INT(số)` | Làm tròn xuống số nguyên gần nhất |
| `TODAY` | `TODAY()` | Ngày hiện tại — không có tham số |

**Tham số** hợp lệ: số (ví dụ `1000000`, `0.5`, `-5.2`) hoặc **mã TPL** (ví dụ `LUONG_CO_BAN`).

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

**Normalize tự động:** Backend tự strip dấu `=` ở đầu công thức trước khi validate và lưu. FE có thể gửi `"=LUONG_CO_BAN * 0.08"` hoặc `"LUONG_CO_BAN * 0.08"` — kết quả giống nhau.

**Validation công thức — phân biệt lỗi cứng và cảnh báo:**

| Trường hợp | Loại | Hành vi |
|---|---|---|
| Lỗi cú pháp (hàm không hợp lệ, thiếu ngoặc...) | **Lỗi cứng** | Trả 400 — không lưu |
| Mã TPL không tồn tại trong hệ thống | **Lỗi cứng** | Trả 400 — không lưu |
| Mã TPL tồn tại nhưng đang **ngừng theo dõi** | **Cảnh báo** | Trả 202 — yêu cầu xác nhận (xem mục 3.6) |

---

### 3.6 Luồng xác nhận khi công thức có TPL ngừng theo dõi

Khi POST / PUT với công thức chứa mã TPL ngừng theo dõi (status = 0), backend **không lưu ngay** mà trả về HTTP **202** để FE hiển thị dialog xác nhận.

**Bước 1 — Gửi lần đầu** (`isSkipUnfollowedComposition: false`, mặc định):

```json
// POST /api/SalaryCompositions
{
  "code": "BHXH_NLD",
  "valueFormula": "LUONG_CO_BAN * 0.08 + PHU_CAP_NGUNG",
  "isSkipUnfollowedComposition": false,
  ...
}
```

**Response HTTP 202** — cảnh báo, chưa lưu:
```json
{
  "isSuccess": false,
  "code": 202,
  "data": {
    "requiresConfirmation": true,
    "inactiveCodes": [
      { "code": "PHU_CAP_NGUNG", "name": "Phụ cấp đã ngừng" }
    ]
  },
  "userMessage": "Công thức có 1 thành phần lương đang ngừng theo dõi: PHU_CAP_NGUNG. Bạn có chắc chắn muốn lưu không?"
}
```

**Bước 2 — Người dùng xác nhận**, FE gửi lại cùng payload với `isSkipUnfollowedComposition: true`:

```json
// POST /api/SalaryCompositions
{
  "code": "BHXH_NLD",
  "valueFormula": "LUONG_CO_BAN * 0.08 + PHU_CAP_NGUNG",
  "isSkipUnfollowedComposition": true,
  ...
}
```

**Response HTTP 201** — lưu thành công.

> ⚠️ `isSkipUnfollowedComposition` **không được lưu vào DB** — backend dùng nó để điều khiển logic validation rồi bỏ qua. SP tự động bỏ qua param thừa.

---

### 3.7 Response chi tiết khi công thức có mã TPL không tồn tại

Khi POST / PUT mà 1 hoặc nhiều công thức (`ValueFormula` / `NormFormula` / `TaxableFormula` / `ExemptFormula`) tham chiếu **mã TPL không tồn tại** trong hệ thống, backend trả 400 kèm danh sách vị trí chi tiết để FE bôi đỏ chính xác từng mã trong ô nhập.

**Đặc điểm:**

- Backend **gom toàn bộ** mã sai trong cùng công thức (không fail-fast). Nếu công thức có 3 mã sai, FE nhận đủ 3.
- Mỗi công thức có lỗi → một entry riêng trong `data[]`, kèm field `missingCodes`.
- Lỗi cú pháp thuần (không liên quan mã) → entry vẫn nằm trong `data[]` nhưng **không** có `missingCodes`.
- Nếu công thức **vừa** có lỗi cú pháp **vừa** có mã không tồn tại: backend giữ lại các mã đã thu thập được tới điểm fail + ghép vào `message`.

**Cấu trúc entry:**

| Field | Type | Mô tả |
|---|---|---|
| `field` | `string` | Tên field công thức (`ValueFormula`, `NormFormula`, `TaxableFormula`, `ExemptFormula`) |
| `message` | `string` | Thông báo tiếng Việt — dùng cho toast / tooltip |
| `missingCodes` | `MissingCode[]?` | Có khi và chỉ khi có mã không tồn tại |

**`MissingCode` object:**

| Field | Type | Mô tả |
|---|---|---|
| `code` | `string` | Mã TPL không tồn tại |
| `position` | `int` | Vị trí tuyệt đối trong chuỗi công thức gốc (FE gửi), tính từ `0`. **Bao gồm cả ký tự `=` đầu** nếu có |
| `length` | `int` | Số ký tự của mã — dùng kèm `position` để highlight range |

**Ví dụ — payload gửi lên:**

```json
{
  "code": "BHXH_NLD",
  "valueFormula": "=LUONG_CO_BAN + PHU_CAP_XANG + LUONG_KHONG_TON_TAI - PHU_CAP_AO",
  "normFormula": "SUM(A, B,)",
  ...
}
```

**Response HTTP 400:**

```json
{
  "isSuccess": false,
  "code": 400,
  "devMessage": "Validate thất bại",
  "userMessage": null,
  "data": [
    {
      "field": "ValueFormula",
      "message": "Công thức giá trị có 2 mã thành phần lương không tồn tại: LUONG_KHONG_TON_TAI, PHU_CAP_AO",
      "missingCodes": [
        { "code": "LUONG_KHONG_TON_TAI", "position": 31, "length": 18 },
        { "code": "PHU_CAP_AO",          "position": 52, "length": 10 }
      ]
    },
    {
      "field": "NormFormula",
      "message": "Công thức định mức không hợp lệ: Mong đợi số hoặc thành phần lương tại vị trí 10, nhưng gặp ')'"
    }
  ]
}
```

> Phân biệt với mã đang **ngừng theo dõi** (status = 0): mã ngừng theo dõi vẫn tồn tại trong hệ thống → trả 202 + `inactiveCodes`, cho phép user xác nhận để lưu (xem mục 3.6). Mã không tồn tại → trả 400 + `missingCodes`, **bắt buộc** sửa trước khi lưu.

**Gợi ý dùng phía FE:**

```js
for (const err of response.data) {
  if (err.missingCodes?.length) {
    // bôi đỏ từng range trong ô input của err.field
    err.missingCodes.forEach(m => highlight(err.field, m.position, m.length));
  } else {
    // chỉ lỗi cú pháp — show toast với err.message
    showError(err.field, err.message);
  }
}
```

---

## 4. Loại TPL — SalaryComponentTypes

> **Base path:** `/api/SalaryComponentTypes`  
> CRUD đầy đủ — không có endpoint nghiệp vụ riêng.

---

### 4.1 Lấy danh sách (phân trang)

```
GET /api/SalaryComponentTypes/paging
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

> Xem mục 4.10 để PATCH nhiều field cùng lúc.

---

### 4.10 Cập nhật nhiều field cùng lúc (PATCH fields)

```
PATCH /api/SalaryComponentTypes/{id}/fields
```

Body là **JSON object** `{ "FieldName": value, ... }`:

```json
{
  "Name": "Phụ cấp lương tháng",
  "SortOrder": 5,
  "IsActive": true
}
```

> Validate tất cả field trước khi ghi — trả về mảng lỗi nếu nhiều field không hợp lệ (khác PATCH đơn field chỉ trả lỗi đầu tiên).  
> Không được PATCH: `ComponentTypeID` (PK), `CreatedBy`, `CreateDate`, `ModifiedBy`, `ModifiedDate`, `State`, `IsDeleted`.

**Response 200:** `data` là số bản ghi bị ảnh hưởng.  
**Response 400:** Validation lỗi — `data` là mảng `[{ field, message }]`.  
**Response 404:** Bản ghi không tìm thấy.

---

## 5. TPL Hệ thống — SalaryCompositionSystems

> **Base path:** `/api/SalaryCompositionSystems`  
> ⚠️ Chỉ đọc — POST / PUT / DELETE / PATCH đều trả `405 Method Not Allowed`.

---

### 5.1 Lấy danh sách (phân trang)

```
GET /api/SalaryCompositionSystems/paging
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

### 5.5 Lọc nâng cao — DataPaging

```
POST /api/SalaryCompositionSystems/datapaging
```

> ⚠️ Chỉ trả về TPL hệ thống **chưa được kế thừa** sang TPL đơn vị. TPL đã có bản ghi tương ứng trong `pa_salary_composition` với `Source = 2` sẽ không xuất hiện trong kết quả.

**Request body — 3 phần lọc:**

```json
{
  "pageIndex": 1,
  "pageSize": 20,
  "sort": "-CreateDate",
  "search": "lương",
  "searchFields": ["Code", "Name"],
  "componentTypeID": "00000000-0000-0000-0000-000000000006",
  "filters": [
    { "field": "Nature", "operator": "Eq", "value": 1 }
  ],
  "filterLogic": 0,
  "columns": ["Code", "Name", "Nature", "ComponentTypeName"]
}
```

| Field | Type | Mô tả |
|---|---|---|
| `pageIndex` | `int` | Trang hiện tại (mặc định `1`) |
| `pageSize` | `int` | Số bản ghi mỗi trang (mặc định `10`) |
| `sort` | `string?` | Xem quy ước sort mục 1.5 |
| `search` | `string?` | **Phần 1** — Từ khóa tìm kiếm |
| `searchFields` | `string[]?` | **Phần 1** — Danh sách trường tìm kiếm (OR). `null` hoặc `[]` = mặc định `["Code", "Name"]` |
| `componentTypeID` | `uuid?` | **Phần 2** — Lọc theo loại TPL. `null` = không lọc |
| `filters` | `FilterCondition[]?` | **Phần 3** — Lọc nâng cao theo trường, xem mục 8 |
| `filterLogic` | `int` | `0` = AND (mặc định), `1` = OR — áp dụng cho `filters` |
| `columns` | `string[]?` | Danh sách property name (PascalCase) muốn trả về. `null` hoặc `[]` = trả tất cả. Backend tự convert sang snake_case trước khi truyền vào stored procedure |

**Response 200:** Cấu trúc phân trang, `data` là mảng `SalaryCompositionSystem` kèm `componentTypeName`.

---

## 6. TPL Đơn vị — SalaryCompositions

> **Base path:** `/api/SalaryCompositions`

---

### 6.1 Lấy danh sách (phân trang + tìm kiếm cơ bản)

```
GET /api/SalaryCompositions/paging
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
  "valueMode": 2,
  "valueFormula": "LUONG_CO_BAN * 0.1",
  "valueScope": null,
  "valueScopeLevel": null,
  "sumSourceCompositionID": null,
  "normFormula": null,
  "taxableFormula": null,
  "exemptFormula": null,
  "allowExceedNorm": false,
  "description": "Phụ cấp ăn trưa hàng tháng",
  "showOnPayslip": true,
  "hideWhenZero": false,
  "organizationIDs": ["uuid-org-1", "uuid-org-2"],
  "isSkipUnfollowedComposition": false
}
```

> **`organizationIDs` là bắt buộc** — phải truyền ít nhất 1 đơn vị. Backend tự normalize (gộp lên cha nếu đủ con).  
> FE không cần truyền: `salaryCompositionID` (auto-generate), `componentTypeName` / `organizationNames` (read-only), `source` (mặc định `Custom`), `status` (mặc định `Active`), `systemCompositionID`, `state`, `isDeleted`, `createDate`, `modifiedDate`.  
> **`isSkipUnfollowedComposition`** — mặc định `false`. Xem mục 3.6 để hiểu luồng xác nhận.

**Response 201:** Lưu thành công — `data` là `1`.  
**Response 202:** Công thức chứa TPL ngừng theo dõi — `data` là `{ requiresConfirmation, inactiveCodes[] }`. Gửi lại với `isSkipUnfollowedComposition: true` để lưu.  
**Response 400:** Validation lỗi — `data` là mảng `[{ field, message }]`. Nếu lỗi do công thức tham chiếu mã TPL không tồn tại, entry sẽ có thêm `missingCodes` để FE highlight — xem mục [3.7](#37-response-chi-tiết-khi-công-thức-có-mã-tpl-không-tồn-tại).  
**Response 409:** Mã đã tồn tại.

---

### 6.6 Cập nhật TPL

```
PUT /api/SalaryCompositions/{id}
```

**Request body:** Tương tự tạo mới (bao gồm `isSkipUnfollowedComposition`).

> ⚠️ `code` **không được thay đổi** sau khi lưu — backend sẽ trả lỗi 400 nếu gửi code khác (luôn khóa cứng, không phụ thuộc `source`).

**Giới hạn khi `source = 2` (InheritedFromSystem):**  
Mỗi TPL hệ thống tự khai báo danh sách field bị khóa qua cột `lockedFields` (JSON array of int — xem enum `LockableField` mục 2). Khi kế thừa, danh sách này được snapshot vào TPL đơn vị tại lúc tạo và **không thay đổi** kể cả khi admin sửa policy hệ thống sau này.

- Ngoài `Code` (khóa cứng), tất cả field còn lại đều **có thể** chỉnh sửa.
- Field nào có ID nằm trong `lockedFields` của bản ghi hiện tại → backend trả 400 nếu giá trị mới khác giá trị cũ trong DB.
- `lockedFields = null` hoặc `"[]"` → ngoài `Code` không bị khóa gì.

**Ví dụ:** TPL hệ thống "Lương cơ bản" có `lockedFields = "[2,5,7]"` (khóa `Nature`, `ValueType`, `ValueFormula`):
- Đơn vị kế thừa rồi đổi `Nature` từ 1 → 2 ⇒ 400 "Thành phần lương kế thừa từ hệ thống không cho sửa: Tính chất".
- Đơn vị kế thừa rồi đổi `Description` ⇒ 200 (không nằm trong khóa).

**Response 200:** Cập nhật thành công — `data` là số bản ghi bị ảnh hưởng.  
**Response 202:** Công thức chứa TPL ngừng theo dõi — yêu cầu xác nhận, xem mục 3.6.  
**Response 400:** Validation lỗi; cố đổi `code`; hoặc cố sửa field nằm trong `lockedFields` của TPL hệ thống — `userMessage` liệt kê tên các field vi phạm (display name tiếng Việt). Nếu lỗi do công thức tham chiếu mã TPL không tồn tại, entry sẽ có thêm `missingCodes` — xem mục [3.7](#37-response-chi-tiết-khi-công-thức-có-mã-tpl-không-tồn-tại).  
**Response 404:** Không tìm thấy.

---

### 6.7 Xóa một TPL

```
DELETE /api/SalaryCompositions/{id}
```

> ⚠️ **Hai điều kiện chặn xóa:**
> - **BR-08:** TPL kế thừa từ hệ thống (`source = 2`) không được phép xóa.
> - **BR-11:** TPL đang được tham chiếu (Code xuất hiện như định danh độc lập, phân biệt word-boundary) trong bất kỳ công thức nào (`ValueFormula` / `NormFormula` / `TaxableFormula` / `ExemptFormula`) của TPL khác không được phép xóa.
>
> FE nên gọi `POST /exit-data` trước để phân loại danh sách trước khi cho phép xóa (xem mục 6.20).

**Response 200:** Xóa thành công.  
**Response 400:** Không thể xóa — TPL hệ thống hoặc đang được tham chiếu trong công thức. `userMessage` nêu rõ lý do.  
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

Nếu **bất kỳ** ID nào thất bại (TPL hệ thống hoặc đang được tham chiếu trong công thức) → rollback toàn bộ, không xóa gì cả.

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
    "succeeded": ["uuid-1"],
    "failed": [
      { "id": "uuid-2", "reason": "Không thể xóa thành phần lương mặc định của hệ thống" },
      { "id": "uuid-3", "reason": "Không thể xóa thành phần lương 'Lương cơ bản' vì đang được sử dụng trong công thức của thành phần lương khác" }
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

Backend lấy thông tin từ `systemCompositionId`, tạo TPL đơn vị với `source = InheritedFromSystem` và gắn với các đơn vị trong body.

**Request body (tùy chọn):**
```json
["uuid-org-1", "uuid-org-2"]
```

> Có thể bỏ trống body hoặc truyền `null` / `[]` — backend tự fill toàn công ty (tất cả đơn vị gốc không có cha).  
> Truyền danh sách UUID cụ thể nếu muốn áp dụng cho một số đơn vị nhất định.

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
{
  "systemCompositionIds": ["uuid-system-1", "uuid-system-2", "uuid-system-3"],
  "organizationIDs": ["uuid-org-1", "uuid-org-2"]
}
```

| Field | Type | Mô tả |
|---|---|---|
| `systemCompositionIds` | `uuid[]` | Danh sách ID TPL hệ thống cần kế thừa — bắt buộc |
| `organizationIDs` | `uuid[]` | Danh sách đơn vị áp dụng — bắt buộc, dùng chung cho tất cả TPL trong batch |

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

> Xem mục 6.18 để PATCH nhiều field cùng lúc trong một request.

---

### 6.18 Cập nhật nhiều field cùng lúc (PATCH fields)

```
PATCH /api/SalaryCompositions/{id}/fields
```

Body là **JSON object** `{ "FieldName": value, ... }`:

```json
{
  "Description": "Mô tả mới",
  "ShowOnPayslip": false,
  "Status": 0
}
```

> Validate tất cả field trước khi ghi — trả về **mảng lỗi** nếu nhiều field không hợp lệ (khác PATCH đơn field chỉ trả lỗi đầu tiên gặp).  
> Không được PATCH: `Code` (`[NotPatchable]`), `SalaryCompositionID` (PK), `ComponentTypeName` / `OrganizationNames` (read-only), `CreatedBy`, `CreateDate`, `ModifiedBy`, `ModifiedDate`, `State`, `IsDeleted`.

**Response 200:** `data` là số bản ghi bị ảnh hưởng.  
**Response 400:** Validation lỗi — `data` là mảng `[{ field, message }]`.  
**Response 404:** Bản ghi không tìm thấy.

---

### 6.19 Lọc nâng cao — DataPaging

```
POST /api/SalaryCompositions/datapaging
```

**Request body — 4 phần lọc:**

```json
{
  "pageIndex": 1,
  "pageSize": 20,
  "sort": "-CreateDate",
  "search": "lương",
  "searchFields": ["Code", "Name"],
  "status": 1,
  "organizationIDs": ["uuid-org-1", "uuid-org-2"],
  "filters": [
    { "field": "Nature", "operator": "Eq", "value": 1 }
  ],
  "filterLogic": 0,
  "columns": ["Code", "Name", "Nature", "Status", "ComponentTypeName", "OrganizationNames"]
}
```

| Field | Type | Mô tả |
|---|---|---|
| `pageIndex` | `int` | Trang hiện tại (mặc định `1`) |
| `pageSize` | `int` | Số bản ghi mỗi trang (mặc định `10`) |
| `sort` | `string?` | Xem quy ước sort mục 1.5 |
| `search` | `string?` | **Phần 1** — Từ khóa tìm kiếm |
| `searchFields` | `string[]?` | **Phần 1** — Danh sách trường tìm kiếm (OR). `null` hoặc `[]` = mặc định `["Code", "Name"]` |
| `status` | `int?` | **Phần 2** — `0` = Bỏ theo dõi, `1` = Đang theo dõi. `null` = không lọc |
| `organizationIDs` | `uuid[]?` | **Phần 3** — Lọc theo đơn vị. `null` hoặc `[]` = không lọc |
| `filters` | `FilterCondition[]?` | **Phần 4** — Lọc nâng cao theo trường, xem mục 8 |
| `filterLogic` | `int` | `0` = AND (mặc định), `1` = OR — áp dụng cho `filters` |
| `columns` | `string[]?` | Danh sách property name (PascalCase) muốn trả về. `null` hoặc `[]` = trả tất cả. Backend tự convert sang snake_case trước khi truyền vào stored procedure |

**Response 200:** Cấu trúc phân trang, `data` là mảng `SalaryComposition` kèm `componentTypeName`, `organizationIDs` (mảng UUID) và `organizationNames` (chuỗi tên ghép).

---

### 6.20 Phân loại TPL trước khi xóa / ngừng theo dõi hàng loạt

```
POST /api/SalaryCompositions/exit-data
```

Nhận vào danh sách ID, phân loại mỗi TPL vào **một trong ba nhóm** để FE hiển thị cảnh báo phù hợp trước khi xóa hoặc ngừng theo dõi hàng loạt. Dữ liệu lấy từ cache — không tốn thêm DB round-trip.

**Ưu tiên phân loại:** `DataSystem` > `DataExist` > `DataNotExist`

| Nhóm | Điều kiện | Gợi ý hành động FE |
|---|---|---|
| `DataSystem` | `source = 2` (InheritedFromSystem) — dù có hay không có trong công thức | Cảnh báo nhẹ — không thể xóa (BR-08), có thể ngừng theo dõi |
| `DataExist` | `source = 1` (Custom) **và** Code đang được tham chiếu (word-boundary) trong bất kỳ trường công thức nào (`ValueFormula` / `NormFormula` / `TaxableFormula` / `ExemptFormula`) của TPL khác | Cảnh báo mạnh — không thể xóa, nếu ngừng theo dõi sẽ ảnh hưởng công thức |
| `DataNotExist` | Không thuộc 2 nhóm trên | An toàn — có thể xóa hoặc ngừng theo dõi |

**Request body:**
```json
{
  "ids": ["uuid-1", "uuid-2", "uuid-3", "uuid-4"],
  "pageIndex": 1,
  "pageSize": 10
}
```

| Field | Type | Mặc định | Mô tả |
|---|---|---|---|
| `ids` | `uuid[]` | — | Danh sách ID cần phân loại — bắt buộc, không được rỗng |
| `pageIndex` | `int` | `1` | Trang hiện tại — phân trang trên danh sách `ids` đầu vào |
| `pageSize` | `int` | `10` | Số phần tử mỗi trang |

> Phân trang áp dụng trên danh sách `ids` đầu vào (không phải toàn bộ TPL trong DB).  
> ID không tồn tại hoặc đã bị xóa mềm sẽ bị bỏ qua.

**Response 200:**
```json
{
  "isSuccess": true,
  "code": 200,
  "data": {
    "dataSystem": [
      { "salaryCompositionID": "uuid-2", "code": "BHXH_CTY", "name": "BHXH Công ty", "source": 2, "status": 1, "referencedBy": [] }
    ],
    "dataExist": [
      {
        "salaryCompositionID": "uuid-1", "code": "LUONG_CO_BAN", "name": "Lương cơ bản", "source": 1, "status": 1,
        "referencedBy": [
          { "salaryCompositionID": "uuid-5", "code": "BHXH_NLD", "name": "BHXH Người lao động" },
          { "salaryCompositionID": "uuid-6", "code": "THUE_TNCN", "name": "Thuế TNCN" }
        ]
      }
    ],
    "dataNotExist": [
      { "salaryCompositionID": "uuid-3", "code": "THUONG_KPI", "name": "Thưởng KPI", "source": 1, "status": 0, "referencedBy": [] }
    ],
    "total": 4,
    "pageSize": 10,
    "currentPage": 1,
    "pageCount": 1
  }
}
```

**Response 400:** `ids` rỗng.

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

## 8. Lọc nâng cao — DataPaging

> Dùng khi cần lọc theo bất kỳ field nào của entity với nhiều điều kiện kết hợp.  
> Mỗi entity có một endpoint `POST /datapaging` chạy qua **stored procedure** — SP tự build WHERE từ JSON filters.

### 8.1 SalaryCompositions — request body

Xem chi tiết tại mục [6.19](#619-lọc-nâng-cao--datapaging).

### 8.2 SalaryCompositionSystems — request body

Xem chi tiết tại mục [5.5](#55-lọc-nâng-cao--datapaging).

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
| `202` | Confirmation Required | Công thức chứa TPL ngừng theo dõi — **không phải lỗi**, cần người dùng xác nhận. `isSuccess: false`, `data.requiresConfirmation: true` |
| `400` | Bad Request | Validation lỗi; cố sửa `Code`; xóa TPL hệ thống hoặc TPL đang được tham chiếu trong công thức; cố sửa field bị lock trên TPL hệ thống |
| `404` | Not Found | Không tìm thấy bản ghi theo ID |
| `405` | Method Not Allowed | Gọi POST/PUT/DELETE/PATCH trên SalaryCompositionSystems |
| `409` | Conflict | Mã (`Code`) đã tồn tại trong hệ thống |
| `500` | Server Error | Lỗi server — kiểm tra `devMessage` |

### Phân biệt 202 với 400

```
isSuccess: false + code: 400  →  Lỗi thật, phải sửa trước khi lưu
isSuccess: false + code: 202  →  Cảnh báo, người dùng có thể bỏ qua và lưu
```

---

## Phụ lục — Field name reference (dùng cho DataPaging)

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
| `LockedFields` | `string?` | JSON array of int — ID các field khóa khi kế thừa. Xem enum LockableField mục 2 |
| `CreateDate` | `datetime` | |

### SalaryComposition fields

| Field (PascalCase) | Kiểu | Ghi chú |
|---|---|---|
| `SalaryCompositionID` | `uuid` | Khóa chính |
| `Code` | `string` | Mã duy nhất — không sửa được sau khi lưu |
| `Name` | `string` | |
| `OrganizationIDs` | `uuid[]` | Danh sách đơn vị áp dụng — bắt buộc khi tạo/sửa |
| `OrganizationNames` | `string?` | Read-only — tên các đơn vị ghép bằng `", "`, không dùng làm filter field |
| `ComponentTypeID` | `uuid` | |
| `ComponentTypeName` | `string?` | Read-only — không dùng làm filter field |
| `SystemCompositionID` | `uuid?` | |
| `Nature` | `int` | Enum SalaryNature |
| `TaxType` | `int?` | Enum SalaryTaxType |
| `TaxDeductible` | `bool` | Giảm trừ khi tính thuế TNCN |
| `ValueType` | `int` | Enum SalaryValueType |
| `ValueMode` | `int` | Enum SalaryValueMode |
| `ValueFormula` | `string?` | |
| `ValueScope` | `int?` | Enum SalaryAutoSumScope — chỉ dùng khi `ValueMode = 1` |
| `ValueScopeLevel` | `byte?` | Số cấp bậc áp dụng khi AutoSum |
| `SumSourceCompositionID` | `uuid?` | UUID TPL nguồn khi AutoSum từ TPL cụ thể |
| `NormFormula` | `string?` | |
| `TaxableFormula` | `string?` | Công thức phần chịu thuế — chỉ khi `TaxType = 3` |
| `ExemptFormula` | `string?` | Công thức phần miễn thuế — chỉ khi `TaxType = 3` |
| `TaxFormulaSource` | `int` | Enum TaxFormulaSource — read-only, backend tự set. FE dùng để biết công thức nào được tự suy và ẩn khỏi UI |
| `AllowExceedNorm` | `bool` | |
| `Description` | `string?` | |
| `ShowOnPayslip` | `bool` | |
| `HideWhenZero` | `bool` | |
| `Source` | `int` | Enum SalaryCompositionSource |
| `Status` | `int` | Enum SalaryCompositionStatus |
| `LockedFields` | `string?` | Snapshot JSON array of int — copy từ TPL hệ thống lúc kế thừa. Read-only, không dùng làm filter field. Xem mục 6.6 |
| `IsSkipUnfollowedComposition` | `bool` | **Không lưu DB** — `true` khi người dùng xác nhận lưu dù công thức (`ValueFormula` / `NormFormula` / `TaxableFormula` / `ExemptFormula`) có TPL ngừng theo dõi. Mặc định `false`. Không dùng làm filter field |
| `CreateDate` | `datetime` | |
| `ModifiedDate` | `datetime?` | |
