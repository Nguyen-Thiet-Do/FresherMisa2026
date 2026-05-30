using FresherMisa2026.Application.Interfaces.Repositories;
using FresherMisa2026.Application.Interfaces.Services;
using FresherMisa2026.Entities.SalaryComponentType;

namespace FresherMisa2026.Application.Services
{
    /// <summary>
    /// Service cho SalaryComponentType
    /// Created By: Nguyen Thiet Do (2026-05-27)
    /// </summary>
    public class SalaryComponentTypeService : BaseService<SalaryComponentType>, ISalaryComponentTypeService
    {
        #region Constructer

        public SalaryComponentTypeService(ISalaryComponentTypeRepository repository)
            : base(repository)
        {
        }

        #endregion
    }
}
