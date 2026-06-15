using FresherMisa2026.Entities;
using FresherMisa2026.Entities.SalaryCompositionSystem;
using FresherMisa2026.Entities.SalaryCompositionSystem.DTO;

namespace FresherMisa2026.Application.Interfaces.Services
{
    /// <summary>
    /// Interface service cho SalaryCompositionSystem
    /// Created By: ntdo (2026-06-05)
    /// </summary>
    public interface ISalaryCompositionSystemService : IBaseService<SalaryCompositionSystem>
    {
        /// <summary>
        /// Lọc thành phần lương hệ thống theo nhiều điều kiện có phân trang
        /// </summary>
        /// Created By: ntdo (2026-06-05)
        Task<ServiceResponse> FilterAsync(SalaryCompositionSystemFilterRequest request);

        /// <summary>Lọc nâng cao 3 phần qua stored procedure</summary>
        /// Created By: ntdo (2026-06-05)
        Task<ServiceResponse> AdvancedFilterWithProcAsync(SalaryCompositionSystemAdvancedFilterRequest request);
    }
}
