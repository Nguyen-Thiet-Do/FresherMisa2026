using FresherMisa2026.Entities;
using FresherMisa2026.Entities.Candidate;
using FresherMisa2026.Entities.Candidate.DTO;

namespace FresherMisa2026.Application.Interfaces.Repositories
{
    public interface ICandidateRepository : IBaseRepository<Candidate>
    {
        /// <summary>
        /// Lọc ứng viên theo các tiêu chí và phân trang
        /// </summary>
        Task<PagingResponse<Candidate>> FilterCandidatesPagingAsync(CandidateFilterRequest request);

        /// <summary>
        /// Lấy ứng viên theo số điện thoại
        /// </summary>
        Task<Candidate?> GetByPhoneNumberAsync(string phoneNumber);

        /// <summary>
        /// Lấy ứng viên theo email
        /// </summary>
        Task<Candidate?> GetByEmailAsync(string email);
    }
}
