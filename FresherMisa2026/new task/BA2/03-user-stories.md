# Danh mục Thực đơn MISA CukCuk — User Stories & Acceptance Criteria

**Version:** 1.0  
**Ngày:** 18/05/2026  
**Sprint:** 1

---

## Epic: Quản lý Danh mục Thực đơn

```
EPIC: Danh mục Thực đơn MISA CukCuk
│
├── Feature 1: Màn hình danh sách Thực đơn
│   ├── US-001: Xem danh sách phân trang
│   ├── US-002: Tìm kiếm thực đơn
│   ├── US-003: Lọc thực đơn theo loại món / nhóm
│   └── US-004: Hover actions (Sửa / Nhân bản / Xóa)
│
├── Feature 2: Thêm / Sửa / Nhân bản thực đơn
│   ├── US-005: Thêm mới thực đơn
│   ├── US-006: Sửa thực đơn
│   ├── US-007: Nhân bản thực đơn
│   └── US-008: Xóa thực đơn
│
├── Feature 3: Sở thích phục vụ
│   └── US-009: Quản lý sở thích phục vụ trong form
│
├── Feature 4: Tiện ích
│   ├── US-010: Upload ảnh thực đơn
│   ├── US-011: Validate nhập liệu FE + BE
│   ├── US-012: Modal xác nhận
│   └── US-013: Toast + Tooltip thông báo
│
└── Feature 5: Bonus
    ├── US-014: Ghim cột và lọc nhanh theo cột
    └── US-015: Thêm nhanh Nhóm thực đơn / Đơn vị tính
```

---

## Feature 1: Màn hình danh sách Thực đơn

---

### US-001: Xem danh sách thực đơn phân trang

**Ưu tiên:** Cao | **Story Points:** 3

```
Là quản lý nhà hàng,
Tôi muốn xem danh sách tất cả món ăn dạng bảng có phân trang,
Để dễ dàng tổng quan và quản lý thực đơn của nhà hàng.
```

**Acceptance Criteria:**

AC-1:
```
GIVEN người dùng truy cập màn hình Danh mục Thực đơn
WHEN trang tải xong
THEN hiển thị bảng danh sách với các cột: Loại món, Mã món, Tên món,
     Nhóm thực đơn, ĐVT, Giá vốn, Giá bán, Thay đổi theo thời giá
```

AC-2:
```
GIVEN danh sách đang hiển thị
WHEN không có tham số nào
THEN mặc định hiển thị 10 dòng/trang, trang 1
```

AC-3:
```
GIVEN danh sách đang hiển thị
WHEN người dùng chọn số dòng/trang là 25
THEN bảng cập nhật hiển thị tối đa 25 dòng
```

AC-4:
```
GIVEN có 50 bản ghi trong DB
WHEN hiển thị mặc định 10 dòng/trang
THEN footer hiển thị "Tổng số: 50" và có đủ nút điều hướng trang |< < > >|
```

AC-5:
```
GIVEN người dùng đang ở trang 2
WHEN click nút Reload
THEN hệ thống gọi lại API và làm mới dữ liệu, giữ nguyên trang hiện tại
```

**Definition of Done:**
- [ ] API `GET /api/InventoryItems/Paging` hoạt động đúng
- [ ] Phân trang render chính xác
- [ ] Format tiền tệ đúng (ví dụ: 25.000)

---

### US-002: Tìm kiếm thực đơn theo Mã món / Tên món

**Ưu tiên:** Cao | **Story Points:** 2

```
Là quản lý nhà hàng,
Tôi muốn tìm kiếm món ăn theo Mã món hoặc Tên món,
Để nhanh chóng tìm thấy món cần quản lý trong danh sách dài.
```

**Acceptance Criteria:**

AC-1:
```
GIVEN người dùng nhập "bún" vào ô tìm kiếm
WHEN hệ thống xử lý
THEN chỉ hiển thị các món có Mã món hoặc Tên món chứa "bún" (không phân biệt hoa thường)
```

AC-2:
```
GIVEN người dùng đang xem kết quả tìm kiếm
WHEN xóa hết nội dung ô tìm kiếm
THEN danh sách trở về hiển thị toàn bộ, reset về trang 1
```

AC-3:
```
GIVEN người dùng nhập từ khóa không khớp bất kỳ món nào
WHEN hệ thống xử lý
THEN hiển thị empty state "Không tìm thấy dữ liệu phù hợp"
```

---

### US-003: Lọc thực đơn theo Loại món

**Ưu tiên:** Cao | **Story Points:** 2

```
Là quản lý nhà hàng,
Tôi muốn lọc danh sách thực đơn theo Loại món (Món ăn / Đồ uống),
Để tập trung quản lý từng nhóm riêng biệt.
```

**Acceptance Criteria:**

AC-1:
```
GIVEN màn hình danh sách đang hiển thị
WHEN người dùng chọn lọc "Đồ uống đóng chai"
THEN chỉ hiển thị các món có MenuItemType = 1
```

AC-2:
```
GIVEN người dùng đã chọn bộ lọc
WHEN click "Bỏ lọc"
THEN danh sách hiển thị lại tất cả loại món, reset trang 1
```

---

### US-004: Hover actions — Sửa, Nhân bản, Xóa

**Ưu tiên:** Cao | **Story Points:** 2

```
Là quản lý nhà hàng,
Tôi muốn thấy nhanh các nút thao tác khi hover vào một dòng,
Để thực hiện Sửa, Nhân bản, Xóa mà không cần vào menu riêng.
```

**Acceptance Criteria:**

AC-1:
```
GIVEN danh sách đang hiển thị
WHEN người dùng rê chuột vào một dòng bất kỳ
THEN hiển thị 3 icon ở cuối dòng: Sửa (bút), Nhân bản (copy), Xóa (thùng rác đỏ)
     mỗi icon có tooltip hiển thị tên hành động khi hover
```

AC-2:
```
GIVEN icon đang hiển thị
WHEN người dùng rê chuột ra khỏi dòng
THEN 3 icon ẩn đi
```

AC-3:
```
GIVEN người dùng double-click vào một dòng
WHEN hệ thống xử lý
THEN mở form Sửa với dữ liệu của dòng đó (tương đương click icon Sửa)
```

---

## Feature 2: Thêm / Sửa / Nhân bản / Xóa

---

### US-005: Thêm mới thực đơn

**Ưu tiên:** Cao | **Story Points:** 5

```
Là quản lý nhà hàng,
Tôi muốn thêm mới một món ăn vào thực đơn,
Để cập nhật danh mục phục vụ khách hàng.
```

**Acceptance Criteria:**

AC-1:
```
GIVEN người dùng ở màn hình danh sách
WHEN click "+ Thêm"
THEN mở form trống với tiêu đề "Thêm thực đơn", auto focus vào ô "Tên món"
```

AC-2:
```
GIVEN form đang mở
WHEN người dùng nhập đủ các trường bắt buộc (Tên món, Mã món, ĐVT, Giá bán)
     và click [Lưu]
THEN hệ thống lưu thành công, hiện toast "Thêm thực đơn thành công"
     và chuyển về màn hình danh sách
```

AC-3:
```
GIVEN form đang mở
WHEN người dùng click [Lưu và thêm]
THEN hệ thống lưu thành công, hiện toast thành công
     và mở form trống mới để nhập tiếp
```

AC-4:
```
GIVEN form đang mở
WHEN người dùng nhập Tên món "Phở Bò"
THEN hệ thống gợi ý tự sinh Mã món (ví dụ "PB-001") vào ô Mã món
     người dùng có thể giữ nguyên hoặc sửa lại
```

AC-5:
```
GIVEN người dùng đã chọn Loại món từ dropdown header
WHEN nhìn vào dropdown Nhóm thực đơn
THEN danh sách nhóm hiển thị đúng theo Loại món đã chọn
```

---

### US-006: Sửa thực đơn

**Ưu tiên:** Cao | **Story Points:** 4

```
Là quản lý nhà hàng,
Tôi muốn chỉnh sửa thông tin của một món ăn đã có,
Để cập nhật giá, mô tả hoặc thông tin khác khi cần.
```

**Acceptance Criteria:**

AC-1:
```
GIVEN người dùng click icon Sửa hoặc double-click một dòng
WHEN hệ thống load xong dữ liệu
THEN form mở với tiêu đề "Sửa thực đơn" và hiển thị đầy đủ thông tin hiện tại
     bao gồm cả Sở thích phục vụ và bếp chế biến đã lưu
```

AC-2:
```
GIVEN form Sửa đang mở
WHEN so sánh với form Thêm
THEN form Sửa hiển thị thêm checkbox "Ngừng bán" trong section Thiết lập
     form Sửa không có nút [Lưu và thêm]
```

AC-3:
```
GIVEN người dùng đang sửa và có thay đổi chưa lưu
WHEN click nút [Hủy]
THEN hiện Modal "Bạn có thay đổi chưa được lưu. Bạn có muốn thoát không?"
```

AC-4:
```
GIVEN Modal thoát đang hiển thị
WHEN người dùng click "Đồng ý"
THEN quay về danh sách, không lưu thay đổi
```

AC-5:
```
GIVEN Modal thoát đang hiển thị
WHEN người dùng click "Hủy"
THEN đóng modal, ở lại form, dữ liệu đã nhập còn nguyên
```

---

### US-007: Nhân bản thực đơn

**Ưu tiên:** Cao | **Story Points:** 3

```
Là quản lý nhà hàng,
Tôi muốn tạo bản sao nhanh của một món ăn,
Để tiết kiệm thời gian khi có nhiều món tương tự nhau.
```

**Acceptance Criteria:**

AC-1:
```
GIVEN người dùng click icon Nhân bản trên một dòng
WHEN hệ thống xử lý
THEN gọi API clone, tạo bản ghi mới với mã món mới (tự sinh, không trùng)
     và mở form với tiêu đề "Nhân bản thực đơn", hiển thị đầy đủ dữ liệu bản sao
```

AC-2:
```
GIVEN form Nhân bản đang mở
WHEN xem Sở thích phục vụ
THEN danh sách sở thích phục vụ của món gốc đã được copy sang
```

AC-3:
```
GIVEN người dùng click [Lưu] trên form Nhân bản
WHEN lưu thành công
THEN hiện toast "Nhân bản thực đơn thành công"
     bản ghi mới xuất hiện trong danh sách
```

---

### US-008: Xóa thực đơn

**Ưu tiên:** Cao | **Story Points:** 2

```
Là quản lý nhà hàng,
Tôi muốn xóa một món ăn khỏi thực đơn khi không còn phục vụ,
Để giữ danh sách thực đơn gọn gàng và chính xác.
```

**Acceptance Criteria:**

AC-1:
```
GIVEN người dùng click icon Xóa trên một dòng
WHEN hệ thống xử lý
THEN hiện Modal "Bạn có chắc chắn muốn xóa thực đơn [Tên món]?"
     với 2 nút: [Xác nhận] và [Hủy]
```

AC-2:
```
GIVEN Modal xác nhận xóa đang hiển thị
WHEN người dùng click [Xác nhận]
THEN gọi API DELETE, xóa thành công
     hiện toast "Xóa thực đơn thành công", cập nhật danh sách
```

AC-3:
```
GIVEN Modal xác nhận xóa đang hiển thị
WHEN người dùng click [Hủy]
THEN đóng modal, không xóa, danh sách không thay đổi
```

---

## Feature 3: Sở thích phục vụ

---

### US-009: Quản lý sở thích phục vụ trong form

**Ưu tiên:** Cao | **Story Points:** 3

```
Là quản lý nhà hàng,
Tôi muốn gắn các sở thích phục vụ (không cay, ít đá...) vào từng món ăn,
Để nhân viên order biết và chọn nhanh khi phục vụ khách.
```

**Acceptance Criteria:**

AC-1:
```
GIVEN form chi tiết đang mở ở tab "Sở thích phục vụ"
WHEN trang load xong
THEN hiển thị bảng với 2 cột: "Sở thích phục vụ" và "Thu thêm"
     có nút [+ Thêm dòng] ở cuối bảng
```

AC-2:
```
GIVEN người dùng click [+ Thêm dòng]
WHEN hệ thống xử lý
THEN thêm một row mới vào bảng với dropdown Sở thích (load từ API) và ô Thu thêm = 0.00
```

AC-3:
```
GIVEN người dùng chọn "20% đá" từ dropdown
WHEN hệ thống xử lý
THEN ô Thu thêm tự điền giá trị mặc định từ master (0.00)
     người dùng có thể thay đổi giá trị này
```

AC-4:
```
GIVEN bảng đang có 2 dòng sở thích
WHEN người dùng click icon xóa ở dòng thứ nhất
THEN dòng đó bị xóa khỏi bảng client-side (chưa gọi API xóa riêng)
```

AC-5:
```
GIVEN người dùng đã điền sở thích phục vụ và click [Lưu]
WHEN API lưu thành công
THEN dữ liệu sở thích được lưu vào inventory_item_addition_detail
     gắn với menuItemID tương ứng
```

---

## Feature 4: Tiện ích

---

### US-010: Upload ảnh thực đơn

**Ưu tiên:** Cao | **Story Points:** 2

```
Là quản lý nhà hàng,
Tôi muốn upload ảnh minh họa cho mỗi món ăn,
Để thực đơn trực quan và hấp dẫn hơn với khách hàng.
```

**Acceptance Criteria:**

AC-1:
```
GIVEN form chi tiết đang mở
WHEN người dùng click [Tải lên]
THEN mở file dialog, cho phép chọn file .jpg/.jpeg/.png/.gif
```

AC-2:
```
GIVEN người dùng chọn file hình ảnh hợp lệ (≤ 5MB)
WHEN hệ thống upload xong
THEN hiển thị preview ảnh trong form, đường dẫn ảnh được lưu vào field Image
```

AC-3:
```
GIVEN người dùng chọn file > 5MB hoặc sai định dạng
WHEN hệ thống xử lý
THEN hiển thị toast lỗi "Ảnh không hợp lệ (chỉ chấp nhận jpg/png/gif, tối đa 5MB)"
```

AC-4:
```
GIVEN form đang hiển thị ảnh preview
WHEN người dùng click nút [X] bên cạnh ảnh
THEN xóa preview, field Image trở về rỗng
```

---

### US-011: Validate nhập liệu

**Ưu tiên:** Cao | **Story Points:** 3

```
Là developer và QA,
Tôi muốn hệ thống validate dữ liệu ở cả FE và BE,
Để đảm bảo dữ liệu lưu vào database luôn đúng và đầy đủ.
```

**Acceptance Criteria:**

AC-1:
```
GIVEN form đang mở
WHEN người dùng click [Lưu] mà chưa nhập Tên món
THEN hệ thống không gọi API, highlight ô Tên món, hiển thị tooltip lỗi "Tên món không được để trống"
     và tự động focus vào ô Tên món
```

AC-2:
```
GIVEN người dùng nhập Mã món đã tồn tại trong hệ thống
WHEN API trả về lỗi 400
THEN hiển thị toast "Mã món đã tồn tại trong hệ thống"
```

AC-3:
```
GIVEN người dùng nhập Mô tả dài hơn 500 ký tự
WHEN hệ thống validate FE
THEN không cho nhập thêm hoặc hiện lỗi "Mô tả không được vượt quá 500 ký tự"
```

AC-4:
```
GIVEN form có nhiều trường lỗi
WHEN người dùng click [Lưu]
THEN auto focus vào trường lỗi đầu tiên (theo thứ tự từ trên xuống)
```

AC-5:
```
GIVEN form đang mở
WHEN người dùng nhấn phím Tab
THEN focus chuyển sang ô nhập liệu tiếp theo theo thứ tự được định nghĩa
```

---

### US-012: Modal xác nhận thao tác nguy hiểm

**Ưu tiên:** Cao | **Story Points:** 1

```
Là người dùng,
Tôi muốn được hỏi xác nhận trước khi thực hiện thao tác không thể hoàn tác,
Để tránh xóa nhầm hoặc mất dữ liệu đã nhập.
```

**Acceptance Criteria:**

AC-1:
```
GIVEN người dùng click icon Xóa
WHEN hệ thống xử lý
THEN hiển thị Modal hỏi xác nhận với tên món trong nội dung
```

AC-2:
```
GIVEN form đang có thay đổi chưa lưu
WHEN người dùng click [Hủy] để thoát form
THEN hiển thị Modal "Bạn có thay đổi chưa được lưu. Bạn có muốn thoát không?"
```

---

### US-013: Toast notification và Tooltip

**Ưu tiên:** Cao | **Story Points:** 1

```
Là người dùng,
Tôi muốn nhận thông báo ngay sau mỗi thao tác và xem gợi ý khi hover button icon,
Để biết thao tác có thành công không và hiểu ý nghĩa của các nút chỉ có icon.
```

**Acceptance Criteria:**

AC-1:
```
GIVEN người dùng thực hiện thao tác CRUD thành công
WHEN API trả về kết quả thành công
THEN hiển thị toast màu xanh ở góc màn hình, tự biến mất sau 3-5 giây
```

AC-2:
```
GIVEN thao tác thất bại (lỗi validate hoặc server)
WHEN API trả về lỗi
THEN hiển thị toast màu đỏ với nội dung lỗi từ userMessage của API
```

AC-3:
```
GIVEN người dùng hover vào button chỉ có icon (ví dụ icon Sửa)
WHEN chuột dừng lại ≥ 500ms
THEN hiển thị tooltip với nhãn tên hành động ("Sửa thực đơn")
```

---

## Feature 5: Bonus

---

### US-014: Ghim cột và lọc nhanh theo cột

**Ưu tiên:** Thấp (Bonus) | **Story Points:** 5

```
Là quản lý nhà hàng,
Tôi muốn ghim các cột quan trọng và lọc nhanh theo từng cột,
Để tùy chỉnh bảng theo nhu cầu xem của mình.
```

**Acceptance Criteria:**

AC-1:
```
GIVEN bảng danh sách đang hiển thị
WHEN người dùng click icon cấu hình cột
THEN mở panel cho phép ẩn/hiện cột và kéo thả sắp xếp thứ tự cột
```

AC-2:
```
GIVEN người dùng click icon lọc tại header cột "Loại món"
WHEN popup lọc mở ra
THEN hiển thị dropdown operator (Chứa / Không chứa / Bằng) và ô giá trị
```

AC-3:
```
GIVEN người dùng điền điều kiện lọc và click [Áp dụng]
WHEN hệ thống gọi API
THEN danh sách lọc theo điều kiện đã chọn, hiển thị badge "đang lọc" tại header cột
```

---

### US-015: Thêm nhanh Nhóm thực đơn / Đơn vị tính

**Ưu tiên:** Thấp (Bonus) | **Story Points:** 3

```
Là quản lý nhà hàng,
Tôi muốn tạo nhanh Nhóm thực đơn hoặc Đơn vị tính ngay trong form thêm món,
Để không phải rời khỏi form và vào module riêng chỉ để thêm một nhóm/đơn vị mới.
```

**Acceptance Criteria:**

AC-1:
```
GIVEN form thêm/sửa đang mở
WHEN người dùng click nút [+] bên cạnh dropdown "Nhóm thực đơn"
THEN mở form con (modal/inline) để nhập tên nhóm mới
```

AC-2:
```
GIVEN người dùng nhập tên nhóm và lưu
WHEN lưu thành công
THEN nhóm mới xuất hiện trong dropdown và được tự động chọn
     form con đóng lại, focus trở về form chính
```

AC-3:
```
GIVEN người dùng nhập tên nhóm đã tồn tại
WHEN hệ thống kiểm tra
THEN hiển thị lỗi "Tên nhóm đã tồn tại"
```
