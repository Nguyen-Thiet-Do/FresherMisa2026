using FresherMisa2026.Application.Interfaces;
using FresherMisa2026.Application.Interfaces.Repositories;
using FresherMisa2026.Application.Interfaces.Services;
using FresherMisa2026.Infrastructure.Repositories;
using FresherMisa2026.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
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
    }
}
