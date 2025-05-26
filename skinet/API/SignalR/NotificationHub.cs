using System.Collections.Concurrent;
using API.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace API.SignalR;

[Authorize]
public class NotificationHub : Hub
{
    private static readonly ConcurrentDictionary<string, string> UserConnections = new();
    private readonly ILogger<NotificationHub> _logger;

    public NotificationHub(ILogger<NotificationHub> logger)
    {
        _logger = logger;
    }

    public override Task OnConnectedAsync()
    {
        var email = Context.User?.GetEmail();
        _logger.LogInformation("SignalR connected: {Email} - {ConnectionId}", email, Context.ConnectionId);

        if (!string.IsNullOrEmpty(email)) UserConnections[email] = Context.ConnectionId;

        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        var email = Context.User?.GetEmail();
        _logger.LogInformation("SignalR disconnected: {Email} - {ConnectionId}", email, Context.ConnectionId);

        if (!string.IsNullOrEmpty(email)) UserConnections.TryRemove(email, out _);

        return base.OnDisconnectedAsync(exception);
    }

    public static string? GetConnectionIdByEmail(string email)
    {
        UserConnections.TryGetValue(email, out var connectionId);
        return connectionId;
    }
}