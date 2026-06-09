using FresherMisa2026.Application.Interfaces.Services;
using FresherMisa2026.Entities;
using FresherMisa2026.Entities.Enums;
using FresherMisa2026.Entities.Position;
using Microsoft.AspNetCore.Mvc;

namespace FresherMisa2026.WebAPI.Controllers
{
    [ApiController]
    public class PositionsController : BaseController<Position>
    {
        private readonly IPositionService _positionService;

        public PositionsController(IPositionService positionService) : base(positionService)
        {
            _positionService = positionService;
        }

        [HttpGet("code/{code}")]
        public async Task<ActionResult<ServiceResponse>> GetByCode(string code)
        {
            var response = await _positionService.GetPositionByCodeAsync(code);

            if (!response.IsSuccess && response.Code == (int)ResponseCode.NotFound)
                return NotFound(response);

            if (!response.IsSuccess && response.Code == (int)ResponseCode.BadRequest)
                return BadRequest(response);

            return Ok(response);
        }
    }
}