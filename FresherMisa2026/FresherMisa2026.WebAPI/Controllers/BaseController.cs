using FresherMisa2026.Application.Interfaces.Services;
using FresherMisa2026.Entities;
using FresherMisa2026.Entities.Enums;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace FresherMisa2026.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BaseController<TEntity> : ControllerBase
    {
        private readonly IBaseService<TEntity> _baseService;

        public BaseController(IBaseService<TEntity> baseService)
        {
            _baseService = baseService;
        }

        /// <summary>
        /// Danh sách paging
        /// </summary>
        [HttpGet("paging")]
        public async Task<ActionResult<ServiceResponse>> GetFilterPaging(
            [FromQuery] string? search,
            [FromQuery] string? sort,
            [FromQuery] int? pageSize = null,
            [FromQuery] int? pageIndex = null,
            [FromQuery] string? searchFields = null
        )
        {
            var pagingRequest = new PagingRequest
            {
                PageIndex = pageIndex ?? 1,
                PageSize = pageSize ?? 10,
                Search = search ?? string.Empty,
                Sort = sort ?? string.Empty,
                SearchFields = searchFields ?? string.Empty
            };
            
            var response = await _baseService.GetFilterPagingAsync(pagingRequest);
            return Ok(response);
        }

        /// <summary>
        /// Danh sách
        /// </summary>
        [HttpGet()]
        public async Task<ActionResult<ServiceResponse>> Get()
        {
            var response = await _baseService.GetEntitiesAsync();
            return Ok(response);
        }

        /// <summary>
        /// Một phần tử
        /// </summary>
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ServiceResponse>> GetByID(Guid id)
        {
            var response = await _baseService.GetEntityByIDAsync(id);

            if (!response.IsSuccess && response.Code == (int)ResponseCode.NotFound)
                return NotFound(response);

            if (!response.IsSuccess && response.Code == (int)ResponseCode.BadRequest)
                return BadRequest(response);
            
            return Ok(response);
        }

        /// <summary>
        /// Xóa nhiều phần tử trong một transaction — fail-fast: rollback toàn bộ nếu có 1 ID lỗi
        /// </summary>
        [HttpPost("bulk-delete")]
        public virtual async Task<ActionResult<ServiceResponse>> DeleteMany([FromBody] List<Guid> ids)
        {
            var response = await _baseService.DeleteManyAsync(ids);

            if (!response.IsSuccess && response.Code == (int)ResponseCode.NotFound)
                return NotFound(response);

            if (!response.IsSuccess && response.Code == (int)ResponseCode.BadRequest)
                return BadRequest(response);

            return Ok(response);
        }

        /// <summary>
        /// Xóa nhiều phần tử — partial result: tiếp tục xóa dù có ID thất bại
        /// </summary>
        [HttpPost("bulk-delete/partial")]
        public virtual async Task<ActionResult<ServiceResponse>> DeleteManyPartial([FromBody] List<Guid> ids)
        {
            var response = await _baseService.DeleteManyPartialAsync(ids);

            if (!response.IsSuccess && response.Code == (int)ResponseCode.BadRequest)
                return BadRequest(response);

            return Ok(response);
        }

        /// <summary>
        /// Xóa một phần tử
        /// </summary>
        [HttpDelete("{id:guid}")]
        public virtual async Task<ActionResult<ServiceResponse>> DeleteByID(Guid id)
        {
            var response = await _baseService.DeleteByIDAsync(id);
            
            if (!response.IsSuccess && response.Code == (int)ResponseCode.NotFound)
                return NotFound(response);
            
            if (!response.IsSuccess && response.Code == (int)ResponseCode.BadRequest)
                return BadRequest(response);
                
            return Ok(response);
        }

        /// <summary>
        /// Thêm một thực thể mới
        /// </summary>
        [HttpPost]
        public virtual async Task<ActionResult<ServiceResponse>> Post([FromBody] TEntity entity)
        {
            var response = await _baseService.InsertAsync(entity);

            if (!response.IsSuccess)
                return BadRequest(response);

            return StatusCode((int)ResponseCode.Created, response);
        }

        /// <summary>
        /// Sửa một thực thể
        /// </summary>
        [HttpPut("{id:guid}")]
        public virtual async Task<ActionResult<ServiceResponse>> Put(Guid id, [FromBody] TEntity entity)
        {
            var response = await _baseService.UpdateAsync(id, entity);

            if (!response.IsSuccess)
            {
                if (response.Code == (int)ResponseCode.NotFound)
                    return NotFound(response);
                return BadRequest(response);
            }

            return Ok(response);
        }

        /// <summary>
        /// Cập nhật một trường cụ thể — validate không cho phép sửa trường bảo mật/hệ thống.
        /// Body là JSON value trực tiếp (ví dụ: 5000000 hoặc "Nguyễn Văn A" hoặc null).
        /// </summary>
        [HttpPatch("{id:guid}/{fieldName}")]
        public virtual async Task<ActionResult<ServiceResponse>> PatchField(Guid id, string fieldName, [FromBody] JsonElement value)
        {
            var response = await _baseService.PatchFieldAsync(id, fieldName, value);

            if (!response.IsSuccess)
            {
                if (response.Code == (int)ResponseCode.NotFound)
                    return NotFound(response);
                return BadRequest(response);
            }

            return Ok(response);
        }

        /// <summary>
        /// Cập nhật nhiều trường cùng lúc — body là object JSON { "fieldName": value, ... }.
        /// </summary>
        [HttpPatch("{id:guid}/fields")]
        public virtual async Task<ActionResult<ServiceResponse>> PatchFields(Guid id, [FromBody] Dictionary<string, JsonElement> fields)
        {
            var response = await _baseService.PatchFieldsAsync(id, fields);

            if (!response.IsSuccess)
            {
                if (response.Code == (int)ResponseCode.NotFound)
                    return NotFound(response);
                return BadRequest(response);
            }

            return Ok(response);
        }
    }
}