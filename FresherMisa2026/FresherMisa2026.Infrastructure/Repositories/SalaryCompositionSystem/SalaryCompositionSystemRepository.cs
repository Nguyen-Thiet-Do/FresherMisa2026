using Dapper;
using FresherMisa2026.Application.Interfaces.Repositories;
using FresherMisa2026.Entities.AdvancedFilter;
using FresherMisa2026.Entities.Extensions;
using FresherMisa2026.Entities.SalaryComposition;
using FresherMisa2026.Entities.SalaryCompositionSystem.DTO;
using FresherMisa2026.Infrastructure.Persistence.Queries;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Text.Json;
using System.Text.Json.Serialization;
using SalaryCompositionSystemEntity = FresherMisa2026.Entities.SalaryCompositionSystem.SalaryCompositionSystem;

namespace FresherMisa2026.Infrastructure.Repositories
{
    /// <summary>
    /// Repository cho SalaryCompositionSystem — dùng database amis_tien_luong (SalaryConnection).
    /// </summary>
    /// <remarks>Created by: ntdo — 2026-06-06 · Refactor: 03/06/2026</remarks>
    public class SalaryCompositionSystemRepository
        : BaseRepository<SalaryCompositionSystemEntity>, ISalaryCompositionSystemRepository
    {
        #region Declare

        private static readonly JsonSerializerOptions CamelCaseOptions = new()
        {
            PropertyNamingPolicy   = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        };

        #endregion

        #region Constructer

        public SalaryCompositionSystemRepository(
            IConfiguration configuration,
            ILogger<BaseRepository<SalaryCompositionSystemEntity>> logger)
            : base(configuration, logger)
        {
            _connectionString = configuration.GetConnectionString(SalaryCompositionConstants.ConnectionName)!;
        }

        #endregion

        #region OVERRIDE METHODS - Get

        /// <summary>
        /// Lấy toàn bộ danh mục TPL hệ thống kèm tên loại.
        /// </summary>
        /// <remarks>Created by: ntdo — 2026-06-06</remarks>
        protected override async Task<IEnumerable<SalaryCompositionSystemEntity>> GetEntitiesUsingCommandTextAsync()
        {
            using var connection = CreateConnection();
            await connection.OpenAsync();
            return (await connection.QueryAsync<SalaryCompositionSystemEntity>(
                SalaryQueries.SelectSalaryCompositionSystemWithJoin)).ToList();
        }

        /// <summary>Lấy 1 TPL hệ thống theo ID.</summary>
        /// <param name="id">ID dạng chuỗi (Guid.ToString()).</param>
        /// <remarks>Created by: ntdo — 2026-06-06</remarks>
        protected override async Task<SalaryCompositionSystemEntity> GetEntitieByIdUsingCommandTextAsync(string id)
        {
            var sql = SalaryQueries.SelectSalaryCompositionSystemWithJoin
                    + " WHERE sc.`system_composition_id` = @Id";
            using var connection = CreateConnection();
            await connection.OpenAsync();
            return await connection.QueryFirstOrDefaultAsync<SalaryCompositionSystemEntity>(sql, new { Id = id });
        }

        #endregion

        #region Methods - Lookup

        /// <summary>Tìm TPL hệ thống theo Code — trả null nếu không tồn tại.</summary>
        public async Task<SalaryCompositionSystemEntity?> GetByCodeAsync(string code)
        {
            var sql = SalaryQueries.SelectSalaryCompositionSystem
                    + " WHERE `code` = @code";
            using var connection = CreateConnection();
            await connection.OpenAsync();
            return await connection.QueryFirstOrDefaultAsync<SalaryCompositionSystemEntity>(sql, new { code });
        }

        #endregion

        #region Methods - Filter

        /// <summary>Lọc TPL hệ thống có phân trang qua stored procedure.</summary>
        /// <param name="request">Điều kiện lọc cơ bản.</param>
        /// <returns>Dữ liệu trang hiện tại và tổng số.</returns>
        /// <remarks>Created by: ntdo — 2026-06-06</remarks>
        public async Task<(IEnumerable<SalaryCompositionSystemEntity> Data, long Total)>
            FilterAsync(SalaryCompositionSystemFilterRequest request)
        {
            var parameters = new DynamicParameters();
            parameters.Add("v_search",            request.Search);
            parameters.Add("v_component_type_id", request.ComponentTypeID?.ToString());
            parameters.Add("v_nature",            request.Nature.HasValue ? (int?)request.Nature : null);
            parameters.Add("v_page_index",        request.PageIndex);
            parameters.Add("v_page_size",         request.PageSize);
            parameters.Add("v_exclude_inherited", request.ExcludeInherited ? 1 : 0);

            using var connection = CreateConnection();
            await connection.OpenAsync();
            using var multi = await connection.QueryMultipleAsync(
                SalaryQueries.ProcSalaryCompositionSystemFilter,
                parameters,
                commandType: CommandType.StoredProcedure);

            var data  = (await multi.ReadAsync<SalaryCompositionSystemEntity>()).ToList();
            var total = await multi.ReadFirstAsync<long>();
            return (data, total);
        }

        /// <summary>Lọc nâng cao 3 phần qua stored procedure.</summary>
        /// <param name="request">Điều kiện lọc nâng cao.</param>
        /// <returns>Dữ liệu trang hiện tại và tổng số.</returns>
        /// <remarks>Created by: ntdo — 2026-06-06</remarks>
        public async Task<(IEnumerable<SalaryCompositionSystemEntity> Data, long Total)>
            AdvancedFilterWithProcAsync(SalaryCompositionSystemAdvancedFilterRequest request)
        {
            var parameters = new DynamicParameters();
            parameters.Add("v_page_index",        Math.Max(1, request.PageIndex));
            parameters.Add("v_page_size",         Math.Max(1, request.PageSize));
            parameters.Add("v_sort",              ConvertSortToSnakeCase(request.Sort) ?? string.Empty);
            parameters.Add("v_search",            request.Search);
            parameters.Add("v_search_fields",     request.SearchFields?.Count > 0
                ? JsonSerializer.Serialize(request.SearchFields.Select(Naming.ToSnakeCase)) : null);
            parameters.Add("v_component_type_id", request.ComponentTypeID?.ToString());
            parameters.Add("v_filters",           JsonSerializer.Serialize(ProjectFiltersForSp(request.Filters ?? new()), CamelCaseOptions));
            parameters.Add("v_filter_logic",      (int)request.FilterLogic);
            parameters.Add("v_columns",           request.Columns is { Count: > 0 }
                ? JsonSerializer.Serialize(request.Columns.Select(Naming.ToSnakeCase))
                : null);
            parameters.Add("v_exclude_inherited", request.ExcludeInherited ? 1 : 0);

            using var connection = CreateConnection();
            await connection.OpenAsync();
            using var multi = await connection.QueryMultipleAsync(
                SalaryQueries.ProcSalaryCompositionSystemAdvanced,
                parameters,
                commandType: CommandType.StoredProcedure);

            var data  = (await multi.ReadAsync<SalaryCompositionSystemEntity>()).ToList();
            var total = await multi.ReadFirstAsync<long>();
            return (data, total);
        }

        /// <summary>Convert sort string từ PascalCase sang snake_case, giữ nguyên prefix - hoặc +.</summary>
        private static string? ConvertSortToSnakeCase(string? sort)
        {
            if (string.IsNullOrEmpty(sort)) return sort;
            return string.Join(",", sort.Split(',').Select(part =>
            {
                part = part.Trim();
                if (part.StartsWith('-') || part.StartsWith('+'))
                    return part[0] + Naming.ToSnakeCase(part[1..]);
                return Naming.ToSnakeCase(part);
            }));
        }

        #endregion
    }
}
