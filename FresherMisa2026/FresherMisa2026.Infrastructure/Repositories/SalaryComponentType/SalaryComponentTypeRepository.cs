using FresherMisa2026.Application.Interfaces.Repositories;
using FresherMisa2026.Entities.SalaryComponentType;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FresherMisa2026.Infrastructure.Repositories
{
    /// <summary>
    /// Repository cho SalaryComponentType — dùng database amis_tien_luong
    /// Created By: Nguyen Thiet Do (2026-05-27)
    /// </summary>
    public class SalaryComponentTypeRepository : BaseRepository<SalaryComponentType>, ISalaryComponentTypeRepository
    {
        #region Constructer

        public SalaryComponentTypeRepository(
            IConfiguration configuration,
            IMemoryCache cache,
            ILogger<BaseRepository<SalaryComponentType>> logger)
            : base(configuration, cache, logger)
        {
            _connectionString = configuration.GetConnectionString("SalaryConnection")!;
        }

        #endregion
    }
}
