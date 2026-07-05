using Microsoft.AspNetCore.Http;
using StockManagement.Application.Common.Interfaces;

namespace StockManagement.Infrastructure.Services;

/// <summary>
/// Resolves the current tenant from the HTTP context (header, subdomain, or query string).
/// Falls back to the default seed tenant when no header is provided.
/// </summary>
public class TenantContext : ITenantContext
{
    public Guid? TenantId { get; private set; }
    public string? TenantName { get; private set; }

    public TenantContext(IHttpContextAccessor accessor)
    {
        var http = accessor.HttpContext;
        if (http == null) return;

        if (http.Request.Headers.TryGetValue("X-Tenant-Id", out var tenantIdHeader) &&
            Guid.TryParse(tenantIdHeader.ToString(), out var tid))
        {
            TenantId = tid;
        }

        TenantName = http.Request.Headers["X-Tenant-Name"].FirstOrDefault();
    }

    // Allow resolving tenant from authenticated user's claims as well (useful for Blazor Server circuits)
    public TenantContext(IHttpContextAccessor accessor, System.Security.Claims.ClaimsPrincipal? user = null)
    {
        var http = accessor.HttpContext;
        var principal = user ?? http?.User;
        if (principal != null)
        {
            var claim = principal.FindFirst("tenant_id") ?? principal.FindFirst("tenant") ?? principal.FindFirst("X-Tenant-Id");
            if (claim != null && Guid.TryParse(claim.Value, out var tid))
            {
                TenantId = tid;
            }
            TenantName = principal.FindFirst("tenant_name")?.Value ?? TenantName;
        }
    }

    public void SetTenant(Guid id, string? name = null)
    {
        TenantId = id;
        TenantName = name;
    }
}
