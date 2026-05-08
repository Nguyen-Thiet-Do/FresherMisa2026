using FresherMisa2026.Application.Interfaces.Services;
using FresherMisa2026.Entities;
using FresherMisa2026.Entities.Candidate;
using FresherMisa2026.Entities.Candidate.DTO;
using Microsoft.AspNetCore.Mvc;

namespace FresherMisa2026.WebAPI.Controllers
{
    [ApiController]
    public class CandidatesController : BaseController<Candidate>
    {
        private readonly ICandidateService _candidateService;

        public CandidatesController(
            ICandidateService candidateService) : base(candidateService)
        {
            _candidateService = candidateService;
        }

        /// <summary>
        /// Lọc ứng viên theo các tiêu chí và phân trang
        /// </summary>
        [HttpGet("filter")]
        public async Task<ActionResult<ServiceResponse>> Filter([FromQuery] CandidateFilterRequest request)
        {
            var response = await _candidateService.FilterCandidatesPagingAsync(request);

            if (!response.IsSuccess)
                return BadRequest(response);

            return Ok(response);
        }
    }
}
