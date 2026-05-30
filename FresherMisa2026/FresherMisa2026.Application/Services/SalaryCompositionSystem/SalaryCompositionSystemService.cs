using FresherMisa2026.Application.Interfaces.Repositories;
using FresherMisa2026.Application.Interfaces.Services;
using FresherMisa2026.Entities;
using FresherMisa2026.Entities.Enums;
using FresherMisa2026.Entities.SalaryCompositionSystem.DTO;
using SalaryCompositionSystemEntity = FresherMisa2026.Entities.SalaryCompositionSystem.SalaryCompositionSystem;

namespace FresherMisa2026.Application.Services
{
    /// <summary>
    /// Service cho SalaryCompositionSystem
    /// Created By: Nguyen Thiet Do (2026-05-27)
    /// </summary>
    public class SalaryCompositionSystemService : BaseService<SalaryCompositionSystemEntity>, ISalaryCompositionSystemService
    {
        #region Declare

        private readonly ISalaryCompositionSystemRepository _systemRepository;

        #endregion

        #region Constructer

        public SalaryCompositionSystemService(ISalaryCompositionSystemRepository repository)
            : base(repository)
        {
            _systemRepository = repository;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Lọc thành phần lương hệ thống theo nhiều điều kiện có phân trang
        /// </summary>
        /// Created By: Nguyen Thiet Do (2026-05-27)
        public async Task<ServiceResponse> FilterAsync(SalaryCompositionSystemFilterRequest request)
        {
            var (data, total) = await _systemRepository.FilterAsync(request);

            var pagingResponse = new PagingResponse<SalaryCompositionSystemEntity>
            {
                Data = data.ToList(),
                Total = total,
                PageSize = request.PageSize,
                CurrentPage = request.PageIndex,
                PageCount = (long)Math.Ceiling((double)total / request.PageSize)
            };

            return CreateSuccessResponse(pagingResponse);
        }

        /// <summary>
        /// Lọc nâng cao 3 phần: search (mã/tên), loại thành phần, field conditions
        /// </summary>
        public async Task<ServiceResponse> AdvancedFilterAsync(SalaryCompositionSystemAdvancedFilterRequest request)
        {
            var (data, total) = await _systemRepository.AdvancedFilterAsync(request);
            return CreatePagingResponse(data, total, request.PageIndex, request.PageSize);
        }

        /// <summary>Lọc nâng cao 3 phần qua stored procedure</summary>
        public async Task<ServiceResponse> AdvancedFilterWithProcAsync(SalaryCompositionSystemAdvancedFilterRequest request)
        {
            var (data, total) = await _systemRepository.AdvancedFilterWithProcAsync(request);
            return CreatePagingResponse(data, total, request.PageIndex, request.PageSize);
        }

        private ServiceResponse CreatePagingResponse(IEnumerable<SalaryCompositionSystemEntity> data, long total, int pageIndex, int pageSize)
            => CreateSuccessResponse(new PagingResponse<SalaryCompositionSystemEntity>
            {
                Data        = data.ToList(),
                Total       = total,
                PageSize    = pageSize,
                CurrentPage = pageIndex,
                PageCount   = (long)Math.Ceiling((double)total / pageSize)
            });

        #endregion

        #region OVERRIDE METHODS

        /// <summary>
        /// Validate tùy chỉnh — BR-05: TaxType chỉ có ý nghĩa khi Nature = Thu nhập
        /// </summary>
        protected override List<ValidationError> ValidateCustom(SalaryCompositionSystemEntity entity)
        {
            var errors = new List<ValidationError>();

            if (entity.TaxType.HasValue && entity.Nature != SalaryNature.Income)
                errors.Add(new ValidationError("TaxType", "Loại thuế TNCN chỉ áp dụng khi tính chất là Thu nhập"));

            return errors;
        }

        #endregion
    }
}
