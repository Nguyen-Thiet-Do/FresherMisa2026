using FresherMisa2026.Application.Interfaces.Repositories;
using FresherMisa2026.Application.Interfaces.Services;
using FresherMisa2026.Entities.SalaryComponentType;

namespace FresherMisa2026.Application.Services
{
    /// <summary>
    /// Service cho SalaryComponentType
    /// Created By: ntdo (2026-06-05)
    /// </summary>
    public class SalaryComponentTypeService : BaseService<SalaryComponentType>, ISalaryComponentTypeService
    {
        #region Constructer

        public SalaryComponentTypeService(
            ISalaryComponentTypeRepository repository,
            IAuditLogRepository auditLogRepository)
            : base(repository, auditLogRepository)
        {
        }

        #endregion
    }
}
