using FresherMisa2026.Application.Interfaces.Repositories;
using FresherMisa2026.Application.Interfaces.Services;
using FresherMisa2026.Entities;
using FresherMisa2026.Entities.Enums;
using FresherMisa2026.Entities.Organization;
using FresherMisa2026.Entities.SalaryComposition.DTO;
using System.Text.RegularExpressions;
using SalaryCompositionEntity = FresherMisa2026.Entities.SalaryComposition.SalaryComposition;
using SalaryCompositionSystemEntity = FresherMisa2026.Entities.SalaryCompositionSystem.SalaryCompositionSystem;
using SalaryCompositionSuggestion = FresherMisa2026.Entities.SalaryComposition.DTO.SalaryCompositionSuggestion;
using InactiveCodeInfo = FresherMisa2026.Entities.SalaryComposition.DTO.InactiveCodeInfo;

namespace FresherMisa2026.Application.Services
{
    /// <summary>
    /// Service cho SalaryComposition
    /// Created By: Nguyen Thiet Do (2026-05-27)
    /// </summary>
    public class SalaryCompositionService : BaseService<SalaryCompositionEntity>, ISalaryCompositionService
    {
        #region Declare

        private readonly ISalaryCompositionRepository _salaryCompositionRepository;
        private readonly ISalaryCompositionSystemRepository _systemRepository;
        private readonly IOrganizationRepository _organizationRepository;

        #endregion

        #region Constructer

        public SalaryCompositionService(
            ISalaryCompositionRepository salaryCompositionRepository,
            ISalaryCompositionSystemRepository systemRepository,
            IOrganizationRepository organizationRepository)
            : base(salaryCompositionRepository)
        {
            _salaryCompositionRepository = salaryCompositionRepository;
            _systemRepository = systemRepository;
            _organizationRepository = organizationRepository;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Override InsertAsync — nếu công thức chứa TPL ngừng theo dõi và IsSkipUnfollowedComposition=false,
        /// trả về response yêu cầu xác nhận thay vì lưu.
        /// </summary>
        /// Created By: Nguyen Thiet Do (2026-06-01)
        public override async Task<ServiceResponse> InsertAsync(SalaryCompositionEntity entity)
        {
            if (!entity.IsSkipUnfollowedComposition)
            {
                NormalizeFormulas(entity);
                var (activeCodes, allCodes, nameMap) = await GetCodesWithNamesAsync();
                var (_, inactiveCodes) = CollectFormulaWarnings(entity, activeCodes, allCodes);
                if (inactiveCodes.Count > 0)
                    return BuildConfirmationRequiredResponse(inactiveCodes, nameMap);
            }
            return await base.InsertAsync(entity);
        }

        /// <summary>
        /// Override UpdateAsync — tương tự InsertAsync.
        /// </summary>
        /// Created By: Nguyen Thiet Do (2026-06-01)
        public override async Task<ServiceResponse> UpdateAsync(Guid id, SalaryCompositionEntity entity)
        {
            if (!entity.IsSkipUnfollowedComposition)
            {
                NormalizeFormulas(entity);
                var (activeCodes, allCodes, nameMap) = await GetCodesWithNamesAsync();
                var (_, inactiveCodes) = CollectFormulaWarnings(entity, activeCodes, allCodes);
                if (inactiveCodes.Count > 0)
                    return BuildConfirmationRequiredResponse(inactiveCodes, nameMap);
            }
            return await base.UpdateAsync(id, entity);
        }

        /// <summary>
        /// Chuyển TPL hệ thống sang TPL đơn vị — map field và insert qua pipeline bình thường
        /// </summary>
        /// Created By: Nguyen Thiet Do (2026-05-27)
        public async Task<ServiceResponse> InheritFromSystemAsync(Guid systemCompositionId, List<Guid>? organizationIds)
        {
            var system = await _systemRepository.GetEntityByIDAsync(systemCompositionId);
            if (system == null)
                return CreateErrorResponse(ResponseCode.NotFound, "Không tìm thấy thành phần lương hệ thống");

            var resolvedOrgIds = (organizationIds == null || organizationIds.Count == 0)
                ? await GetRootOrganizationIdsAsync()
                : organizationIds;

            var composition = new SalaryCompositionEntity
            {
                Code                = system.Code,
                Name                = system.Name,
                ComponentTypeID     = system.ComponentTypeID ?? Guid.Empty,
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
                OrganizationIDs     = resolvedOrgIds,
            };

            return await InsertAsync(composition);
        }

        private async Task<List<Guid>> GetRootOrganizationIdsAsync()
        {
            var allOrgs = (await _organizationRepository.GetEntitiesAsync())
                .Cast<Organization>()
                .Where(o => !o.IsDeleted && o.ParentID == null)
                .Select(o => o.OrganizationID)
                .ToList();
            return allOrgs;
        }

        /// <summary>
        /// Chuyển nhiều TPL hệ thống sang TPL đơn vị — tiếp tục kể cả khi một số ID thất bại
        /// </summary>
        /// Created By: Nguyen Thiet Do (2026-05-27)
        public async Task<ServiceResponse> InheritFromSystemBatchAsync(InheritFromSystemBatchRequest request)
        {
            if (request.SystemCompositionIds == null || request.SystemCompositionIds.Count == 0)
                return CreateErrorResponse(ResponseCode.BadRequest, "Danh sách Id không được rỗng");

            var result = new BulkInheritResult();

            foreach (var systemId in request.SystemCompositionIds)
            {
                try
                {
                    var response = await InheritFromSystemAsync(systemId, request.OrganizationIDs);
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
                    result.Failed.Add(new BulkDeleteFailedItem(systemId, ex.Message));
                }
            }

            return CreateSuccessResponse(result);
        }

        /// <summary>
        /// Chuyển trạng thái theo dõi của TPL — ghi thẳng qua PatchFieldAsync, không qua full validation
        /// </summary>
        /// Created By: Nguyen Thiet Do (2026-05-27)
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

        public async Task<ServiceResponse> SetStatusBulkAsync(List<Guid> ids, SalaryCompositionStatus status)
        {
            if (ids == null || ids.Count == 0)
                return CreateErrorResponse(ResponseCode.BadRequest, "Danh sách Id không được rỗng");

            var result = new BulkDeleteResult();

            foreach (var id in ids)
            {
                var response = await SetStatusAsync(id, status);
                if (response.IsSuccess)
                    result.Succeeded.Add(id);
                else
                    result.Failed.Add(new BulkDeleteFailedItem(id,
                        response.DevMessage?.ToString() ?? "Không tìm thấy thành phần lương"));
            }

            return CreateSuccessResponse(result);
        }

        /// <summary>
        /// Lấy danh sách gợi ý Code/Name/Description cho ô nhập công thức — chỉ TPL đang theo dõi
        /// </summary>
        /// Created By: Nguyen Thiet Do (2026-05-27)
        public async Task<ServiceResponse> GetSuggestionsAsync(string? search)
        {
            var all = await _baseRepository.GetEntitiesAsync();

            var query = all
                .Cast<SalaryCompositionEntity>()
                .Where(e => e.Status == SalaryCompositionStatus.Active);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(e =>
                    e.Code.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    e.Name.Contains(term, StringComparison.OrdinalIgnoreCase));
            }

            var suggestions = query
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

        /// <summary>
        /// Lọc nâng cao 4 phần: search (mã/tên), trạng thái, đơn vị, field conditions
        /// </summary>
        public async Task<ServiceResponse> AdvancedFilterAsync(SalaryCompositionAdvancedFilterRequest request)
        {
            var (data, total) = await _salaryCompositionRepository.AdvancedFilterAsync(request);
            return CreatePagingResponse(data, total, request.PageIndex, request.PageSize);
        }

        public async Task<ServiceResponse> AdvancedFilterWithProcAsync(SalaryCompositionAdvancedFilterRequest request)
        {
            var (data, total) = await _salaryCompositionRepository.AdvancedFilterWithProcAsync(request);
            return CreatePagingResponse(data, total, request.PageIndex, request.PageSize);
        }

        private ServiceResponse CreatePagingResponse(IEnumerable<SalaryCompositionEntity> data, long total, int pageIndex, int pageSize)
            => CreateSuccessResponse(new PagingResponse<SalaryCompositionEntity>
            {
                Data        = data.ToList(),
                Total       = total,
                PageSize    = pageSize,
                CurrentPage = pageIndex,
                PageCount   = (long)Math.Ceiling((double)total / pageSize)
            });

        /// <summary>
        /// Phân loại danh sách TPL thành 3 nhóm trước khi xóa / ngừng theo dõi hàng loạt.
        /// Ưu tiên phân loại: DataExist (đang dùng trong CT) > DataSystem (hệ thống) > DataNotExist (an toàn).
        /// Dùng cache — không thêm DB round-trip.
        /// </summary>
        /// Created By: Nguyen Thiet Do (2026-06-02)
        public async Task<ServiceResponse> ExitDataAsync(ExitDataRequest request)
        {
            if (request.Ids == null || request.Ids.Count == 0)
                return CreateErrorResponse(ResponseCode.BadRequest, "Danh sách Id không được rỗng");

            var all = (await _baseRepository.GetEntitiesAsync())
                .Cast<SalaryCompositionEntity>()
                .ToList();

            var idMap = all.ToDictionary(e => e.SalaryCompositionID);

            // Gom tất cả nội dung công thức một lần để tránh lặp nhiều lần
            var allFormulas = all
                .SelectMany(e => new[] { e.ValueFormula, e.NormFormula, e.TaxableFormula, e.ExemptFormula })
                .Where(f => !string.IsNullOrWhiteSpace(f))
                .ToList();

            // Phân trang trên danh sách ID đầu vào
            var total = request.Ids.Count;
            var pageIds = request.Ids
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            var result = new ExitDataResult
            {
                Total    = total,
                PageSize = request.PageSize,
                CurrentPage = request.PageIndex,
                PageCount   = (long)Math.Ceiling((double)total / request.PageSize)
            };

            foreach (var id in pageIds)
            {
                if (!idMap.TryGetValue(id, out var entity)) continue;

                if (IsCodeReferencedInFormulas(entity.Code, allFormulas))
                    result.DataExist.Add(entity);
                else if (entity.Source == SalaryCompositionSource.InheritedFromSystem)
                    result.DataSystem.Add(entity);
                else
                    result.DataNotExist.Add(entity);
            }

            return CreateSuccessResponse(result);
        }

        /// <summary>
        /// Kiểm tra code TPL có xuất hiện dưới dạng định danh độc lập trong bất kỳ công thức nào không.
        /// Dùng word-boundary regex để tránh false positive (ví dụ LUONG khớp trong LUONG_CO_BAN).
        /// </summary>
        private static bool IsCodeReferencedInFormulas(string code, List<string?> allFormulas)
        {
            var pattern = $@"(?<![A-Za-z0-9_]){Regex.Escape(code)}(?![A-Za-z0-9_])";
            return allFormulas.Any(f => Regex.IsMatch(f!, pattern));
        }

        /// <summary>
        /// Lọc thành phần lương theo nhiều điều kiện có phân trang
        /// </summary>
        /// Created By: Nguyen Thiet Do (2026-05-27)
        public async Task<ServiceResponse> FilterAsync(SalaryCompositionFilterRequest request)
        {
            var (data, total) = await _salaryCompositionRepository.FilterAsync(request);

            var pagingResponse = new PagingResponse<SalaryCompositionEntity>
            {
                Data = data.ToList(),
                Total = total,
                PageSize = request.PageSize,
                CurrentPage = request.PageIndex,
                PageCount = (long)Math.Ceiling((double)total / request.PageSize)
            };

            return CreateSuccessResponse(pagingResponse);
        }

        #endregion

        #region OVERRIDE METHODS

        private static readonly Regex _codePattern = new(@"^[A-Za-z0-9_]+$", RegexOptions.Compiled);
        private const int MaxCodeLength = 255;
        private const int MaxNameLength = 255;

        /// <summary>
        /// Validate tùy chỉnh — BR-03: max length, format Code, BR-05: TaxType/Nature
        /// </summary>
        protected override List<ValidationError> ValidateCustom(SalaryCompositionEntity entity)
        {
            var errors = new List<ValidationError>();

            // BR-03: max length
            if (!string.IsNullOrEmpty(entity.Code) && entity.Code.Length > MaxCodeLength)
                errors.Add(new ValidationError("Code",
                    $"Mã thành phần không được vượt quá {MaxCodeLength} ký tự"));

            if (!string.IsNullOrEmpty(entity.Name) && entity.Name.Length > MaxNameLength)
                errors.Add(new ValidationError("Name",
                    $"Tên thành phần không được vượt quá {MaxNameLength} ký tự"));

            // Format Code — chỉ check khi trong giới hạn max length
            if (!string.IsNullOrEmpty(entity.Code) && entity.Code.Length <= MaxCodeLength)
            {
                if (!_codePattern.IsMatch(entity.Code))
                    errors.Add(new ValidationError("Code",
                        "Mã thành phần chỉ được chứa chữ cái (A-Z, a-z), số (0-9) và dấu gạch dưới (_)"));

                else if (decimal.TryParse(entity.Code, System.Globalization.NumberStyles.Any,
                             System.Globalization.CultureInfo.InvariantCulture, out _))
                    errors.Add(new ValidationError("Code",
                        "Mã thành phần không được là một số thực"));
            }

            // BR-05: TaxType chỉ có ý nghĩa khi Nature = Thu nhập
            if (entity.TaxType.HasValue && entity.Nature != SalaryNature.Income)
                errors.Add(new ValidationError("TaxType",
                    "Loại thuế TNCN chỉ áp dụng khi tính chất là Thu nhập"));

            // TaxableFormula / ExemptFormula chỉ dùng khi TaxType = PartiallyExempt
            if (entity.TaxType != SalaryTaxType.PartiallyExempt)
            {
                if (!string.IsNullOrWhiteSpace(entity.TaxableFormula))
                    errors.Add(new ValidationError("TaxableFormula",
                        "Công thức phần chịu thuế chỉ áp dụng khi loại thuế là 'Miễn một phần'"));
                if (!string.IsNullOrWhiteSpace(entity.ExemptFormula))
                    errors.Add(new ValidationError("ExemptFormula",
                        "Công thức phần miễn thuế chỉ áp dụng khi loại thuế là 'Miễn một phần'"));
            }
            else
            {
                if (string.IsNullOrWhiteSpace(entity.TaxableFormula) && string.IsNullOrWhiteSpace(entity.ExemptFormula))
                    errors.Add(new ValidationError("TaxableFormula",
                        "Cần nhập ít nhất một trong hai: công thức phần chịu thuế hoặc công thức phần miễn thuế"));
            }

            return errors;
        }

        /// <summary>
        /// Validate trước khi thêm — kiểm tra OrganizationIDs bắt buộc + normalize + kiểm tra công thức
        /// </summary>
        protected override async Task<List<ValidationError>> ValidateBeforeInsertAsync(SalaryCompositionEntity entity)
        {
            var errors = new List<ValidationError>();

            NormalizeFormulas(entity);

            var orgErrors = await ValidateAndNormalizeOrganizationIDsAsync(entity);
            errors.AddRange(orgErrors);

            var (activeCodes, allCodes) = await GetCodesAsync();
            var effectiveActiveCodes = entity.IsSkipUnfollowedComposition ? allCodes : activeCodes;
            var (formulaErrors, _) = CollectFormulaWarnings(entity, effectiveActiveCodes, allCodes);
            errors.AddRange(formulaErrors);

            return errors;
        }

        /// <summary>
        /// Validate trước khi update — BR-01: Code không đổi · BR-10: TPL hệ thống chỉ sửa được 5 field
        /// </summary>
        protected override async Task<List<ValidationError>> ValidateBeforeUpdateAsync(Guid entityId, SalaryCompositionEntity entity)
        {
            var errors = new List<ValidationError>();

            NormalizeFormulas(entity);

            var existing = await _baseRepository.GetEntityByIDAsync(entityId);

            if (existing != null && existing.Code != entity.Code)
                errors.Add(new ValidationError("Code", "Mã thành phần lương không được thay đổi sau khi lưu"));

            // BR-10: TPL kế thừa từ hệ thống — chỉ được sửa Name, OrganizationIDs, Description, ShowOnPayslip, Status
            if (existing?.Source == SalaryCompositionSource.InheritedFromSystem)
            {
                var locked = GetLockedFieldChanges(entity, existing);
                if (locked.Count > 0)
                    errors.Add(new ValidationError("Source",
                        "Thành phần lương kế thừa từ hệ thống chỉ được sửa: Tên, Đơn vị áp dụng, Mô tả, Hiển thị trên phiếu lương, Trạng thái. "
                        + $"Không thể sửa: {string.Join(", ", locked)}"));
            }

            var orgErrors = await ValidateAndNormalizeOrganizationIDsAsync(entity);
            errors.AddRange(orgErrors);

            var (activeCodes, allCodes) = await GetCodesAsync();
            var effectiveActiveCodes = entity.IsSkipUnfollowedComposition ? allCodes : activeCodes;
            var (formulaErrors, _) = CollectFormulaWarnings(entity, effectiveActiveCodes, allCodes);
            errors.AddRange(formulaErrors);

            return errors;
        }

        /// <summary>
        /// Validate trước khi xóa — BR-08: Không xóa TPL kế thừa từ hệ thống;
        /// BR-11: Không xóa TPL đang được tham chiếu trong công thức của TPL khác.
        /// </summary>
        protected override async Task<bool> ValidateBeforeDeleteAsync(Guid entityId)
        {
            var entity = await _baseRepository.GetEntityByIDAsync(entityId);
            if (entity == null) return true;

            if (entity.Source == SalaryCompositionSource.InheritedFromSystem) return false;

            var all = (await _baseRepository.GetEntitiesAsync())
                .Cast<SalaryCompositionEntity>()
                .Where(e => e.SalaryCompositionID != entityId)
                .ToList();

            var allFormulas = all
                .SelectMany(e => new[] { e.ValueFormula, e.NormFormula, e.TaxableFormula, e.ExemptFormula })
                .Where(f => !string.IsNullOrWhiteSpace(f))
                .ToList();

            return !IsCodeReferencedInFormulas(entity.Code, allFormulas);
        }

        protected override async Task<string?> GetDeleteValidationMessageAsync(Guid entityId)
        {
            var entity = await _baseRepository.GetEntityByIDAsync(entityId);
            if (entity == null) return null;

            if (entity.Source == SalaryCompositionSource.InheritedFromSystem)
                return "Không thể xóa thành phần lương mặc định của hệ thống";

            return $"Không thể xóa thành phần lương '{entity.Name}' vì đang được sử dụng trong công thức của thành phần lương khác";
        }

        #endregion

        #region Private helpers

        /// <summary>
        /// BR-10: Trả về tên hiển thị các field bị khóa mà người dùng cố thay đổi trên TPL hệ thống.
        /// Chỉ cho phép sửa: Name, OrganizationIDs, Description, ShowOnPayslip, Status.
        /// </summary>
        private static List<string> GetLockedFieldChanges(SalaryCompositionEntity e, SalaryCompositionEntity existing)
        {
            var changed = new List<string>();
            if (e.ComponentTypeID != existing.ComponentTypeID)             changed.Add("Loại thành phần");
            if (e.Nature != existing.Nature)                               changed.Add("Tính chất");
            if (e.TaxType != existing.TaxType)                             changed.Add("Loại thuế TNCN");
            if (e.TaxDeductible != existing.TaxDeductible)                 changed.Add("Giảm trừ thuế");
            if (e.ValueType != existing.ValueType)                         changed.Add("Kiểu giá trị");
            if (e.ValueMode != existing.ValueMode)                         changed.Add("Chế độ tính");
            if (e.ValueFormula != existing.ValueFormula)                   changed.Add("Công thức giá trị");
            if (e.ValueScope != existing.ValueScope)                       changed.Add("Phạm vi cộng tổng");
            if (e.ValueScopeLevel != existing.ValueScopeLevel)             changed.Add("Cấp phạm vi");
            if (e.SumSourceCompositionID != existing.SumSourceCompositionID) changed.Add("TPL nguồn AutoSum");
            if (e.NormFormula != existing.NormFormula)                     changed.Add("Công thức định mức");
            if (e.TaxableFormula != existing.TaxableFormula)               changed.Add("Công thức phần chịu thuế");
            if (e.ExemptFormula != existing.ExemptFormula)                 changed.Add("Công thức phần miễn thuế");
            if (e.AllowExceedNorm != existing.AllowExceedNorm)             changed.Add("Cho phép vượt định mức");
            if (e.HideWhenZero != existing.HideWhenZero)                   changed.Add("Ẩn khi bằng 0");
            return changed;
        }

        private async Task<(HashSet<string> Active, HashSet<string> All)> GetCodesAsync()
        {
            var all = (await _baseRepository.GetEntitiesAsync())
                .Cast<SalaryCompositionEntity>()
                .ToList();

            var active = all
                .Where(e => e.Status == SalaryCompositionStatus.Active)
                .Select(e => e.Code)
                .ToHashSet(StringComparer.Ordinal);

            var allCodes = all
                .Select(e => e.Code)
                .ToHashSet(StringComparer.Ordinal);

            return (active, allCodes);
        }

        /// <summary>
        /// Như GetCodesAsync nhưng kèm map Code → Name để hiển thị tên trong cảnh báo xác nhận.
        /// </summary>
        private async Task<(HashSet<string> Active, HashSet<string> All, Dictionary<string, string> NameMap)> GetCodesWithNamesAsync()
        {
            var all = (await _baseRepository.GetEntitiesAsync())
                .Cast<SalaryCompositionEntity>()
                .ToList();

            var active = all
                .Where(e => e.Status == SalaryCompositionStatus.Active)
                .Select(e => e.Code)
                .ToHashSet(StringComparer.Ordinal);

            var allCodes = all
                .Select(e => e.Code)
                .ToHashSet(StringComparer.Ordinal);

            var nameMap = all
                .ToDictionary(e => e.Code, e => e.Name, StringComparer.Ordinal);

            return (active, allCodes, nameMap);
        }

        private static void NormalizeFormulas(SalaryCompositionEntity entity)
        {
            if (entity.ValueFormula?.StartsWith('=') == true)
                entity.ValueFormula = entity.ValueFormula[1..].TrimStart();
            if (entity.NormFormula?.StartsWith('=') == true)
                entity.NormFormula = entity.NormFormula[1..].TrimStart();
            if (entity.TaxableFormula?.StartsWith('=') == true)
                entity.TaxableFormula = entity.TaxableFormula[1..].TrimStart();
            if (entity.ExemptFormula?.StartsWith('=') == true)
                entity.ExemptFormula = entity.ExemptFormula[1..].TrimStart();
        }

        /// <summary>
        /// Validate tất cả công thức: trả về lỗi cứng (cú pháp / mã không tồn tại) và
        /// danh sách mã ngừng theo dõi (cảnh báo mềm, chưa có trong activeCodes).
        /// Các mã inactive có thể xuất hiện nhiều lần trong các công thức khác nhau — kết quả đã deduplicate.
        /// </summary>
        private static (List<ValidationError> Errors, List<string> InactiveCodes) CollectFormulaWarnings(
            SalaryCompositionEntity entity, HashSet<string> activeCodes, HashSet<string> allCodes)
        {
            var errors = new List<ValidationError>();
            var inactiveCodes = new HashSet<string>(StringComparer.Ordinal);

            void Check(string? formula, string field, string label)
            {
                var result = FormulaValidator.Validate(formula, activeCodes, allCodes);
                if (!result.IsValid)
                    errors.Add(new ValidationError(field, $"{label} không hợp lệ: {result.Error}"));
                foreach (var code in result.InactiveCodes)
                    inactiveCodes.Add(code);
            }

            Check(entity.ValueFormula,   nameof(entity.ValueFormula),   "Công thức giá trị");
            Check(entity.NormFormula,    nameof(entity.NormFormula),    "Công thức định mức");
            Check(entity.TaxableFormula, nameof(entity.TaxableFormula), "Công thức phần chịu thuế");
            Check(entity.ExemptFormula,  nameof(entity.ExemptFormula),  "Công thức phần miễn thuế");

            return (errors, inactiveCodes.ToList());
        }

        /// <summary>
        /// Tạo response yêu cầu xác nhận — isSuccess=false, code=200 (không phải lỗi, chỉ cần confirm).
        /// Data chứa danh sách TPL ngừng theo dõi kèm tên đầy đủ.
        /// </summary>
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

        /// <summary>
        /// Validate OrganizationIDs bắt buộc và normalize: nếu tất cả children của một cha đều có mặt
        /// thì gộp lại thành cha — lặp đến khi ổn định. Cập nhật entity.OrganizationIDs trực tiếp.
        /// </summary>
        private async Task<List<ValidationError>> ValidateAndNormalizeOrganizationIDsAsync(SalaryCompositionEntity entity)
        {
            if (entity.OrganizationIDs == null || entity.OrganizationIDs.Count == 0)
                return new List<ValidationError>
                {
                    new ValidationError("OrganizationIDs", "Đơn vị áp dụng không được để trống")
                };

            entity.OrganizationIDs = await NormalizeOrganizationIDsAsync(entity.OrganizationIDs);
            return new List<ValidationError>();
        }

        /// <summary>
        /// Rút gọn danh sách đơn vị bottom-up: nếu tất cả direct children của một cha đều có trong
        /// input thì thay thế bằng cha, lặp lại đến khi không còn thay đổi.
        /// </summary>
        private async Task<List<Guid>> NormalizeOrganizationIDsAsync(List<Guid> inputIds)
        {
            var allOrgs = (await _organizationRepository.GetEntitiesAsync())
                .Cast<Organization>()
                .Where(o => !o.IsDeleted)
                .ToList();

            // parentId → tập hợp direct children
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
                        foreach (var child in children)
                            result.Remove(child);
                        result.Add(parentId);
                        changed = true;
                        break; // khởi động lại vì set đã thay đổi
                    }
                }
            }

            return result.ToList();
        }

        #endregion
    }
}
