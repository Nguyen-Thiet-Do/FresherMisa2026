using Dapper;
using FresherMisa2026.Application.Extensions;
using FresherMisa2026.Application.Interfaces.Repositories;
using FresherMisa2026.Entities;
using FresherMisa2026.Entities.Employee;
using FresherMisa2026.Entities.Employee.DTO;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Data;

namespace FresherMisa2026.Infrastructure.Repositories
{
    public class EmployeeRepository : BaseRepository<Employee>, IEmployeeRepository
    {
        public EmployeeRepository(IConfiguration configuration, IMemoryCache cache, ILogger<BaseRepository<Employee>> logger) : base(configuration, cache, logger)
        {
        }

        public async Task<Employee> GetEmployeeByCode(string code)
        {
            string query = SQLExtension.GetQuery("Employee.GetByCode");
            var param = new Dictionary<string, object>
            {
                {"@EmployeeCode", code }
            };
            using var connection = CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<Employee>(query, param, commandType: System.Data.CommandType.Text);
        }

        public async Task<IEnumerable<Employee>> GetEmployeesByDepartmentId(Guid departmentId)
        {
            string query = SQLExtension.GetQuery("Employee.GetByDepartmentId");
            var param = new Dictionary<string, object>
            {
                {"@DepartmentID", departmentId }
            };
            using var connection = CreateConnection();
            return await connection.QueryAsync<Employee>(query, param, commandType: System.Data.CommandType.Text);
        }

        public async Task<IEnumerable<Employee>> GetEmployeesByPositionId(Guid positionId)
        {
            string query = SQLExtension.GetQuery("Employee.GetByPositionId");
            var param = new Dictionary<string, object>
            {
                {"@PositionID", positionId }
            };
            using var connection = CreateConnection();
            return await connection.QueryAsync<Employee>(query, param, commandType: System.Data.CommandType.Text);
        }

        public async Task<IEnumerable<Employee>> FilterEmployeesAsync(EmployeeFilterRequest request)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@v_DepartmentID", request.DepartmentId);
            parameters.Add("@v_PositionID", request.PositionId);
            parameters.Add("@v_SalaryFrom", request.SalaryFrom);
            parameters.Add("@v_SalaryTo", request.SalaryTo);
            parameters.Add("@v_Gender", request.Gender);
            parameters.Add("@v_HireDateFrom", request.HireDateFrom);
            parameters.Add("@v_HireDateTo", request.HireDateTo);

            using var connection = CreateConnection();
            return await connection.QueryAsync<Employee>(
                "Proc_Employee_Filter",
                parameters,
                commandType: CommandType.StoredProcedure);
        }

        public async Task<int> CountEmployeesByDepartmentIdAsync(Guid departmentId)
        {
            const string sql = "SELECT COUNT(*) FROM Employee WHERE DepartmentID = @DepartmentID";
            using var connection = CreateConnection();
            return await connection.ExecuteScalarAsync<int>(sql, new { DepartmentID = departmentId });
        }

        public async Task<PagingResponse<Employee>> FilterEmployeesPagingAsync(EmployeeFilterRequest request)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@v_DepartmentID", request.DepartmentId);
            parameters.Add("@v_PositionID", request.PositionId);
            parameters.Add("@v_SalaryFrom", request.SalaryFrom);
            parameters.Add("@v_SalaryTo", request.SalaryTo);
            parameters.Add("@v_Gender", request.Gender);
            parameters.Add("@v_HireDateFrom", request.HireDateFrom);
            parameters.Add("@v_HireDateTo", request.HireDateTo);
            parameters.Add("@v_PageSize", request.PageSize);
            parameters.Add("@v_PageIndex", request.PageIndex);
            parameters.Add("@v_Total", dbType: System.Data.DbType.Int64, direction: System.Data.ParameterDirection.Output);

            using var connection = CreateConnection();
            var data = await connection.QueryAsync<Employee>(
                "Proc_Employee_Filter_Paging",
                parameters,
                commandType: CommandType.StoredProcedure);

            var total = parameters.Get<long>("@v_Total");

            return new PagingResponse<Employee>
            {
                Total = total,
                Data = data.ToList()
            };
        }
    }
}