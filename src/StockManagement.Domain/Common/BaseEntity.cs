namespace StockManagement.Domain.Common;

/// <summary>
/// Base entity for all domain entities. Provides a globally unique identifier
/// and concurrency/audit timestamps shared across the entire domain model.
/// </summary>
public abstract class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ModifiedAt { get; set; }
}
