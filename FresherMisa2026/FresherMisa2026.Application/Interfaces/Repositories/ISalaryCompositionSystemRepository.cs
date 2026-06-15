using FresherMisa2026.Entities.SalaryCompositionSystem;
using FresherMisa2026.Entities.SalaryCompositionSystem.DTO;

namespace FresherMisa2026.Application.Interfaces.Repositories
{
    /// <summary>
    /// Interface repository cho SalaryCompositionSystem
    /// Created By: ntdo (2026-06-04)
    /// </summary>
    public interface ISalaryCompositionSystemRepository : IBaseRepository<SalaryCompositionSystem>
    {
        /// <summary>
        /// Lọc thành phần lương hệ thống theo nhiều điều kiện có phân trang
        /// </summary>
        /// Created By: ntdo (2026-06-04)
        Task<(IEnumerable<SalaryCompositionSystem> Data, long Total)> FilterAsync(SalaryCompositionSystemFilterRequest request);

        /// <summary>Lọc nâng cao 3 phần qua stored procedure</summary>
        /// Created By: ntdo (2026-06-04)
        Task<(IEnumerable<SalaryCompositionSystem> Data, long Total)> AdvancedFilterWithProcAsync(SalaryCompositionSystemAdvancedFilterRequest request);

        /// <summary>Tìm TPL hệ thống theo Code — trả null nếu không tồn tại.</summary>
        /// Created By: ntdo (2026-06-04)
        Task<SalaryCompositionSystem?> GetByCodeAsync(string code);
    }
}
