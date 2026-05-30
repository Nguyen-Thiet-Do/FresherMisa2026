using FresherMisa2026.Application.Interfaces.Repositories;
using FresherMisa2026.Application.Interfaces.Services;
using FresherMisa2026.Entities.Organization;

namespace FresherMisa2026.Application.Services
{
    /// <summary>
    /// Service cho Organization
    /// Created By: Nguyen Thiet Do (2026-05-27)
    /// </summary>
    public class OrganizationService : BaseService<Organization>, IOrganizationService
    {
        #region Constructer

        public OrganizationService(IOrganizationRepository organizationRepository)
            : base(organizationRepository)
        {
        }

        #endregion
    }
}
