using FresherMisa2026.Application.Interfaces.Repositories;
using FresherMisa2026.Application.Interfaces.Services;
using FresherMisa2026.Entities;
using FresherMisa2026.Entities.Enums;
using FresherMisa2026.Entities.SalaryComposition.DTO;
using System.Text.RegularExpressions;
using SalaryCompositionEntity = FresherMisa2026.Entities.SalaryComposition.SalaryComposition;
using SalaryCompositionSystemEntity = FresherMisa2026.Entities.SalaryCompositionSystem.SalaryCompositionSystem;
using SalaryCompositionSuggestion = FresherMisa2026.Entities.SalaryComposition.DTO.SalaryCompositionSuggestion;

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

        #endregion

        #region Constructer

        public SalaryCompositionService(
            ISalaryCompositionRepository salaryCompositionRepository,
            ISalaryCompositionSystemRepository systemRepository)
            : base(salaryCompositionRepository)
        {
            _salaryCompositionRepository = salaryCompositionRepository;
            _systemRepository = systemRepository;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Chuyển TPL hệ thống sang TPL đơn vị — map field và insert qua pipeline bình thường
        /// </summary>
        /// Created By: Nguyen Thiet Do (2026-05-27)
        public async Task<ServiceResponse> InheritFromSystemAsync(Guid systemCompositionId)
        {
            var system = await _systemRepository.GetEntityByIDAsync(systemCompositionId);
            if (system == null)
                return CreateErrorResponse(ResponseCode.NotFound, "Không tìm thấy thành phần lương hệ thống");

            var composition = new SalaryCompositionEntity
            {
                Code              = system.Code,
                Name              = system.Name,
                ComponentTypeID   = system.ComponentTypeID ?? Guid.Empty,
                SystemCompositionID = system.SystemCompositionID,
                Nature            = system.Nature,
                TaxType           = system.TaxType,
                ValueType         = system.ValueType,
                NormFormula       = system.NormFormula,
                Description       = system.Description,
                Source            = SalaryCompositionSource.InheritedFromSystem,
                Status            = SalaryCompositionStatus.Active,
            };

            return await InsertAsync(composition);
        }

        /// <summary>
        /// Chuyển nhiều TPL hệ thống sang TPL đơn vị — tiếp tục kể cả khi một số ID thất bại
        /// </summary>
        /// Created By: Nguyen Thiet Do (2026-05-27)
        public async Task<ServiceResponse> InheritFromSystemBatchAsync(List<Guid> systemCompositionIds)
        {
            if (systemCompositionIds == null || systemCompositionIds.Count == 0)
                return CreateErrorResponse(ResponseCode.BadRequest, "Danh sách Id không được rỗng");

            var result = new BulkInheritResult();

            foreach (var systemId in systemCompositionIds)
            {
                try
                {
                    var response = await InheritFromSystemAsync(systemId);
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

            return errors;
        }

        /// <summary>
        /// Validate trước khi thêm — kiểm tra cú pháp ValueFormula và NormFormula
        /// </summary>
        protected override async Task<List<ValidationError>> ValidateBeforeInsertAsync(SalaryCompositionEntity entity)
        {
            var validCodes = await GetActiveCodesAsync();
            return ValidateFormulas(entity, validCodes);
        }

        /// <summary>
        /// Validate trước khi update — BR-01: Code không được thay đổi + kiểm tra công thức
        /// </summary>
        protected override async Task<List<ValidationError>> ValidateBeforeUpdateAsync(Guid entityId, SalaryCompositionEntity entity)
        {
            var errors = new List<ValidationError>();

            var existing = await _baseRepository.GetEntityByIDAsync(entityId);
            if (existing != null && existing.Code != entity.Code)
                errors.Add(new ValidationError("Code", "Mã thành phần lương không được thay đổi sau khi lưu"));

            var validCodes = await GetActiveCodesAsync();
            errors.AddRange(ValidateFormulas(entity, validCodes));

            return errors;
        }

        /// <summary>
        /// Validate trước khi xóa — BR-08: Không xóa TPL kế thừa từ hệ thống
        /// </summary>
        protected override async Task<bool> ValidateBeforeDeleteAsync(Guid entityId)
        {
            var entity = await _baseRepository.GetEntityByIDAsync(entityId);
            return entity == null || entity.Source != SalaryCompositionSource.InheritedFromSystem;
        }

        protected override Task<string?> GetDeleteValidationMessageAsync(Guid entityId)
        {
            return Task.FromResult<string?>("Không thể xóa thành phần lương mặc định của hệ thống");
        }

        #endregion

        #region Private helpers

        /// <summary>
        /// Lấy tập mã của tất cả thành phần lương đang theo dõi (Status = Active)
        /// </summary>
        private async Task<HashSet<string>> GetActiveCodesAsync()
        {
            var all = await _baseRepository.GetEntitiesAsync();
            return all
                .Cast<SalaryCompositionEntity>()
                .Where(e => e.Status == SalaryCompositionStatus.Active)
                .Select(e => e.Code)
                .ToHashSet(StringComparer.Ordinal);
        }

        private static List<ValidationError> ValidateFormulas(SalaryCompositionEntity entity, HashSet<string> validCodes)
        {
            var errors = new List<ValidationError>();

            var (valOk, valErr) = FormulaValidator.Validate(entity.ValueFormula, validCodes);
            if (!valOk)
                errors.Add(new ValidationError(nameof(entity.ValueFormula),
                    $"Công thức giá trị không hợp lệ: {valErr}"));

            var (normOk, normErr) = FormulaValidator.Validate(entity.NormFormula, validCodes);
            if (!normOk)
                errors.Add(new ValidationError(nameof(entity.NormFormula),
                    $"Công thức định mức không hợp lệ: {normErr}"));

            return errors;
        }

        #endregion
    }
}
