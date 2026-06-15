# Luồng xử lý — Module Thành Phần Lương (SalaryComposition)

---

## 1. Luồng Thêm (`POST /api/SalaryCompositions`)

```
Controller.Post (:135)
 │  Override BaseController — xử lý thêm response 202
 │
 ├─► SalaryCompositionService.InsertAsync (:59)
 │    │  Override BaseService — chạy trước base để check công thức
 │    │
 │    ├─► CheckUnfollowedConfirmationAsync (:480)
 │    │    │  Nếu IsSkipUnfollowedComposition = true → bỏ qua toàn bộ
 │    │    ├─► NormalizeFormulas (:617)          trim whitespace 4 công thức
 │    │    ├─► LoadCodeSnapshotAsync (:600)      lấy activeCodes, allCodes, nameMap (1 query)
 │    │    ├─► CollectFormulaWarnings (:700)     tìm mã ngừng theo dõi trong công thức
 │    │    └─► BuildConfirmationRequiredResponse (:737)
 │    │         Nếu có mã inactive → trả 202, dừng luồng
 │    │
 │    └─► BaseService.InsertAsync (BaseService.cs:392)
 │         │  Set State=Add, CreateDate=now, CreatedBy
 │         │
 │         ├─► Validate() (:323)
 │         │    ├─► ValidateRequired (:355)      check [IRequired] null/rỗng → message tiếng Việt
 │         │    └─► ValidateCustom (:345)
 │         │         - max length Code (25), Name (255)
 │         │         - format Code: chỉ A-Z, 0-9, _ (regex)
 │         │         - Code không được là số thực
 │         │         - Code không trùng tên hàm: SUM, IF, AND, OR, INT, TODAY
 │         │         - BR-05: TaxType chỉ set khi Nature = Income
 │         │         - TaxableFormula/ExemptFormula chỉ set khi TaxType = PartiallyExempt
 │         │
 │         ├─► ValidateBeforeInsertAsync (:398)
 │         │    ├─► NormalizeFormulas (:617)
 │         │    ├─► AutoDerivePartialExemptFormulas (:635)
 │         │    │    Khi PartiallyExempt + chỉ có 1 công thức:
 │         │    │    - Có TaxableFormula → derive ExemptFormula = expr - TaxableFormula
 │         │    │    - Có ExemptFormula  → derive TaxableFormula = expr - ExemptFormula
 │         │    │    - expr lấy từ ResolveValueExpressionAsync (:685)
 │         │    ├─► ValidateAndNormalizeOrganizationIDsAsync (:754)
 │         │    │    - OrganizationIDs không được rỗng
 │         │    │    - NormalizeOrganizationIDsAsync (:763): roll-up nếu đủ con → thay bằng cha
 │         │    ├─► LoadCodeSnapshotAsync (:600)
 │         │    └─► CollectFormulaWarnings (:700)
 │         │         - Lỗi cứng (mã không tồn tại, sai cú pháp) → block insert
 │         │         - Mã inactive → bỏ qua (đã xử lý ở bước CheckUnfollowed)
 │         │
 │         └─► BaseRepository.InsertAsync (:330)
 │              ├─► ValidateUniqueColumnsAsync (:653)   ← CHECK TRÙNG CODE
 │              │    Đọc uniqueColumns từ [ConfigTable] → SELECT COUNT(*) theo cột Code
 │              │    Throw DuplicateEntityException nếu trùng (trước khi mở transaction)
 │              ├─► Mở connection + transaction
 │              ├─► EnsurePrimaryKeyForInsert           auto Guid nếu FE không truyền
 │              ├─► MappingDbType                       map tất cả property → @v_{Name}
 │              ├─► Gọi Proc_InsertPa_Salary_Composition
 │              │    MySql 1062 (race condition) → TranslateMySqlException (:31) → DuplicateEntityException
 │              └─► Commit / Rollback
 │
 └─► OnAfterInsert()   lifecycle hook (hiện để trống)
     Controller trả 201 Created
```

---

## 2. Luồng Sửa (`PUT /api/SalaryCompositions/{id}`)

```
Controller.Put (:146)
 │  Override BaseController — xử lý thêm response 202 và 404
 │
 ├─► SalaryCompositionService.UpdateAsync (:70)
 │    │  Override BaseService — chạy trước base để check công thức (y hệt Insert)
 │    │
 │    ├─► CheckUnfollowedConfirmationAsync (:480)   [giống luồng thêm]
 │    │
 │    └─► BaseService.UpdateAsync (BaseService.cs:432)
 │         │  Check entityId != Guid.Empty
 │         │  Fetch existing từ DB/cache → 404 nếu không tìm thấy
 │         │  Set State=Update, ModifiedDate=now, ModifiedBy
 │         │
 │         ├─► Validate() (:323)                    [giống luồng thêm]
 │         │    ├─► ValidateRequired (:355)
 │         │    └─► ValidateCustom (:345)
 │         │
 │         ├─► ValidateBeforeUpdateAsync (:416)
 │         │    ├─► NormalizeFormulas (:617)
 │         │    ├─► Fetch existing từ DB/cache (:421)
 │         │    ├─► AutoDerivePartialExemptFormulas (:635)
 │         │    │    Truyền thêm existingSource để re-derive đúng khi valueExpression thay đổi
 │         │    ├─► BR-01: Code không được thay đổi (:424)
 │         │    │    So sánh entity.Code với existing.Code → lỗi nếu khác
 │         │    ├─► BR-10: TPL kế thừa hệ thống — chỉ cho sửa 5 field (:428)
 │         │    │    GetLockedFieldChanges (:533)
 │         │    │    Đọc LockedFields (JSON array of int) trên existing
 │         │    │    So sánh từng field tương ứng với LockableField enum
 │         │    │    Trả tên field vi phạm → lỗi nếu có
 │         │    ├─► ValidateAndNormalizeOrganizationIDsAsync (:754)
 │         │    └─► LoadCodeSnapshotAsync + CollectFormulaWarnings (:700)
 │         │
 │         └─► BaseRepository.UpdateAsync (:378)
 │              ├─► ValidateUniqueColumnsAsync(entity, excludeId) (:653)
 │              │    Truyền excludeId để không tự so sánh với chính bản ghi đang sửa
 │              ├─► Mở connection + transaction
 │              ├─► MappingDbType → @v_{Name}
 │              ├─► Gọi Proc_UpdatePa_Salary_Composition
 │              │    MySql 1062 → TranslateMySqlException → DuplicateEntityException
 │              └─► Commit / Rollback
 │
 └─► OnAfterUpdate()   lifecycle hook
     Controller trả 200 OK
```

---

## 3. Luồng Xóa (`DELETE /api/SalaryCompositions/{id}`)

```
BaseController.DeleteByID (:107)
 │
 └─► BaseService.DeleteByIDAsync (:181)
      │  Check entityId != Guid.Empty
      │  Fetch existing → 404 nếu không tìm thấy
      │
      ├─► ValidateBeforeDeleteAsync (:447)
      │    ├─► BR-08: Source = InheritedFromSystem → return false (block xóa)
      │    └─► BR-11: Load tất cả TPL khác, check công thức có tham chiếu Code không
      │         CollectFormulas (:837)      gom 4 công thức của mỗi TPL
      │         IsCodeReferencedInFormulas (:848)
      │         Word-boundary regex để tránh false positive (LUONG ≠ LUONG_CO_BAN)
      │         return false nếu đang được tham chiếu
      │
      ├─► GetDeleteValidationMessageAsync (:462)   lấy message tiếng Việt tương ứng
      │    - InheritedFromSystem → "Không thể xóa thành phần lương mặc định của hệ thống"
      │    - Đang được tham chiếu → "Không thể xóa ... vì đang được sử dụng trong công thức..."
      │
      ├─► BaseRepository.DeleteAsync
      │    Gọi Proc_DeletePa_Salary_CompositionById
      │
      ├─► AfterDelete()     lifecycle hook (override để cleanup file...)
      └─► OnAfterDelete()   lifecycle hook
          Controller trả 200 OK
```

---

## 4. Luồng Xóa nhiều

### 4a. Fail-fast (`POST /api/SalaryCompositions/bulk-delete`)

```
BaseController.DeleteMany (:76)
 └─► BaseService.DeleteManyAsync (:221)
      │  Duyệt từng ID:
      │   - Fetch entity → dừng toàn bộ nếu không tìm thấy
      │   - ValidateBeforeDeleteAsync → dừng toàn bộ nếu không được xóa
      │
      └─► BaseRepository.DeleteManyAsync   xóa tất cả trong 1 transaction
          Rollback toàn bộ nếu bất kỳ ID nào lỗi
```

### 4b. Partial result (`POST /api/SalaryCompositions/bulk-delete/partial`)

```
BaseController.DeleteManyPartial (:93)
 └─► BaseService.DeleteManyPartialAsync (:264)
      Duyệt từng ID, lỗi ID nào thì ghi vào Failed, tiếp tục ID tiếp theo
      Trả về { Succeeded: [...], Failed: [{id, reason}] }
```

---

## 5. Luồng Chuyển TPL hệ thống → TPL đơn vị

### 5a. Đơn lẻ (`POST /api/SalaryCompositions/inherit/{systemCompositionId}`)

```
Controller.InheritFromSystem (:39)
 └─► SalaryCompositionService.InheritFromSystemAsync (:88)
      ├─► Fetch SalaryCompositionSystem theo ID → 404 nếu không tìm thấy
      ├─► Check ComponentTypeID hợp lệ (không null, không Guid.Empty) → 400 nếu chưa gán
      ├─► GetRootOrganizationIdsAsync (:520)
      │    Nếu OrganizationIDs không truyền → lấy tất cả đơn vị gốc (ParentID = null)
      ├─► MapSystemToComposition (:497)
      │    Copy các field từ System sang Composition
      │    Set Source = InheritedFromSystem, Status = Active
      └─► InsertAsync → toàn bộ luồng thêm (mục 1)
```

### 5b. Batch (`POST /api/SalaryCompositions/inherit/batch`)

```
Controller.InheritFromSystemBatch (:52)
 └─► SalaryCompositionService.InheritFromSystemBatchAsync (:111)
      Prefetch OrganizationIDs 1 lần (tránh N+1)
      Duyệt từng SystemCompositionId:
       - Fetch System → ghi Failed nếu không tìm thấy
       - Check ComponentTypeID → ghi Failed nếu chưa gán
       - MapSystemToComposition + InsertAsync
       - Ghi Succeeded nếu thành công, Failed nếu lỗi
      Trả về { Succeeded: [...], Failed: [{id, reason}] }
```

---

## 6. Luồng Sửa nhiều field (`PATCH /api/SalaryCompositions/{id}/fields`)

```
BaseController.PatchFields (:174)
 └─► BaseService.PatchFieldsAsync (:585)
      │  Check entityId != Guid.Empty, fields không rỗng
      │  Duyệt từng field trong request:
      │   - Tìm property theo tên (case-insensitive) → lỗi nếu không tồn tại
      │   - Không cho sửa PK
      │   - Không cho sửa security fields (CreatedBy, CreateDate, ModifiedBy, ModifiedDate, State, IsDeleted)
      │   - Không cho sửa field gắn [NotPatchable]
      │   - ConvertJsonElementToType (:646) — chuyển JSON value sang đúng kiểu C#
      │   - Check [IRequired] nếu field có annotation đó
      │  Gom tất cả lỗi trước, trả 400 nếu có
      │
      ├─► Fetch existing → 404 nếu không tìm thấy
      └─► BaseRepository.PatchFieldsAsync
           1 câu UPDATE duy nhất cho tất cả field
```

---

## 7. Luồng Config cột

### 7a. Lấy config (`GET /api/GridConfigs/by-grid`)

```
GridConfigsController.GetByGrid (:38)
 └─► GridConfigService.GetByGridAsync
      Query theo UserID + GridCode
      Trả danh sách cấu hình cột của user cho lưới đó
```

### 7b. Lưu config (`PUT /api/GridConfigs/batch`)

```
GridConfigsController.BatchUpsert (:49)
 └─► GridConfigService.BatchUpsertAsync
      Xóa toàn bộ config cũ của UserID + GridCode
      Insert lại toàn bộ danh sách mới trong 1 transaction
```

### 7c. Reset về mặc định (`DELETE /api/GridConfigs/reset`)

```
GridConfigsController.Reset (:60)
 └─► GridConfigService.ResetAsync
      Xóa toàn bộ config của UserID + GridCode
      FE sẽ fallback về cấu hình mặc định hệ thống
```

---

## 8. Validate trùng Code — 2 lớp bảo vệ

```
Lớp 1 — Pre-check (BaseRepository.ValidateUniqueColumnsAsync :653)
 Chạy trước khi mở transaction
 Đọc uniqueColumns từ [ConfigTable] → "Code"
 SELECT COUNT(*) WHERE Code = ? [AND ID != excludeId khi Update]
 Throw DuplicateEntityException ngay nếu trùng

Lớp 2 — Safety net race condition (BaseRepository.TranslateMySqlException :31)
 Bắt MySqlException 1062 khi 2 request vượt qua Lớp 1 cùng lúc
 Parse tên constraint UQ_{ColumnName} → tra [Display(Name)] → message tiếng Việt
 Throw DuplicateEntityException
```

---

## Tóm tắt file / dòng quan trọng

| Hàm | File | Dòng |
|---|---|---|
| `Controller.Post` | `SalaryCompositionsController.cs` | 135 |
| `Controller.Put` | `SalaryCompositionsController.cs` | 146 |
| `Controller.InheritFromSystem` | `SalaryCompositionsController.cs` | 39 |
| `SalaryCompositionService.InsertAsync` | `SalaryCompositionService.cs` | 59 |
| `SalaryCompositionService.UpdateAsync` | `SalaryCompositionService.cs` | 70 |
| `CheckUnfollowedConfirmationAsync` | `SalaryCompositionService.cs` | 480 |
| `ValidateCustom` | `SalaryCompositionService.cs` | 345 |
| `ValidateBeforeInsertAsync` | `SalaryCompositionService.cs` | 398 |
| `ValidateBeforeUpdateAsync` | `SalaryCompositionService.cs` | 416 |
| `GetLockedFieldChanges` (BR-10) | `SalaryCompositionService.cs` | 533 |
| `ValidateBeforeDeleteAsync` | `SalaryCompositionService.cs` | 447 |
| `AutoDerivePartialExemptFormulas` | `SalaryCompositionService.cs` | 635 |
| `CollectFormulaWarnings` | `SalaryCompositionService.cs` | 700 |
| `NormalizeOrganizationIDsAsync` | `SalaryCompositionService.cs` | 763 |
| `MapSystemToComposition` | `SalaryCompositionService.cs` | 497 |
| `BaseService.InsertAsync` | `BaseService.cs` | 392 |
| `BaseService.UpdateAsync` | `BaseService.cs` | 432 |
| `Validate` | `BaseService.cs` | 323 |
| `ValidateRequired` | `BaseService.cs` | 355 |
| `PatchFieldsAsync` | `BaseService.cs` | 585 |
| `BaseRepository.InsertAsync` | `BaseRepository.cs` | 330 |
| `BaseRepository.UpdateAsync` | `BaseRepository.cs` | 378 |
| `ValidateUniqueColumnsAsync` | `BaseRepository.cs` | 653 |
| `TranslateMySqlException` | `BaseRepository.cs` | 31 |
