using StockManagement.Application.Common.Interfaces;

namespace StockManagement.Web.Services;

/// <summary>
/// Maintains tenant state across a Blazor circuit. This is an ambient context that persists
/// the tenant ID for the life of a Blazor Server circuit, which does not have HTTP request context.
/// </summary>
public class CircuitTenantState : ITenantContext
{
    public Guid? TenantId { get; private set; }
    public string? TenantName { get; private set; }

    public void SetTenant(Guid id, string? name = null)
    {
        TenantId = id;
        TenantName = name;
    }

    public void ClearTenant()
    {
        TenantId = null;
        TenantName = null;
    }
}
