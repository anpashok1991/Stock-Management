using Microsoft.AspNetCore.Identity;
using StockManagement.Domain.Common;

namespace StockManagement.Domain.Entities.Identity;

/// <summary>
/// Application user extending ASP.NET Core Identity with tenant binding and
/// profile data required by the ERP (employee/customer context).
/// </summary>
public class ApplicationUser : IdentityUser
{
    public Guid? TenantId { get; set; }
    public Tenant? Tenant { get; set; }

    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? AvatarUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? LastLoginAt { get; set; }
    public DateTimeOffset? LockoutEndUtc { get; set; }

    public string FullName => string.IsNullOrWhiteSpace($"{FirstName} {LastName}".Trim()) ? UserName ?? Email ?? "User" : $"{FirstName} {LastName}".Trim();
}
