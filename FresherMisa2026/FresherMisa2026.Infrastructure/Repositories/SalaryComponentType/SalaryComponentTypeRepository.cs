using FresherMisa2026.Application.Interfaces.Repositories;
using FresherMisa2026.Entities.SalaryComponentType;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FresherMisa2026.Infrastructure.Repositories
{
    /// <summary>
    /// Repository cho SalaryComponentType — dùng database amis_tien_luong
    /// Created By: ntdo (2026-06-05)
    /// </summary>
    public class SalaryComponentTypeRepository : BaseRepository<SalaryComponentType>, ISalaryComponentTypeRepository
    {
        #region Constructer

        public SalaryComponentTypeRepository(
            IConfiguration configuration,
            ILogger<BaseRepository<SalaryComponentType>> logger)
            : base(configuration, logger)
        {
            _connectionString = configuration.GetConnectionString("SalaryConnection")!;
        }

        #endregion
    }
}
