using FresherMisa2026.Entities.SalaryCompositionSystem;
using FresherMisa2026.Entities.SalaryCompositionSystem.DTO;

namespace FresherMisa2026.Application.Interfaces.Repositories
{
    public interface ISalaryCompositionSystemRepository : IBaseRepository<SalaryCompositionSystem>
    {
        /// <summary>
        /// Lọc thành phần lương hệ thống theo nhiều điều kiện có phân trang
        /// </summary>
        /// Created By: Nguyen Thiet Do (2026-05-27)
        Task<(IEnumerable<SalaryCompositionSystem> Data, long Total)> FilterAsync(SalaryCompositionSystemFilterRequest request);

        /// <summary>
        /// Lọc nâng cao 3 phần: search (mã/tên), loại thành phần, field conditions
        /// </summary>
        Task<(IEnumerable<SalaryCompositionSystem> Data, long Total)> AdvancedFilterAsync(SalaryCompositionSystemAdvancedFilterRequest request);

        /// <summary>Lọc nâng cao 3 phần qua stored procedure</summary>
        Task<(IEnumerable<SalaryCompositionSystem> Data, long Total)> AdvancedFilterWithProcAsync(SalaryCompositionSystemAdvancedFilterRequest request);
    }
}
