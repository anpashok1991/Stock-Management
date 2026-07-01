namespace StockManagement.Application.Common.Interfaces;

/// <summary>
/// Resolves the current tenant for the active request. Set by TenantResolutionMiddleware.
/// </summary>
public interface ITenantContext
{
    Guid? TenantId { get; }
    string? TenantName { get; }
}
