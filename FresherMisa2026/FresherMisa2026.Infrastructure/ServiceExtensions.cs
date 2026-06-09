using Dapper;
using FresherMisa2026.Application.Interfaces;
using FresherMisa2026.Application.Interfaces.Repositories;
using FresherMisa2026.Application.Interfaces.Services;
using FresherMisa2026.Entities;
using FresherMisa2026.Entities.Extensions;
using FresherMisa2026.Infrastructure.Repositories;
using FresherMisa2026.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace FresherMisa2026.Infrastructure
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services)
        {
            Dapper.SqlMapper.AddTypeHandler(new GuidTypeHandler());
            Dapper.SqlMapper.AddTypeHandler(new TypeHandlers.GuidListTypeHandler());

            // Dapper map snake_case column → PascalCase property cho entity có [ConfigTable(useSnakeCase: true)]
            RegisterSnakeCaseTypeMaps();

            //base
            services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));

            services.AddScoped<IDepartmentRepository, DepartmentRepository>();
            services.AddScoped<IPositionRepository, PositionRepository>();
            services.AddScoped<IEmployeeRepository, EmployeeRepository>();
            services.AddScoped<ICandidateRepository, CandidateRepository>();
            services.AddScoped<ISalaryCompositionRepository, SalaryCompositionRepository>();
            services.AddScoped<IOrganizationRepository, OrganizationRepository>();
            services.AddScoped<ISalaryComponentTypeRepository, SalaryComponentTypeRepository>();
            services.AddScoped<ISalaryCompositionSystemRepository, SalaryCompositionSystemRepository>();
            services.AddScoped<IGridConfigRepository, GridConfigRepository>();

            services.AddScoped<IFileService, FileService>();

            // đăng ký cache
            services.AddMemoryCache();
            return services;
        }

        /// <summary>
        /// Scan assembly Entities tìm các type có ConfigTable.UseSnakeCase = true → đăng ký
        /// Dapper CustomPropertyTypeMap để map column name (snake_case) về property C# (PascalCase).
        /// </summary>
        private static void RegisterSnakeCaseTypeMaps()
        {
            var entityAssembly = typeof(BaseModel).Assembly;
            var snakeTypes = entityAssembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract)
                .Where(t => t.GetCustomAttribute<ConfigTable>()?.UseSnakeCase == true);

            foreach (var type in snakeTypes)
            {
                SqlMapper.SetTypeMap(type, new CustomPropertyTypeMap(
                    type,
                    (t, columnName) => t.GetPropertyByColumnName(columnName)!));
            }
        }
    }
}
