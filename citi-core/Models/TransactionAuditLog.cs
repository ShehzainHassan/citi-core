namespace citi_core.Models
{
    public class TransactionAuditLog : BaseEntity
    {
        public Guid AuditLogId { get; set; }
        public Guid UserId { get; set; }
        public string Action { get; set; } = string.Empty;
        public string Reference { get; set; } = string.Empty;
    }
}
