using Dapper;
using FresherMisa2026.Application.Interfaces.Repositories;
using FresherMisa2026.Entities.GridConfig;
using FresherMisa2026.Entities.SalaryComposition;
using FresherMisa2026.Infrastructure.Persistence.Queries;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Data;

namespace FresherMisa2026.Infrastructure.Repositories
{
    /// <summary>
    /// Repository cho cấu hình lưới của người dùng (pa_grid_config).
    /// </summary>
    /// <remarks>Created by: ntdo — 27/05/2026 · Refactor: 03/06/2026</remarks>
    public class GridConfigRepository : BaseRepository<GridConfig>, IGridConfigRepository
    {
        #region Constructer

        public GridConfigRepository(
            IConfiguration configuration,
            IMemoryCache cache,
            ILogger<BaseRepository<GridConfig>> logger)
            : base(configuration, cache, logger)
        {
            _connectionString = configuration.GetConnectionString(SalaryCompositionConstants.ConnectionName)!;
        }

        #endregion

        #region Methods

        /// <summary>Lấy toàn bộ config cột của 1 user cho 1 lưới (sort theo OrderIndex).</summary>
        /// <param name="userID">UserID của người dùng.</param>
        /// <param name="gridCode">Mã lưới cần lấy cấu hình.</param>
        /// <returns>Danh sách config cột.</returns>
        /// <remarks>Created by: ntdo — 28/05/2026</remarks>
        public async Task<IEnumerable<GridConfig>> GetByGridAsync(string userID, string gridCode)
        {
            using var connection = CreateConnection();
            return await connection.QueryAsync<GridConfig>(
                SalaryQueries.ProcGridConfigGetByGrid,
                new { v_user_id = userID, v_grid_code = gridCode },
                commandType: CommandType.StoredProcedure);
        }

        /// <summary>
        /// Ghi đè toàn bộ config cột cho 1 user + 1 lưới — DELETE rồi INSERT batch trong 1 transaction.
        /// </summary>
        /// <param name="userID">UserID của người dùng.</param>
        /// <param name="gridCode">Mã lưới cần upsert.</param>
        /// <param name="columns">Danh sách entity cột đã được Service build sẵn (kèm GridConfigID).</param>
        /// <returns>Số dòng đã insert.</returns>
        /// <remarks>Created by: ntdo — 28/05/2026</remarks>
        public async Task<int> BatchUpsertAsync(string userID, string gridCode, IEnumerable<GridConfig> columns)
        {
            using var connection = CreateConnection();
            await connection.OpenAsync();
            using var transaction = await connection.BeginTransactionAsync();

            try
            {
                // Bước 1: xóa toàn bộ config cũ
                await connection.ExecuteAsync(
                    SalaryQueries.DeleteGridConfigByUserAndGrid,
                    new { userID, gridCode },
                    transaction);

                // Bước 2: insert batch — Dapper sẽ lặp tham số trên cùng 1 prepared statement
                var now = DateTime.Now;
                var rows = columns.Select(col => new
                {
                    col.GridConfigID,
                    UserID      = userID,
                    GridCode    = gridCode,
                    col.ColumnKey,
                    col.Caption,
                    col.OrderIndex,
                    col.Width,
                    col.IsPinned,
                    col.PinPosition,
                    col.IsVisible,
                    CreatedBy   = userID,
                    CreateDate  = now
                }).ToList();

                var count = await connection.ExecuteAsync(SalaryQueries.InsertGridConfig, rows, transaction);

                await transaction.CommitAsync();

                _logger.LogInformation(
                    "[GRID CONFIG] BatchUpsert | User: {UserId} | Grid: {GridCode} | Inserted: {Count}",
                    userID, gridCode, count);

                return count;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        /// <summary>Xóa toàn bộ config cột tùy chỉnh của 1 user cho 1 lưới (reset về mặc định).</summary>
        /// <param name="userID">UserID của người dùng.</param>
        /// <param name="gridCode">Mã lưới cần reset.</param>
        /// <returns>Số dòng bị xóa.</returns>
        /// <remarks>Created by: ntdo — 29/05/2026</remarks>
        public async Task<int> ResetAsync(string userID, string gridCode)
        {
            using var connection = CreateConnection();
            var rows = await connection.ExecuteAsync(
                SalaryQueries.DeleteGridConfigByUserAndGrid,
                new { userID, gridCode });

            _logger.LogInformation(
                "[GRID CONFIG] Reset | User: {UserId} | Grid: {GridCode} | Deleted: {Count}",
                userID, gridCode, rows);

            return rows;
        }

        #endregion
    }
}
