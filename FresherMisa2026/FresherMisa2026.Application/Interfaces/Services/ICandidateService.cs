using FresherMisa2026.Entities;
using FresherMisa2026.Entities.Candidate;
using FresherMisa2026.Entities.Candidate.DTO;

namespace FresherMisa2026.Application.Interfaces.Services
{
    public interface ICandidateService : IBaseService<Candidate>
    {
        /// <summary>
        /// Lọc ứng viên theo các tiêu chí và phân trang
        /// </summary>
        Task<ServiceResponse> FilterCandidatesPagingAsync(CandidateFilterRequest request);
    }
}
