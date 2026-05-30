using FresherMisa2026.Entities.GridConfig;

namespace FresherMisa2026.Application.Interfaces.Repositories
{
    public interface IGridConfigRepository : IBaseRepository<GridConfig>
    {
        Task<IEnumerable<GridConfig>> GetByGridAsync(string userID, string gridCode);

        Task<int> BatchUpsertAsync(string userID, string gridCode, IEnumerable<GridConfig> columns);

        Task<int> ResetAsync(string userID, string gridCode);
    }
}
