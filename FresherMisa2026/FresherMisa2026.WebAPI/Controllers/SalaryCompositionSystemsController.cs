using FresherMisa2026.Application.Interfaces.Services;
using FresherMisa2026.Entities;
using FresherMisa2026.Entities.SalaryCompositionSystem.DTO;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using SalaryCompositionSystemEntity = FresherMisa2026.Entities.SalaryCompositionSystem.SalaryCompositionSystem;

namespace FresherMisa2026.WebAPI.Controllers
{
    /// <summary>
    /// Controller quản lý danh mục thành phần lương hệ thống — chỉ đọc, không cho phép ghi
    /// Created By: ntdo (2026-06-10)
    /// </summary>
    [ApiController]
    public class SalaryCompositionSystemsController : BaseController<SalaryCompositionSystemEntity>
    {
        #region Declare

        private readonly ISalaryCompositionSystemService _salaryCompositionSystemService;

        #endregion

        #region Constructer

        public SalaryCompositionSystemsController(ISalaryCompositionSystemService salaryCompositionSystemService)
            : base(salaryCompositionSystemService)
        {
            _salaryCompositionSystemService = salaryCompositionSystemService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Lọc thành phần lương hệ thống theo nhiều điều kiện
        /// </summary>
        /// Created By: ntdo (2026-06-10)
        [HttpGet("filter")]
        public async Task<ActionResult<ServiceResponse>> Filter([FromQuery] SalaryCompositionSystemFilterRequest request)
        {
            var response = await _salaryCompositionSystemService.FilterAsync(request);
            return Ok(response);
        }

        /// <summary>
        /// Lọc nâng cao qua stored procedure: 1-search mã/tên, 2-loại thành phần, 3-field conditions
        /// </summary>
        /// Created By: ntdo (2026-06-10)
        [HttpPost("datapaging")]
        public async Task<ActionResult<ServiceResponse>> AdvancedFilterProcSystem([FromBody] SalaryCompositionSystemAdvancedFilterRequest request)
        {
            var response = await _salaryCompositionSystemService.AdvancedFilterWithProcAsync(request);
            return Ok(response);
        }

        #endregion

        #region Read-only guard — 405 Method Not Allowed

        /// <summary>Block POST — TPL hệ thống chỉ đọc</summary>
        /// Created By: ntdo (2026-06-10)
        [HttpPost]
        public override Task<ActionResult<ServiceResponse>> Post([FromBody] SalaryCompositionSystemEntity entity)
            => Task.FromResult<ActionResult<ServiceResponse>>(StatusCode(405));

        /// <summary>Block PUT — TPL hệ thống chỉ đọc</summary>
        /// Created By: ntdo (2026-06-10)
        [HttpPut("{id:guid}")]
        public override Task<ActionResult<ServiceResponse>> Put(Guid id, [FromBody] SalaryCompositionSystemEntity entity)
            => Task.FromResult<ActionResult<ServiceResponse>>(StatusCode(405));

        /// <summary>Block DELETE — TPL hệ thống chỉ đọc</summary>
        /// Created By: ntdo (2026-06-10)
        [HttpDelete("{id:guid}")]
        public override Task<ActionResult<ServiceResponse>> DeleteByID(Guid id)
            => Task.FromResult<ActionResult<ServiceResponse>>(StatusCode(405));

        /// <summary>Block bulk-delete — TPL hệ thống chỉ đọc</summary>
        /// Created By: ntdo (2026-06-10)
        [HttpPost("bulk-delete")]
        public override Task<ActionResult<ServiceResponse>> DeleteMany([FromBody] List<Guid> ids)
            => Task.FromResult<ActionResult<ServiceResponse>>(StatusCode(405));

        /// <summary>Block bulk-delete/partial — TPL hệ thống chỉ đọc</summary>
        /// Created By: ntdo (2026-06-10)
        [HttpPost("bulk-delete/partial")]
        public override Task<ActionResult<ServiceResponse>> DeleteManyPartial([FromBody] List<Guid> ids)
            => Task.FromResult<ActionResult<ServiceResponse>>(StatusCode(405));

        /// <summary>Block PATCH — TPL hệ thống chỉ đọc</summary>
        /// Created By: ntdo (2026-06-10)
        [HttpPatch("{id:guid}/{fieldName}")]
        public override Task<ActionResult<ServiceResponse>> PatchField(Guid id, string fieldName, [FromBody] JsonElement value)
            => Task.FromResult<ActionResult<ServiceResponse>>(StatusCode(405));

        #endregion
    }
}
