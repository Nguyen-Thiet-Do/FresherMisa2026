using Dapper;
using FresherMisa2026.Application.Interfaces.Repositories;
using FresherMisa2026.Entities.GridConfig;
using FresherMisa2026.Entities.Settings;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Data;

namespace FresherMisa2026.Infrastructure.Repositories
{
    /// <summary>
    /// Repository cho GridConfig — dùng database amis_tien_luong
    /// Created By: Nguyen Thiet Do (2026-05-27)
    /// </summary>
    public class GridConfigRepository : BaseRepository<GridConfig>, IGridConfigRepository
    {
        #region Constructer

        public GridConfigRepository(
            IConfiguration configuration,
            IMemoryCache cache,
            ILogger<BaseRepository<GridConfig>> logger,
            IOptions<CacheSettings> cacheSettings)
            : base(configuration, cache, logger, cacheSettings)
        {
            _connectionString = configuration.GetConnectionString("SalaryConnection")!;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Lấy toàn bộ config cột của 1 user cho 1 lưới, sắp xếp theo OrderIndex
        /// </summary>
        /// Created By: Nguyen Thiet Do (2026-05-28)
        public async Task<IEnumerable<GridConfig>> GetByGridAsync(string userID, string gridCode)
        {
            using var connection = CreateConnection();
            return await connection.QueryAsync<GridConfig>(
                "Proc_pa_grid_config_GetByGrid",
                new { v_UserID = userID, v_GridCode = gridCode },
                commandType: CommandType.StoredProcedure);
        }

        /// <summary>
        /// Xóa toàn bộ config cũ và ghi lại config mới cho 1 user + 1 lưới (batch upsert)
        /// </summary>
        /// Created By: Nguyen Thiet Do (2026-05-28)
        public async Task<int> BatchUpsertAsync(string userID, string gridCode, IEnumerable<GridConfig> columns)
        {
            using var connection = CreateConnection();
            await connection.OpenAsync();
            using var transaction = await connection.BeginTransactionAsync();

            await connection.ExecuteAsync(
                "DELETE FROM pa_grid_config WHERE UserID = @userID AND GridCode = @gridCode",
                new { userID, gridCode }, transaction);

            var now = DateTime.Now;
            var rows = columns.Select(col => new
            {
                GridConfigID = Guid.NewGuid(),
                UserID = userID,
                GridCode = gridCode,
                col.ColumnKey,
                col.Caption,
                col.OrderIndex,
                col.Width,
                col.IsPinned,
                col.PinPosition,
                col.IsVisible,
                CreatedBy = userID,
                CreateDate = now
            }).ToList();

            var count = await connection.ExecuteAsync(@"
                INSERT INTO pa_grid_config
                  (GridConfigID, UserID, GridCode, ColumnKey, Caption, OrderIndex, Width,
                   IsPinned, PinPosition, IsVisible, CreatedBy, CreateDate)
                VALUES
                  (@GridConfigID, @UserID, @GridCode, @ColumnKey, @Caption, @OrderIndex, @Width,
                   @IsPinned, @PinPosition, @IsVisible, @CreatedBy, @CreateDate)",
                rows, transaction);

            await transaction.CommitAsync();
            return count;
        }

        /// <summary>
        /// Xóa toàn bộ config cột tùy chỉnh của 1 user cho 1 lưới (reset về mặc định)
        /// </summary>
        /// Created By: Nguyen Thiet Do (2026-05-29)
        public async Task<int> ResetAsync(string userID, string gridCode)
        {
            using var connection = CreateConnection();
            return await connection.ExecuteAsync(
                "DELETE FROM pa_grid_config WHERE UserID = @userID AND GridCode = @gridCode",
                new { userID, gridCode });
        }

        #endregion
    }
}
