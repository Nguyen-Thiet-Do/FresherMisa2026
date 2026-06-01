using Dapper;
using FresherMisa2026.Application.Interfaces.Repositories;
using FresherMisa2026.Entities.AdvancedFilter;
using FresherMisa2026.Entities.SalaryComposition.DTO;
using FresherMisa2026.Entities.Settings;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using SalaryCompositionEntity = FresherMisa2026.Entities.SalaryComposition.SalaryComposition;

namespace FresherMisa2026.Infrastructure.Repositories
{
    /// <summary>
    /// Repository cho SalaryComposition — dùng database amis_tien_luong
    /// Created By: Nguyen Thiet Do (2026-05-27)
    /// </summary>
    public class SalaryCompositionRepository : BaseRepository<SalaryCompositionEntity>, ISalaryCompositionRepository
    {
        #region Constructer

        public SalaryCompositionRepository(
            IConfiguration configuration,
            IMemoryCache cache,
            ILogger<BaseRepository<SalaryCompositionEntity>> logger,
            IOptions<CacheSettings> cacheSettings)
            : base(configuration, cache, logger, cacheSettings)
        {
            _connectionString = configuration.GetConnectionString("SalaryConnection")!;
        }

        #endregion

        #region Methods

        // Correlated subqueries để lấy danh sách đơn vị từ junction table
        private const string OrgIDsSubquery = @"
            (SELECT GROUP_CONCAT(sco.OrganizationID ORDER BY sco.OrganizationID SEPARATOR ',')
             FROM pa_salary_composition_organization sco
             WHERE sco.SalaryCompositionID = sc.SalaryCompositionID) AS OrganizationIDs";

        private const string OrgNamesSubquery = @"
            (SELECT GROUP_CONCAT(org2.Name ORDER BY sco2.OrganizationID SEPARATOR ', ')
             FROM pa_salary_composition_organization sco2
             JOIN pa_organization org2 ON sco2.OrganizationID = org2.OrganizationID
             WHERE sco2.SalaryCompositionID = sc.SalaryCompositionID) AS OrganizationNames";

        private const string SelectWithJoin = @"
            SELECT sc.*, ct.Name AS ComponentTypeName,
                   (SELECT GROUP_CONCAT(sco.OrganizationID ORDER BY sco.OrganizationID SEPARATOR ',')
                    FROM pa_salary_composition_organization sco
                    WHERE sco.SalaryCompositionID = sc.SalaryCompositionID) AS OrganizationIDs,
                   (SELECT GROUP_CONCAT(org2.Name ORDER BY sco2.OrganizationID SEPARATOR ', ')
                    FROM pa_salary_composition_organization sco2
                    JOIN pa_organization org2 ON sco2.OrganizationID = org2.OrganizationID
                    WHERE sco2.SalaryCompositionID = sc.SalaryCompositionID) AS OrganizationNames
            FROM   pa_salary_composition sc
            LEFT JOIN pa_salary_component_type ct ON sc.ComponentTypeID = ct.ComponentTypeID";

        protected override async Task<IEnumerable<SalaryCompositionEntity>> GetEntitiesUsingCommandTextAsync()
        {
            var sql = SelectWithJoin + " WHERE sc.IsDeleted = FALSE";
            using var connection = CreateConnection();
            await connection.OpenAsync();
            return (await connection.QueryAsync<SalaryCompositionEntity>(sql)).ToList();
        }

        protected override async Task<SalaryCompositionEntity> GetEntitieByIdUsingCommandTextAsync(string id)
        {
            var sql = SelectWithJoin + " WHERE sc.SalaryCompositionID = @Id AND sc.IsDeleted = FALSE";
            using var connection = CreateConnection();
            await connection.OpenAsync();
            return await connection.QueryFirstOrDefaultAsync<SalaryCompositionEntity>(sql, new { Id = id });
        }

        public override async Task<(long Total, IEnumerable<SalaryCompositionEntity> Data)> GetAdvancedFilterPagingAsync(AdvancedFilterRequest request)
        {
            var filters = request.Filters ?? new List<FilterCondition>();
            var parameters = new DynamicParameters();

            var mandatoryParts = new List<string> { "IsDeleted = FALSE" };
            var userParts = new List<string>();
            BuildFilterConditions(filters, parameters, userParts);

            var whereSection = BuildWhereSection(mandatoryParts, userParts, request.Logic);
            var orderBy = BuildSortSql(request.Sort);
            var pageIndex = Math.Max(1, request.PageIndex);
            var pageSize = Math.Max(1, request.PageSize);
            var offset = (pageIndex - 1) * pageSize;

            parameters.Add("@_limit", pageSize);
            parameters.Add("@_offset", offset);

            var dataSql = $@"
                SELECT subq.*, ct.Name AS ComponentTypeName,
                       (SELECT GROUP_CONCAT(sco.OrganizationID ORDER BY sco.OrganizationID SEPARATOR ',')
                        FROM pa_salary_composition_organization sco
                        WHERE sco.SalaryCompositionID = subq.SalaryCompositionID) AS OrganizationIDs,
                       (SELECT GROUP_CONCAT(org2.Name ORDER BY sco2.OrganizationID SEPARATOR ', ')
                        FROM pa_salary_composition_organization sco2
                        JOIN pa_organization org2 ON sco2.OrganizationID = org2.OrganizationID
                        WHERE sco2.SalaryCompositionID = subq.SalaryCompositionID) AS OrganizationNames
                FROM (SELECT * FROM `{_tableName}` {whereSection} {orderBy} LIMIT @_limit OFFSET @_offset) subq
                LEFT JOIN pa_salary_component_type ct ON subq.ComponentTypeID = ct.ComponentTypeID
                {orderBy}";

            var countSql = $"SELECT COUNT(*) FROM `{_tableName}` {whereSection}";

            using var connection = CreateConnection();
            await connection.OpenAsync();
            var data = await connection.QueryAsync<SalaryCompositionEntity>(dataSql, parameters, commandType: CommandType.Text);
            var total = await connection.ExecuteScalarAsync<long>(countSql, parameters, commandType: CommandType.Text);
            return (total, data.ToList());
        }

        /// <summary>
        /// Lọc nâng cao 4 phần: search (mã/tên OR), status, orgs, field conditions
        /// </summary>
        public async Task<(IEnumerable<SalaryCompositionEntity> Data, long Total)> AdvancedFilterAsync(SalaryCompositionAdvancedFilterRequest request)
        {
            var parameters = new DynamicParameters();
            var conditions = new List<string> { "IsDeleted = FALSE" };

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                parameters.Add("@_search", $"%{request.Search.Trim()}%");
                conditions.Add("(Code LIKE @_search OR Name LIKE @_search)");
            }

            if (request.Status.HasValue)
            {
                parameters.Add("@_status", (int)request.Status.Value);
                conditions.Add("Status = @_status");
            }

            // Phần 3: đơn vị áp dụng — query junction table
            if (request.OrganizationIDs != null && request.OrganizationIDs.Count > 0)
            {
                var orgParams = request.OrganizationIDs.Select((_, i) => $"@_orgId{i}").ToList();
                for (int i = 0; i < request.OrganizationIDs.Count; i++)
                    parameters.Add($"@_orgId{i}", request.OrganizationIDs[i].ToString());
                conditions.Add($"SalaryCompositionID IN (SELECT SalaryCompositionID FROM pa_salary_composition_organization WHERE OrganizationID IN ({string.Join(", ", orgParams)}))");
            }

            var userParts = new List<string>();
            BuildFilterConditions(request.Filters ?? new(), parameters, userParts);
            if (userParts.Count > 0)
            {
                var sep = request.FilterLogic == FilterLogic.Or ? " OR " : " AND ";
                conditions.Add(userParts.Count == 1 ? userParts[0] : $"({string.Join(sep, userParts)})");
            }

            var whereSection = $"WHERE {string.Join(" AND ", conditions)}";
            var orderBy = BuildSortSql(request.Sort);
            var pageIndex = Math.Max(1, request.PageIndex);
            var pageSize = Math.Max(1, request.PageSize);

            parameters.Add("@_limit", pageSize);
            parameters.Add("@_offset", (pageIndex - 1) * pageSize);

            var dataSql = $@"
                SELECT subq.*, ct.Name AS ComponentTypeName,
                       (SELECT GROUP_CONCAT(sco.OrganizationID ORDER BY sco.OrganizationID SEPARATOR ',')
                        FROM pa_salary_composition_organization sco
                        WHERE sco.SalaryCompositionID = subq.SalaryCompositionID) AS OrganizationIDs,
                       (SELECT GROUP_CONCAT(org2.Name ORDER BY sco2.OrganizationID SEPARATOR ', ')
                        FROM pa_salary_composition_organization sco2
                        JOIN pa_organization org2 ON sco2.OrganizationID = org2.OrganizationID
                        WHERE sco2.SalaryCompositionID = subq.SalaryCompositionID) AS OrganizationNames
                FROM (SELECT * FROM `{_tableName}` {whereSection} {orderBy} LIMIT @_limit OFFSET @_offset) subq
                LEFT JOIN pa_salary_component_type ct ON subq.ComponentTypeID = ct.ComponentTypeID
                {orderBy}";

            var countSql = $"SELECT COUNT(*) FROM `{_tableName}` {whereSection}";

            using var connection = CreateConnection();
            await connection.OpenAsync();
            var data = await connection.QueryAsync<SalaryCompositionEntity>(dataSql, parameters);
            var total = await connection.ExecuteScalarAsync<long>(countSql, parameters);
            return (data.ToList(), total);
        }

        /// <summary>
        /// Lọc nâng cao 4 phần qua stored procedure
        /// </summary>
        public async Task<(IEnumerable<SalaryCompositionEntity> Data, long Total)> AdvancedFilterWithProcAsync(SalaryCompositionAdvancedFilterRequest request)
        {
            var parameters = new DynamicParameters();
            parameters.Add("v_pageIndex",    Math.Max(1, request.PageIndex));
            parameters.Add("v_pageSize",     Math.Max(1, request.PageSize));
            parameters.Add("v_sort",         request.Sort ?? string.Empty);
            parameters.Add("v_search",       request.Search);
            parameters.Add("v_status",       request.Status.HasValue ? (int?)request.Status.Value : null);
            parameters.Add("v_org_ids",
                request.OrganizationIDs != null && request.OrganizationIDs.Count > 0
                    ? System.Text.Json.JsonSerializer.Serialize(request.OrganizationIDs.Select(id => id.ToString()))
                    : null);
            parameters.Add("v_filters",      System.Text.Json.JsonSerializer.Serialize(request.Filters ?? new()));
            parameters.Add("v_filter_logic", (int)request.FilterLogic);

            using var connection = CreateConnection();
            using var multi = await connection.QueryMultipleAsync(
                "Proc_pa_salary_composition_AdvancedFilterPaging",
                parameters,
                commandType: CommandType.StoredProcedure);

            var data = (await multi.ReadAsync<SalaryCompositionEntity>()).ToList();
            var total = await multi.ReadFirstAsync<long>();
            return (data, total);
        }

        /// <summary>
        /// Lọc thành phần lương theo nhiều điều kiện có phân trang
        /// </summary>
        /// Created By: Nguyen Thiet Do (2026-05-27)
        public async Task<(IEnumerable<SalaryCompositionEntity> Data, long Total)> FilterAsync(SalaryCompositionFilterRequest request)
        {
            var parameters = new DynamicParameters();
            parameters.Add("v_Search", request.Search);
            parameters.Add("v_OrganizationIDs",
                request.OrganizationIDs != null && request.OrganizationIDs.Count > 0
                    ? System.Text.Json.JsonSerializer.Serialize(request.OrganizationIDs.Select(id => id.ToString()))
                    : null);
            parameters.Add("v_ComponentTypeID", request.ComponentTypeID?.ToString());
            parameters.Add("v_Nature", request.Nature.HasValue ? (int?)request.Nature : null);
            parameters.Add("v_Status", request.Status.HasValue ? (int?)request.Status : null);
            parameters.Add("v_Source", request.Source.HasValue ? (int?)request.Source : null);
            parameters.Add("v_PageIndex", request.PageIndex);
            parameters.Add("v_PageSize", request.PageSize);

            using var connection = CreateConnection();
            using var multi = await connection.QueryMultipleAsync(
                "Proc_pa_salary_composition_Filter",
                parameters,
                commandType: CommandType.StoredProcedure);

            var data = (await multi.ReadAsync<SalaryCompositionEntity>()).ToList();
            var total = await multi.ReadFirstAsync<long>();
            return (data, total);
        }

        #endregion

        #region OVERRIDE METHODS

        protected override async Task OnAfterInsertInTransactionAsync(
            SalaryCompositionEntity entity, IDbConnection connection, IDbTransaction transaction)
        {
            if (entity.OrganizationIDs == null || entity.OrganizationIDs.Count == 0) return;

            foreach (var orgId in entity.OrganizationIDs)
            {
                await connection.ExecuteAsync(
                    "INSERT INTO pa_salary_composition_organization (SalaryCompositionID, OrganizationID) VALUES (@ScId, @OrgId)",
                    new { ScId = entity.SalaryCompositionID.ToString(), OrgId = orgId.ToString() },
                    transaction);
            }
        }

        protected override async Task OnAfterUpdateInTransactionAsync(
            SalaryCompositionEntity entity, Guid entityId, IDbConnection connection, IDbTransaction transaction)
        {
            // Xóa hết rồi insert lại — đơn giản và tránh diff logic
            await connection.ExecuteAsync(
                "DELETE FROM pa_salary_composition_organization WHERE SalaryCompositionID = @ScId",
                new { ScId = entityId.ToString() },
                transaction);

            if (entity.OrganizationIDs == null || entity.OrganizationIDs.Count == 0) return;

            foreach (var orgId in entity.OrganizationIDs)
            {
                await connection.ExecuteAsync(
                    "INSERT INTO pa_salary_composition_organization (SalaryCompositionID, OrganizationID) VALUES (@ScId, @OrgId)",
                    new { ScId = entityId.ToString(), OrgId = orgId.ToString() },
                    transaction);
            }
        }

        #endregion
    }
}
