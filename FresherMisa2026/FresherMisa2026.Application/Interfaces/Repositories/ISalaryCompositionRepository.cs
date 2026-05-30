using FresherMisa2026.Entities.SalaryComposition;
using FresherMisa2026.Entities.SalaryComposition.DTO;

namespace FresherMisa2026.Application.Interfaces.Repositories
{
    public interface ISalaryCompositionRepository : IBaseRepository<SalaryComposition>
    {
        /// <summary>
        /// Lọc thành phần lương theo nhiều điều kiện
        /// </summary>
        /// Created By: Nguyen Thiet Do (2026-05-27)
        Task<(IEnumerable<SalaryComposition> Data, long Total)> FilterAsync(SalaryCompositionFilterRequest request);

        /// <summary>
        /// Lọc nâng cao 4 phần: search, status, orgs, field conditions
        /// </summary>
        Task<(IEnumerable<SalaryComposition> Data, long Total)> AdvancedFilterAsync(SalaryCompositionAdvancedFilterRequest request);

        /// <summary>Lọc nâng cao 4 phần qua stored procedure</summary>
        Task<(IEnumerable<SalaryComposition> Data, long Total)> AdvancedFilterWithProcAsync(SalaryCompositionAdvancedFilterRequest request);
    }
}
