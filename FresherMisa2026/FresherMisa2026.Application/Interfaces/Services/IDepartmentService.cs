
using FresherMisa2026.Entities;
using FresherMisa2026.Entities.Department;
using System;
using System.Collections.Generic;
using System.Text;

namespace FresherMisa2026.Application.Interfaces.Services
{
    public interface IDepartmentService : IBaseService<Department>
    {
        /// <summary>
        /// Lấy department theo code
        /// </summary>
        /// <returns></returns>
        /// Created By: dvhai (10/04/2026)
        Task<ServiceResponse> GetDepartmentByCodeAsync(string code);
        Task<ServiceResponse> GetEmployeesByDepartmentCodeAsync(string code);
        Task<ServiceResponse> GetEmployeeCountByDepartmentCodeAsync(string code);
    }
}
