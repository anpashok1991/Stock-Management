using Microsoft.AspNetCore.SignalR;
using StockManagement.Application.Common.Interfaces;

namespace StockManagement.Infrastructure.Services;

public class AppNotificationHub : Hub, INotificationHub
{
    public async Task SendNotificationAsync(string userId, string title, string message, string? link = null)
    {
        await Clients.User(userId).SendAsync("ReceiveNotification", new { title, message, link, timestamp = DateTime.UtcNow });
    }

    public async Task BroadcastNotificationAsync(string title, string message, string? link = null)
    {
        await Clients.All.SendAsync("ReceiveNotification", new { title, message, link, timestamp = DateTime.UtcNow });
    }
}
