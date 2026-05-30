using FresherMisa2026.Application.Interfaces.Repositories;
using FresherMisa2026.Entities.SalaryComponentType;
using FresherMisa2026.Entities.Settings;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

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
            ILogger<BaseRepository<SalaryComponentType>> logger,
            IOptions<CacheSettings> cacheSettings)
            : base(configuration, cache, logger, cacheSettings)
        {
            _connectionString = configuration.GetConnectionString("SalaryConnection")!;
        }

        #endregion
    }
}
