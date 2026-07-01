using StockManagement.Domain.Common;

namespace StockManagement.Domain.Entities;

/// <summary>
/// Immutable audit log tracking every operation in the system.
/// </summary>
public class AuditLog : BaseEntity
{
    public Guid? TenantId { get; set; }
    public string? UserId { get; set; }
    public string? UserName { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? EntityName { get; set; }
    public string? EntityId { get; set; }
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
