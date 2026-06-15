using FresherMisa2026.Application.Interfaces.Services;
using FresherMisa2026.Entities.Organization;
using Microsoft.AspNetCore.Mvc;

namespace FresherMisa2026.WebAPI.Controllers
{
    /// <summary>
    /// Controller quản lý đơn vị công tác
    /// Created By: ntdo (2026-06-09)
    /// </summary>
    [ApiController]
    public class OrganizationsController : BaseController<Organization>
    {
        #region Constructer

        public OrganizationsController(IOrganizationService organizationService)
            : base(organizationService)
        {
        }

        #endregion
    }
}
