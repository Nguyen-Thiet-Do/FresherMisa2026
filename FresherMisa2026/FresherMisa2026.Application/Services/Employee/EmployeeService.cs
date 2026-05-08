using FresherMisa2026.Application.Interfaces;
using FresherMisa2026.Application.Interfaces.Repositories;
using FresherMisa2026.Application.Interfaces.Services;
using FresherMisa2026.Entities;
using FresherMisa2026.Entities.Employee;
using FresherMisa2026.Entities.Employee.DTO;
using FresherMisa2026.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace FresherMisa2026.Application.Services
{
    public partial class EmployeeService : BaseService<Employee>, IEmployeeService
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IDepartmentRepository _departmentRepository;
        private readonly IPositionRepository _positionRepository;
        private static readonly Regex EmailRegex = new(@"^[^\s@]+@[^\s@]+\.[^\s@]+$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex PhoneNumberRegex = new(@"^(?:0|\+84)(?:3|5|7|8|9)\d{8}$", RegexOptions.Compiled);

        public EmployeeService(
            IBaseRepository<Employee> baseRepository,
            IEmployeeRepository employeeRepository,
            IDepartmentRepository departmentRepository,
            IPositionRepository positionRepository
            ) : base(baseRepository)
        {
            _employeeRepository = employeeRepository;
            _departmentRepository = departmentRepository;
            _positionRepository = positionRepository;
        }

        public async Task<Employee> GetEmployeeByCodeAsync(string code)
        {
            var employee = await _employeeRepository.GetEmployeeByCode(code);
            if (employee == null)
                throw new KeyNotFoundException("Không tìm thấy nhân viên");

            return employee;
        }

        public async Task<IEnumerable<Employee>> GetEmployeesByDepartmentIdAsync(Guid departmentId)
        {
            return await _employeeRepository.GetEmployeesByDepartmentId(departmentId);
        }

        public async Task<IEnumerable<Employee>> GetEmployeesByPositionIdAsync(Guid positionId)
        {
            return await _employeeRepository.GetEmployeesByPositionId(positionId);
        }

        protected override async Task<List<ValidationError>> ValidateBeforeInsertAsync(Employee employee)
        {
            return await ValidateBusinessRulesAsync(employee, null);
        }

        protected override async Task<List<ValidationError>> ValidateBeforeUpdateAsync(Guid entityId, Employee employee)
        {
            return await ValidateBusinessRulesAsync(employee, entityId);
        }

        /// <summary>
        /// override phương thức ValidateCustom để thực hiện các kiểm tra tùy chỉnh cho đối tượng Employee trước khi lưu vào cơ sở dữ liệu.
        /// </summary>
        /// <param name="employee"></param>
        /// <returns></returns>

        protected override List<ValidationError> ValidateCustom(Employee employee)
        {
            var errors = new List<ValidationError>();

            if (!string.IsNullOrEmpty(employee.EmployeeCode) && employee.EmployeeCode.Length > 20)
            {
                errors.Add(new ValidationError("EmployeeCode", "Mã nhân viên không được vượt quá 20 ký tự"));
            }

            if (string.IsNullOrEmpty(employee.EmployeeName))
            {
                errors.Add(new ValidationError("EmployeeName", "Tên nhân viên không được để trống"));
            }
            // kiểm tra ngày sinh không được lớn hơn ngày hiện tại(nếu có nhập nagyf sinh)
            if (employee.DateOfBirth.HasValue && employee.DateOfBirth.Value > DateTime.Now)
            {
                errors.Add(new ValidationError("DateOfBirth", "Ngày sinh không được lớn hơn ngày hiện tại"));
            }
            //kiểm tra email có đúng định dạng hay không (nếu có nhập email)
            if (!string.IsNullOrWhiteSpace(employee.Email) && !EmailRegex.IsMatch(employee.Email.Trim()))
            {
                errors.Add(new ValidationError("Email", "Email không đúng định dạng"));
            }

            if (!string.IsNullOrWhiteSpace(employee.PhoneNumber) && !PhoneNumberRegex.IsMatch(employee.PhoneNumber.Trim()))
            {
                errors.Add(new ValidationError("PhoneNumber", "Số điện thoại không đúng định dạng"));
            }

            return errors;
        }
        /// <summary>
        /// validateBusinessRulesAsync để thực hiện các kiểm tra liên quan đến nghiệp vụ
        /// kiểm tra mã nhân viên trùng lặp, 
        /// kiểm tra sự tồn tại của phòng ban và vị trí liên quan đến nhân viên.
        /// </summary>
        /// <param name="employee"></param>
        /// <param name="currentEmployeeId"></param>
        /// <returns></returns>
        /// Created By: ntdo (17/04/2026)

        private async Task<List<ValidationError>> ValidateBusinessRulesAsync(Employee employee, Guid? currentEmployeeId)
        {
            var errors = new List<ValidationError>();

            var duplicateCodeError = await ValidateDuplicateCodeAsync(employee.EmployeeCode, currentEmployeeId);
            if (duplicateCodeError != null)
            {
                errors.Add(duplicateCodeError);
            }

            if (employee.DepartmentID == Guid.Empty)
            {
                errors.Add(new ValidationError(nameof(Employee.DepartmentID), "Phòng ban không được để trống"));
            }
            else
            {
                var department = await _departmentRepository.GetEntityByIDAsync(employee.DepartmentID);
                if (department == null)
                {
                    errors.Add(new ValidationError(nameof(Employee.DepartmentID), "Phòng ban không tồn tại"));
                }
            }

            if (employee.PositionID == Guid.Empty)
            {
                errors.Add(new ValidationError(nameof(Employee.PositionID), "Vị trí không được để trống"));
            }
            else
            {
                var position = await _positionRepository.GetEntityByIDAsync(employee.PositionID);
                if (position == null)
                {
                    errors.Add(new ValidationError(nameof(Employee.PositionID), "Vị trí không tồn tại"));
                }
            }

            return errors;
        }
        /// <summary>
        /// kiểm tra xem mã nhân viên đã tồn tại trong hệ thống hay chưa  
        /// Nếu mã nhân viên đã tồn tại và không phải là mã của nhân viên hiện tại (trong trường hợp cập nhật), 
        /// hàm sẽ trả về một lỗi xác thực cho biết rằng mã nhân viên đã tồn tại.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// Created By: ntdo (17/04/2026)
        private async Task<ValidationError?> ValidateDuplicateCodeAsync(string? employeeCode, Guid? currentEmployeeId)
        {
            if (string.IsNullOrWhiteSpace(employeeCode))
                return null;
            var existingEmployee = await _employeeRepository.GetEmployeeByCode(employeeCode.Trim());
            if (existingEmployee == null)
            {
                return null;
            }
            if (!currentEmployeeId.HasValue || existingEmployee.EmployeeID != currentEmployeeId.Value)
            {
                return new ValidationError(nameof(Employee.EmployeeCode), "Mã nhân viên đã tồn tại");
            }
            return null;
        }
        /// <summary>
        /// lọc nhân viên theo các tiêu chí như phòng ban, 
        /// vị trí, mức lương, giới tính và ngày tuyển dụng.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// Created By: ntdo (17/04/2026)
        public async Task<ServiceResponse> FilterEmployeesAsync(EmployeeFilterRequest request)
        {
            var validationError = ValidateFilterRequest(request);
            if (validationError != null) return validationError;

            var data = await _employeeRepository.FilterEmployeesAsync(request);
            return CreateSuccessResponse(data);
        }

        public async Task<ServiceResponse> FilterEmployeesPagingAsync(EmployeeFilterRequest request)
        {
            var validationError = ValidateFilterRequest(request);
            if (validationError != null) return validationError;

            if (request.PageSize <= 0) request.PageSize = 10;
            if (request.PageIndex <= 0) request.PageIndex = 1;

            var pagingResult = await _employeeRepository.FilterEmployeesPagingAsync(request);

            return CreateSuccessResponse(new
            {
                pagingResult.Total,
                PageSize = request.PageSize,
                PageIndex = request.PageIndex,
                pagingResult.Data
            });
        }

        private ServiceResponse? ValidateFilterRequest(EmployeeFilterRequest request)
        {
            if (request.SalaryFrom.HasValue && request.SalaryTo.HasValue && request.SalaryFrom > request.SalaryTo)
                return CreateErrorResponse(ResponseCode.BadRequest,
                    "salaryFrom không được lớn hơn salaryTo",
                    "salaryFrom không được lớn hơn salaryTo");

            if (request.HireDateFrom.HasValue && request.HireDateTo.HasValue && request.HireDateFrom > request.HireDateTo)
                return CreateErrorResponse(ResponseCode.BadRequest,
                    "hireDateFrom không được lớn hơn hireDateTo",
                    "hireDateFrom không được lớn hơn hireDateTo");

            if (request.Gender.HasValue && request.Gender is < 0 or > 2)
                return CreateErrorResponse(ResponseCode.BadRequest,
                    "gender chỉ nhận các giá trị 0, 1, 2",
                    "gender chỉ nhận các giá trị 0, 1, 2");

            return null;
        }
    }
}