namespace StockManagement.Domain.Common;

/// <summary>
/// Entity scoped to a tenant (shop). Multi-tenant isolation is enforced at the
/// EF Core query-filter level so a tenant can never read another tenant's data.
/// </summary>
public abstract class TenantEntity : AuditableEntity
{
    public Guid TenantId { get; set; }
}
