# Thiết kế API

**Base URL:** `http://localhost:5237/api`  
**Response format:** `ServiceResponse` wrapper chuẩn của hệ thống

```json
{
  "isSuccess": true,
  "code": 200,
  "data": {},
  "userMessage": null,
  "devMessage": null
}
```

---

## 1. Unit API — `/api/Units`

| Method | Route | Mô tả |
|---|---|---|
| GET | `/api/Units` | Lấy tất cả đơn vị tính (có cache) |
| GET | `/api/Units/{id}` | Lấy theo ID |
| POST | `/api/Units` | Tạo mới |
| PUT | `/api/Units/{id}` | Cập nhật |
| DELETE | `/api/Units/{id}` | Xóa |

---

## 2. Kitchen API — `/api/Kitchens`

| Method | Route | Mô tả |
|---|---|---|
| GET | `/api/Kitchens` | Lấy tất cả bếp (có cache) |
| GET | `/api/Kitchens/{id}` | Lấy theo ID |
| POST | `/api/Kitchens` | Tạo mới |
| PUT | `/api/Kitchens/{id}` | Cập nhật |
| DELETE | `/api/Kitchens/{id}` | Xóa |

---

## 3. InventoryItemCategory API — `/api/InventoryItemCategories`

| Method | Route | Mô tả |
|---|---|---|
| GET | `/api/InventoryItemCategories` | Lấy tất cả nhóm (có cache) |
| GET | `/api/InventoryItemCategories/{id}` | Lấy theo ID |
| GET | `/api/InventoryItemCategories/Paging` | Phân trang |
| POST | `/api/InventoryItemCategories` | Tạo mới |
| PUT | `/api/InventoryItemCategories/{id}` | Cập nhật |
| DELETE | `/api/InventoryItemCategories/{id}` | Xóa |

---

## 4. InventoryItemAddition API — `/api/InventoryItemAdditions`

| Method | Route | Mô tả |
|---|---|---|
| GET | `/api/InventoryItemAdditions` | Lấy tất cả sở thích (dropdown source) |
| GET | `/api/InventoryItemAdditions/{id}` | Lấy theo ID |
| POST | `/api/InventoryItemAdditions` | Tạo mới |
| PUT | `/api/InventoryItemAdditions/{id}` | Cập nhật |
| DELETE | `/api/InventoryItemAdditions/{id}` | Xóa |

---

## 5. InventoryItem API — `/api/InventoryItems` *(entity chính)*

| Method | Route | Mô tả | HTTP |
|---|---|---|---|
| GET | `/api/InventoryItems/Paging` | Danh sách phân trang + tìm kiếm + lọc | 200 |
| GET | `/api/InventoryItems` | Lấy tất cả (có cache) | 200 |
| GET | `/api/InventoryItems/{id}` | Lấy chi tiết 1 món (kèm kitchens + additions) | 200 |
| POST | `/api/InventoryItems` | Tạo mới | 201 |
| PUT | `/api/InventoryItems/{id}` | Cập nhật | 200 |
| DELETE | `/api/InventoryItems/{id}` | Xóa | 200 |
| POST | `/api/InventoryItems/{id}/clone` | Nhân bản | 201 |
| GET | `/api/InventoryItems/filter` | Lọc nâng cao | 200 |

---

### 5.1 GET /Paging — Query params

| Param | Kiểu | Mô tả | Ví dụ |
|---|---|---|---|
| `search` | string | Tìm theo Mã món hoặc Tên món | `bún` |
| `pageIndex` | int | Trang hiện tại (bắt đầu từ 1) | `1` |
| `pageSize` | int | Số dòng/trang | `10` |
| `sort` | string | Cột sắp xếp, prefix `-` là DESC | `MenuItemName` hoặc `-SalePrice` |
| `menuItemType` | int? | Lọc theo loại món | `0` |
| `categoryId` | guid? | Lọc theo nhóm thực đơn | |
| `isDiscontinued` | bool? | Lọc món ngừng bán | `false` |

**Response data:**
```json
{
  "data": [
    {
      "menuItemID": "...",
      "menuItemCode": "M-1",
      "menuItemName": "Bún chả",
      "menuItemType": 0,
      "categoryName": "Món đặc biệt",
      "unitName": "Suất",
      "costPrice": 0,
      "salePrice": 25000,
      "isPriceChangeable": false
    }
  ],
  "totalCount": 100
}
```

---

### 5.2 GET /{id} — Chi tiết món ăn

**Response data:**
```json
{
  "menuItemID": "...",
  "menuItemCode": "M-001",
  "menuItemName": "Phở bát đá",
  "menuItemNameOther": "",
  "menuItemType": 0,
  "sortOrder": 1,
  "isSignatureDish": true,
  "categoryID": "...",
  "categoryName": "Phở",
  "unitID": "...",
  "unitName": "Bát",
  "salePrice": 80000,
  "isPriceChangeable": false,
  "isPriceAdjustable": false,
  "costPrice": 0,
  "image": "/uploads/inventory/abc.jpg",
  "description": "",
  "industryGroup": "Dịch vụ ăn uống",
  "vatRate": 3,
  "pitRate": 1.5,
  "isVATReduced": true,
  "isHiddenFromMenu": false,
  "isSemiFinishedProduct": false,
  "isAddedToOnlineMenu": false,
  "isCopiedToOtherRestaurant": false,
  "isDiscontinued": false,
  "kitchens": [
    { "kitchenID": "...", "kitchenName": "Bếp" }
  ],
  "servicePreferences": [
    { "detailID": "...", "additionID": "...", "additionName": "20% đá", "extraCharge": 0 }
  ]
}
```

---

### 5.3 POST / — Tạo mới món ăn

**Request body:**
```json
{
  "menuItemCode": "M-001",
  "menuItemName": "Phở bát đá",
  "menuItemNameOther": "",
  "menuItemType": 0,
  "sortOrder": 1,
  "isSignatureDish": true,
  "categoryID": "...",
  "unitID": "...",
  "salePrice": 80000,
  "isPriceChangeable": false,
  "isPriceAdjustable": false,
  "costPrice": 0,
  "image": "base64 hoặc đường dẫn sau khi upload",
  "description": "",
  "industryGroup": "",
  "vatRate": null,
  "pitRate": null,
  "isVATReduced": true,
  "isHiddenFromMenu": false,
  "isSemiFinishedProduct": false,
  "isAddedToOnlineMenu": false,
  "isCopiedToOtherRestaurant": false,
  "kitchenIds": ["...", "..."],
  "servicePreferences": [
    { "additionID": "...", "extraCharge": 0 }
  ]
}
```

---

### 5.4 PUT /{id} — Cập nhật món ăn

Body giống POST, thêm `isDiscontinued` (bool).

---

### 5.5 POST /{id}/clone — Nhân bản

**Request body:** (tùy chọn override)
```json
{
  "newMenuItemCode": "M-001-COPY"
}
```

**Logic:**
1. Load toàn bộ dữ liệu món gốc (kể cả kitchens + servicePreferences)
2. Tạo bản ghi mới với `newMenuItemCode` (nếu không truyền thì tự sinh)
3. Copy toàn bộ `inventory_item_kitchen` và `inventory_item_addition_detail`
4. Trả về ID của bản ghi mới

---

### 5.6 Upload ảnh — `/api/InventoryItems/upload-image`

| Method | Route | Mô tả |
|---|---|---|
| POST | `/api/InventoryItems/upload-image` | Upload ảnh, trả về đường dẫn |

**Request:** `multipart/form-data`, field `file`  
**Response:**
```json
{
  "isSuccess": true,
  "data": "/uploads/inventory/2026/05/abc123.jpg"
}
```

**Validation:**
- Chỉ chấp nhận: `.jpg`, `.jpeg`, `.png`, `.gif`
- Giới hạn kích thước: 5MB

---

## 6. Stored Procedures cần tạo

| SP | Dùng cho |
|---|---|
| `Proc_InsertInventoryItem` | Insert món + kitchens + additions |
| `Proc_UpdateInventoryItem` | Update món + kitchens + additions |
| `Proc_DeleteInventoryItemById` | Xóa món (cascade kitchens + details) |
| `Proc_InventoryItem_FilterPaging` | Danh sách phân trang + filter |
| `Proc_InventoryItem_GetDetail` | Lấy chi tiết + join |
| `Proc_InsertInventoryItemCategory` | Insert nhóm + kitchens |
| `Proc_UpdateInventoryItemCategory` | Update nhóm + kitchens |
| `Proc_DeleteInventoryItemCategoryById` | Xóa nhóm |
| `Proc_InventoryItemCategory_FilterPaging` | Paging nhóm |
| `Proc_InsertUnit` | Insert đơn vị tính |
| `Proc_UpdateUnit` | Update đơn vị tính |
| `Proc_DeleteUnitById` | Xóa đơn vị tính |
| `Proc_InsertKitchen` | Insert bếp |
| `Proc_UpdateKitchen` | Update bếp |
| `Proc_DeleteKitchenById` | Xóa bếp |
| `Proc_InsertInventoryItemAddition` | Insert sở thích |
| `Proc_UpdateInventoryItemAddition` | Update sở thích |
| `Proc_DeleteInventoryItemAdditionById` | Xóa sở thích |
