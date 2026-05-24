# Danh mục Thực đơn MISA CukCuk — Use Case Specifications

**Version:** 1.0  
**Ngày:** 18/05/2026

---

## Danh sách Use Case

| UC ID | Tên Use Case | Actor | Ưu tiên |
|---|---|---|---|
| UC-001 | Xem danh sách Thực đơn | Quản lý nhà hàng | Cao |
| UC-002 | Tìm kiếm Thực đơn | Quản lý nhà hàng | Cao |
| UC-003 | Thêm mới Thực đơn | Quản lý nhà hàng | Cao |
| UC-004 | Sửa Thực đơn | Quản lý nhà hàng | Cao |
| UC-005 | Xóa Thực đơn | Quản lý nhà hàng | Cao |
| UC-006 | Nhân bản Thực đơn | Quản lý nhà hàng | Cao |
| UC-007 | Quản lý Sở thích phục vụ | Quản lý nhà hàng | Cao |
| UC-008 | Upload ảnh Thực đơn | Quản lý nhà hàng | Cao |

---

## UC-001: Xem danh sách Thực đơn

**Use Case ID:** UC-001  
**Tên:** Xem danh sách Thực đơn  
**Actor chính:** Quản lý nhà hàng  
**Actor phụ:** Hệ thống Backend API, MySQL Database  
**Mô tả:** Người dùng truy cập màn hình Danh mục Thực đơn để xem tất cả món ăn dưới dạng bảng phân trang.  
**Tiền điều kiện:** Người dùng đã đăng nhập và có quyền truy cập module.  
**Hậu điều kiện:** Danh sách thực đơn được hiển thị đúng dữ liệu từ database.

**Luồng chính (Main Flow):**

1. Người dùng click vào menu "Thực đơn" trong Sidebar.
2. Hệ thống gọi `GET /api/InventoryItems/Paging?pageIndex=1&pageSize=10`.
3. Database thực thi `Proc_InventoryItem_FilterPaging` và trả về 10 bản ghi đầu tiên + totalCount.
4. Frontend render bảng với các cột: Loại món, Mã món, Tên món, Nhóm thực đơn, ĐVT, Giá vốn, Giá bán, Thay đổi theo thời giá.
5. Footer hiển thị tổng số bản ghi và bộ điều hướng phân trang.

**Luồng thay thế (Alternative Flow):**

3a. Người dùng chọn số dòng/trang khác (25/50/100):
  - 3a.1 Hệ thống gọi lại API với `pageSize` mới và `pageIndex=1`.
  - 3a.2 Bảng cập nhật theo số dòng mới.

3b. Người dùng click nút Reload:
  - 3b.1 Hệ thống gọi lại API với tham số hiện tại.
  - 3b.2 Dữ liệu được làm mới.

**Luồng ngoại lệ (Exception Flow):**

E1. API trả về lỗi 500:
  - E1.1 Hiển thị toast "Có lỗi xảy ra, vui lòng thử lại".
  - E1.2 Bảng hiển thị trạng thái lỗi.

**Tần suất sử dụng:** Nhiều lần mỗi ngày.  
**Độ ưu tiên:** Cao

---

## UC-002: Tìm kiếm Thực đơn

**Use Case ID:** UC-002  
**Tên:** Tìm kiếm Thực đơn  
**Actor chính:** Quản lý nhà hàng  
**Actor phụ:** Backend API  
**Mô tả:** Người dùng nhập từ khóa để lọc danh sách theo Mã món hoặc Tên món.  
**Tiền điều kiện:** Màn hình danh sách đang hiển thị.  
**Hậu điều kiện:** Danh sách được lọc theo từ khóa, reset về trang 1.

**Luồng chính:**

1. Người dùng nhập từ khóa vào ô tìm kiếm.
2. Hệ thống gọi `GET /api/InventoryItems/Paging?search={keyword}&pageIndex=1`.
3. Backend tìm LIKE `%keyword%` trên `MenuItemCode` và `MenuItemName`.
4. Frontend render lại bảng với kết quả tìm kiếm.

**Luồng thay thế:**

2a. Người dùng xóa hết từ khóa:
  - 2a.1 Gọi lại API không có tham số search.
  - 2a.2 Danh sách trở về hiển thị toàn bộ.

**Luồng ngoại lệ:**

E1. Không có kết quả:
  - E1.1 Hiển thị empty state "Không tìm thấy dữ liệu phù hợp".

**Tần suất sử dụng:** Thường xuyên.  
**Độ ưu tiên:** Cao

---

## UC-003: Thêm mới Thực đơn

**Use Case ID:** UC-003  
**Tên:** Thêm mới Thực đơn  
**Actor chính:** Quản lý nhà hàng  
**Actor phụ:** Backend API, MySQL Database  
**Mô tả:** Người dùng điền thông tin và lưu một món ăn mới vào hệ thống.  
**Tiền điều kiện:** Người dùng đang ở màn hình danh sách Thực đơn.  
**Hậu điều kiện:** Bản ghi mới được tạo trong bảng `inventory_item` (và các bảng liên quan), danh sách cập nhật.

**Luồng chính:**

1. Người dùng click nút "+ Thêm".
2. Hệ thống load các dropdown: `GET /Kitchens`, `GET /Units`, `GET /InventoryItemCategories`.
3. Form trống mở ra, tiêu đề "Thêm thực đơn", auto focus vào ô "Tên món".
4. Người dùng điền thông tin (xem FR-007 cho danh sách trường).
5. Người dùng click [Lưu].
6. Hệ thống validate FE: kiểm tra required, format, max length.
7. Gọi `POST /api/InventoryItems` với dữ liệu form.
8. Backend validate: kiểm tra unique Mã món trong DB.
9. `Proc_InsertInventoryItem` lưu dữ liệu: insert `inventory_item` + `inventory_item_kitchen` + `inventory_item_addition_detail`.
10. API trả về 201 Created.
11. Hiển thị toast "Thêm thực đơn thành công", chuyển về màn hình danh sách.

**Luồng thay thế:**

5a. Người dùng click [Lưu và thêm]:
  - 5a.1 Thực hiện bước 6-10.
  - 5a.2 Hiển thị toast thành công.
  - 5a.3 Mở form trống mới để nhập tiếp.

5b. Người dùng click [Hủy]:
  - 5b.1 Nếu có dữ liệu đã nhập → hiện Modal xác nhận thoát.
  - 5b.2 Đồng ý → quay về danh sách.
  - 5b.3 Hủy → ở lại form.

**Luồng ngoại lệ:**

E1. Validate FE fail (bước 6):
  - E1.1 Không gọi API.
  - E1.2 Focus vào input lỗi đầu tiên, hiển thị tooltip lỗi.
  - E1.3 Use case dừng lại ở bước 6.

E2. Validate BE fail — Mã món trùng (bước 8):
  - E2.1 API trả về 400 với `userMessage: "Mã món đã tồn tại"`.
  - E2.2 Hiển thị toast lỗi.
  - E2.3 Giữ form, người dùng có thể sửa Mã món.

E3. Lỗi server (bước 9):
  - E3.1 Hiển thị toast "Có lỗi xảy ra, vui lòng thử lại".
  - E3.2 Giữ form.

**Tần suất sử dụng:** Hàng ngày.  
**Độ ưu tiên:** Cao

---

## UC-004: Sửa Thực đơn

**Use Case ID:** UC-004  
**Tên:** Sửa Thực đơn  
**Actor chính:** Quản lý nhà hàng  
**Actor phụ:** Backend API, MySQL Database  
**Mô tả:** Người dùng chỉnh sửa thông tin một món ăn đã tồn tại.  
**Tiền điều kiện:** Danh sách đang hiển thị và có ít nhất một bản ghi.  
**Hậu điều kiện:** Dữ liệu trong DB được cập nhật, danh sách phản ánh thay đổi.

**Luồng chính:**

1. Người dùng hover vào một dòng → click icon Sửa (hoặc double-click dòng).
2. Hệ thống gọi `GET /api/InventoryItems/{id}` để lấy đầy đủ thông tin (kitchens + additions).
3. Form mở với dữ liệu đã load, tiêu đề "Sửa thực đơn".
4. Người dùng chỉnh sửa thông tin cần thiết.
5. Người dùng click [Lưu].
6. Validate FE.
7. Gọi `PUT /api/InventoryItems/{id}`.
8. Validate BE + `Proc_UpdateInventoryItem` cập nhật (DELETE + INSERT lại bảng join).
9. Hiển thị toast "Cập nhật thực đơn thành công".

**Luồng thay thế:**

4a. Người dùng click [Hủy] khi có thay đổi chưa lưu:
  - 4a.1 Hiện Modal "Bạn có thay đổi chưa được lưu. Bạn có muốn thoát không?"
  - 4a.2 Đồng ý → quay về danh sách.
  - 4a.3 Hủy → ở lại form.

**Luồng ngoại lệ:**

E1. ID không tồn tại (bước 2):
  - E1.1 API trả về 404.
  - E1.2 Toast lỗi, không mở form.

E2. Validate fail (bước 6-8):
  - E2.1 Giữ form, hiện lỗi cụ thể.

**Tần suất sử dụng:** Hàng ngày.  
**Độ ưu tiên:** Cao

---

## UC-005: Xóa Thực đơn

**Use Case ID:** UC-005  
**Tên:** Xóa Thực đơn  
**Actor chính:** Quản lý nhà hàng  
**Actor phụ:** Backend API, MySQL Database  
**Mô tả:** Người dùng xóa mềm một thực đơn sau khi xác nhận.  
**Tiền điều kiện:** Danh sách đang hiển thị và có bản ghi cần xóa.  
**Hậu điều kiện:** `IsDeleted=1` trên bản ghi, bản ghi không còn xuất hiện trong danh sách.

**Luồng chính:**

1. Người dùng hover vào dòng → click icon Xóa (thùng rác đỏ).
2. Hệ thống hiển thị Modal: "Bạn có chắc chắn muốn xóa thực đơn **[Tên món]**?"
3. Người dùng click [Xác nhận].
4. Gọi `DELETE /api/InventoryItems/{id}`.
5. `Proc_DeleteInventoryItemById` set `IsDeleted=1`, cascade xóa `inventory_item_kitchen` và `inventory_item_addition_detail`.
6. Toast "Xóa thực đơn thành công", danh sách cập nhật (không còn dòng vừa xóa).

**Luồng thay thế:**

3a. Người dùng click [Hủy] trong Modal:
  - 3a.1 Đóng Modal.
  - 3a.2 Không xóa gì.

**Luồng ngoại lệ:**

E1. API lỗi (bước 4):
  - E1.1 Toast "Có lỗi xảy ra, vui lòng thử lại".
  - E1.2 Danh sách không thay đổi.

**Tần suất sử dụng:** Thỉnh thoảng.  
**Độ ưu tiên:** Cao

---

## UC-006: Nhân bản Thực đơn

**Use Case ID:** UC-006  
**Tên:** Nhân bản Thực đơn  
**Actor chính:** Quản lý nhà hàng  
**Actor phụ:** Backend API, MySQL Database  
**Mô tả:** Tạo bản sao của một thực đơn, bao gồm toàn bộ thông tin, sở thích phục vụ và bếp chế biến.  
**Tiền điều kiện:** Có ít nhất một bản ghi trong danh sách.  
**Hậu điều kiện:** Bản ghi mới được tạo với mã tự sinh, form mở để người dùng xem lại trước khi lưu.

**Luồng chính:**

1. Người dùng hover vào dòng → click icon Nhân bản (copy).
2. Gọi `POST /api/InventoryItems/{id}/clone`.
3. Backend load toàn bộ dữ liệu món gốc.
4. Tự sinh mã mới (kiểm tra không trùng).
5. Insert bản ghi mới kèm bản sao `inventory_item_kitchen` và `inventory_item_addition_detail`.
6. API trả về `newMenuItemID`.
7. Gọi `GET /api/InventoryItems/{newMenuItemID}` để load dữ liệu bản sao.
8. Mở form "Nhân bản thực đơn" với dữ liệu bản sao.
9. Người dùng review và click [Lưu] để confirm.
10. Toast "Nhân bản thực đơn thành công".

**Luồng ngoại lệ:**

E1. Tự sinh mã thất bại (không thể tạo mã không trùng):
  - E1.1 API trả về lỗi.
  - E1.2 Toast "Không thể tạo mã món mới tự động. Vui lòng nhập thủ công".
  - E1.3 Form mở nhưng ô Mã món rỗng, yêu cầu người dùng nhập.

**Tần suất sử dụng:** Thỉnh thoảng.  
**Độ ưu tiên:** Cao

---

## UC-007: Quản lý Sở thích phục vụ

**Use Case ID:** UC-007  
**Tên:** Quản lý Sở thích phục vụ trong form Thực đơn  
**Actor chính:** Quản lý nhà hàng  
**Actor phụ:** Backend API  
**Mô tả:** Gắn, chỉnh sửa và xóa sở thích phục vụ cho một món ăn trong form chi tiết.  
**Tiền điều kiện:** Form Thêm/Sửa/Nhân bản đang mở.  
**Hậu điều kiện:** Dữ liệu sở thích được lưu vào `inventory_item_addition_detail`.

**Luồng chính:**

1. Người dùng chuyển sang tab "Sở thích phục vụ".
2. Hệ thống load dropdown từ `GET /api/InventoryItemAdditions`.
3. Người dùng click [+ Thêm dòng].
4. Row mới xuất hiện với dropdown Sở thích và ô Thu thêm = 0.00.
5. Người dùng chọn sở thích từ dropdown → Thu thêm tự điền giá trị mặc định.
6. Người dùng có thể override giá trị Thu thêm.
7. Lặp lại bước 3-6 để thêm nhiều sở thích.
8. Người dùng click [Lưu] ở footer form.
9. Dữ liệu sở thích được submit cùng form chính.
10. API lưu vào `inventory_item_addition_detail`.

**Luồng thay thế:**

5a. Người dùng muốn xóa một dòng:
  - 5a.1 Click icon xóa (thùng rác đỏ) ở cuối dòng.
  - 5a.2 Row bị xóa khỏi danh sách client-side.

**Luồng ngoại lệ:**

E1. API load sở thích lỗi:
  - E1.1 Dropdown hiển thị trống.
  - E1.2 Toast "Không thể tải danh sách sở thích phục vụ".

**Tần suất sử dụng:** Khi thêm/sửa món.  
**Độ ưu tiên:** Cao

---

## UC-008: Upload ảnh Thực đơn

**Use Case ID:** UC-008  
**Tên:** Upload ảnh minh họa Thực đơn  
**Actor chính:** Quản lý nhà hàng  
**Actor phụ:** Backend API, File System  
**Mô tả:** Upload ảnh minh họa cho một món ăn trong form chi tiết.  
**Tiền điều kiện:** Form Thêm/Sửa/Nhân bản đang mở.  
**Hậu điều kiện:** File ảnh lưu trên server, đường dẫn ghi vào field Image của form.

**Luồng chính:**

1. Người dùng click [Tải lên] trong khu vực ảnh.
2. File dialog mở, cho phép chọn file.
3. Người dùng chọn file ảnh (.jpg/.jpeg/.png/.gif, ≤ 5MB).
4. Validate client: kiểm tra định dạng và kích thước.
5. Gọi `POST /api/InventoryItems/upload-image` với `multipart/form-data`.
6. Server lưu file vào `wwwroot/uploads/inventory/{year}/{month}/{guid}.{ext}`.
7. API trả về relative path.
8. Frontend hiển thị preview ảnh, lưu đường dẫn vào field Image.

**Luồng thay thế:**

8a. Người dùng muốn xóa ảnh đã upload:
  - 8a.1 Click nút [X] bên cạnh ảnh.
  - 8a.2 Preview bị xóa, field Image trở về rỗng.

**Luồng ngoại lệ:**

E1. File sai định dạng (bước 4):
  - E1.1 Toast "Chỉ chấp nhận file .jpg/.jpeg/.png/.gif".
  - E1.2 Không upload.

E2. File quá lớn (bước 4):
  - E2.1 Toast "Ảnh không được vượt quá 5MB".

E3. Server lỗi (bước 5-6):
  - E3.1 Toast "Upload ảnh thất bại. Vui lòng thử lại".

**Tần suất sử dụng:** Khi thêm/sửa món.  
**Độ ưu tiên:** Cao
