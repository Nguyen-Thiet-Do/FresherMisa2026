using FresherMisa2026.Application.Interfaces.Services;
using FresherMisa2026.Entities.SalaryComponentType;
using Microsoft.AspNetCore.Mvc;

namespace FresherMisa2026.WebAPI.Controllers
{
    /// <summary>
    /// Controller quản lý loại thành phần lương
    /// Created By: ntdo (2026-06-09)
    /// </summary>
    [ApiController]
    public class SalaryComponentTypesController : BaseController<SalaryComponentType>
    {
        #region Constructer

        public SalaryComponentTypesController(ISalaryComponentTypeService salaryComponentTypeService)
            : base(salaryComponentTypeService)
        {
        }

        #endregion
    }
}
