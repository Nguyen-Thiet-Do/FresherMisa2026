# Kế hoạch Implementation

## Thứ tự thực hiện (Backend)

### Phase 1: Master tables (không phụ thuộc nhau)
1. `Unit` — entity + stored procs + API
2. `Kitchen` — entity + stored procs + API
3. `InventoryItemAddition` — entity + stored procs + API

### Phase 2: Nhóm thực đơn
4. `InventoryItemCategory` — entity + stored procs + API (có join kitchen)

### Phase 3: Entity chính
5. `InventoryItem` — entity + stored procs + API (phức tạp nhất)
   - CRUD cơ bản
   - Upload ảnh
   - Xử lý kitchens (join table)
   - Xử lý servicePreferences (join table)
   - FilterPaging
   - Clone endpoint

---

## Checklist Backend

### Unit
- [ ] Entity `Unit.cs` với `[ConfigTable("unit", true, "UnitName")]`
- [ ] `IUnitRepository`, `UnitRepository`
- [ ] `IUnitService`, `UnitService`
- [ ] `UnitsController`
- [ ] Stored procs: Insert, Update, DeleteById
- [ ] Đăng ký DI

### Kitchen
- [ ] Entity `Kitchen.cs` với `[ConfigTable("kitchen", true)]`
- [ ] `IKitchenRepository`, `KitchenRepository`
- [ ] `IKitchenService`, `KitchenService`
- [ ] `KitchensController`
- [ ] Stored procs: Insert, Update, DeleteById
- [ ] Đăng ký DI

### InventoryItemAddition (master sở thích)
- [ ] Entity `InventoryItemAddition.cs` với `[ConfigTable("inventory_item_addition", true)]`
- [ ] Repository, Service, Controller
- [ ] Stored procs: Insert, Update, DeleteById

### InventoryItemCategory (nhóm thực đơn)
- [ ] Entity `InventoryItemCategory.cs` với `[ConfigTable("inventory_item_category", true, "CategoryCode")]`
- [ ] Repository xử lý join `inventory_item_category_kitchen`
- [ ] Service, Controller
- [ ] Stored procs: Insert, Update, DeleteById, FilterPaging

### InventoryItem (thực đơn - entity chính)
- [ ] Entity `InventoryItem.cs` với `[ConfigTable("inventory_item", true, "MenuItemCode")]`
- [ ] DTO: `InventoryItemFilterRequest`, `InventoryItemDetailResponse`
- [ ] Repository xử lý:
  - [ ] Insert kèm kitchens + additions
  - [ ] Update kèm kitchens + additions
  - [ ] Delete cascade
  - [ ] GetDetail (join kitchens + additions)
  - [ ] FilterPaging
  - [ ] Clone
- [ ] Service: ValidateCustom (giá bán >= 0, max length), ValidateBeforeInsertAsync (check trùng mã)
- [ ] Controller: CRUD + clone + upload-image + filter
- [ ] Stored procs: Insert, Update, DeleteById, FilterPaging, GetDetail

---

## Stored Procedure convention

Tham số đặc biệt cần truyền:
- `v_KitchenIds` — JSON array string: `'["id1","id2"]'`
- `v_ServicePreferences` — JSON array string: `'[{"AdditionID":"...","ExtraCharge":0}]'`

SP xử lý JSON → DELETE + INSERT lại vào bảng join (tránh diff phức tạp).

---

## Validate rules (BE)

```csharp
// InventoryItem ValidateCustom
if (string.IsNullOrWhiteSpace(entity.MenuItemName))
    errors.Add("Tên món không được để trống");

if (string.IsNullOrWhiteSpace(entity.MenuItemCode))
    errors.Add("Mã món không được để trống");

if (entity.MenuItemCode?.Length > 255)
    errors.Add("Mã món không được vượt quá 255 ký tự");

if (entity.MenuItemNameOther?.Length > 255)
    errors.Add("Tên món theo ngôn ngữ khác không được vượt quá 255 ký tự");

if (entity.Description?.Length > 500)
    errors.Add("Mô tả không được vượt quá 500 ký tự");

if (entity.SalePrice < 0)
    errors.Add("Giá bán không được âm");

if (entity.UnitID == Guid.Empty)
    errors.Add("Đơn vị tính không được để trống");
```

---

## Upload ảnh

- Lưu vào thư mục: `wwwroot/uploads/inventory/{year}/{month}/`
- Tên file: `{Guid}.{extension}`
- Trả về relative path: `/uploads/inventory/2026/05/abc.jpg`
- Giới hạn: 5MB, chỉ nhận `.jpg .jpeg .png .gif`
- Khi xóa món: cân nhắc xóa file ảnh (dùng `OnAfterDelete` hook)
