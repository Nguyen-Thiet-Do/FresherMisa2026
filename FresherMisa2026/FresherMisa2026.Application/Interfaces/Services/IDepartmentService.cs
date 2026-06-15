
using FresherMisa2026.Entities;
using FresherMisa2026.Entities.Department;
using System;
using System.Collections.Generic;
using System.Text;

namespace FresherMisa2026.Application.Interfaces.Services
{
    /// <summary>
    /// Interface service cho Department
    /// Created By: ntdo (2026-04-10)
    /// </summary>
    public interface IDepartmentService : IBaseService<Department>
    {
        /// <summary>
        /// Lấy department theo code
        /// </summary>
        /// <returns></returns>
        /// Created By: ntdo (2026-04-10)
        Task<ServiceResponse> GetDepartmentByCodeAsync(string code);

        /// <summary>Lấy nhân viên theo mã phòng ban</summary>
        /// Created By: ntdo (2026-04-10)
        Task<ServiceResponse> GetEmployeesByDepartmentCodeAsync(string code);

        /// <summary>Đếm số lượng nhân viên theo mã phòng ban</summary>
        /// Created By: ntdo (2026-04-10)
        Task<ServiceResponse> GetEmployeeCountByDepartmentCodeAsync(string code);
    }
}
