using HostTool.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace HostTool.Services
{
    public interface INotificationService
    {
        Task SendNotificationToAllAsync(string title, string message, string type = "info");
        Task SendNotificationToClientAsync(string connectionId, string title, string message, string type = "info");
        Task SendNotificationToGroupAsync(string groupName, string title, string message, string type = "info");
        Task NotifySchedulerCreatedAsync(string schedulerName);
        Task NotifySchedulerUpdatedAsync(string schedulerName);
        Task NotifySchedulerDeletedAsync(string schedulerName);
        Task NotifySystemEventAsync(string eventType, string description);
    }

    public class NotificationService : INotificationService
    {
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(IHubContext<NotificationHub> hubContext, ILogger<NotificationService> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
        }

        public async Task SendNotificationToAllAsync(string title, string message, string type = "info")
        {
            try
            {
                var notification = new
                {
                    Title = title,
                    Message = message,
                    Type = type,
                    Timestamp = DateTime.Now,
                    From = "System"
                };

                await _hubContext.Clients.All.SendAsync("ReceiveNotification", notification);
                _logger.LogInformation($"Notification sent to all clients: {title} - {message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending notification to all clients: {title} - {message}");
            }
        }

        public async Task SendNotificationToClientAsync(string connectionId, string title, string message, string type = "info")
        {
            try
            {
                var notification = new
                {
                    Title = title,
                    Message = message,
                    Type = type,
                    Timestamp = DateTime.Now,
                    From = "System"
                };

                await _hubContext.Clients.Client(connectionId).SendAsync("ReceiveNotification", notification);
                _logger.LogInformation($"Notification sent to client {connectionId}: {title} - {message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending notification to client {connectionId}: {title} - {message}");
            }
        }

        public async Task SendNotificationToGroupAsync(string groupName, string title, string message, string type = "info")
        {
            try
            {
                var notification = new
                {
                    Title = title,
                    Message = message,
                    Type = type,
                    Timestamp = DateTime.Now,
                    From = "System",
                    GroupName = groupName
                };

                await _hubContext.Clients.Group(groupName).SendAsync("ReceiveNotification", notification);
                _logger.LogInformation($"Notification sent to group {groupName}: {title} - {message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending notification to group {groupName}: {title} - {message}");
            }
        }

        public async Task NotifySchedulerCreatedAsync(string schedulerName)
        {
            await SendNotificationToAllAsync(
                "Scheduler Created", 
                $"Scheduler '{schedulerName}' đã được tạo thành công!", 
                "success"
            );
        }

        public async Task NotifySchedulerUpdatedAsync(string schedulerName)
        {
            await SendNotificationToAllAsync(
                "Scheduler Updated", 
                $"Scheduler '{schedulerName}' đã được cập nhật!", 
                "info"
            );
        }

        public async Task NotifySchedulerDeletedAsync(string schedulerName)
        {
            await SendNotificationToAllAsync(
                "Scheduler Deleted", 
                $"Scheduler '{schedulerName}' đã được xóa!", 
                "warning"
            );
        }

        public async Task NotifySystemEventAsync(string eventType, string description)
        {
            await SendNotificationToAllAsync(
                $"System Event: {eventType}", 
                description, 
                "info"
            );
        }
    }
}
