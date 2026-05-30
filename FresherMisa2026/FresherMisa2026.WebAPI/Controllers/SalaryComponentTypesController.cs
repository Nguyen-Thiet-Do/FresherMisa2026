using FresherMisa2026.Application.Interfaces.Services;
using FresherMisa2026.Entities.SalaryComponentType;
using FresherMisa2026.Entities.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace FresherMisa2026.WebAPI.Controllers
{
    /// <summary>
    /// Controller quản lý loại thành phần lương
    /// Created By: Nguyen Thiet Do (2026-05-27)
    /// </summary>
    [ApiController]
    public class SalaryComponentTypesController : BaseController<SalaryComponentType>
    {
        #region Constructer

        public SalaryComponentTypesController(
            ISalaryComponentTypeService salaryComponentTypeService,
            IOptions<PagingSettings> pagingSettings)
            : base(salaryComponentTypeService, pagingSettings)
        {
        }

        #endregion
    }
}
