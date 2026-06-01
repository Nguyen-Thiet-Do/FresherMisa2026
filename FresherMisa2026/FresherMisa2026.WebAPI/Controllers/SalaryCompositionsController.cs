using FresherMisa2026.Application.Interfaces.Services;
using FresherMisa2026.Entities;
using FresherMisa2026.Entities.AdvancedFilter;
using FresherMisa2026.Entities.Enums;
using FresherMisa2026.Entities.SalaryComposition.DTO;
using FresherMisa2026.Entities.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SalaryCompositionEntity = FresherMisa2026.Entities.SalaryComposition.SalaryComposition;

namespace FresherMisa2026.WebAPI.Controllers
{
    /// <summary>
    /// Controller quản lý thành phần lương
    /// Created By: Nguyen Thiet Do (2026-05-27)
    /// </summary>
    [ApiController]
    public class SalaryCompositionsController : BaseController<SalaryCompositionEntity>
    {
        #region Declare

        private readonly ISalaryCompositionService _salaryCompositionService;

        #endregion

        #region Constructer

        public SalaryCompositionsController(
            ISalaryCompositionService salaryCompositionService,
            IOptions<PagingSettings> pagingSettings)
            : base(salaryCompositionService, pagingSettings)
        {
            _salaryCompositionService = salaryCompositionService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Chuyển TPL hệ thống sang TPL đơn vị
        /// </summary>
        /// Created By: Nguyen Thiet Do (2026-05-27)
        [HttpPost("inherit/{systemCompositionId:guid}")]
        public async Task<ActionResult<ServiceResponse>> InheritFromSystem(
            Guid systemCompositionId,
            [FromBody] List<Guid>? organizationIds = null)
        {
            var response = await _salaryCompositionService.InheritFromSystemAsync(systemCompositionId, organizationIds);
            return StatusCode(response.Code, response);
        }

        /// <summary>
        /// Chuyển nhiều TPL hệ thống sang TPL đơn vị cùng lúc — partial result
        /// </summary>
        /// Created By: Nguyen Thiet Do (2026-05-27)
        [HttpPost("inherit/batch")]
        public async Task<ActionResult<ServiceResponse>> InheritFromSystemBatch([FromBody] InheritFromSystemBatchRequest request)
        {
            var response = await _salaryCompositionService.InheritFromSystemBatchAsync(request);
            return Ok(response);
        }

        /// <summary>
        /// Chuyển TPL sang đang theo dõi
        /// </summary>
        /// Created By: Nguyen Thiet Do (2026-05-27)
        [HttpPatch("{id:guid}/activate")]
        public async Task<ActionResult<ServiceResponse>> Activate(Guid id)
        {
            var response = await _salaryCompositionService.SetStatusAsync(id, SalaryCompositionStatus.Active);
            if (!response.IsSuccess)
                return response.Code == (int)ResponseCode.NotFound ? NotFound(response) : BadRequest(response);
            return Ok(response);
        }

        /// <summary>
        /// Chuyển nhiều TPL sang đang theo dõi — partial result
        /// </summary>
        /// Created By: Nguyen Thiet Do (2026-05-28)
        [HttpPatch("bulk-activate")]
        public async Task<ActionResult<ServiceResponse>> BulkActivate([FromBody] List<Guid> ids)
        {
            var response = await _salaryCompositionService.SetStatusBulkAsync(ids, SalaryCompositionStatus.Active);
            return Ok(response);
        }

        /// <summary>
        /// Chuyển TPL sang bỏ theo dõi
        /// </summary>
        /// Created By: Nguyen Thiet Do (2026-05-27)
        [HttpPatch("{id:guid}/deactivate")]
        public async Task<ActionResult<ServiceResponse>> Deactivate(Guid id)
        {
            var response = await _salaryCompositionService.SetStatusAsync(id, SalaryCompositionStatus.Inactive);
            if (!response.IsSuccess)
                return response.Code == (int)ResponseCode.NotFound ? NotFound(response) : BadRequest(response);
            return Ok(response);
        }

        /// <summary>
        /// Chuyển nhiều TPL sang bỏ theo dõi — partial result
        /// </summary>
        /// Created By: Nguyen Thiet Do (2026-05-28)
        [HttpPatch("bulk-deactivate")]
        public async Task<ActionResult<ServiceResponse>> BulkDeactivate([FromBody] List<Guid> ids)
        {
            var response = await _salaryCompositionService.SetStatusBulkAsync(ids, SalaryCompositionStatus.Inactive);
            return Ok(response);
        }

        /// <summary>
        /// Phân loại danh sách TPL thành DataExist / DataSystem / DataNotExist có phân trang.
        /// Dùng trước khi xóa hoặc ngừng theo dõi hàng loạt để FE hiển thị cảnh báo phù hợp.
        /// </summary>
        /// Created By: Nguyen Thiet Do (2026-06-02)
        [HttpPost("exit-data")]
        public async Task<ActionResult<ServiceResponse>> ExitData([FromBody] ExitDataRequest request)
        {
            var response = await _salaryCompositionService.ExitDataAsync(request);
            if (!response.IsSuccess) return BadRequest(response);
            return Ok(response);
        }

        /// <summary>
        /// Gợi ý thành phần lương cho ô nhập công thức — trả về Code, Name, Description
        /// </summary>
        /// Created By: Nguyen Thiet Do (2026-05-27)
        [HttpGet("suggestions")]
        public async Task<ActionResult<ServiceResponse>> GetSuggestions([FromQuery] string? search)
        {
            var response = await _salaryCompositionService.GetSuggestionsAsync(search);
            return Ok(response);
        }

        /// <summary>
        /// Thêm mới — trả HTTP 202 + isSuccess=false nếu công thức có TPL ngừng theo dõi (cần xác nhận).
        /// FE set IsSkipUnfollowedComposition=true rồi gửi lại để lưu.
        /// </summary>
        public override async Task<ActionResult<ServiceResponse>> Post([FromBody] SalaryCompositionEntity entity)
        {
            var response = await _salaryCompositionService.InsertAsync(entity);
            if (response.IsSuccess) return StatusCode((int)ResponseCode.Created, response);
            if (response.Code == (int)ResponseCode.ConfirmationRequired) return StatusCode((int)ResponseCode.ConfirmationRequired, response);
            return BadRequest(response);
        }

        /// <summary>
        /// Cập nhật — tương tự Post: trả HTTP 202 nếu cần xác nhận.
        /// </summary>
        public override async Task<ActionResult<ServiceResponse>> Put(Guid id, [FromBody] SalaryCompositionEntity entity)
        {
            var response = await _salaryCompositionService.UpdateAsync(id, entity);
            if (response.IsSuccess) return Ok(response);
            if (response.Code == (int)ResponseCode.ConfirmationRequired) return StatusCode((int)ResponseCode.ConfirmationRequired, response);
            if (response.Code == (int)ResponseCode.NotFound) return NotFound(response);
            return BadRequest(response);
        }

        [NonAction]
        public override Task<ActionResult<ServiceResponse>> AdvancedFilter([FromBody] AdvancedFilterRequest request)
            => base.AdvancedFilter(request);

        [NonAction]
        public override Task<ActionResult<ServiceResponse>> AdvancedFilterProc([FromBody] AdvancedFilterRequest request)
            => base.AdvancedFilterProc(request);

        /// <summary>
        /// Lọc nâng cao: 1-search mã/tên, 2-trạng thái, 3-đơn vị, 4-field conditions
        /// </summary>
        [HttpPost("AdvancedFilter")]
        public async Task<ActionResult<ServiceResponse>> AdvancedFilterSalary([FromBody] SalaryCompositionAdvancedFilterRequest request)
        {
            var response = await _salaryCompositionService.AdvancedFilterAsync(request);
            return Ok(response);
        }

        /// <summary>
        /// Lọc nâng cao qua stored procedure: 1-search mã/tên, 2-trạng thái, 3-đơn vị, 4-field conditions
        /// </summary>
        [HttpPost("AdvancedFilterProc")]
        public async Task<ActionResult<ServiceResponse>> AdvancedFilterProcSalary([FromBody] SalaryCompositionAdvancedFilterRequest request)
        {
            var response = await _salaryCompositionService.AdvancedFilterWithProcAsync(request);
            return Ok(response);
        }

        /// <summary>
        /// Lọc thành phần lương theo nhiều điều kiện
        /// </summary>
        /// Created By: Nguyen Thiet Do (2026-05-27)
        [HttpGet("filter")]
        public async Task<ActionResult<ServiceResponse>> Filter([FromQuery] SalaryCompositionFilterRequest request)
        {
            var response = await _salaryCompositionService.FilterAsync(request);
            return Ok(response);
        }

        #endregion
    }
}
