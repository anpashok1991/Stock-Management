namespace StockManagement.Domain.Common;

/// <summary>
/// Entity that records which user created/modified it. Used for audit trails.
/// </summary>
public abstract class AuditableEntity : BaseEntity
{
    public string? CreatedBy { get; set; }
    public string? ModifiedBy { get; set; }
}
