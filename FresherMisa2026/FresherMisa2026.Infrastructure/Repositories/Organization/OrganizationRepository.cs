using FresherMisa2026.Application.Interfaces.Repositories;
using FresherMisa2026.Entities.Organization;
using FresherMisa2026.Entities.Settings;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FresherMisa2026.Infrastructure.Repositories
{
    /// <summary>
    /// Repository cho Organization — dùng database amis_tien_luong
    /// Created By: Nguyen Thiet Do (2026-05-27)
    /// </summary>
    public class OrganizationRepository : BaseRepository<Organization>, IOrganizationRepository
    {
        #region Constructer

        public OrganizationRepository(
            IConfiguration configuration,
            IMemoryCache cache,
            ILogger<BaseRepository<Organization>> logger,
            IOptions<CacheSettings> cacheSettings)
            : base(configuration, cache, logger, cacheSettings)
        {
            _connectionString = configuration.GetConnectionString("SalaryConnection")!;
        }

        #endregion
    }
}
