# Tổng quan dự án — Danh mục Thực đơn MISA CukCuk

## 1. Mục tiêu

Xây dựng module **Danh mục Thực đơn** cho hệ thống MISA CukCuk, cho phép nhà hàng quản lý toàn bộ các món ăn, đồ uống trong thực đơn.

## 2. Phạm vi thực hiện

| Hạng mục | Trạng thái | Ghi chú |
|---|---|---|
| Header + Sidebar | Bắt buộc | UI tĩnh, không có chức năng |
| Màn hình danh sách Thực đơn | Bắt buộc | Phân trang, tìm kiếm, hover actions |
| Form Thêm / Sửa / Nhân bản | Bắt buộc | Chỉ tab **Thông tin chung** + **Sở thích phục vụ** |
| Modal xác nhận | Bắt buộc | Thoát form khi đang sửa, xác nhận xóa |
| Tooltip + Toast | Bắt buộc | Button icon cần tooltip, kết quả thao tác cần toast |
| Validate FE + BE | Bắt buộc | Xem chi tiết mục validate |
| CRUD API + DB | Bắt buộc | |
| Ghim cột, lọc nhanh theo cột | Bonus | |
| Thêm nhanh Nhóm thực đơn, Đơn vị tính | Bonus | |

## 3. Tech stack

- **Backend:** .NET 10, ASP.NET Core, Dapper, MySQL
- **Stored Procedures:** toàn bộ DB operation qua stored procedure
- **Frontend:** (theo yêu cầu riêng của đề — không thuộc phạm vi backend task này)

## 4. Các entity chính

| Entity | Bảng DB | Mô tả |
|---|---|---|
| Thực đơn | `inventory_item` | Entity chính |
| Nhóm thực đơn | `inventory_item_category` | Phân nhóm món ăn |
| Đơn vị tính | `unit` | Suất, Bát, Đĩa, Cốc... |
| Bếp chế biến | `kitchen` | Bếp, Bar, Lò... |
| Sở thích phục vụ | `inventory_item_addition` | Catalog: không cay, ít đá... |
| Sở thích theo món | `inventory_item_addition_detail` | Gắn sở thích vào từng món |

## 5. Quy tắc nghiệp vụ quan trọng

- Mã món là **unique** trong toàn hệ thống
- Tên món, Mã món, Đơn vị tính, Giá bán là **bắt buộc**
- Khi xóa món có sở thích phục vụ → xóa cascade `inventory_item_addition_detail`
- "Chế biến tại" là **multi-select** (1 món có thể chế biến ở nhiều bếp)
- Form Sửa có thêm checkbox **Ngừng bán** — form Thêm không có
- **Nhân bản**: copy toàn bộ thông tin + sở thích phục vụ của món gốc, sinh mã mới
