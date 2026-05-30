using FresherMisa2026.Application.Interfaces.Services;
using FresherMisa2026.Entities.Organization;
using FresherMisa2026.Entities.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace FresherMisa2026.WebAPI.Controllers
{
    /// <summary>
    /// Controller quản lý đơn vị công tác
    /// Created By: Nguyen Thiet Do (2026-05-27)
    /// </summary>
    [ApiController]
    public class OrganizationsController : BaseController<Organization>
    {
        #region Constructer

        public OrganizationsController(
            IOrganizationService organizationService,
            IOptions<PagingSettings> pagingSettings)
            : base(organizationService, pagingSettings)
        {
        }

        #endregion
    }
}
