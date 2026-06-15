using FresherMisa2026.Entities.Organization;

namespace FresherMisa2026.Application.Interfaces.Repositories
{
    /// <summary>
    /// Interface repository cho Organization
    /// Created By: ntdo (2026-06-04)
    /// </summary>
    public interface IOrganizationRepository : IBaseRepository<Organization>
    {
        /// <summary>Lấy ID các đơn vị gốc (không có cha) — thay thế GetAll+filter in-memory.</summary>
        /// Created By: ntdo (2026-06-04)
        Task<IEnumerable<Guid>> GetRootOrganizationIdsAsync();
    }
}
