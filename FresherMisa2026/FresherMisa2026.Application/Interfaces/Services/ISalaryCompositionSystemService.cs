using FresherMisa2026.Entities;
using FresherMisa2026.Entities.SalaryCompositionSystem;
using FresherMisa2026.Entities.SalaryCompositionSystem.DTO;

namespace FresherMisa2026.Application.Interfaces.Services
{
    public interface ISalaryCompositionSystemService : IBaseService<SalaryCompositionSystem>
    {
        /// <summary>
        /// Lọc thành phần lương hệ thống theo nhiều điều kiện có phân trang
        /// </summary>
        /// Created By: Nguyen Thiet Do (2026-05-27)
        Task<ServiceResponse> FilterAsync(SalaryCompositionSystemFilterRequest request);

        /// <summary>Lọc nâng cao 3 phần qua stored procedure</summary>
        Task<ServiceResponse> AdvancedFilterWithProcAsync(SalaryCompositionSystemAdvancedFilterRequest request);
    }
}
