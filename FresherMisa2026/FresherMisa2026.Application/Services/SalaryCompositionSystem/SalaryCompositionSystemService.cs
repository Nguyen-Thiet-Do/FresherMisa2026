using FresherMisa2026.Application.Interfaces.Repositories;
using FresherMisa2026.Application.Interfaces.Services;
using FresherMisa2026.Entities;
using FresherMisa2026.Entities.Enums;
using FresherMisa2026.Entities.SalaryCompositionSystem.DTO;
using Microsoft.Extensions.Logging;
using SalaryCompositionSystemEntity = FresherMisa2026.Entities.SalaryCompositionSystem.SalaryCompositionSystem;

namespace FresherMisa2026.Application.Services
{
    /// <summary>
    /// Service cho danh mục TPL hệ thống.
    /// </summary>
    /// <remarks>Created By: ntdo (2026-06-06) · Refactor: 2026-06-03</remarks>
    public class SalaryCompositionSystemService
        : BaseService<SalaryCompositionSystemEntity>, ISalaryCompositionSystemService
    {
        #region Declare

        private readonly ISalaryCompositionSystemRepository _systemRepository;
        private readonly ILogger<SalaryCompositionSystemService> _logger;

        #endregion

        #region Constructer

        public SalaryCompositionSystemService(
            ISalaryCompositionSystemRepository repository,
            ILogger<SalaryCompositionSystemService> logger,
            IAuditLogRepository auditLogRepository)
            : base(repository, auditLogRepository)
        {
            _systemRepository = repository;
            _logger = logger;
        }

        #endregion

        #region Methods

        /// <summary>Lọc TPL hệ thống có phân trang.</summary>
        /// Created By: ntdo (2026-06-06)
        public async Task<ServiceResponse> FilterAsync(SalaryCompositionSystemFilterRequest request)
        {
            var (data, total) = await _systemRepository.FilterAsync(request);
            return CreatePagingResponse(total, request.PageIndex, request.PageSize, data);
        }

        /// <summary>Lọc nâng cao 3 phần qua Stored Procedure.</summary>
        /// Created By: ntdo (2026-06-06)
        public async Task<ServiceResponse> AdvancedFilterWithProcAsync(SalaryCompositionSystemAdvancedFilterRequest request)
        {
            var fieldErrors = ValidateFilterFieldNames(request.SearchFields, request.Filters, typeof(SalaryCompositionSystemEntity));
            fieldErrors.AddRange(ValidateSortFieldNames(request.Sort, typeof(SalaryCompositionSystemEntity)));
            if (fieldErrors.Count > 0) return CreateValidationErrorResponse(fieldErrors);

            var (data, total) = await _systemRepository.AdvancedFilterWithProcAsync(request);

            if (request.Columns?.Count > 0)
                return CreateSuccessResponse(BuildProjectedPaging(total, request.PageIndex, request.PageSize, ProjectColumns(data, request.Columns)));

            return CreatePagingResponse(total, request.PageIndex, request.PageSize, data);
        }

        #endregion

        #region OVERRIDE METHODS

        /// <summary>Validate: BR-05 — TaxType chỉ có ý nghĩa khi Nature = Income.</summary>
        /// Created By: ntdo (2026-06-06)
        protected override List<ValidationError> ValidateCustom(SalaryCompositionSystemEntity entity)
        {
            var errors = new List<ValidationError>();

            if (entity.TaxType.HasValue && entity.Nature != SalaryNature.Income)
                errors.Add(new ValidationError("TaxType",
                    "Loại thuế TNCN chỉ áp dụng khi tính chất là Thu nhập"));

            return errors;
        }

        #endregion

        #region Private helpers

        /// Created By: ntdo (2026-06-07)
        private ServiceResponse CreatePagingResponse(long total, int pageIndex, int pageSize,
            IEnumerable<SalaryCompositionSystemEntity> data)
            => CreateSuccessResponse(new PagingResponse<SalaryCompositionSystemEntity>
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
