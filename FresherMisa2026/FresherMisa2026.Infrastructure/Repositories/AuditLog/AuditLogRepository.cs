using Dapper;
using FresherMisa2026.Application.Interfaces.Repositories;
using FresherMisa2026.Entities.AuditLog;
using FresherMisa2026.Entities.Enums;
using FresherMisa2026.Entities.SalaryComposition;
using Microsoft.Extensions.Configuration;
using MySqlConnector;

namespace FresherMisa2026.Infrastructure.Repositories.AuditLog
{
    public class AuditLogRepository : IAuditLogRepository
    {
        private readonly string _connectionString;

        public AuditLogRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString(SalaryCompositionConstants.ConnectionName)!;
        }

        public async Task LogAsync(Guid entityId, string entityType, AuditAction action, string? actionBy)
        {
            const string sql = @"
                INSERT INTO audit_log (AuditLogID, EntityType, EntityId, Action, ActionBy, ActionAt)
                VALUES (@AuditLogID, @EntityType, @EntityId, @Action, @ActionBy, @ActionAt)";

            using var connection = new MySqlConnection(_connectionString);
            await connection.ExecuteAsync(sql, new
            {
                AuditLogID = Guid.NewGuid().ToString(),
                EntityType  = entityType,
                EntityId    = entityId.ToString(),
                Action      = (int)action,
                ActionBy    = actionBy,
                ActionAt    = DateTime.Now
            });
        }

        public async Task<IEnumerable<Entities.AuditLog.AuditLog>> GetByEntityAsync(string entityType, Guid entityId)
        {
            const string sql = @"
                SELECT AuditLogID, EntityType, EntityId, Action, ActionBy, ActionAt
                FROM audit_log
                WHERE EntityType = @EntityType AND EntityId = @EntityId
                ORDER BY ActionAt DESC";

            using var connection = new MySqlConnection(_connectionString);
            var rows = await connection.QueryAsync<AuditLogRow>(sql, new
            {
                EntityType = entityType,
                EntityId   = entityId.ToString()
            });

            return rows.Select(MapRow);
        }

        public async Task<IEnumerable<Entities.AuditLog.AuditLog>> GetPagedAsync(string? entityType, int pageIndex, int pageSize)
        {
            var where = entityType != null ? "WHERE EntityType = @EntityType" : string.Empty;
            var sql = $@"
                SELECT AuditLogID, EntityType, EntityId, Action, ActionBy, ActionAt
                FROM audit_log
                {where}
                ORDER BY ActionAt DESC
                LIMIT @Offset, @PageSize";

            using var connection = new MySqlConnection(_connectionString);
            var rows = await connection.QueryAsync<AuditLogRow>(sql, new
            {
                EntityType = entityType,
                Offset     = pageIndex * pageSize,
                PageSize   = pageSize
            });

            return rows.Select(MapRow);
        }

        private static Entities.AuditLog.AuditLog MapRow(AuditLogRow r) => new()
        {
            AuditLogID = r.AuditLogID,
            EntityType = r.EntityType,
            EntityId   = r.EntityId,
            Action     = (AuditAction)(int)r.Action,
            ActionBy   = r.ActionBy,
            ActionAt   = r.ActionAt
        };

        // GuidTypeHandler đã đăng ký toàn cục nên CHAR(36) → Guid tự động.
        // TINYINT → sbyte nên dùng sbyte rồi cast sang enum.
        private class AuditLogRow
        {
            public Guid     AuditLogID { get; set; }
            public string   EntityType { get; set; } = string.Empty;
            public Guid     EntityId   { get; set; }
            public sbyte    Action     { get; set; }
            public string?  ActionBy   { get; set; }
            public DateTime ActionAt   { get; set; }
        }
    }
}
