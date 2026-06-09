using Dapper;
using FresherMisa2026.Application.Interfaces.Repositories;
using FresherMisa2026.Entities.AdvancedFilter;
using FresherMisa2026.Entities.Extensions;
using FresherMisa2026.Entities.SalaryComposition;
using FresherMisa2026.Entities.SalaryComposition.DTO;
using FresherMisa2026.Infrastructure.Persistence.Queries;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Text.Json;
using System.Text.Json.Serialization;
using SalaryCompositionEntity = FresherMisa2026.Entities.SalaryComposition.SalaryComposition;

namespace FresherMisa2026.Infrastructure.Repositories
{
    /// <summary>
    /// Repository cho SalaryComposition — dùng database amis_tien_luong (SalaryConnection).
    /// </summary>
    /// <remarks>Created by: ntdo — 27/05/2026 · Refactor: 03/06/2026</remarks>
    public class SalaryCompositionRepository
        : BaseRepository<SalaryCompositionEntity>, ISalaryCompositionRepository
    {
        private static readonly JsonSerializerOptions CamelCaseOptions = new()
        {
            PropertyNamingPolicy    = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition  = JsonIgnoreCondition.WhenWritingNull,
        };

        #region Constructer

        public SalaryCompositionRepository(
            IConfiguration configuration,
            IMemoryCache cache,
            ILogger<BaseRepository<SalaryCompositionEntity>> logger)
            : base(configuration, cache, logger)
        {
            _connectionString = configuration.GetConnectionString(SalaryCompositionConstants.ConnectionName)!;
        }

        #endregion

        #region OVERRIDE METHODS - Get

        /// <summary>
        /// Lấy toàn bộ TPL (kèm tên loại + danh sách đơn vị áp dụng).
        /// </summary>
        /// <returns>Danh sách TPL không bị xóa.</returns>
        /// <remarks>Created by: ntdo — 27/05/2026</remarks>
        protected override async Task<IEnumerable<SalaryCompositionEntity>> GetEntitiesUsingCommandTextAsync()
        {
            var sql = SalaryQueries.SelectSalaryCompositionWithJoin + " WHERE sc.`is_deleted` = FALSE";
            using var connection = CreateConnection();
            await connection.OpenAsync();
            return (await connection.QueryAsync<SalaryCompositionEntity>(sql)).ToList();
        }

        /// <summary>
        /// Lấy 1 TPL theo ID (kèm tên loại + danh sách đơn vị áp dụng).
        /// </summary>
        /// <param name="id">ID dạng chuỗi (Guid.ToString()).</param>
        /// <returns>Entity hoặc null nếu không tìm thấy / đã xóa.</returns>
        /// <remarks>Created by: ntdo — 27/05/2026</remarks>
        protected override async Task<SalaryCompositionEntity> GetEntitieByIdUsingCommandTextAsync(string id)
        {
            var sql = SalaryQueries.SelectSalaryCompositionWithJoin
                    + " WHERE sc.`salary_composition_id` = @Id AND sc.`is_deleted` = FALSE";
            using var connection = CreateConnection();
            await connection.OpenAsync();
            return await connection.QueryFirstOrDefaultAsync<SalaryCompositionEntity>(sql, new { Id = id });
        }

        #endregion

        #region Methods - Advanced Filter

        /// <summary>Lọc nâng cao 4 phần qua stored procedure (SP tự build WHERE).</summary>
        /// <param name="request">Điều kiện lọc nâng cao của TPL.</param>
        /// <returns>Dữ liệu trang hiện tại và tổng số.</returns>
        /// <remarks>Created by: ntdo — 27/05/2026</remarks>
        public async Task<(IEnumerable<SalaryCompositionEntity> Data, long Total)>
            AdvancedFilterWithProcAsync(SalaryCompositionAdvancedFilterRequest request)
        {
            var parameters = new DynamicParameters();
            parameters.Add("v_page_index",       Math.Max(1, request.PageIndex));
            parameters.Add("v_page_size",        Math.Max(1, request.PageSize));
            parameters.Add("v_sort",             ConvertSortToSnakeCase(request.Sort) ?? string.Empty);
            parameters.Add("v_search",           request.Search);
            parameters.Add("v_search_fields",    SerializeSearchFieldsToSnakeOrNull(request.SearchFields));
            parameters.Add("v_status",           request.Status.HasValue ? (int?)request.Status.Value : null);
            parameters.Add("v_org_ids",          SerializeGuidListOrNull(request.OrganizationIDs));
            parameters.Add("v_filters",          JsonSerializer.Serialize(ProjectFiltersForSp(request.Filters ?? new()), CamelCaseOptions));
            parameters.Add("v_filter_logic",     (int)request.FilterLogic);
            parameters.Add("v_columns",          request.Columns is { Count: > 0 }
                ? JsonSerializer.Serialize(request.Columns.Select(Naming.ToSnakeCase))
                : null);

            using var connection = CreateConnection();
            await connection.OpenAsync();
            using var multi = await connection.QueryMultipleAsync(
                SalaryQueries.ProcSalaryCompositionAdvancedFilter,
                parameters,
                commandType: CommandType.StoredProcedure);

            var data  = (await multi.ReadAsync<SalaryCompositionEntity>()).ToList();
            var total = await multi.ReadFirstAsync<long>();
            return (data, total);
        }

        /// <summary>Lọc TPL theo nhiều điều kiện qua stored procedure FilterPaging.</summary>
        /// <param name="request">Điều kiện lọc cơ bản.</param>
        /// <returns>Dữ liệu trang hiện tại và tổng số.</returns>
        /// <remarks>Created by: ntdo — 27/05/2026</remarks>
        public async Task<(IEnumerable<SalaryCompositionEntity> Data, long Total)>
            FilterAsync(SalaryCompositionFilterRequest request)
        {
            var parameters = new DynamicParameters();
            parameters.Add("v_search",            request.Search);
            parameters.Add("v_organization_ids",  SerializeGuidListOrNull(request.OrganizationIDs));
            parameters.Add("v_component_type_id", request.ComponentTypeID?.ToString());
            parameters.Add("v_nature",            request.Nature.HasValue  ? (int?)request.Nature  : null);
            parameters.Add("v_status",            request.Status.HasValue  ? (int?)request.Status  : null);
            parameters.Add("v_source",            request.Source.HasValue  ? (int?)request.Source  : null);
            parameters.Add("v_page_index",        request.PageIndex);
            parameters.Add("v_page_size",         request.PageSize);

            using var connection = CreateConnection();
            await connection.OpenAsync();
            using var multi = await connection.QueryMultipleAsync(
                SalaryQueries.ProcSalaryCompositionFilter,
                parameters,
                commandType: CommandType.StoredProcedure);

            var data  = (await multi.ReadAsync<SalaryCompositionEntity>()).ToList();
            var total = await multi.ReadFirstAsync<long>();
            return (data, total);
        }

        #endregion

        #region OVERRIDE METHODS - Junction table

        /// <summary>
        /// Sau khi insert TPL: đồng bộ junction TPL ↔ Đơn vị.
        /// Dapper iterate parameter collection và execute lại cùng prepared statement → không phải single
        /// multi-row INSERT nhưng tiết kiệm parse cost so với gọi ExecuteAsync N lần độc lập.
        /// </summary>
        protected override async Task OnAfterInsertInTransactionAsync(
            SalaryCompositionEntity entity, IDbConnection connection, IDbTransaction transaction)
        {
            await SyncCompositionOrganizationsAsync(
                entity.SalaryCompositionID, entity.OrganizationIDs, connection, transaction, removeExisting: false);
        }

        /// <summary>
        /// Sau khi update TPL: xóa toàn bộ liên kết cũ rồi insert batch danh sách mới
        /// trong cùng transaction.
        /// </summary>
        protected override async Task OnAfterUpdateInTransactionAsync(
            SalaryCompositionEntity entity, Guid entityId, IDbConnection connection, IDbTransaction transaction)
        {
            await SyncCompositionOrganizationsAsync(
                entityId, entity.OrganizationIDs, connection, transaction, removeExisting: true);
        }

        #endregion

        #region Private helpers

        /// <summary>
        /// Build và chạy paging query có wrap subquery để JOIN ComponentType + 2 subquery org.
        /// Tái sử dụng cho cả lọc nâng cao chung và lọc nâng cao của TPL.
        /// </summary>
        private async Task<(long Total, IEnumerable<SalaryCompositionEntity> Data)>
            ExecutePagedSalaryQueryAsync(string whereSection, string orderBy,
                int pageIndex, int pageSize, DynamicParameters parameters)
        {
            var safePageIndex = Math.Max(1, pageIndex);
            var safePageSize  = Math.Max(1, pageSize);
            parameters.Add("@_limit",  safePageSize);
            parameters.Add("@_offset", (safePageIndex - 1) * safePageSize);

            // Bước 1: lấy ID + cột page-of-rows từ bảng gốc (apply WHERE/ORDER/LIMIT trước cho hiệu năng)
            // Bước 2: JOIN ComponentType và lấy 2 subquery org cho đúng các dòng trong trang
            var dataSql = $@"
                SELECT subq.*, ct.`name` AS component_type_name,
                       {SalaryQueries.OrganizationIdsSubquery.Replace("sc.`salary_composition_id`", "subq.`salary_composition_id`")},
                       {SalaryQueries.OrganizationNamesSubquery.Replace("sc.`salary_composition_id`", "subq.`salary_composition_id`")}
                FROM (SELECT * FROM `{_tableName}` {whereSection} {orderBy} LIMIT @_limit OFFSET @_offset) subq
                LEFT JOIN pa_salary_component_type ct ON subq.`component_type_id` = ct.`component_type_id`
                {orderBy}";

            var countSql = $"SELECT COUNT(*) FROM `{_tableName}` {whereSection}";

            using var connection = CreateConnection();
            await connection.OpenAsync();

            var data  = await connection.QueryAsync<SalaryCompositionEntity>(dataSql, parameters);
            var total = await connection.ExecuteScalarAsync<long>(countSql, parameters);
            return (total, data.ToList());
        }

        /// <summary>
        /// Đồng bộ danh sách đơn vị áp dụng cho 1 TPL (dùng cùng connection + transaction).
        /// Khi <paramref name="removeExisting"/>=true thì xóa toàn bộ liên kết cũ trước.
        /// </summary>
        private static async Task SyncCompositionOrganizationsAsync(
            Guid compositionId,
            IReadOnlyCollection<Guid>? organizationIds,
            IDbConnection connection,
            IDbTransaction transaction,
            bool removeExisting)
        {
            if (removeExisting)
            {
                await connection.ExecuteAsync(
                    SalaryQueries.DeleteSalaryCompositionOrganizations,
                    new { ScId = compositionId.ToString() },
                    transaction);
            }

            if (organizationIds == null || organizationIds.Count == 0) return;

            // Dapper: truyền IEnumerable<anonymous> — Dapper sẽ execute cùng câu SQL nhiều lần
            // tận dụng cùng prepared statement / connection (tiết kiệm parse cost, không phải multi-row INSERT)
            var rows = organizationIds.Select(orgId => new
            {
                ScId  = compositionId.ToString(),
                OrgId = orgId.ToString()
            });

            await connection.ExecuteAsync(
                SalaryQueries.InsertSalaryCompositionOrganization, rows, transaction);
        }

        private static string? SerializeOrNull(List<string>? values)
            => values is { Count: > 0 } ? JsonSerializer.Serialize(values) : null;

        /// <summary>
        /// Serialize danh sách SearchFields — convert mỗi tên field từ PascalCase property name sang snake_case
        /// để SP nhúng trực tiếp vào câu lệnh column reference.
        /// </summary>
        private static string? SerializeSearchFieldsToSnakeOrNull(List<string>? values)
            => values is { Count: > 0 }
                ? JsonSerializer.Serialize(values.Select(Naming.ToSnakeCase))
                : null;

        private static string? SerializeGuidListOrNull(List<Guid>? values)
            => values is { Count: > 0 }
                ? JsonSerializer.Serialize(values.Select(g => g.ToString()))
                : null;

        /// <summary>
        /// Convert sort string từ PascalCase sang snake_case, giữ nguyên prefix - hoặc +.
        /// Ví dụ: "-Code,-CreateDate" → "-code,-create_date"
        /// </summary>
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
