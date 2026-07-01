using StockManagement.Domain.Common;
using StockManagement.Domain.Entities.Identity;

namespace StockManagement.Domain.Entities;

/// <summary>
/// A tenant represents a shop/organization. All tenant-scoped data references
/// this id for complete data isolation in a multi-tenant deployment.
/// </summary>
public class Tenant : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? GstNumber { get; set; }
    public string? Pan { get; set; }
    public string Currency { get; set; } = "INR";
    public string? TimeZone { get; set; } = "India Standard Time";
    public string? Language { get; set; } = "en";
    public string? BusinessType { get; set; }
    public string? BranchCode { get; set; }
    public string? ThemePrimaryColor { get; set; } = "#594AE2";
    public decimal TaxRate { get; set; } = 0m;
    public bool IsActive { get; set; } = true;
    public string? Subdomain { get; set; }

    public ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
}
