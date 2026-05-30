using FresherMisa2026.Application.Interfaces.Services;
using FresherMisa2026.Entities;
using FresherMisa2026.Entities.GridConfig;
using FresherMisa2026.Entities.GridConfig.DTO;
using FresherMisa2026.Entities.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace FresherMisa2026.WebAPI.Controllers
{
    /// <summary>
    /// Controller quản lý cấu hình lưới dữ liệu
    /// Created By: Nguyen Thiet Do (2026-05-27)
    /// </summary>
    [ApiController]
    public class GridConfigsController : BaseController<GridConfig>
    {
        #region Declare

        private readonly IGridConfigService _gridConfigService;

        #endregion

        #region Constructer

        public GridConfigsController(
            IGridConfigService gridConfigService,
            IOptions<PagingSettings> pagingSettings)
            : base(gridConfigService, pagingSettings)
        {
            _gridConfigService = gridConfigService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Lấy toàn bộ config cột của 1 user cho 1 lưới
        /// </summary>
        /// Created By: Nguyen Thiet Do (2026-05-28)
        [HttpGet("by-grid")]
        public async Task<ActionResult<ServiceResponse>> GetByGrid([FromQuery] string userID, [FromQuery] string gridCode)
        {
            var response = await _gridConfigService.GetByGridAsync(userID, gridCode);
            return Ok(response);
        }

        /// <summary>
        /// Ghi lại toàn bộ config cột cho 1 user + 1 lưới (xóa cũ, insert mới)
        /// </summary>
        /// Created By: Nguyen Thiet Do (2026-05-28)
        [HttpPut("batch")]
        public async Task<ActionResult<ServiceResponse>> BatchUpsert([FromBody] GridConfigBatchRequest request)
        {
            var response = await _gridConfigService.BatchUpsertAsync(request.UserID, request.GridCode, request.Columns);
            return Ok(response);
        }

        /// <summary>
        /// Reset cấu hình cột của user về mặc định hệ thống (dùng được cho cả TPL và TPL hệ thống)
        /// </summary>
        /// Created By: Nguyen Thiet Do (2026-05-29)
        [HttpDelete("reset")]
        public async Task<ActionResult<ServiceResponse>> Reset([FromQuery] string userID, [FromQuery] string gridCode)
        {
            var response = await _gridConfigService.ResetAsync(userID, gridCode);
            return Ok(response);
        }

        #endregion
    }
}
