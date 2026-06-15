using Dapper;
using FresherMisa2026.Application.Interfaces.Repositories;
using FresherMisa2026.Entities.Organization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FresherMisa2026.Infrastructure.Repositories
{
    /// <summary>
    /// Repository cho Organization — dùng database amis_tien_luong
    /// Created By: ntdo (2026-06-04)
    /// </summary>
    public class OrganizationRepository : BaseRepository<Organization>, IOrganizationRepository
    {
        #region Constructer

        public OrganizationRepository(
            IConfiguration configuration,
            ILogger<BaseRepository<Organization>> logger)
            : base(configuration, logger)
        {
            _connectionString = configuration.GetConnectionString("SalaryConnection")!;
        }

        #endregion

        #region Methods

        /// <summary>Lấy ID các đơn vị gốc (không có cha) — không fetch toàn bảng.</summary>
        /// Created By: ntdo (2026-06-04)
        public async Task<IEnumerable<Guid>> GetRootOrganizationIdsAsync()
        {
            const string sql = @"SELECT `organization_id` FROM `pa_organization`
                                 WHERE `parent_id` IS NULL AND `is_deleted` = FALSE";
            using var connection = CreateConnection();
            await connection.OpenAsync();
            var rows = await connection.QueryAsync<Organization>(sql);
            return rows.Select(o => o.OrganizationID);
        }

        #endregion
    }
}
