using FresherMisa2026.Entities.Enums;

namespace FresherMisa2026.Entities.AuditLog
{
    /// <summary>
    /// Bản ghi lịch sử thao tác — ai làm gì, khi nào, với bản ghi nào.
    /// Không kế thừa BaseModel vì audit log không được tự audit.
    /// </summary>
    public class AuditLog
    {
        public Guid AuditLogID { get; set; } = Guid.NewGuid();
        public string EntityType { get; set; } = string.Empty;
        public Guid EntityId { get; set; }
        public AuditAction Action { get; set; }
        public string? ActionBy { get; set; }
        public DateTime ActionAt { get; set; } = DateTime.Now;
    }
}
