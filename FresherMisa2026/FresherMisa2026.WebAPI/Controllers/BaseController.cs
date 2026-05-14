using FresherMisa2026.Application.Interfaces.Services;
using FresherMisa2026.Entities;
using FresherMisa2026.Entities.Enums;
using FresherMisa2026.Entities.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace FresherMisa2026.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BaseController<TEntity> : ControllerBase
    {
        private readonly IBaseService<TEntity> _baseService;
        private readonly PagingSettings _pagingSettings;

        public BaseController(IBaseService<TEntity> baseService, IOptions<PagingSettings> pagingSettings)
        {
            _baseService = baseService;
            _pagingSettings = pagingSettings.Value;
        }

        /// <summary>
        /// Danh sách paging
        /// </summary>
        [HttpGet("Paging")]
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
                PageIndex = pageIndex ?? _pagingSettings.DefaultPageIndex,
                PageSize = pageSize ?? _pagingSettings.DefaultPageSize,
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
        /// Xóa một phần tử
        /// </summary>
        [HttpDelete("{id:guid}")]
        public async Task<ActionResult<ServiceResponse>> DeleteByID(Guid id)
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
        public async Task<ActionResult<ServiceResponse>> Post([FromBody] TEntity entity)
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
        public async Task<ActionResult<ServiceResponse>> Put(Guid id, [FromBody] TEntity entity)
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
    }
}