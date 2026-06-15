using FresherMisa2026.Application.Interfaces.Repositories;
using FresherMisa2026.Application.Interfaces.Services;
using FresherMisa2026.Entities.Organization;

namespace FresherMisa2026.Application.Services
{
    /// <summary>
    /// Service cho Organization
    /// Created By: ntdo (2026-06-04)
    /// </summary>
    public class OrganizationService : BaseService<Organization>, IOrganizationService
    {
        #region Constructer

        public OrganizationService(
            IOrganizationRepository organizationRepository,
            IAuditLogRepository auditLogRepository)
            : base(organizationRepository, auditLogRepository)
        {
        }

        #endregion
    }
}
