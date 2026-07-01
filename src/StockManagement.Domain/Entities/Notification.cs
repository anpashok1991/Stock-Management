using StockManagement.Domain.Common;

namespace StockManagement.Domain.Entities;

public class Notification : TenantEntity
{
    public string UserId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Link { get; set; }
    public bool IsRead { get; set; }
    public string? Type { get; set; }
}
