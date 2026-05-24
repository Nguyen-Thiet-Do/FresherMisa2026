# Màn hình và luồng xử lý

## 1. Màn hình danh sách Thực đơn

```
┌─────────────────────────────────────────────────────────────────────────────┐
│  Thực đơn                              [Nhập từ Excel]  [+ Thêm]  [...]     │
├─────────────────────────────────────────────────────────────────────────────┤
│  [🔍 Tìm kiếm...]              [↺]  [↓]  [⚙ Cột]  [▼ Lọc]                │
├──────────┬────────┬──────────┬───────────────┬──────────┬───────┬───────┬──┤
│ Loại món │ Mã món │ Tên món  │ Nhóm thực đơn │ ĐVT      │ Giá   │ Giá   │☑ │
│          │        │          │               │          │ vốn   │ bán   │  │
├──────────┼────────┼──────────┼───────────────┼──────────┼───────┼───────┼──┤
│ Món ăn   │ M-1    │ Bún chả  │ Món đặc biệt  │ Suất     │ 0     │25.000 │☐ │
│ Món ăn   │ M-2    │ Phở bát đá│ Món bán chạy │ Bát      │ 0     │30.000 │☐ │
│ Món ăn   │ M-3    │ Bánh mì  │ Món ăn kèm   │ Suất     │ 0     │35.000 │☐ │← HOVER: [✏][⧉][🗑]
├──────────┴────────┴──────────┴───────────────┴──────────┴───────┴───────┴──┤
│ Tổng số: 10          Số dòng/trang: [10▼]                 1-100  |< < > >| │
└─────────────────────────────────────────────────────────────────────────────┘
```

### Luồng xử lý danh sách

```
Vào trang
    │
    ▼
GET /api/InventoryItems/Paging?pageIndex=1&pageSize=10
    │
    ▼
Render bảng dữ liệu
    │
    ├─ Nhập từ khóa tìm kiếm ──► GET /Paging?search=xxx&...
    │
    ├─ Hover vào dòng ──────────► Hiện icon [Sửa][Nhân bản][Xóa]
    │       ├─ Click Sửa ────────► Mở form Sửa (load GET /{id})
    │       ├─ Click Nhân bản ──► POST /{id}/clone ──► Mở form Nhân bản
    │       └─ Click Xóa ────────► Modal xác nhận ──► DELETE /{id} ──► Reload
    │
    ├─ Double click dòng ───────► Mở form Sửa
    │
    └─ Click "+ Thêm" ──────────► Mở form Thêm mới
```

---

## 2. Form Thêm / Sửa / Nhân bản

```
┌─────────────────────────────────────────────────────────────────────────────┐
│ ← Thêm thực đơn          [Loại món: Món ăn ▼]                              │
├─────────────────────────────────────────────────────────────────────────────┤
│ [Thông tin chung]  Sở thích phục vụ  Định lượng NVL  Chính sách giá bán    │
│                    (bắt buộc làm)    (bỏ qua)        (bỏ qua)              │
├─────────────────────────────────────────────────────────────────────────────┤
│ Thông tin cơ bản                                                            │
│                                                                             │
│ ┌──────────┐    Tên món (*)  [________________________]                     │
│ │          │    Mã món (*)   [________________________]                     │
│ │  Ảnh    │    Tên theo NN  [________________________]                     │
│ │  món    │    Thứ tự món   [Món khai vị        ▼]  □ Là món đặc trưng   │
│ │         │    Nhóm TD      [                   ▼] [+]                    │
│ └──────────┘    Đơn vị tính  [                   ▼] [+]                   │
│ [Tải lên][...][X]                                                           │
│                 Giá bán (*)  [__________] □ Thay đổi theo thời giá         │
│                                           □ Điều chỉnh giá tự do           │
│                 Giá vốn ℹ   [__________]                                   │
│                 Chế biến tại [Bếp ×     ▼]                                 │
│                 Mô tả        [________________________]                     │
│                              [________________________]                     │
│                                                                             │
│ Thuế suất                                                                   │
│  Nhóm ngành nghề  [                ▼]                                       │
│  Tỷ lệ GTGT (%)   [       ▼]    Tỷ lệ TNCN (%)  [       ▼]               │
│  ☑ Món được giảm thuế GTGT                                                 │
│                                                                             │
│ Thiết lập                                                                   │
│  □ Không hiển thị trên thực đơn                                             │
│  □ Là bán thành phẩm                                                        │
│  □ Thêm vào thực đơn trang bán hàng Online                                  │
│  □ Sao chép sang nhà hàng khác                                              │
│  □ Ngừng bán  (chỉ hiện khi Sửa)                                           │
├─────────────────────────────────────────────────────────────────────────────┤
│                                        [Hủy]  [Lưu và thêm]  [Lưu]         │
└─────────────────────────────────────────────────────────────────────────────┘
```

### Luồng xử lý form Thêm

```
Mở form Thêm
    │
    ├─ Load dropdown: GET /Kitchens, GET /Units, GET /InventoryItemCategories
    │
    ▼
Nhập liệu
    │
    ├─ Tab "Thông tin chung"
    │       ├─ Upload ảnh ──────► POST /upload-image ──► lưu đường dẫn
    │       └─ Nhóm TD / ĐVT ──► [+] mở form con (Bonus)
    │
    └─ Tab "Sở thích phục vụ"
            ├─ Load dropdown: GET /InventoryItemAdditions
            └─ Thêm/xóa dòng trên client (chưa gọi API)
    │
    ▼
Click [Lưu]
    │
    ├─ Validate FE (required, max length, định dạng số)
    │       └─ Fail ──► Focus input lỗi đầu tiên, hiện tooltip lỗi
    │
    ▼
POST /api/InventoryItems
    │
    ├─ Success ──► Toast "Thêm thành công" ──► Quay về danh sách
    └─ Fail ────► Toast "Có lỗi: [message]" ──► Giữ form
```

### Luồng xử lý form Sửa

```
Click Sửa (hover) hoặc Double click dòng
    │
    ▼
GET /api/InventoryItems/{id}  (load đầy đủ: kitchens + additions)
    │
    ▼
Render form với dữ liệu hiện có
    │
    ▼
Chỉnh sửa ──► Click [Lưu]
    │
    ├─ Validate FE
    ▼
PUT /api/InventoryItems/{id}
    │
    ├─ Success ──► Toast "Cập nhật thành công"
    └─ Fail ────► Toast lỗi
    
Nếu click [Hủy] khi đang có thay đổi:
    └─► Modal "Bạn có thay đổi chưa lưu. Thoát không?"
            ├─ Đồng ý ──► Quay về danh sách (không lưu)
            └─ Hủy ────► Ở lại form
```

---

## 3. Tab Sở thích phục vụ

```
┌─────────────────────────────────────────────────────────────────────────────┐
│ Thông tin chung  [Sở thích phục vụ]  Định lượng NVL  Chính sách giá bán    │
├─────────────────────────────────────────────────────────────────────────────┤
│ Món ăn                                                                      │
│ ℹ Ghi lại các sở thích của khách hàng giúp nhân viên phục vụ chọn nhanh    │
│   order. VD: không cay/ ít hành/ thêm phomai...                             │
│                                                                             │
│ [🔍 Tìm kiếm...]                                                            │
│ ┌─────────────────────────────────────────────┬────────────┬───┐           │
│ │ Sở thích phục vụ                            │ Thu thêm   │   │           │
│ ├─────────────────────────────────────────────┼────────────┼───┤           │
│ │ [20% đá                              ▼] [+] │ 0,00       │ 🗑│           │
│ │ [Không cay                           ▼] [+] │ 0,00       │ 🗑│           │
│ └─────────────────────────────────────────────┴────────────┴───┘           │
│ [+ Thêm dòng]                                                               │
└─────────────────────────────────────────────────────────────────────────────┘
```

**Luồng:**
- Mở dropdown → Gọi `GET /api/InventoryItemAdditions` lấy danh sách
- Chọn sở thích → Điền tự động `ExtraCharge` mặc định, cho phép override
- Thêm dòng → Thêm 1 row mới (client-side)
- Xóa dòng → Xóa row khỏi danh sách (client-side)
- Dữ liệu được submit cùng với form chính khi click Lưu

---

## 4. Bộ lọc nhanh theo cột (Bonus)

```
Click icon [▼ Lọc] ở header cột "Loại món"
    │
    ▼
┌─────────────────────┐
│ Lọc loại món     [X]│
│ [Chứa          ▼]   │
│ [Món ăn        ▼]   │
│ [Bỏ lọc] [Hủy] [Áp dụng] │
└─────────────────────┘
    │
    Click [Áp dụng]
    │
    ▼
GET /Paging?menuItemType=0&...
```

---

## 5. Luồng Upload ảnh

```
Click [Tải lên]
    │
    ├─ Mở file dialog, chọn file
    │
    ├─ Validate client: đúng định dạng (jpg/jpeg/png/gif)
    │
    ▼
POST /api/InventoryItems/upload-image (multipart/form-data)
    │
    ├─ Success ──► Hiển thị preview ảnh trong form
    │              Lưu đường dẫn vào field Image
    └─ Fail ────► Toast "Upload thất bại"

Click [X] bên cạnh ảnh ──► Xóa preview, clear field Image
```

---

## 6. Toast notifications

| Hành động | Toast |
|---|---|
| Thêm thành công | ✅ "Thêm thực đơn thành công" |
| Sửa thành công | ✅ "Cập nhật thực đơn thành công" |
| Xóa thành công | ✅ "Xóa thực đơn thành công" |
| Nhân bản thành công | ✅ "Nhân bản thực đơn thành công" |
| Lỗi validate BE | ❌ "[userMessage từ API]" |
| Lỗi server | ❌ "Có lỗi xảy ra, vui lòng thử lại" |
| Upload ảnh thành công | ✅ "Tải ảnh lên thành công" |
