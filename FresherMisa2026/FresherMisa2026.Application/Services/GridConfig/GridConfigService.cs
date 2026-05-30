using FresherMisa2026.Application.Interfaces.Repositories;
using FresherMisa2026.Application.Interfaces.Services;
using FresherMisa2026.Entities;
using FresherMisa2026.Entities.Enums;
using FresherMisa2026.Entities.GridConfig;
using FresherMisa2026.Entities.GridConfig.DTO;

namespace FresherMisa2026.Application.Services
{
    /// <summary>
    /// Service cho GridConfig
    /// Created By: Nguyen Thiet Do (2026-05-27)
    /// </summary>
    public class GridConfigService : BaseService<GridConfig>, IGridConfigService
    {
        #region Declare

        private readonly IGridConfigRepository _gridConfigRepository;

        #endregion

        #region Constructer

        public GridConfigService(IGridConfigRepository repository)
            : base(repository)
        {
            _gridConfigRepository = repository;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Lấy toàn bộ config cột của 1 user cho 1 lưới
        /// </summary>
        /// Created By: Nguyen Thiet Do (2026-05-28)
        public async Task<ServiceResponse> GetByGridAsync(string userID, string gridCode)
        {
            if (string.IsNullOrWhiteSpace(userID) || string.IsNullOrWhiteSpace(gridCode))
                return CreateErrorResponse(ResponseCode.BadRequest, "UserID và GridCode không được để trống");

            var data = await _gridConfigRepository.GetByGridAsync(userID, gridCode);
            return CreateSuccessResponse(data);
        }

        /// <summary>
        /// Ghi lại toàn bộ config cột cho 1 user + 1 lưới (xóa cũ, insert mới)
        /// </summary>
        /// Created By: Nguyen Thiet Do (2026-05-28)
        public async Task<ServiceResponse> BatchUpsertAsync(string userID, string gridCode, List<GridConfigColumnDto> columns)
        {
            if (string.IsNullOrWhiteSpace(userID) || string.IsNullOrWhiteSpace(gridCode))
                return CreateErrorResponse(ResponseCode.BadRequest, "UserID và GridCode không được để trống");

            if (userID.Equals("SYSTEM", StringComparison.OrdinalIgnoreCase))
                return CreateErrorResponse(ResponseCode.BadRequest, "Không thể ghi đè cấu hình mặc định của hệ thống");

            if (columns == null || columns.Count == 0)
                return CreateErrorResponse(ResponseCode.BadRequest, "Danh sách cột không được rỗng");

            var entities = columns.Select(col => new GridConfig
            {
                GridConfigID = Guid.NewGuid(),
                UserID = userID,
                GridCode = gridCode,
                ColumnKey = col.ColumnKey,
                Caption = col.Caption,
                OrderIndex = col.OrderIndex,
                Width = col.Width,
                IsPinned = col.IsPinned,
                PinPosition = col.PinPosition,
                IsVisible = col.IsVisible
            });

            var count = await _gridConfigRepository.BatchUpsertAsync(userID, gridCode, entities);
            return CreateSuccessResponse(count);
        }

        /// <summary>
        /// Xóa config cột tùy chỉnh của user, trả về mặc định hệ thống
        /// </summary>
        /// Created By: Nguyen Thiet Do (2026-05-29)
        public async Task<ServiceResponse> ResetAsync(string userID, string gridCode)
        {
            if (string.IsNullOrWhiteSpace(userID) || string.IsNullOrWhiteSpace(gridCode))
                return CreateErrorResponse(ResponseCode.BadRequest, "UserID và GridCode không được để trống");

            var count = await _gridConfigRepository.ResetAsync(userID, gridCode);
            return CreateSuccessResponse(count);
        }

        #endregion
    }
}
