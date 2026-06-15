using FresherMisa2026.Application.Interfaces.Repositories;
using FresherMisa2026.Application.Interfaces.Services;
using FresherMisa2026.Entities;
using FresherMisa2026.Entities.Enums;
using FresherMisa2026.Entities.GridConfig;
using FresherMisa2026.Entities.GridConfig.DTO;
using FresherMisa2026.Entities.SalaryComposition;
using Microsoft.Extensions.Logging;

namespace FresherMisa2026.Application.Services
{
    /// <summary>
    /// Service cho cấu hình lưới của người dùng (pa_grid_config).
    /// </summary>
    /// <remarks>Created By: ntdo (2026-06-07) · Refactor: 2026-06-03</remarks>
    public class GridConfigService : BaseService<GridConfig>, IGridConfigService
    {
        #region Declare

        private readonly IGridConfigRepository _gridConfigRepository;
        private readonly ILogger<GridConfigService> _logger;

        #endregion

        #region Constructer

        public GridConfigService(
            IGridConfigRepository repository,
            ILogger<GridConfigService> logger,
            IAuditLogRepository auditLogRepository)
            : base(repository, auditLogRepository)
        {
            _gridConfigRepository = repository;
            _logger = logger;
        }

        #endregion

        #region Methods

        /// <summary>Lấy config cột của 1 user cho 1 lưới.</summary>
        /// Created By: ntdo (2026-06-07)
        public async Task<ServiceResponse> GetByGridAsync(string userID, string gridCode)
        {
            if (string.IsNullOrWhiteSpace(userID) || string.IsNullOrWhiteSpace(gridCode))
                return CreateErrorResponse(ResponseCode.BadRequest, "UserID và GridCode không được để trống");

            var data = await _gridConfigRepository.GetByGridAsync(userID, gridCode);
            return CreateSuccessResponse(data);
        }

        /// <summary>Ghi đè toàn bộ config cột cho 1 user + 1 lưới (xóa cũ, insert mới).</summary>
        /// Created By: ntdo (2026-06-07)
        public async Task<ServiceResponse> BatchUpsertAsync(string userID, string gridCode, List<GridConfigColumnDto> columns)
        {
            if (string.IsNullOrWhiteSpace(userID) || string.IsNullOrWhiteSpace(gridCode))
                return CreateErrorResponse(ResponseCode.BadRequest, "UserID và GridCode không được để trống");

            if (string.Equals(userID, SalaryCompositionConstants.SystemUserId, StringComparison.OrdinalIgnoreCase))
                return CreateErrorResponse(ResponseCode.BadRequest,
                    "Không thể ghi đè cấu hình mặc định của hệ thống");

            if (columns == null || columns.Count == 0)
                return CreateErrorResponse(ResponseCode.BadRequest, "Danh sách cột không được rỗng");

            var entities = columns.Select(col => new GridConfig
            {
                GridConfigID = Guid.NewGuid(),
                UserID       = userID,
                GridCode     = gridCode,
                ColumnKey    = col.ColumnKey,
                Caption      = col.Caption,
                OrderIndex   = col.OrderIndex,
                Width        = col.Width,
                IsPinned     = col.IsPinned,
                PinPosition  = col.PinPosition,
                IsVisible    = col.IsVisible
            }).ToList();

            var count = await _gridConfigRepository.BatchUpsertAsync(userID, gridCode, entities);

            _logger.LogInformation(
                "[GRID CONFIG] BatchUpsert | User: {UserId} | Grid: {GridCode} | Columns: {Count}",
                userID, gridCode, count);

            return CreateSuccessResponse(count);
        }

        /// <summary>Reset cấu hình cột của user về mặc định hệ thống.</summary>
        /// Created By: ntdo (2026-06-07)
        public async Task<ServiceResponse> ResetAsync(string userID, string gridCode)
        {
            if (string.IsNullOrWhiteSpace(userID) || string.IsNullOrWhiteSpace(gridCode))
                return CreateErrorResponse(ResponseCode.BadRequest, "UserID và GridCode không được để trống");

            var count = await _gridConfigRepository.ResetAsync(userID, gridCode);

            _logger.LogInformation(
                "[GRID CONFIG] Reset | User: {UserId} | Grid: {GridCode} | Deleted: {Count}",
                userID, gridCode, count);

            return CreateSuccessResponse(count);
        }

        #endregion
    }
}
