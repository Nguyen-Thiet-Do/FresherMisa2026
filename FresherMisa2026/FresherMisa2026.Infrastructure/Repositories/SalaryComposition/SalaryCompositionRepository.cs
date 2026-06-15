using Dapper;
using FresherMisa2026.Application.Interfaces.Repositories;
using FresherMisa2026.Entities.AdvancedFilter;
using FresherMisa2026.Entities.Extensions;
using FresherMisa2026.Entities.Enums;
using FresherMisa2026.Entities.SalaryComposition;
using FresherMisa2026.Entities.SalaryComposition.DTO;
using FresherMisa2026.Infrastructure.Persistence.Queries;
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
    /// <remarks>Created by: ntdo — 2026-06-07 · Refactor: 03/06/2026</remarks>
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
            ILogger<BaseRepository<SalaryCompositionEntity>> logger)
            : base(configuration, logger)
        {
            _connectionString = configuration.GetConnectionString(SalaryCompositionConstants.ConnectionName)!;
        }

        #endregion

        #region OVERRIDE METHODS - Get

        /// <summary>
        /// Lấy toàn bộ TPL (kèm tên loại + danh sách đơn vị áp dụng).
        /// </summary>
        /// <returns>Danh sách TPL không bị xóa.</returns>
        /// <remarks>Created by: ntdo — 2026-06-07</remarks>
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
        /// <remarks>Created by: ntdo — 2026-06-07</remarks>
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
        /// <remarks>Created by: ntdo — 2026-06-07</remarks>
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

#endregion

        #region Methods - Lookup

        /// <summary>Kiểm tra có TPL kế thừa từ hệ thống với Code cho trước chưa.</summary>
        public async Task<bool> ExistsInheritedByCodeAsync(string code)
        {
            const string sql = @"SELECT COUNT(1) FROM `pa_salary_composition`
                                 WHERE `source` = @source AND `code` = @code AND `is_deleted` = FALSE";
            using var connection = CreateConnection();
            await connection.OpenAsync();
            var count = await connection.ExecuteScalarAsync<int>(sql,
                new { source = (int)SalaryCompositionSource.InheritedFromSystem, code });
            return count > 0;
        }

        /// <summary>
        /// Lấy Code, Name, Status của các TPL khớp danh sách code — chỉ query đúng K rows thay vì toàn bảng.
        /// </summary>
        public async Task<IEnumerable<(string Code, string Name, SalaryCompositionStatus Status)>>
            GetCodeInfoByCodesAsync(IEnumerable<string> codes)
        {
            var codeList = codes.ToList();
            if (codeList.Count == 0)
                return Enumerable.Empty<(string, string, SalaryCompositionStatus)>();

            const string sql = @"SELECT `code`, `name`, `status`
                                 FROM `pa_salary_composition`
                                 WHERE `code` IN @codes AND `is_deleted` = FALSE";
            using var connection = CreateConnection();
            await connection.OpenAsync();
            var rows = await connection.QueryAsync<SalaryCompositionEntity>(sql, new { codes = codeList });
            return rows.Select(e => (e.Code, e.Name, e.Status));
        }

/// <summary>
        /// Kiểm tra mã TPL có đang được tham chiếu trong công thức của bất kỳ TPL nào khác không.
        /// Dùng MySQL REGEXP word-boundary [[:<:]] / [[:>:]] thay vì fetch toàn bảng về C#.
        /// </summary>
        public async Task<bool> IsCodeReferencedInFormulasAsync(string code, Guid excludeId)
        {
            // Code chỉ chứa [A-Za-z0-9_] (đã validate) — không cần escape REGEXP
            var pattern = $"[[:<:]]{code}[[:>:]]";
            const string sql = @"SELECT COUNT(1) FROM `pa_salary_composition`
                                 WHERE `salary_composition_id` != @excludeId AND `is_deleted` = FALSE
                                   AND (    `value_formula`   REGEXP @pattern
                                         OR `norm_formula`    REGEXP @pattern
                                         OR `taxable_formula` REGEXP @pattern
                                         OR `exempt_formula`  REGEXP @pattern)";
            using var connection = CreateConnection();
            await connection.OpenAsync();
            var count = await connection.ExecuteScalarAsync<int>(sql,
                new { excludeId = excludeId.ToString(), pattern });
            return count > 0;
        }

        #endregion

        #region Methods - Junction table public API

        /// <summary>Đồng bộ danh sách đơn vị áp dụng cho 1 TPL — dùng cho PATCH /fields.</summary>
        public async Task<int> UpdateOrganizationIDsAsync(Guid compositionId, List<Guid> organizationIds)
        {
            using var connection = CreateConnection();
            await connection.OpenAsync();
            using var transaction = await connection.BeginTransactionAsync();

            await SyncCompositionOrganizationsAsync(
                compositionId, organizationIds, connection, transaction, removeExisting: true);

            await transaction.CommitAsync();

            return 1;
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

        /// <summary>Serialize list thành JSON string; trả null nếu list rỗng hoặc null.</summary>
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
