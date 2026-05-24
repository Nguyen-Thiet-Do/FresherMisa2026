# Danh mục Thực đơn MISA CukCuk — Business Requirements Document

**Version:** 1.0  
**Ngày:** 18/05/2026  
**Tác giả:** BA Team  
**Trạng thái:** Draft

---

## 1. Tổng quan dự án

### 1.1 Mục tiêu nghiệp vụ

Xây dựng module **Danh mục Thực đơn** trong hệ thống MISA CukCuk, cho phép nhà hàng quản lý toàn diện các món ăn và đồ uống. Module cung cấp đầy đủ công cụ để nhân viên quản lý thực hiện CRUD thực đơn, tìm kiếm, lọc, cấu hình sở thích phục vụ và quản lý danh mục hỗ trợ (nhóm thực đơn, đơn vị tính, bếp chế biến).

### 1.2 Phạm vi (Scope)

#### In-Scope

| Hạng mục | Mô tả |
|---|---|
| Màn hình danh sách Thực đơn | Hiển thị bảng phân trang, tìm kiếm, lọc, hover actions |
| Form Thêm / Sửa / Nhân bản | Nhập liệu đầy đủ thông tin thực đơn (tab Thông tin chung + Sở thích phục vụ) |
| Quản lý danh mục hỗ trợ | Nhóm thực đơn, Đơn vị tính, Bếp/Bar, Sở thích phục vụ |
| CRUD API + Database | REST API và MySQL schema đầy đủ |
| Validate FE + BE | Kiểm tra dữ liệu ở cả frontend và backend |
| Upload ảnh | Upload và xem ảnh minh họa cho món ăn |
| Toast + Modal + Tooltip | Thông báo kết quả thao tác, xác nhận nguy hiểm, gợi ý icon |
| Header + Sidebar | UI tĩnh (không có chức năng) |

#### Out-of-Scope

| Hạng mục | Lý do |
|---|---|
| Tab Định lượng NVL | Không nằm trong phạm vi bài tập |
| Tab Chính sách giá bán | Không nằm trong phạm vi bài tập |
| Quản lý đơn hàng, bán hàng | Module khác |
| Xác thực người dùng (Authentication) | Module khác |
| Báo cáo doanh thu | Module khác |

#### Bonus (tùy chọn)

- Ghim cột, tùy chỉnh độ rộng cột, lọc nhanh theo cột
- Thêm nhanh Nhóm thực đơn và Đơn vị tính từ form chi tiết

### 1.3 Stakeholder liên quan

| Stakeholder | Vai trò | Kỳ vọng |
|---|---|---|
| Người quản lý nhà hàng | End User chính | Thêm/sửa/xóa thực đơn nhanh chóng, giao diện thân thiện |
| Nhân viên thu ngân / order | End User phụ | Tra cứu món ăn, xem sở thích phục vụ |
| Developer (Fresher) | Người thực hiện | Yêu cầu rõ ràng, có đủ thông tin để code |
| Mentor / Evaluator | Người đánh giá | Sản phẩm đúng nghiệp vụ, đủ chức năng, UI/UX theo style guide |

---

## 2. Bối cảnh & Vấn đề

### 2.1 Hiện trạng (As-Is)

Hệ thống MISA CukCuk hiện có màn hình quản lý thực đơn theo UI cũ. Giao diện mới đang được thiết kế lại với trải nghiệm người dùng tốt hơn, phù hợp với màn hình từ 1366px trở lên.

### 2.2 Vấn đề cần giải quyết

- Quản lý thực đơn nhà hàng cần giao diện trực quan, dễ thao tác.
- Nhân viên cần tìm kiếm và lọc món ăn nhanh theo nhiều tiêu chí.
- Hệ thống cần hỗ trợ ghi lại sở thích phục vụ của khách hàng để nhân viên order thuận tiện hơn.
- Dữ liệu thực đơn cần được quản lý nhất quán với validate chặt chẽ tránh dữ liệu sai.

### 2.3 Giải pháp đề xuất (To-Be)

Xây dựng module Danh mục Thực đơn với:
- Bảng danh sách phân trang, tìm kiếm đa tiêu chí.
- Form chi tiết đầy đủ với validate FE + BE.
- Tab Sở thích phục vụ để gắn catalog sở thích vào từng món.
- API RESTful và MySQL database chuẩn hóa.

---

## 3. Yêu cầu nghiệp vụ

### 3.1 Yêu cầu chức năng

**BR-001: Hiển thị danh sách thực đơn phân trang**
- Mô tả: Hệ thống hiển thị danh sách món ăn dạng bảng với phân trang. Mặc định 10 dòng/trang, cho phép chọn 10/25/50/100.
- Ưu tiên: **Cao**
- Nguồn: Đề bài Fresher MISA

**BR-002: Tìm kiếm và lọc thực đơn**
- Mô tả: Người dùng tìm kiếm theo Mã món hoặc Tên món. Lọc nhanh theo Loại món, Nhóm thực đơn.
- Ưu tiên: **Cao**
- Nguồn: Đề bài Fresher MISA

**BR-003: Thêm mới thực đơn**
- Mô tả: Form nhập liệu đầy đủ thông tin món ăn. Validate bắt buộc: Tên món, Mã món (unique), Đơn vị tính, Giá bán.
- Ưu tiên: **Cao**
- Nguồn: Đề bài Fresher MISA

**BR-004: Sửa thực đơn**
- Mô tả: Load dữ liệu hiện có vào form, cho phép chỉnh sửa. Form Sửa có thêm trường Ngừng bán.
- Ưu tiên: **Cao**
- Nguồn: Đề bài Fresher MISA

**BR-005: Xóa thực đơn**
- Mô tả: Xóa mềm (soft delete) thực đơn, yêu cầu xác nhận qua Modal trước khi xóa.
- Ưu tiên: **Cao**
- Nguồn: Đề bài Fresher MISA

**BR-006: Nhân bản thực đơn**
- Mô tả: Sao chép toàn bộ thông tin món gốc (kể cả sở thích phục vụ, bếp chế biến), sinh mã món mới tự động, mở form để người dùng xem lại trước khi lưu.
- Ưu tiên: **Cao**
- Nguồn: Đề bài Fresher MISA

**BR-007: Quản lý sở thích phục vụ cho từng món**
- Mô tả: Cho phép gắn/gỡ sở thích phục vụ vào từng món ăn. Mỗi sở thích có thể override phí thu thêm.
- Ưu tiên: **Cao**
- Nguồn: Đề bài Fresher MISA

**BR-008: Upload ảnh minh họa cho món ăn**
- Mô tả: Hỗ trợ upload file .jpg/.jpeg/.png/.gif, giới hạn 5MB, hiển thị preview trong form.
- Ưu tiên: **Cao**
- Nguồn: Đề bài Fresher MISA

**BR-009: Validate và thông báo lỗi**
- Mô tả: Validate tại FE (trước khi gọi API) và BE (trước khi lưu DB). Hiển thị tooltip lỗi tại trường vi phạm, auto focus vào trường lỗi đầu tiên.
- Ưu tiên: **Cao**
- Nguồn: Đề bài Fresher MISA

**BR-010: Điều hướng bằng phím Tab**
- Mô tả: Nhấn Tab chuyển focus giữa các ô nhập liệu theo thứ tự từ trên xuống dưới. Khi mở form, tự động focus vào ô đầu tiên.
- Ưu tiên: **Cao**
- Nguồn: Đề bài Fresher MISA

**BR-011: Toast thông báo kết quả**
- Mô tả: Hiển thị toast notification khi thao tác thành công hoặc thất bại.
- Ưu tiên: **Cao**
- Nguồn: Đề bài Fresher MISA

**BR-012: Ghim cột và lọc nhanh theo cột (Bonus)**
- Mô tả: Cho phép ghim cột, điều chỉnh độ rộng cột, lọc nhanh popup tại header cột.
- Ưu tiên: **Thấp** (Bonus)
- Nguồn: Đề bài Fresher MISA

**BR-013: Thêm nhanh Nhóm thực đơn / Đơn vị tính (Bonus)**
- Mô tả: Trong form chi tiết, nút [+] bên cạnh dropdown Nhóm thực đơn và Đơn vị tính cho phép tạo nhanh mà không rời khỏi form.
- Ưu tiên: **Thấp** (Bonus)
- Nguồn: Đề bài Fresher MISA

### 3.2 Yêu cầu phi chức năng

**NFR-001: Giao diện**
- Tuân thủ Style Guide của MISA CukCuk (màu sắc, typography, icon).
- Font chữ: Inter.
- Responsive với màn hình ≥ 1366px.

**NFR-002: Hiệu năng**
- Danh sách 100 bản ghi tải trong < 2 giây với kết nối localhost.
- API response time < 500ms cho các thao tác CRUD thông thường.

**NFR-003: Dữ liệu**
- Mã món (MenuItemCode) là unique trong toàn hệ thống.
- Soft delete — không xóa vật lý bản ghi khỏi database.
- Charset: utf8mb4 để hỗ trợ đầy đủ tiếng Việt.

**NFR-004: Kỹ thuật**
- Backend: .NET 10, ASP.NET Core, Dapper, MySQL 8+.
- Toàn bộ DB operation qua Stored Procedures.
- API trả về cấu trúc `ServiceResponse` chuẩn.

---

## 4. Giả định & Ràng buộc

### 4.1 Giả định

- Người dùng đã đăng nhập thành công vào hệ thống trước khi truy cập module.
- Dữ liệu master (Nhóm thực đơn, Đơn vị tính, Bếp/Bar) có thể được fix sẵn nhưng phải query từ database.
- Môi trường phát triển là localhost.

### 4.2 Ràng buộc

- Chỉ thực hiện trên dữ liệu tự tạo, không được sửa dữ liệu hệ thống.
- Phạm vi bắt buộc chỉ bao gồm tab Thông tin chung và tab Sở thích phục vụ trong form chi tiết.
- Tech stack bắt buộc: .NET 10 backend, MySQL database.

---

## 5. Tiêu chí thành công

| Tiêu chí | Đo lường |
|---|---|
| CRUD hoạt động đúng | Thêm/Sửa/Xóa/Nhân bản không lỗi |
| Validate đúng | Tất cả rule validate được kiểm tra tại FE và BE |
| UI theo style guide | Tuân thủ ≥ 80% style guide (trọng số 30%) |
| Phân trang và tìm kiếm | Kết quả đúng, đủ |
| Điều hướng bằng phím | Tab, auto focus hoạt động đúng |
| Upload ảnh | Upload thành công, xem được preview |

### Definition of Done

- [ ] Code được review
- [ ] API test pass (Postman hoặc tương đương)
- [ ] UI test pass trên Chrome ≥ 1366px
- [ ] Validate FE + BE đều hoạt động
- [ ] Không có lỗi console nghiêm trọng

---

## 6. Phụ lục

### 6.1 Glossary

| Thuật ngữ | Định nghĩa |
|---|---|
| Thực đơn / Món ăn | `inventory_item` — entity chính trong module |
| Nhóm thực đơn | `inventory_item_category` — phân loại món ăn theo nhóm |
| Đơn vị tính | `unit` — Suất, Bát, Đĩa, Cốc, Lon... |
| Bếp/Bar | `kitchen` — nơi chế biến: Bếp, Bar, Lò |
| Sở thích phục vụ | `inventory_item_addition` — catalog sở thích: không cay, ít đá... |
| Loại món | Enum: 0=Món ăn, 1=Đồ uống đóng chai, 2=Đồ uống pha chế |
| Nhân bản | Clone toàn bộ thông tin món, sinh mã mới |
| Ngừng bán | Flag `IsDiscontinued` — chỉ có trong form Sửa |
| Soft delete | Đặt `IsDeleted=1`, không xóa vật lý khỏi DB |

### 6.2 Tài liệu tham khảo

- Đề bài: `ĐỀ-FRESHER-GPBL.pdf`
- Hệ thống tham khảo: https://misatest06.cukcuk.vn/#menu-food
- Style guide icon + màu: Google Drive (xem đề bài)
