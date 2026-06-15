using FresherMisa2026.Application.Interfaces;
using FresherMisa2026.Application.Interfaces.Repositories;
using FresherMisa2026.Application.Interfaces.Services;
using FresherMisa2026.Entities;
using FresherMisa2026.Entities.Department;
using FresherMisa2026.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace FresherMisa2026.Application.Services
{
    /// <summary>
    /// Service cho Department
    /// Created By: ntdo (2026-04-10)
    /// </summary>
    public class DepartmentService : BaseService<Department>, IDepartmentService
    {
        private readonly IDepartmentRepository _deptRepository;
        private readonly IEmployeeRepository _employeeRepository;

        public DepartmentService(
            IBaseRepository<Department> baseRepository,
            IDepartmentRepository departmentRepository,
            IEmployeeRepository employeeRepository,
            IAuditLogRepository auditLogRepository
            ) : base(baseRepository, auditLogRepository)
        {
            _deptRepository = departmentRepository;
            _employeeRepository = employeeRepository;
        }

        /// <summary>
        /// Lấy department theo code
        /// </summary>
        /// <returns></returns>
        /// Created By: ntdo (2026-04-10)
        public async Task<ServiceResponse> GetDepartmentByCodeAsync(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return CreateErrorResponse(ResponseCode.BadRequest,
                    "Mã phòng ban không được để trống",
                    "Mã phòng ban không được để trống");
            }

            var department = await _deptRepository.GetDepartmentByCode(code.Trim());
            if (department == null)
            {
                return CreateErrorResponse(ResponseCode.NotFound,
                    "Không tìm thấy phòng ban",
                    "Không tìm thấy phòng ban");
            }

            return CreateSuccessResponse(department);
        }
        /// <summary>
        /// lấy nhân viên theo mã phòng ban
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        /// Created By: ntdo (2026-04-12)
        public async Task<ServiceResponse> GetEmployeesByDepartmentCodeAsync(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return CreateErrorResponse(ResponseCode.BadRequest,
                    "Mã phòng ban không được để trống",
                    "Mã phòng ban không được để trống");
            }

            var department = await _deptRepository.GetDepartmentByCode(code.Trim());
            if (department == null)
            {
                return CreateErrorResponse(ResponseCode.NotFound,
                    "Không tìm thấy phòng ban",
                    "Không tìm thấy phòng ban");
            }

            var employees = await _employeeRepository.GetEmployeesByDepartmentId(department.DepartmentID);
            return CreateSuccessResponse(employees);
        }

        /// <summary>
        /// đếm số lượng nhân viên theo mã phòng ban
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        /// Created By: ntdo (2026-04-14)

        public async Task<ServiceResponse> GetEmployeeCountByDepartmentCodeAsync(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return CreateErrorResponse(ResponseCode.BadRequest,
                    "Mã phòng ban không được để trống",
                    "Mã phòng ban không được để trống");
            }

            var department = await _deptRepository.GetDepartmentByCode(code.Trim());
            if (department == null)
            {
                return CreateErrorResponse(ResponseCode.NotFound,
                    "Không tìm thấy phòng ban",
                    "Không tìm thấy phòng ban");
            }

            var count = await _employeeRepository.CountEmployeesByDepartmentIdAsync(department.DepartmentID);
            return CreateSuccessResponse(count);
        }

        #region OVERRIDE METHODS
        /// <summary>Kiểm tra còn nhân viên trong phòng ban không trước khi xóa</summary>
        /// Created By: ntdo (2026-04-15)
        protected override async Task<bool> ValidateBeforeDeleteAsync(Guid entityId, Department existingEntity)
        {
            var count = await _employeeRepository.CountEmployeesByDepartmentIdAsync(entityId);
            return count == 0;
        }

        /// <summary>Trả về thông báo khi block xóa phòng ban</summary>
        /// Created By: ntdo (2026-04-15)
        protected override Task<string?> GetDeleteValidationMessageAsync(Guid entityId, Department existingEntity)
        {
            return Task.FromResult<string?>("Không thể xóa phòng ban vì vẫn còn nhân viên thuộc phòng ban này");
        }

        /// <summary>
        /// Validate tùy chỉnh cho Department
        /// </summary>
        /// Created By: ntdo (2026-04-17)
        protected override List<ValidationError> ValidateCustom(Department department)
        {
            var errors = new List<ValidationError>();

            // Ví dụ: Kiểm tra mã phòng ban không được vượt quá 20 ký tự
            if (!string.IsNullOrEmpty(department.DepartmentCode) && department.DepartmentCode.Length > 20)
            {
                errors.Add(new ValidationError("DepartmentCode", "Mã phòng ban không được vượt quá 20 ký tự"));
            }

            return errors;
        }
        #endregion OVERRIDE METHODS
    }
}
