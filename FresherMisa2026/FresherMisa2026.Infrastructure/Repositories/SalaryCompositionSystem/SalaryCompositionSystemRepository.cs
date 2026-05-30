using Dapper;
using FresherMisa2026.Application.Interfaces.Repositories;
using FresherMisa2026.Entities.AdvancedFilter;
using FresherMisa2026.Entities.SalaryCompositionSystem.DTO;
using FresherMisa2026.Entities.Settings;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Data;
using SalaryCompositionSystemEntity = FresherMisa2026.Entities.SalaryCompositionSystem.SalaryCompositionSystem;

namespace FresherMisa2026.Infrastructure.Repositories
{
    /// <summary>
    /// Repository cho SalaryCompositionSystem — dùng database amis_tien_luong
    /// Created By: Nguyen Thiet Do (2026-05-27)
    /// </summary>
    public class SalaryCompositionSystemRepository : BaseRepository<SalaryCompositionSystemEntity>, ISalaryCompositionSystemRepository
    {
        #region Constructer

        public SalaryCompositionSystemRepository(
            IConfiguration configuration,
            IMemoryCache cache,
            ILogger<BaseRepository<SalaryCompositionSystemEntity>> logger,
            IOptions<CacheSettings> cacheSettings)
            : base(configuration, cache, logger, cacheSettings)
        {
            _connectionString = configuration.GetConnectionString("SalaryConnection")!;
        }

        #endregion

        #region Methods

        private const string SelectWithJoin = @"
            SELECT sc.*, ct.Name AS ComponentTypeName
            FROM   pa_salary_composition_system sc
            LEFT JOIN pa_salary_component_type ct ON sc.ComponentTypeID = ct.ComponentTypeID";

        protected override async Task<IEnumerable<SalaryCompositionSystemEntity>> GetEntitiesUsingCommandTextAsync()
        {
            using var connection = CreateConnection();
            await connection.OpenAsync();
            return (await connection.QueryAsync<SalaryCompositionSystemEntity>(SelectWithJoin)).ToList();
        }

        protected override async Task<SalaryCompositionSystemEntity> GetEntitieByIdUsingCommandTextAsync(string id)
        {
            var sql = SelectWithJoin + " WHERE sc.SystemCompositionID = @Id";
            using var connection = CreateConnection();
            await connection.OpenAsync();
            return await connection.QueryFirstOrDefaultAsync<SalaryCompositionSystemEntity>(sql, new { Id = id });
        }

        /// <summary>
        /// Lọc thành phần lương hệ thống theo nhiều điều kiện có phân trang
        /// </summary>
        /// Created By: Nguyen Thiet Do (2026-05-27)
        public async Task<(IEnumerable<SalaryCompositionSystemEntity> Data, long Total)> FilterAsync(SalaryCompositionSystemFilterRequest request)
        {
            var parameters = new DynamicParameters();
            parameters.Add("v_Search", request.Search);
            parameters.Add("v_ComponentTypeID", request.ComponentTypeID?.ToString());
            parameters.Add("v_Nature", request.Nature.HasValue ? (int?)request.Nature : null);
            parameters.Add("v_PageIndex", request.PageIndex);
            parameters.Add("v_PageSize", request.PageSize);

            using var connection = CreateConnection();
            using var multi = await connection.QueryMultipleAsync(
                "Proc_pa_salary_composition_system_Filter",
                parameters,
                commandType: CommandType.StoredProcedure);

            var data = (await multi.ReadAsync<SalaryCompositionSystemEntity>()).ToList();
            var total = await multi.ReadFirstAsync<long>();

            return (data, total);
        }

        /// <summary>
        /// Lọc nâng cao 3 phần: search (mã/tên OR), loại thành phần, field conditions
        /// </summary>
        public async Task<(IEnumerable<SalaryCompositionSystemEntity> Data, long Total)> AdvancedFilterAsync(SalaryCompositionSystemAdvancedFilterRequest request)
        {
            var parameters = new DynamicParameters();
            var conditions = new List<string>
            {
                "SystemCompositionID NOT IN (SELECT SystemCompositionID FROM pa_salary_composition WHERE Source = 2 AND IsDeleted = FALSE)"
            };

            // Phần 1: search trên Code hoặc Name (OR)
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                parameters.Add("@_search", $"%{request.Search.Trim()}%");
                conditions.Add("(Code LIKE @_search OR Name LIKE @_search)");
            }

            // Phần 2: loại thành phần
            if (request.ComponentTypeID.HasValue)
            {
                parameters.Add("@_compTypeId", request.ComponentTypeID.Value.ToString());
                conditions.Add("ComponentTypeID = @_compTypeId");
            }

            // Phần 3: lọc nâng cao theo trường
            var userParts = new List<string>();
            BuildFilterConditions(request.Filters ?? new(), parameters, userParts);
            if (userParts.Count > 0)
            {
                var sep = request.FilterLogic == FilterLogic.Or ? " OR " : " AND ";
                conditions.Add(userParts.Count == 1
                    ? userParts[0]
                    : $"({string.Join(sep, userParts)})");
            }

            var whereSection = conditions.Count > 0 ? $"WHERE {string.Join(" AND ", conditions)}" : string.Empty;
            var orderBy = BuildSortSql(request.Sort);
            var pageIndex = Math.Max(1, request.PageIndex);
            var pageSize = Math.Max(1, request.PageSize);

            parameters.Add("@_limit", pageSize);
            parameters.Add("@_offset", (pageIndex - 1) * pageSize);

            var dataSql = $@"SELECT subq.*, ct.Name AS ComponentTypeName
                FROM (SELECT * FROM `{_tableName}` {whereSection} {orderBy} LIMIT @_limit OFFSET @_offset) subq
                LEFT JOIN pa_salary_component_type ct ON subq.ComponentTypeID = ct.ComponentTypeID
                {orderBy}";

            var countSql = $"SELECT COUNT(*) FROM `{_tableName}` {whereSection}";

            using var connection = CreateConnection();
            await connection.OpenAsync();
            var data = await connection.QueryAsync<SalaryCompositionSystemEntity>(dataSql, parameters);
            var total = await connection.ExecuteScalarAsync<long>(countSql, parameters);
            return (data.ToList(), total);
        }

        /// <summary>
        /// Lọc nâng cao 3 phần qua stored procedure
        /// </summary>
        public async Task<(IEnumerable<SalaryCompositionSystemEntity> Data, long Total)> AdvancedFilterWithProcAsync(SalaryCompositionSystemAdvancedFilterRequest request)
        {
            var parameters = new DynamicParameters();
            parameters.Add("v_pageIndex",          Math.Max(1, request.PageIndex));
            parameters.Add("v_pageSize",           Math.Max(1, request.PageSize));
            parameters.Add("v_sort",               request.Sort ?? string.Empty);
            parameters.Add("v_search",             request.Search);
            parameters.Add("v_component_type_id",  request.ComponentTypeID?.ToString());
            parameters.Add("v_filters",            System.Text.Json.JsonSerializer.Serialize(request.Filters ?? new()));
            parameters.Add("v_filter_logic",       (int)request.FilterLogic);

            using var connection = CreateConnection();
            using var multi = await connection.QueryMultipleAsync(
                "Proc_pa_salary_composition_system_AdvancedFilterPaging",
                parameters,
                commandType: CommandType.StoredProcedure);

            var data = (await multi.ReadAsync<SalaryCompositionSystemEntity>()).ToList();
            var total = await multi.ReadFirstAsync<long>();
            return (data, total);
        }

        #endregion
    }
}
