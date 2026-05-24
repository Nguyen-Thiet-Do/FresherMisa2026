# Phân tích yêu cầu chi tiết

## 1. Màn hình danh sách Thực đơn

### 1.1 Hiển thị

- Bảng dữ liệu phân trang, mặc định **10 dòng/trang**
- Cho phép chọn số dòng/trang: 10 / 25 / 50 / 100
- Hiển thị tổng số bản ghi ở footer
- Button **Reload** để tải lại dữ liệu

**Các cột hiển thị mặc định:**

| STT | Tên cột | Trường DB | Ghi chú |
|---|---|---|---|
| 1 | Loại món | `MenuItemType` | Enum text |
| 2 | Mã món | `MenuItemCode` | |
| 3 | Tên món | `MenuItemName` | |
| 4 | Nhóm thực đơn | `CategoryName` | Join từ category |
| 5 | Đơn vị tính | `UnitName` | Join từ unit |
| 6 | Giá vốn | `CostPrice` | Format tiền tệ |
| 7 | Giá bán | `SalePrice` | Format tiền tệ |
| 8 | Thay đổi theo thời giá | `IsPriceChangeable` | Checkbox, edit inline |

### 1.2 Tìm kiếm

- Ô tìm kiếm chung: tìm theo **Mã món** hoặc **Tên món**
- Lọc nhanh theo cột (Bonus): popup filter tại header cột với operator (Chứa / Không chứa...) + giá trị

### 1.3 Hover actions

Khi hover vào 1 dòng → hiện 3 icon ở cuối dòng:

| Icon | Hành động |
|---|---|
| Bút (Sửa) | Mở form Sửa thực đơn |
| Copy (Nhân bản) | Tạo bản sao của món, sinh mã mới |
| Thùng rác đỏ (Xóa) | Hiện Modal xác nhận → xóa |

- **Double click** vào dòng → mở form Sửa

### 1.4 Toolbar

- **+ Thêm**: mở form Thêm mới
- **Nhập từ Excel**: (bonus hoặc UI tĩnh)
- **...** (more actions)

---

## 2. Form Thêm / Sửa / Nhân bản

### 2.1 Header form

- Tiêu đề: "Thêm thực đơn" / "Sửa thực đơn" / "Nhân bản thực đơn"
- Dropdown **Loại món** ở header (áp dụng cho toàn bộ form):
  - Món ăn (default)
  - Đồ uống đóng chai
  - Đồ uống pha chế

### 2.2 Tab: Thông tin chung

#### Section: Thông tin cơ bản

| Trường | Bắt buộc | Kiểu | Ghi chú |
|---|---|---|---|
| Ảnh món | Không | File upload | .jpg, .jpeg, .png, .gif |
| Tên món | **Có** | Text | |
| Mã món | **Có** | Text | Unique, max 255 ký tự. Gợi ý: tự sinh từ tên món |
| Tên món theo ngôn ngữ khác | Không | Text | Max 255 ký tự |
| Thứ tự món | Không | Dropdown | Món khai vị / Món chính / Món tráng miệng / Món ăn vặt / Món nước / Món nướng / Món lẩu |
| Là món đặc trưng | Không | Checkbox | |
| Nhóm thực đơn | Không | Combobox + [+] | FK inventory_item_category; [+] tạo nhanh (Bonus) |
| Đơn vị tính | **Có** | Combobox + [+] | FK unit; [+] tạo nhanh (Bonus) |
| Giá bán | **Có** | Số | Định dạng tiền tệ |
| Thay đổi theo thời giá | Không | Checkbox | Inline với Giá bán |
| Điều chỉnh giá tự do | Không | Checkbox | Inline với Giá bán |
| Giá vốn | Không | Số | |
| Chế biến tại | Không | Multi-select tags | Nguồn từ bảng kitchen |
| Mô tả | Không | Textarea | Max 500 ký tự |

#### Section: Thuế suất

| Trường | Bắt buộc | Kiểu |
|---|---|---|
| Nhóm ngành nghề | Không | Dropdown |
| Tỷ lệ tính thuế GTGT (%) | Không | Dropdown |
| Tỷ lệ tính thuế TNCN (%) | Không | Dropdown |
| Món được giảm thuế GTGT | Không | Checkbox (mặc định **true**) |

#### Section: Thiết lập

| Trường | Form Thêm | Form Sửa |
|---|---|---|
| Không hiển thị trên thực đơn | Có | Có |
| Là bán thành phẩm | Có | Có |
| Thêm vào thực đơn trang bán hàng Online | Có | Có |
| Sao chép sang nhà hàng khác | Có | Có |
| Ngừng bán | **Không có** | **Có** |

### 2.3 Tab: Sở thích phục vụ

- Mô tả gợi ý: "Ghi lại các sở thích của khách hàng giúp nhân viên phục vụ chọn nhanh order. VD: không cay/ ít hành/ thêm phomai..."
- Bảng với 2 cột: **Sở thích phục vụ** | **Thu thêm**
- Mỗi dòng:
  - Dropdown chọn sở thích (load từ `inventory_item_addition`, có tìm kiếm)
  - Nút [+] tạo nhanh sở thích mới
  - Ô nhập số tiền Thu thêm (mặc định 0.00)
  - Nút xóa dòng (icon thùng rác đỏ)
- Nút **+ Thêm dòng** ở cuối bảng

### 2.4 Footer form

| Button | Thêm | Sửa | Nhân bản |
|---|---|---|---|
| Hủy | Có | Có | Có |
| Lưu và thêm | Có | Không | Không |
| Lưu | Có | Có | Có |

---

## 3. Validate

### Frontend + Backend đều validate:

| Trường | Rule |
|---|---|
| Tên món | Required |
| Mã món | Required, Unique, max 255 ký tự |
| Đơn vị tính | Required |
| Giá bán | Required, >= 0, định dạng số |
| Tên món theo ngôn ngữ khác | Max 255 ký tự |
| Mô tả | Max 500 ký tự |

### UX validate:
- Mặc định **auto focus** vào input đầu tiên khi mở form
- Khi validate lỗi → **auto focus** vào input lỗi đầu tiên
- Nhấn **Tab** để chuyển giữa các ô nhập liệu

---

## 4. Modal xác nhận

| Tình huống | Nội dung Modal |
|---|---|
| Xóa thực đơn | "Bạn có chắc chắn muốn xóa thực đơn [Tên món]?" |
| Thoát form khi đang sửa (có thay đổi) | "Bạn có thay đổi chưa được lưu. Bạn có muốn thoát không?" |

---

## 5. Nghiệp vụ Nhân bản

1. Lấy toàn bộ dữ liệu của món gốc (bao gồm sở thích phục vụ, bếp chế biến)
2. Tạo bản ghi mới với:
   - Mã món mới (sinh tự động, không trùng)
   - Tên món: giữ nguyên hoặc thêm prefix/suffix "(Copy)"
3. Copy toàn bộ `inventory_item_addition_detail` sang món mới
4. Copy toàn bộ `inventory_item_kitchen` sang món mới
