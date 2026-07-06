using System.Threading.Tasks;
using StockManagement.Application.Common.Interfaces;
using StockManagement.Infrastructure.Data;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace StockManagement.Web.Services;

public class TenantInitializer
{
    private readonly AppDbContext _db;
    private readonly ITenantContext _tenant;
    private readonly ILogger<TenantInitializer>? _logger;

    public TenantInitializer(AppDbContext db, ITenantContext tenant, ILogger<TenantInitializer>? logger = null)
    {
        _db = db;
        _tenant = tenant;
        _logger = logger;
    }

    public Task InitializeAsync()
    {
        try
        {
            var resolvedTenantId = _tenant?.TenantId ?? Guid.Empty;
            _logger?.LogInformation("TenantInitializer: Resolved tenant ID={TenantId} from ITenantContext", resolvedTenantId);

            // Set CurrentTenantId on the DbContext
            _db.CurrentTenantId = resolvedTenantId;

            // If tenant not resolved yet (Guid.Empty), fallback to first seeded tenant
            if (resolvedTenantId == Guid.Empty)
            {
                var tenants = _db.Tenants.IgnoreQueryFilters().ToList();
                if (tenants.Any())
                {
                    var defaultTenant = tenants.First();
                    _db.CurrentTenantId = defaultTenant.Id;

                    // Also update the ITenantContext so subsequent queries use the same tenant
                    if (_tenant is CircuitTenantState circuitState)
                    {
                        circuitState.SetTenant(defaultTenant.Id, defaultTenant.Name);
                        _logger?.LogInformation("TenantInitializer: Set CircuitTenantState to default tenant ID={TenantId} Name={TenantName}", defaultTenant.Id, defaultTenant.Name);
                    }
                    else
                    {
                        _logger?.LogWarning("TenantInitializer: ITenantContext is not CircuitTenantState, cannot persist tenant state in circuit");
                    }
                }
            }
            else
            {
                _logger?.LogInformation("TenantInitializer: Using resolved tenant ID={TenantId}", resolvedTenantId);
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "TenantInitializer.InitializeAsync failed");
        }

        return Task.CompletedTask;
    }
}
