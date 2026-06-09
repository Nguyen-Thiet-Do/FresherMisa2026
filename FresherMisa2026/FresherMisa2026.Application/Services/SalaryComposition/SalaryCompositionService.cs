using FresherMisa2026.Application.Interfaces.Repositories;
using FresherMisa2026.Application.Interfaces.Services;
using FresherMisa2026.Entities;
using FresherMisa2026.Entities.Enums;
using FresherMisa2026.Entities.Organization;
using FresherMisa2026.Entities.SalaryComposition;
using FresherMisa2026.Entities.SalaryComposition.DTO;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.RegularExpressions;
using SalaryCompositionEntity = FresherMisa2026.Entities.SalaryComposition.SalaryComposition;

namespace FresherMisa2026.Application.Services
{
    /// <summary>
    /// Service cho SalaryComposition — xử lý nghiệp vụ TPL của đơn vị.
    /// </summary>
    /// <remarks>Created by: ntdo — 27/05/2026 · Refactor: 03/06/2026</remarks>
    public class SalaryCompositionService
        : BaseService<SalaryCompositionEntity>, ISalaryCompositionService
    {
        #region Declare

        private readonly ISalaryCompositionRepository _salaryCompositionRepository;
        private readonly ISalaryCompositionSystemRepository _systemRepository;
        private readonly IOrganizationRepository _organizationRepository;
        private readonly ILogger<SalaryCompositionService> _logger;

        private static readonly Regex _codePattern =
            new(SalaryCompositionConstants.CodePattern, RegexOptions.Compiled);

        #endregion

        #region Constructer

        public SalaryCompositionService(
            ISalaryCompositionRepository salaryCompositionRepository,
            ISalaryCompositionSystemRepository systemRepository,
            IOrganizationRepository organizationRepository,
            ILogger<SalaryCompositionService> logger)
            : base(salaryCompositionRepository)
        {
            _salaryCompositionRepository = salaryCompositionRepository;
            _systemRepository = systemRepository;
            _organizationRepository = organizationRepository;
            _logger = logger;
        }

        #endregion

        #region Methods - Insert/Update (override để kiểm tra TPL ngừng theo dõi)

        /// <summary>
        /// Thêm mới — nếu công thức chứa TPL ngừng theo dõi và FE chưa xác nhận thì trả về 202 yêu cầu xác nhận.
        /// </summary>
        /// <param name="entity">Thông tin TPL cần thêm.</param>
        /// <returns>ServiceResponse: Created, ConfirmationRequired hoặc BadRequest.</returns>
        /// <remarks>Created by: ntdo — 01/06/2026</remarks>
        public override async Task<ServiceResponse> InsertAsync(SalaryCompositionEntity entity)
        {
            var confirmation = await CheckUnfollowedConfirmationAsync(entity);
            if (confirmation != null) return confirmation;
            return await base.InsertAsync(entity);
        }

        /// <summary>
        /// Cập nhật — kiểm tra TPL ngừng theo dõi như InsertAsync.
        /// </summary>
        /// <remarks>Created by: ntdo — 01/06/2026</remarks>
        public override async Task<ServiceResponse> UpdateAsync(Guid id, SalaryCompositionEntity entity)
        {
            var confirmation = await CheckUnfollowedConfirmationAsync(entity);
            if (confirmation != null) return confirmation;
            return await base.UpdateAsync(id, entity);
        }

        #endregion

        #region Methods - Nghiệp vụ riêng

        /// <summary>
        /// Chuyển 1 TPL hệ thống thành TPL đơn vị — kế thừa các field cơ bản, gán Source = InheritedFromSystem.
        /// </summary>
        /// <param name="systemCompositionId">ID TPL hệ thống nguồn.</param>
        /// <param name="organizationIds">Danh sách đơn vị áp dụng; nếu null/empty thì lấy toàn bộ đơn vị gốc.</param>
        /// <returns>ServiceResponse của InsertAsync.</returns>
        /// <remarks>Created by: ntdo — 27/05/2026</remarks>
        public async Task<ServiceResponse> InheritFromSystemAsync(Guid systemCompositionId, List<Guid>? organizationIds)
        {
            var system = await _systemRepository.GetEntityByIDAsync(systemCompositionId);
            if (system == null)
                return CreateErrorResponse(ResponseCode.NotFound, "Không tìm thấy thành phần lương hệ thống");

            // TPL hệ thống bắt buộc phải có ComponentTypeID hợp lệ để kế thừa được — fallback Guid.Empty sẽ tạo data rác
            if (!system.ComponentTypeID.HasValue || system.ComponentTypeID.Value == Guid.Empty)
                return CreateErrorResponse(ResponseCode.BadRequest,
                    $"Thành phần lương hệ thống '{system.Code}' chưa được gán Loại thành phần — không thể kế thừa");

            var resolvedOrgIds = organizationIds is { Count: > 0 }
                ? organizationIds
                : await GetRootOrganizationIdsAsync();

            var composition = MapSystemToComposition(system, resolvedOrgIds);
            return await InsertAsync(composition);
        }

        /// <summary>
        /// Chuyển nhiều TPL hệ thống thành TPL đơn vị — partial result: ID lỗi sẽ vào danh sách Failed nhưng vẫn tiếp tục.
        /// </summary>
        /// <remarks>Created by: ntdo — 27/05/2026</remarks>
        public async Task<ServiceResponse> InheritFromSystemBatchAsync(InheritFromSystemBatchRequest request)
        {
            if (request.SystemCompositionIds == null || request.SystemCompositionIds.Count == 0)
                return CreateErrorResponse(ResponseCode.BadRequest, "Danh sách Id không được rỗng");

            // Prefetch 1 lần thay vì N lần để tránh N+1 trên repository tổ chức
            var resolvedOrgIds = request.OrganizationIDs is { Count: > 0 }
                ? request.OrganizationIDs
                : await GetRootOrganizationIdsAsync();

            var result = new BulkInheritResult();

            foreach (var systemId in request.SystemCompositionIds)
            {
                try
                {
                    var system = await _systemRepository.GetEntityByIDAsync(systemId);
                    if (system == null)
                    {
                        result.Failed.Add(new BulkDeleteFailedItem(systemId, "Không tìm thấy TPL hệ thống"));
                        continue;
                    }

                    if (!system.ComponentTypeID.HasValue || system.ComponentTypeID.Value == Guid.Empty)
                    {
                        result.Failed.Add(new BulkDeleteFailedItem(systemId,
                            $"TPL hệ thống '{system.Code}' chưa được gán Loại thành phần"));
                        continue;
                    }

                    var composition = MapSystemToComposition(system, resolvedOrgIds);
                    var response = await InsertAsync(composition);

                    if (response.IsSuccess)
                    {
                        result.Succeeded.Add(systemId);
                    }
                    else
                    {
                        var reason = response.Data is List<ValidationError> errors
                            ? string.Join("; ", errors.Select(e => e.Message))
                            : response.DevMessage?.ToString() ?? "Lỗi không xác định";
                        result.Failed.Add(new BulkDeleteFailedItem(systemId, reason));
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "[INHERIT BATCH] Lỗi khi kế thừa TPL hệ thống {SystemId}", systemId);
                    result.Failed.Add(new BulkDeleteFailedItem(systemId, ex.Message));
                }
            }

            return CreateSuccessResponse(result);
        }

        /// <summary>
        /// Đổi trạng thái theo dõi của 1 TPL — patch trực tiếp, không qua full validation.
        /// </summary>
        /// <remarks>Created by: ntdo — 27/05/2026</remarks>
        public async Task<ServiceResponse> SetStatusAsync(Guid id, SalaryCompositionStatus status)
        {
            var entity = await _baseRepository.GetEntityByIDAsync(id);
            if (entity == null)
                return CreateErrorResponse(ResponseCode.NotFound, "Không tìm thấy thành phần lương");

            int rows = await _baseRepository.PatchFieldAsync(id, nameof(entity.Status), (int)status);
            return rows > 0
                ? CreateSuccessResponse(rows)
                : CreateErrorResponse(ResponseCode.NotFound, "Không tìm thấy bản ghi để cập nhật");
        }

        /// <summary>Đổi trạng thái theo dõi nhiều TPL — partial result.</summary>
        /// <remarks>Created by: ntdo — 28/05/2026</remarks>
        public async Task<ServiceResponse> SetStatusBulkAsync(List<Guid> ids, SalaryCompositionStatus status)
        {
            if (ids == null || ids.Count == 0)
                return CreateErrorResponse(ResponseCode.BadRequest, "Danh sách Id không được rỗng");

            var result = new BulkDeleteResult();

            foreach (var id in ids)
            {
                try
                {
                    var response = await SetStatusAsync(id, status);
                    if (response.IsSuccess)
                        result.Succeeded.Add(id);
                    else
                        result.Failed.Add(new BulkDeleteFailedItem(id,
                            response.DevMessage?.ToString() ?? "Không tìm thấy thành phần lương"));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[STATUS BULK] Lỗi khi đổi trạng thái TPL {Id}", id);
                    result.Failed.Add(new BulkDeleteFailedItem(id, ex.Message));
                }
            }

            return CreateSuccessResponse(result);
        }

        /// <summary>
        /// Gợi ý Code/Name/Description cho ô nhập công thức — chỉ trả TPL đang theo dõi.
        /// </summary>
        /// <remarks>Created by: ntdo — 27/05/2026</remarks>
        public async Task<ServiceResponse> GetSuggestionsAsync(string? search)
        {
            var all = (await _baseRepository.GetEntitiesAsync())
                .Cast<SalaryCompositionEntity>()
                .Where(e => e.Status == SalaryCompositionStatus.Active);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                all = all.Where(e =>
                    e.Code.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    e.Name.Contains(term, StringComparison.OrdinalIgnoreCase));
            }

            var suggestions = all
                .OrderBy(e => e.Code)
                .Select(e => new SalaryCompositionSuggestion
                {
                    Code        = e.Code,
                    Name        = e.Name,
                    Description = e.Description,
                })
                .ToList();

            return CreateSuccessResponse(suggestions);
        }

        /// <summary>Lọc nâng cao 4 phần qua Stored Procedure.</summary>
        public async Task<ServiceResponse> AdvancedFilterWithProcAsync(SalaryCompositionAdvancedFilterRequest request)
        {
            var fieldErrors = ValidateFilterFieldNames(request.SearchFields, request.Filters);
            if (fieldErrors.Count > 0) return CreateValidationErrorResponse(fieldErrors);

            var (data, total) = await _salaryCompositionRepository.AdvancedFilterWithProcAsync(request);

            if (request.Columns?.Count > 0)
                return CreateSuccessResponse(BuildProjectedPaging(total, request.PageIndex, request.PageSize, ProjectColumns(data, request.Columns)));

            return CreatePagingResponse(total, request.PageIndex, request.PageSize, data);
        }

        /// <summary>
        /// Phân loại danh sách TPL trước khi xóa/ngừng theo dõi hàng loạt: DataSystem, DataExist, DataNotExist.
        /// Build index Code→Referencers một lần (O(N)) thay vì O(N²) regex.
        /// </summary>
        /// <remarks>Created by: ntdo — 02/06/2026 · Refactor 03/06/2026: build index để fix O(N²).</remarks>
        public async Task<ServiceResponse> ExitDataAsync(ExitDataRequest request)
        {
            if (request.Ids == null || request.Ids.Count == 0)
                return CreateErrorResponse(ResponseCode.BadRequest, "Danh sách Id không được rỗng");

            // Bước 1: load toàn bộ TPL (qua cache 5')
            var all = (await _baseRepository.GetEntitiesAsync())
                .Cast<SalaryCompositionEntity>()
                .ToList();

            var idMap = all.ToDictionary(e => e.SalaryCompositionID);

            // Bước 2: build index Code → danh sách TPL đang tham chiếu (chỉ duyệt toàn bộ all 1 lần)
            //   tổng chi phí ≈ tổng số token Identifier trong tất cả formula
            var referencerIndex = BuildReferencerIndex(all);

            // Bước 3: phân trang trên danh sách ID đầu vào
            var total = request.Ids.Count;
            var pageIds = request.Ids
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            var result = new ExitDataResult
            {
                Total       = total,
                PageSize    = request.PageSize,
                CurrentPage = request.PageIndex,
                PageCount   = (long)Math.Ceiling((double)total / request.PageSize)
            };

            foreach (var id in pageIds)
            {
                if (!idMap.TryGetValue(id, out var entity)) continue;

                var item = new ExitDataItem
                {
                    SalaryCompositionID = entity.SalaryCompositionID,
                    Code   = entity.Code,
                    Name   = entity.Name,
                    Source = (int)entity.Source,
                    Status = (int)entity.Status,
                };

                if (entity.Source == SalaryCompositionSource.InheritedFromSystem)
                {
                    result.DataSystem.Add(item);
                }
                else if (referencerIndex.TryGetValue(entity.Code, out var referencers)
                         && referencers.Any(r => r.Id != entity.SalaryCompositionID))
                {
                    item.ReferencedBy = referencers
                        .Where(r => r.Id != entity.SalaryCompositionID)
                        .Select(r => new ReferencingCompositionInfo(r.Id, r.Code, r.Name))
                        .ToList();
                    result.DataExist.Add(item);
                }
                else
                {
                    result.DataNotExist.Add(item);
                }
            }

            return CreateSuccessResponse(result);
        }

        /// <summary>Lọc TPL theo nhiều điều kiện có phân trang (qua SP).</summary>
        /// <remarks>Created by: ntdo — 27/05/2026</remarks>
        public async Task<ServiceResponse> FilterAsync(SalaryCompositionFilterRequest request)
        {
            var (data, total) = await _salaryCompositionRepository.FilterAsync(request);
            return CreatePagingResponse(total, request.PageIndex, request.PageSize, data);
        }

        #endregion

        #region OVERRIDE METHODS - Validate

        /// <summary>
        /// Validate: BR-03 độ dài + định dạng Code, BR-05 TaxType chỉ áp dụng khi Nature = Income,
        /// TaxableFormula/ExemptFormula chỉ dùng với PartiallyExempt.
        /// </summary>
        protected override List<ValidationError> ValidateCustom(SalaryCompositionEntity entity)
        {
            var errors = new List<ValidationError>();

            // Bước 1: max length
            if (!string.IsNullOrEmpty(entity.Code) && entity.Code.Length > SalaryCompositionConstants.MaxCodeLength)
                errors.Add(new ValidationError("Code",
                    $"Mã thành phần không được vượt quá {SalaryCompositionConstants.MaxCodeLength} ký tự"));

            if (!string.IsNullOrEmpty(entity.Name) && entity.Name.Length > SalaryCompositionConstants.MaxNameLength)
                errors.Add(new ValidationError("Name",
                    $"Tên thành phần không được vượt quá {SalaryCompositionConstants.MaxNameLength} ký tự"));

            // Bước 2: format Code (chỉ kiểm tra nếu trong giới hạn max length)
            if (!string.IsNullOrEmpty(entity.Code)
                && entity.Code.Length <= SalaryCompositionConstants.MaxCodeLength)
            {
                if (!_codePattern.IsMatch(entity.Code))
                    errors.Add(new ValidationError("Code",
                        "Mã thành phần chỉ được chứa chữ cái (A-Z, a-z), số (0-9) và dấu gạch dưới (_)"));

                else if (decimal.TryParse(entity.Code,
                             System.Globalization.NumberStyles.Any,
                             System.Globalization.CultureInfo.InvariantCulture, out _))
                    errors.Add(new ValidationError("Code", "Mã thành phần không được là một số thực"));

                // Cấm Code trùng tên hàm dựng sẵn (SUM, IF, AND, OR, INT, TODAY) — tránh ambiguity
                // khi BuildReferencerIndex / IsCodeReferencedInFormulas match identifier trong công thức.
                else if (FormulaValidator.AllowedFunctions.Contains(entity.Code))
                    errors.Add(new ValidationError("Code",
                        $"Mã thành phần không được trùng tên hàm dựng sẵn: {string.Join(", ", FormulaValidator.AllowedFunctions)}"));
            }

            // Bước 3: BR-05 — TaxType chỉ áp dụng khi Nature = Income
            if (entity.TaxType.HasValue && entity.Nature != SalaryNature.Income)
                errors.Add(new ValidationError("TaxType",
                    "Loại thuế TNCN chỉ áp dụng khi tính chất là Thu nhập"));

            // Bước 4: TaxableFormula / ExemptFormula chỉ dùng khi PartiallyExempt
            if (entity.TaxType != SalaryTaxType.PartiallyExempt)
            {
                if (!string.IsNullOrWhiteSpace(entity.TaxableFormula))
                    errors.Add(new ValidationError("TaxableFormula",
                        "Công thức phần chịu thuế chỉ áp dụng khi loại thuế là 'Miễn một phần'"));
                if (!string.IsNullOrWhiteSpace(entity.ExemptFormula))
                    errors.Add(new ValidationError("ExemptFormula",
                        "Công thức phần miễn thuế chỉ áp dụng khi loại thuế là 'Miễn một phần'"));
            }

            return errors;
        }

        /// <summary>Trước khi thêm: bắt buộc OrganizationIDs, normalize công thức và check tham chiếu mã.</summary>
        protected override async Task<List<ValidationError>> ValidateBeforeInsertAsync(SalaryCompositionEntity entity)
        {
            var errors = new List<ValidationError>();

            NormalizeFormulas(entity);
            AutoDerivePartialExemptFormulas(entity);

            errors.AddRange(await ValidateAndNormalizeOrganizationIDsAsync(entity));

            var (activeCodes, allCodes, _) = await LoadCodeSnapshotAsync();
            var effectiveActive = entity.IsSkipUnfollowedComposition ? allCodes : activeCodes;
            var (formulaErrors, _) = CollectFormulaWarnings(entity, effectiveActive, allCodes);
            errors.AddRange(formulaErrors);

            return errors;
        }

        /// <summary>Trước khi cập nhật: BR-01 Code không đổi, BR-10 TPL hệ thống chỉ sửa 5 field.</summary>
        protected override async Task<List<ValidationError>> ValidateBeforeUpdateAsync(Guid entityId, SalaryCompositionEntity entity)
        {
            var errors = new List<ValidationError>();

            NormalizeFormulas(entity);
            var existing = await _baseRepository.GetEntityByIDAsync(entityId);
            AutoDerivePartialExemptFormulas(entity, existing?.TaxFormulaSource ?? TaxFormulaSource.None);

            if (existing != null && existing.Code != entity.Code)
                errors.Add(new ValidationError("Code", "Mã thành phần lương không được thay đổi sau khi lưu"));

            // BR-10: TPL kế thừa hệ thống — Code đã chặn ở trên (hard), các field còn lại chặn động theo LockedFields snapshot.
            if (existing?.Source == SalaryCompositionSource.InheritedFromSystem)
            {
                var locked = GetLockedFieldChanges(entity, existing);
                if (locked.Count > 0)
                    errors.Add(new ValidationError("LockedFields",
                        $"Thành phần lương kế thừa từ hệ thống không cho sửa: {string.Join(", ", locked)}"));
            }

            errors.AddRange(await ValidateAndNormalizeOrganizationIDsAsync(entity));

            var (activeCodes, allCodes, _) = await LoadCodeSnapshotAsync();
            var effectiveActive = entity.IsSkipUnfollowedComposition ? allCodes : activeCodes;
            var (formulaErrors, _) = CollectFormulaWarnings(entity, effectiveActive, allCodes);
            errors.AddRange(formulaErrors);

            return errors;
        }

        /// <summary>BR-08: không xóa TPL hệ thống. BR-11: không xóa TPL đang được tham chiếu.</summary>
        protected override async Task<bool> ValidateBeforeDeleteAsync(Guid entityId)
        {
            var entity = await _baseRepository.GetEntityByIDAsync(entityId);
            if (entity == null) return true;
            if (entity.Source == SalaryCompositionSource.InheritedFromSystem) return false;

            var all = (await _baseRepository.GetEntitiesAsync())
                .Cast<SalaryCompositionEntity>()
                .Where(e => e.SalaryCompositionID != entityId)
                .ToList();

            var allFormulas = CollectFormulas(all);
            return !IsCodeReferencedInFormulas(entity.Code, allFormulas);
        }

        protected override async Task<string?> GetDeleteValidationMessageAsync(Guid entityId)
        {
            var entity = await _baseRepository.GetEntityByIDAsync(entityId);
            if (entity == null) return null;

            return entity.Source == SalaryCompositionSource.InheritedFromSystem
                ? "Không thể xóa thành phần lương mặc định của hệ thống"
                : $"Không thể xóa thành phần lương '{entity.Name}' vì đang được sử dụng trong công thức của thành phần lương khác";
        }

        #endregion

        #region Private helpers

        /// <summary>
        /// Kiểm tra công thức có chứa mã TPL ngừng theo dõi không.
        /// Trả về response 202 (cần xác nhận) nếu có; null nếu OK để tiếp tục lưu.
        /// </summary>
        private async Task<ServiceResponse?> CheckUnfollowedConfirmationAsync(SalaryCompositionEntity entity)
        {
            if (entity.IsSkipUnfollowedComposition) return null;

            NormalizeFormulas(entity);
            var (activeCodes, allCodes, nameMap) = await LoadCodeSnapshotAsync();
            var (_, inactiveCodes) = CollectFormulaWarnings(entity, activeCodes, allCodes);

            return inactiveCodes.Count > 0
                ? BuildConfirmationRequiredResponse(inactiveCodes, nameMap)
                : null;
        }

        /// <summary>
        /// Map TPL hệ thống sang entity TPL đơn vị (Source = InheritedFromSystem).
        /// Caller phải đảm bảo <c>system.ComponentTypeID</c> không null — không fallback Guid.Empty để tránh data rác.
        /// </summary>
        private static SalaryCompositionEntity MapSystemToComposition(
            Entities.SalaryCompositionSystem.SalaryCompositionSystem system,
            List<Guid> organizationIds) => new()
        {
            Code                = system.Code,
            Name                = system.Name,
            ComponentTypeID     = system.ComponentTypeID!.Value,
            SystemCompositionID = system.SystemCompositionID,
            Nature              = system.Nature,
            TaxType             = system.TaxType,
            ValueType           = system.ValueType,
            NormFormula         = system.NormFormula,
            TaxableFormula      = system.TaxableFormula,
            ExemptFormula       = system.ExemptFormula,
            Description         = system.Description,
            Source              = SalaryCompositionSource.InheritedFromSystem,
            Status              = SalaryCompositionStatus.Active,
            OrganizationIDs     = organizationIds,
            LockedFields        = system.LockedFields,
        };

        private async Task<List<Guid>> GetRootOrganizationIdsAsync()
            => (await _organizationRepository.GetEntitiesAsync())
                .Cast<Organization>()
                .Where(o => !o.IsDeleted && o.ParentID == null)
                .Select(o => o.OrganizationID)
                .ToList();

        /// <summary>
        /// BR-10: lấy tên hiển thị các field bị khóa mà user cố thay đổi.
        /// Đọc snapshot LockedFields (JSON array of int) trên existing → so sánh từng field tương ứng.
        /// Code không nằm trong enum LockableField vì đã chặn cứng bên ngoài.
        /// </summary>
        /// <remarks>Created by: ntdo — 05/06/2026</remarks>
        private static List<string> GetLockedFieldChanges(SalaryCompositionEntity e, SalaryCompositionEntity existing)
        {
            var lockedIds = ParseLockedFieldIds(existing.LockedFields);
            if (lockedIds.Count == 0) return new List<string>();

            var changed = new List<string>();

            // Mỗi case so sánh đúng property tương ứng với LockableField ID.
            // Khi rename property C#, IDE Rename tự fix — DB lưu int nên không cần migrate.
            foreach (var id in lockedIds)
            {
                bool isChanged = id switch
                {
                    (int)LockableField.ComponentTypeID        => e.ComponentTypeID        != existing.ComponentTypeID,
                    (int)LockableField.Nature                 => e.Nature                 != existing.Nature,
                    (int)LockableField.TaxType                => e.TaxType                != existing.TaxType,
                    (int)LockableField.TaxDeductible          => e.TaxDeductible          != existing.TaxDeductible,
                    (int)LockableField.ValueType              => e.ValueType              != existing.ValueType,
                    (int)LockableField.ValueMode              => e.ValueMode              != existing.ValueMode,
                    (int)LockableField.ValueFormula           => e.ValueFormula           != existing.ValueFormula,
                    (int)LockableField.ValueScope             => e.ValueScope             != existing.ValueScope,
                    (int)LockableField.ValueScopeLevel        => e.ValueScopeLevel        != existing.ValueScopeLevel,
                    (int)LockableField.SumSourceCompositionID => e.SumSourceCompositionID != existing.SumSourceCompositionID,
                    (int)LockableField.NormFormula            => e.NormFormula            != existing.NormFormula,
                    (int)LockableField.TaxableFormula         => e.TaxableFormula         != existing.TaxableFormula,
                    (int)LockableField.ExemptFormula          => e.ExemptFormula          != existing.ExemptFormula,
                    (int)LockableField.AllowExceedNorm        => e.AllowExceedNorm        != existing.AllowExceedNorm,
                    (int)LockableField.HideWhenZero           => e.HideWhenZero           != existing.HideWhenZero,
                    (int)LockableField.Name                   => e.Name                   != existing.Name,
                    (int)LockableField.Description            => e.Description            != existing.Description,
                    (int)LockableField.ShowOnPayslip          => e.ShowOnPayslip          != existing.ShowOnPayslip,
                    (int)LockableField.OrganizationIDs        => !AreOrganizationIdsEqual(e.OrganizationIDs, existing.OrganizationIDs),
                    (int)LockableField.Status                 => e.Status                 != existing.Status,
                    _                                         => false,
                };

                if (isChanged) changed.Add(LockableFieldRegistry.GetDisplayName(id));
            }

            return changed;
        }

        /// <summary>Parse JSON array of int — robust: trả empty set nếu null/rỗng/sai format.</summary>
        private static HashSet<int> ParseLockedFieldIds(string? json)
        {
            if (string.IsNullOrWhiteSpace(json)) return new HashSet<int>();
            try
            {
                return JsonSerializer.Deserialize<HashSet<int>>(json) ?? new HashSet<int>();
            }
            catch (JsonException)
            {
                return new HashSet<int>();
            }
        }

        /// <summary>So sánh 2 danh sách OrganizationID không quan tâm thứ tự.</summary>
        private static bool AreOrganizationIdsEqual(List<Guid>? a, List<Guid>? b)
        {
            var aSet = a is null ? new HashSet<Guid>() : new HashSet<Guid>(a);
            var bSet = b is null ? new HashSet<Guid>() : new HashSet<Guid>(b);
            return aSet.SetEquals(bSet);
        }

        /// <summary>
        /// Snapshot codes (active, all) + map Code → Name. Một query DB duy nhất, tính 3 collection từ cùng nguồn.
        /// </summary>
        private async Task<(HashSet<string> Active, HashSet<string> All, Dictionary<string, string> NameMap)>
            LoadCodeSnapshotAsync()
        {
            var all = (await _baseRepository.GetEntitiesAsync())
                .Cast<SalaryCompositionEntity>()
                .ToList();

            var active  = all.Where(e => e.Status == SalaryCompositionStatus.Active)
                             .Select(e => e.Code)
                             .ToHashSet(StringComparer.Ordinal);
            var allCodes = all.Select(e => e.Code).ToHashSet(StringComparer.Ordinal);
            var nameMap  = all.ToDictionary(e => e.Code, e => e.Name, StringComparer.Ordinal);

            return (active, allCodes, nameMap);
        }

        /// <summary>Trim whitespace đầu/cuối cho cả 4 trường công thức. Giữ nguyên dấu "=" nếu có.</summary>
        private static void NormalizeFormulas(SalaryCompositionEntity entity)
        {
            entity.ValueFormula   = entity.ValueFormula?.Trim();
            entity.NormFormula    = entity.NormFormula?.Trim();
            entity.TaxableFormula = entity.TaxableFormula?.Trim();
            entity.ExemptFormula  = entity.ExemptFormula?.Trim();
        }

        /// <summary>Lấy phần biểu thức không có dấu "=" đầu — dùng khi build biểu thức toán học.</summary>
        private static string StripEquals(string formula)
            => formula.StartsWith('=') ? formula[1..].TrimStart() : formula;

        /// <summary>
        /// Khi PartiallyExempt và ValueFormula tồn tại: tự động tính phần còn lại
        /// = ValueFormula - phần đã truyền. Gọi SAU NormalizeFormulas.
        /// existingSource: truyền vào khi update để re-derive đúng khi ValueFormula thay đổi.
        /// </summary>
        private static void AutoDerivePartialExemptFormulas(SalaryCompositionEntity entity, TaxFormulaSource existingSource = TaxFormulaSource.None)
        {
            if (entity.TaxType != SalaryTaxType.PartiallyExempt)
            {
                entity.TaxFormulaSource = TaxFormulaSource.None;
                return;
            }
            if (string.IsNullOrWhiteSpace(entity.ValueFormula)) return;

            var hasTaxable = !string.IsNullOrWhiteSpace(entity.TaxableFormula);
            var hasExempt  = !string.IsNullOrWhiteSpace(entity.ExemptFormula);

            // Khi update: nếu bản ghi cũ đã có formula được tự suy, luôn re-derive để theo kịp ValueFormula mới
            if (existingSource == TaxFormulaSource.ExemptDerived && hasTaxable)
            {
                entity.ExemptFormula    = $"({StripEquals(entity.ValueFormula)}) - ({StripEquals(entity.TaxableFormula)})";
                entity.TaxFormulaSource = TaxFormulaSource.ExemptDerived;
                return;
            }
            if (existingSource == TaxFormulaSource.TaxableDerived && hasExempt)
            {
                entity.TaxableFormula   = $"({StripEquals(entity.ValueFormula)}) - ({StripEquals(entity.ExemptFormula)})";
                entity.TaxFormulaSource = TaxFormulaSource.TaxableDerived;
                return;
            }

            // Insert hoặc thay đổi nguồn: suy từ cái nào đang null
            if (hasTaxable && !hasExempt)
            {
                entity.ExemptFormula    = $"({StripEquals(entity.ValueFormula)}) - ({StripEquals(entity.TaxableFormula)})";
                entity.TaxFormulaSource = TaxFormulaSource.ExemptDerived;
            }
            else if (hasExempt && !hasTaxable)
            {
                entity.TaxableFormula   = $"({StripEquals(entity.ValueFormula)}) - ({StripEquals(entity.ExemptFormula)})";
                entity.TaxFormulaSource = TaxFormulaSource.TaxableDerived;
            }
            else
            {
                entity.TaxFormulaSource = TaxFormulaSource.None;
            }
        }

        /// <summary>Validate cả 4 công thức: trả lỗi cứng + danh sách mã ngừng theo dõi (đã dedup).</summary>
        private static (List<ValidationError> Errors, List<string> InactiveCodes) CollectFormulaWarnings(
            SalaryCompositionEntity entity, HashSet<string> activeCodes, HashSet<string> allCodes)
        {
            var errors = new List<ValidationError>();
            var inactiveCodes = new HashSet<string>(StringComparer.Ordinal);

            void Check(string? formula, string field, string label)
            {
                var result = FormulaValidator.Validate(formula, activeCodes, allCodes);

                if (result.MissingCodes.Count > 0)
                {
                    var distinctCodes = result.MissingCodes
                        .Select(m => m.Code)
                        .Distinct(StringComparer.Ordinal)
                        .ToList();
                    var msg = result.Error != null
                        ? $"{label} không hợp lệ: {result.Error}; có {distinctCodes.Count} mã không tồn tại: {string.Join(", ", distinctCodes)}"
                        : $"{label} có {distinctCodes.Count} mã thành phần lương không tồn tại: {string.Join(", ", distinctCodes)}";
                    errors.Add(new ValidationError(field, msg) { MissingCodes = result.MissingCodes });
                }
                else if (!result.IsValid)
                {
                    errors.Add(new ValidationError(field, $"{label} không hợp lệ: {result.Error}"));
                }

                foreach (var code in result.InactiveCodes) inactiveCodes.Add(code);
            }

            Check(entity.ValueFormula,   nameof(entity.ValueFormula),   "Công thức giá trị");
            Check(entity.NormFormula,    nameof(entity.NormFormula),    "Công thức định mức");
            Check(entity.TaxableFormula, nameof(entity.TaxableFormula), "Công thức phần chịu thuế");
            Check(entity.ExemptFormula,  nameof(entity.ExemptFormula),  "Công thức phần miễn thuế");

            return (errors, inactiveCodes.ToList());
        }

        private static ServiceResponse BuildConfirmationRequiredResponse(
            List<string> inactiveCodes, Dictionary<string, string> nameMap) => new()
        {
            IsSuccess = false,
            Code = (int)ResponseCode.ConfirmationRequired,
            Data = new FormulaConfirmationRequired
            {
                InactiveCodes = inactiveCodes
                    .Select(c => new InactiveCodeInfo(c, nameMap.GetValueOrDefault(c, c)))
                    .ToList()
            },
            UserMessage = $"Công thức có {inactiveCodes.Count} thành phần lương đang ngừng theo dõi: "
                        + string.Join(", ", inactiveCodes)
                        + ". Bạn có chắc chắn muốn lưu không?"
        };

        /// <summary>Bắt buộc OrganizationIDs + bottom-up roll-up nếu đủ children của 1 cha.</summary>
        private async Task<List<ValidationError>> ValidateAndNormalizeOrganizationIDsAsync(SalaryCompositionEntity entity)
        {
            if (entity.OrganizationIDs == null || entity.OrganizationIDs.Count == 0)
                return new() { new ValidationError("OrganizationIDs", "Đơn vị áp dụng không được để trống") };

            entity.OrganizationIDs = await NormalizeOrganizationIDsAsync(entity.OrganizationIDs);
            return new List<ValidationError>();
        }

        private async Task<List<Guid>> NormalizeOrganizationIDsAsync(List<Guid> inputIds)
        {
            var allOrgs = (await _organizationRepository.GetEntitiesAsync())
                .Cast<Organization>()
                .Where(o => !o.IsDeleted)
                .ToList();

            var childrenOf = allOrgs
                .Where(o => o.ParentID.HasValue)
                .GroupBy(o => o.ParentID!.Value)
                .ToDictionary(g => g.Key, g => g.Select(o => o.OrganizationID).ToHashSet());

            var result = new HashSet<Guid>(inputIds);

            bool changed = true;
            while (changed)
            {
                changed = false;
                foreach (var (parentId, children) in childrenOf)
                {
                    if (children.Count > 0 && children.All(c => result.Contains(c)))
                    {
                        foreach (var child in children) result.Remove(child);
                        result.Add(parentId);
                        changed = true;
                        break; // set đã thay đổi, restart vòng lặp
                    }
                }
            }

            return result.ToList();
        }

        /// <summary>
        /// Pattern khớp identifier trong công thức (chữ/số/_, bắt đầu bằng chữ hoặc _).
        /// </summary>
        private static readonly Regex _identifierPattern =
            new(@"[A-Za-z_][A-Za-z0-9_]*", RegexOptions.Compiled);

        /// <summary>
        /// Build map Code → danh sách TPL đang tham chiếu mã đó trong bất kỳ công thức nào.
        /// O(N × số_công_thức) regex; tránh O(N²) khi mỗi entity lại scan toàn bộ all.
        /// Lọc bỏ tên hàm dựng sẵn (SUM, IF, ...) khỏi index để tránh false-positive với Code legacy.
        /// </summary>
        private static Dictionary<string, List<(Guid Id, string Code, string Name)>>
            BuildReferencerIndex(List<SalaryCompositionEntity> all)
        {
            var index = new Dictionary<string, List<(Guid Id, string Code, string Name)>>(StringComparer.Ordinal);

            foreach (var e in all)
            {
                var tokens = new HashSet<string>(StringComparer.Ordinal);
                foreach (var formula in new[] { e.ValueFormula, e.NormFormula, e.TaxableFormula, e.ExemptFormula })
                {
                    if (string.IsNullOrWhiteSpace(formula)) continue;
                    foreach (Match m in _identifierPattern.Matches(formula))
                    {
                        // Bỏ qua tên hàm dựng sẵn — không phải Code tham chiếu
                        if (FormulaValidator.AllowedFunctions.Contains(m.Value)) continue;
                        tokens.Add(m.Value);
                    }
                }

                foreach (var token in tokens)
                {
                    if (!index.TryGetValue(token, out var list))
                        index[token] = list = new List<(Guid, string, string)>();
                    list.Add((e.SalaryCompositionID, e.Code, e.Name));
                }
            }

            return index;
        }

        private static List<string?> CollectFormulas(IEnumerable<SalaryCompositionEntity> entities)
            => entities
                .SelectMany(e => new[] { e.ValueFormula, e.NormFormula, e.TaxableFormula, e.ExemptFormula })
                .Where(f => !string.IsNullOrWhiteSpace(f))
                .ToList();

        /// <summary>
        /// Word-boundary regex để tránh false positive (ví dụ LUONG khớp trong LUONG_CO_BAN).
        /// Code trùng tên hàm dựng sẵn (đã bị ValidateCustom reject) thì coi như không tham chiếu —
        /// bảo vệ data legacy nếu lỡ tồn tại trước khi rule có hiệu lực.
        /// </summary>
        private static bool IsCodeReferencedInFormulas(string code, List<string?> allFormulas)
        {
            if (FormulaValidator.AllowedFunctions.Contains(code)) return false;
            var pattern = $@"(?<![A-Za-z0-9_]){Regex.Escape(code)}(?![A-Za-z0-9_])";
            return allFormulas.Any(f => Regex.IsMatch(f!, pattern));
        }

        /// <summary>Helper local — wrap BuildPagingResponse trong BaseService.</summary>
        private ServiceResponse CreatePagingResponse(long total, int pageIndex, int pageSize,
            IEnumerable<SalaryCompositionEntity> data)
            => CreateSuccessResponse(new PagingResponse<SalaryCompositionEntity>
            {
                Total       = total,
                PageSize    = pageSize,
                CurrentPage = pageIndex,
                PageCount   = (long)Math.Ceiling((double)total / pageSize),
                Data        = data.ToList()
            });

        #endregion
    }
}
