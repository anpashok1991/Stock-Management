namespace StockManagement.Application.Common.Interfaces;

/// <summary>
/// Abstraction for pushing real-time notifications to connected clients.
/// </summary>
public interface INotificationHub
{
    Task SendNotificationAsync(string userId, string title, string message, string? link = null);
    Task BroadcastNotificationAsync(string title, string message, string? link = null);
}
