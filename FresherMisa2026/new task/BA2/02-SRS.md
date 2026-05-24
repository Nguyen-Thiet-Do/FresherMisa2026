# Danh mục Thực đơn MISA CukCuk — Software Requirements Specification

**Version:** 1.0  
**Ngày:** 18/05/2026  
**Chuẩn:** IEEE 830

---

## 1. Giới thiệu

### 1.1 Mục đích

Tài liệu này mô tả chi tiết các yêu cầu phần mềm cho module **Danh mục Thực đơn** thuộc hệ thống MISA CukCuk. Tài liệu dành cho đội phát triển (Developer, QA) và dùng làm căn cứ kiểm thử.

### 1.2 Phạm vi hệ thống

Module cho phép người dùng quản lý toàn bộ danh mục món ăn, đồ uống của nhà hàng: thêm, sửa, xóa, nhân bản, upload ảnh, gắn sở thích phục vụ, tìm kiếm và lọc.

### 1.3 Định nghĩa & Viết tắt

| Ký hiệu | Ý nghĩa |
|---|---|
| FR | Functional Requirement — Yêu cầu chức năng |
| NFR | Non-Functional Requirement — Yêu cầu phi chức năng |
| BE | Backend |
| FE | Frontend |
| CRUD | Create / Read / Update / Delete |
| SP | Stored Procedure |
| DTO | Data Transfer Object |
| UUID/GUID | Globally Unique Identifier — định danh duy nhất |

### 1.4 Tổng quan tài liệu

- Mục 2: Mô tả tổng thể hệ thống
- Mục 3: Yêu cầu chức năng chi tiết
- Mục 4: Yêu cầu phi chức năng
- Mục 5: Yêu cầu giao diện
- Mục 6: Yêu cầu dữ liệu

---

## 2. Mô tả tổng thể

### 2.1 Góc nhìn hệ thống

Module Danh mục Thực đơn là một phần của hệ thống MISA CukCuk. Frontend (Vue.js) giao tiếp với Backend (.NET 10) thông qua REST API. Backend sử dụng Dapper + MySQL 8 với Stored Procedures.

```
[Người dùng] ──► [Frontend Vue.js] ──► [REST API .NET 10] ──► [MySQL 8]
                                                │
                                       [File Storage /uploads]
```

### 2.2 Chức năng sản phẩm (tóm tắt)

- Xem danh sách thực đơn phân trang
- Tìm kiếm / lọc thực đơn
- Thêm / Sửa / Xóa / Nhân bản thực đơn
- Quản lý sở thích phục vụ theo món
- Upload ảnh minh họa
- Thêm nhanh Nhóm thực đơn, Đơn vị tính (Bonus)

### 2.3 Người dùng và đặc điểm

| Loại người dùng | Mô tả | Tần suất |
|---|---|---|
| Quản lý nhà hàng | Thêm/sửa/xóa thực đơn, cấu hình nhóm món | Hàng ngày |
| Nhân viên | Xem danh sách, tra cứu thông tin món | Thường xuyên |

### 2.4 Môi trường vận hành

- **Server:** .NET 10 chạy trên Windows/Linux
- **Database:** MySQL 8.0+
- **Browser:** Chrome (khuyến nghị), Edge, Firefox phiên bản mới nhất
- **Độ phân giải:** ≥ 1366 × 768px

### 2.5 Ràng buộc thiết kế

- Toàn bộ thao tác database phải qua Stored Procedures.
- API trả về cấu trúc `ServiceResponse` chuẩn: `{ isSuccess, code, data, userMessage, devMessage }`.
- Soft delete — không xóa vật lý.

---

## 3. Yêu cầu chức năng (Functional Requirements)

---

### FR-001: Hiển thị danh sách thực đơn

**Mô tả:** Hệ thống hiển thị danh sách món ăn dạng bảng với phân trang.

**Input:** Tham số phân trang (pageIndex, pageSize), từ khóa tìm kiếm (search), bộ lọc (menuItemType, categoryId).

**Process:**
1. Gọi `GET /api/InventoryItems/Paging` với các tham số tương ứng.
2. Stored Procedure `Proc_InventoryItem_FilterPaging` trả về danh sách + totalCount.
3. Frontend render bảng dữ liệu với phân trang.

**Output:** Bảng gồm các cột: Loại món, Mã món, Tên món, Nhóm thực đơn, ĐVT, Giá vốn, Giá bán, Thay đổi theo thời giá. Footer hiển thị tổng số bản ghi và điều hướng trang.

**Exception:**
- Nếu API lỗi → hiển thị toast "Có lỗi xảy ra, vui lòng thử lại".
- Không có dữ liệu → hiển thị empty state phù hợp.

---

### FR-002: Tìm kiếm thực đơn

**Mô tả:** Người dùng nhập từ khóa để tìm kiếm theo Mã món hoặc Tên món.

**Input:** Chuỗi tìm kiếm nhập vào ô Search.

**Process:**
1. Người dùng gõ vào ô tìm kiếm (debounce hoặc nhấn Enter).
2. Gọi lại `GET /api/InventoryItems/Paging?search={keyword}&pageIndex=1`.
3. Backend tìm kiếm LIKE `%keyword%` trên cả `MenuItemCode` và `MenuItemName`.

**Output:** Danh sách lọc theo từ khóa, reset về trang 1.

**Exception:** Từ khóa rỗng → trả về toàn bộ danh sách.

---

### FR-003: Hover actions trên dòng

**Mô tả:** Khi rê chuột vào một dòng trong bảng, hiện 3 icon thao tác.

**Input:** Sự kiện hover (mouseenter) vào một row.

**Process:** Frontend hiển thị 3 icon ở cuối dòng: Sửa (bút), Nhân bản (copy), Xóa (thùng rác đỏ). Khi rời chuột (mouseleave) → ẩn icon.

**Output:** Icon hiển thị/ẩn theo trạng thái hover.

---

### FR-004: Mở form Sửa

**Mô tả:** Mở form Sửa khi click icon Sửa hoặc double-click vào dòng.

**Input:** Click icon Sửa hoặc double-click dòng → `menuItemID`.

**Process:**
1. Gọi `GET /api/InventoryItems/{id}` để lấy đầy đủ thông tin (kể cả kitchens + servicePreferences).
2. Render form với dữ liệu đã load.
3. Tiêu đề form: "Sửa thực đơn".

**Output:** Form Sửa hiển thị đầy đủ dữ liệu hiện tại của món.

**Exception:** ID không tồn tại → toast lỗi, không mở form.

---

### FR-005: Thêm mới thực đơn

**Mô tả:** Người dùng điền thông tin và lưu món mới.

**Input:** Dữ liệu từ form (xem danh sách trường tại FR-007).

**Process:**
1. Người dùng click "+ Thêm" → mở form trống, auto focus vào Tên món.
2. Người dùng điền thông tin.
3. Click [Lưu] hoặc [Lưu và thêm]:
   - Validate FE → nếu lỗi: focus input lỗi đầu tiên, hiện tooltip.
   - Gọi `POST /api/InventoryItems`.
   - Validate BE.
4. Thành công → toast "Thêm thực đơn thành công".
   - [Lưu]: Quay về danh sách.
   - [Lưu và thêm]: Mở form trống tiếp theo.

**Output:** Bản ghi mới trong DB, danh sách cập nhật.

**Exception:**
- Mã món trùng → BE trả lỗi "Mã món đã tồn tại".
- Trường bắt buộc bỏ trống → tooltip lỗi tại trường đó.

---

### FR-006: Sửa thực đơn

**Mô tả:** Người dùng chỉnh sửa thông tin món và lưu.

**Input:** Dữ liệu đã chỉnh sửa trên form.

**Process:**
1. Sau khi chỉnh sửa, click [Lưu].
2. Validate FE.
3. Gọi `PUT /api/InventoryItems/{id}`.
4. Validate BE.

**Output:** Dữ liệu cập nhật trong DB, toast "Cập nhật thực đơn thành công".

**Exception:**
- Click [Hủy] khi có thay đổi chưa lưu → hiện Modal xác nhận thoát.
- Nếu đồng ý thoát → quay về danh sách, không lưu thay đổi.

---

### FR-007: Dữ liệu form chi tiết — Tab Thông tin chung

**Mô tả:** Các trường nhập liệu trong tab Thông tin chung.

#### Section: Thông tin cơ bản

| Trường | Bắt buộc | Kiểu dữ liệu | Validate |
|---|---|---|---|
| Ảnh món | Không | File upload | .jpg, .jpeg, .png, .gif; ≤ 5MB |
| Tên món | **Có** | Text | Required |
| Mã món | **Có** | Text | Required, Unique, max 255 ký tự |
| Tên món theo ngôn ngữ khác | Không | Text | Max 255 ký tự |
| Thứ tự món | Không | Dropdown | Enum: Món khai vị / Món chính / Món tráng miệng / Món ăn vặt / Món nước / Món nướng / Món lẩu |
| Là món đặc trưng | Không | Checkbox | — |
| Nhóm thực đơn | Không | Combobox | FK inventory_item_category |
| Đơn vị tính | **Có** | Combobox | Required, FK unit |
| Giá bán | **Có** | Số thập phân | Required, ≥ 0 |
| Thay đổi theo thời giá | Không | Checkbox | — |
| Điều chỉnh giá tự do | Không | Checkbox | — |
| Giá vốn | Không | Số thập phân | ≥ 0 |
| Chế biến tại | Không | Multi-select | FK kitchen (nhiều giá trị) |
| Mô tả | Không | Textarea | Max 500 ký tự |

#### Section: Thuế suất

| Trường | Bắt buộc | Kiểu |
|---|---|---|
| Nhóm ngành nghề | Không | Dropdown |
| Tỷ lệ tính thuế GTGT (%) | Không | Dropdown |
| Tỷ lệ tính thuế TNCN (%) | Không | Dropdown |
| Món được giảm thuế GTGT | Không | Checkbox (mặc định true) |

#### Section: Thiết lập

| Trường | Form Thêm | Form Sửa |
|---|---|---|
| Không hiển thị trên thực đơn | Có | Có |
| Là bán thành phẩm | Có | Có |
| Thêm vào thực đơn trang bán hàng Online | Có | Có |
| Sao chép sang nhà hàng khác | Có | Có |
| **Ngừng bán** | **Không có** | **Có** |

---

### FR-008: Xóa thực đơn

**Mô tả:** Xóa mềm một thực đơn sau khi xác nhận.

**Input:** Click icon Xóa → `menuItemID`.

**Process:**
1. Hiện Modal xác nhận: "Bạn có chắc chắn muốn xóa thực đơn **[Tên món]**?"
2. Người dùng xác nhận → `DELETE /api/InventoryItems/{id}`.
3. Backend set `IsDeleted=1`, cascade xóa `inventory_item_addition_detail` và `inventory_item_kitchen`.

**Output:** Toast "Xóa thực đơn thành công", danh sách cập nhật.

**Exception:** Nếu API lỗi → toast "Có lỗi xảy ra".

---

### FR-009: Nhân bản thực đơn

**Mô tả:** Tạo bản sao của một thực đơn với mã mới.

**Input:** Click icon Nhân bản → `menuItemID`.

**Process:**
1. Gọi `POST /api/InventoryItems/{id}/clone`.
2. Backend copy toàn bộ dữ liệu, tự sinh mã mới (không trùng), copy kitchens và servicePreferences.
3. Mở form với dữ liệu bản sao để người dùng xem lại.

**Output:** Form Nhân bản mở với dữ liệu đã clone, tiêu đề "Nhân bản thực đơn".

---

### FR-010: Tab Sở thích phục vụ

**Mô tả:** Quản lý danh sách sở thích phục vụ được gắn với một món ăn.

**Input:** Tương tác với bảng sở thích trong form.

**Process:**
- Load dropdown từ `GET /api/InventoryItemAdditions`.
- Thêm dòng: click [+ Thêm dòng] → thêm row mới với dropdown sở thích + ô Thu thêm.
- Xóa dòng: click icon thùng rác → xóa row khỏi danh sách client-side.
- Chọn sở thích: tự động điền ExtraCharge mặc định, cho phép override.
- Dữ liệu submit cùng form chính khi lưu.

**Output:** Dữ liệu sở thích được lưu vào `inventory_item_addition_detail`.

---

### FR-011: Upload ảnh thực đơn

**Mô tả:** Upload ảnh minh họa cho món ăn.

**Input:** File ảnh .jpg/.jpeg/.png/.gif ≤ 5MB.

**Process:**
1. Click [Tải lên] → mở file dialog.
2. Validate client: đúng định dạng, ≤ 5MB.
3. Gọi `POST /api/InventoryItems/upload-image` (multipart/form-data).
4. Server lưu file vào `wwwroot/uploads/inventory/{year}/{month}/{guid}.{ext}`.
5. Trả về relative path → hiển thị preview trong form.

**Output:** Preview ảnh hiển thị, đường dẫn lưu vào field Image.

**Exception:** File sai định dạng hoặc quá lớn → toast "Upload thất bại".

---

### FR-012: Modal xác nhận

**Mô tả:** Hiển thị hộp thoại xác nhận trước các thao tác nguy hiểm.

| Tình huống | Nội dung Modal |
|---|---|
| Xóa thực đơn | "Bạn có chắc chắn muốn xóa thực đơn **[Tên món]**?" |
| Thoát form khi có thay đổi chưa lưu | "Bạn có thay đổi chưa được lưu. Bạn có muốn thoát không?" |

**Output:** Người dùng chọn Đồng ý hoặc Hủy.

---

### FR-013: Tooltip và Toast

**Mô tả:**
- **Tooltip:** Hiển thị nhãn mô tả khi hover vào button chỉ có icon.
- **Toast:** Thông báo kết quả thao tác (thành công / lỗi), tự động biến mất sau 3–5 giây.

| Hành động | Toast |
|---|---|
| Thêm thành công | ✅ "Thêm thực đơn thành công" |
| Sửa thành công | ✅ "Cập nhật thực đơn thành công" |
| Xóa thành công | ✅ "Xóa thực đơn thành công" |
| Nhân bản thành công | ✅ "Nhân bản thực đơn thành công" |
| Lỗi validate BE | ❌ "[userMessage từ API]" |
| Lỗi server | ❌ "Có lỗi xảy ra, vui lòng thử lại" |

---

### FR-014: Validate nhập liệu

#### Frontend validate (trước khi gọi API)

| Trường | Rule |
|---|---|
| Tên món | Không được để trống |
| Mã món | Không được để trống, max 255 ký tự |
| Đơn vị tính | Không được để trống |
| Giá bán | Không được để trống, phải là số ≥ 0 |
| Tên món theo ngôn ngữ khác | Max 255 ký tự |
| Mô tả | Max 500 ký tự |

#### Backend validate (tại API)

Tất cả rule FE + kiểm tra unique Mã món trong database.

#### UX validate

- Auto focus vào input đầu tiên khi mở form.
- Khi validate lỗi → auto focus và hiển thị tooltip lỗi tại input vi phạm đầu tiên.
- Nhấn Tab chuyển focus theo thứ tự từ trên xuống dưới.

---

### FR-015: Header + Sidebar (UI tĩnh)

**Mô tả:** Dựng HTML/CSS cho header và sidebar theo thiết kế. Không yêu cầu chức năng thực sự.

**Output:** Header và sidebar hiển thị đúng theo style guide.

---

## 4. Yêu cầu phi chức năng

### 4.1 Hiệu năng (Performance)

- Danh sách 100 bản ghi phải tải xong trong < 2 giây (môi trường localhost).
- API CRUD response time < 500ms.
- Upload ảnh 5MB hoàn thành trong < 5 giây.

### 4.2 Bảo mật (Security)

- Validate file upload: chỉ chấp nhận đúng MIME type ảnh.
- Không expose thông tin nội bộ hệ thống trong error message trả về client.

### 4.3 Khả năng mở rộng (Scalability)

- Stored Procedures dễ dàng tối ưu index độc lập với code.
- API thiết kế RESTful chuẩn để dễ tích hợp sau này.

### 4.4 Khả năng bảo trì (Maintainability)

- Code tuân thủ convention của dự án.
- Mỗi entity có Repository, Service, Controller riêng biệt (Clean Architecture).

### 4.5 Tính sẵn sàng (Availability)

- Môi trường dev: uptime trong giờ làm việc.

---

## 5. Yêu cầu giao diện

### 5.1 Giao diện người dùng (UI)

- Tuân thủ style guide MISA CukCuk: màu sắc, khoảng cách, bo góc, shadow.
- Font: Inter (CDN hoặc Google Fonts).
- Responsive ≥ 1366px.
- Hỗ trợ hover state, focus state, disabled state cho tất cả input và button.

### 5.2 Giao diện API

- Base URL: `http://localhost:5237/api`
- Định dạng response: JSON, cấu trúc `ServiceResponse`
- HTTP status codes: 200 OK, 201 Created, 400 Bad Request, 404 Not Found, 500 Internal Server Error

### 5.3 Giao diện Database

- MySQL 8.0+, charset utf8mb4
- Kết nối qua connection string trong `appsettings.json`

---

## 6. Yêu cầu dữ liệu

### 6.1 Data model tóm tắt

| Bảng | Loại | Mô tả |
|---|---|---|
| `unit` | Master | Đơn vị tính |
| `kitchen` | Master | Bếp chế biến |
| `inventory_item_category` | Master | Nhóm thực đơn |
| `inventory_item_category_kitchen` | Join | Nhóm ↔ Bếp |
| `inventory_item` | Chính | Thực đơn |
| `inventory_item_kitchen` | Join | Món ↔ Bếp |
| `inventory_item_addition` | Master | Catalog sở thích phục vụ |
| `inventory_item_addition_detail` | Detail | Sở thích gắn với từng món |

### 6.2 Quy tắc dữ liệu

- `MenuItemCode`: UNIQUE, max 255 ký tự, NOT NULL.
- Khi xóa `inventory_item`: cascade xóa `inventory_item_addition_detail` và `inventory_item_kitchen`.
- `IsDeleted`: soft delete flag, default 0.
- Tất cả PK dùng CHAR(36) UUID/GUID.

### 6.3 Stored Procedures

| SP | Mục đích |
|---|---|
| `Proc_InventoryItem_FilterPaging` | Danh sách phân trang + filter + search |
| `Proc_InventoryItem_GetDetail` | Chi tiết món kèm kitchens + additions |
| `Proc_InsertInventoryItem` | Thêm mới (kèm kitchens + additions) |
| `Proc_UpdateInventoryItem` | Cập nhật (kèm kitchens + additions) |
| `Proc_DeleteInventoryItemById` | Soft delete |
| `Proc_InsertInventoryItemCategory` | Thêm nhóm thực đơn |
| `Proc_InsertUnit` | Thêm đơn vị tính |
| `Proc_InsertKitchen` | Thêm bếp/bar |
| `Proc_InsertInventoryItemAddition` | Thêm sở thích phục vụ |
