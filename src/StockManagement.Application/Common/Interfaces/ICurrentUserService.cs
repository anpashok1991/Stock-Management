namespace StockManagement.Application.Common.Interfaces;

/// <summary>
/// Resolves the current authenticated user for the active request.
/// </summary>
public interface ICurrentUserService
{
    string? UserId { get; }
    string? UserName { get; }
    string? Email { get; }
    bool IsAuthenticated { get; }
    Guid? TenantId { get; }
    IReadOnlyList<string> Roles { get; }
}
