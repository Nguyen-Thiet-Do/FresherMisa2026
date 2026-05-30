using FresherMisa2026.Entities;
using FresherMisa2026.Entities.GridConfig;
using FresherMisa2026.Entities.GridConfig.DTO;

namespace FresherMisa2026.Application.Interfaces.Services
{
    public interface IGridConfigService : IBaseService<GridConfig>
    {
        Task<ServiceResponse> GetByGridAsync(string userID, string gridCode);

        Task<ServiceResponse> BatchUpsertAsync(string userID, string gridCode, List<GridConfigColumnDto> columns);

        Task<ServiceResponse> ResetAsync(string userID, string gridCode);
    }
}
