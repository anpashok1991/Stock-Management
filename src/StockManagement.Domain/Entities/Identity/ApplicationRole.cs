using Microsoft.AspNetCore.Identity;

namespace StockManagement.Domain.Entities.Identity;

public class ApplicationRole : IdentityRole
{
    public string? Description { get; set; }
    public bool IsSystemRole { get; set; }
}
