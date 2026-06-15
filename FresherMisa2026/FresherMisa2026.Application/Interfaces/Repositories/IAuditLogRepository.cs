using FresherMisa2026.Entities.AuditLog;
using FresherMisa2026.Entities.Enums;

namespace FresherMisa2026.Application.Interfaces.Repositories
{
    public interface IAuditLogRepository
    {
        Task LogAsync(Guid entityId, string entityType, AuditAction action, string? actionBy);
        Task<IEnumerable<AuditLog>> GetByEntityAsync(string entityType, Guid entityId);
        Task<IEnumerable<AuditLog>> GetPagedAsync(string? entityType, int pageIndex, int pageSize);
    }
}
