using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace HostTool.Hubs
{
    public class NotificationHub : Hub
    {
        private static readonly ConcurrentDictionary<string, string> ConnectedUsers = new();
        private readonly ILogger<NotificationHub> _logger;

        public NotificationHub(ILogger<NotificationHub> logger)
        {
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            var connectionId = Context.ConnectionId;
            var userAgent = Context.GetHttpContext()?.Request.Headers["User-Agent"].ToString() ?? "Unknown";
            var clientIp = Context.GetHttpContext()?.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            
            // Lưu thông tin client đã kết nối
            ConnectedUsers.TryAdd(connectionId, $"{clientIp} - {userAgent}");
            
            _logger.LogInformation($"Client connected: {connectionId} from {clientIp}");
            
            // Gửi thông báo đến tất cả client khác về việc có client mới kết nối
            await Clients.Others.SendAsync("ClientConnected", new
            {
                ConnectionId = connectionId,
                ClientIp = clientIp,
                UserAgent = userAgent,
                ConnectedAt = DateTime.Now,
                TotalConnections = ConnectedUsers.Count
            });

            // Gửi thông báo chào mừng đến client vừa kết nối
            await Clients.Caller.SendAsync("WelcomeMessage", new
            {
                Message = "Chào mừng bạn đến với hệ thống thông báo real-time!",
                ConnectionId = connectionId,
                ConnectedAt = DateTime.Now
            });

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var connectionId = Context.ConnectionId;
            
            // Xóa thông tin client đã ngắt kết nối
            ConnectedUsers.TryRemove(connectionId, out _);
            
            _logger.LogInformation($"Client disconnected: {connectionId}");
            
            // Gửi thông báo đến tất cả client khác về việc có client ngắt kết nối
            await Clients.Others.SendAsync("ClientDisconnected", new
            {
                ConnectionId = connectionId,
                DisconnectedAt = DateTime.Now,
                TotalConnections = ConnectedUsers.Count
            });

            await base.OnDisconnectedAsync(exception);
        }

        // Method để gửi thông báo đến tất cả client
        public async Task SendNotificationToAll(string title, string message, string type = "info")
        {
            var notification = new
            {
                Title = title,
                Message = message,
                Type = type, // info, success, warning, error
                Timestamp = DateTime.Now,
                From = "System"
            };

            await Clients.All.SendAsync("ReceiveNotification", notification);
            _logger.LogInformation($"Notification sent to all clients: {title} - {message}");
        }

        // Method để gửi thông báo đến một client cụ thể
        public async Task SendNotificationToClient(string connectionId, string title, string message, string type = "info")
        {
            var notification = new
            {
                Title = title,
                Message = message,
                Type = type,
                Timestamp = DateTime.Now,
                From = "System"
            };

            await Clients.Client(connectionId).SendAsync("ReceiveNotification", notification);
            _logger.LogInformation($"Notification sent to client {connectionId}: {title} - {message}");
        }

        // Method để lấy danh sách client đang kết nối
        public async Task GetConnectedClients()
        {
            var clients = ConnectedUsers.Select(kvp => new
            {
                ConnectionId = kvp.Key,
                Info = kvp.Value
            }).ToList();

            await Clients.Caller.SendAsync("ConnectedClientsList", clients);
        }

        // Method để client gửi tin nhắn đến tất cả client khác
        public async Task SendMessageToAll(string message)
        {
            var chatMessage = new
            {
                Message = message,
                Sender = Context.ConnectionId,
                Timestamp = DateTime.Now
            };

            await Clients.All.SendAsync("ReceiveMessage", chatMessage);
            _logger.LogInformation($"Message sent from {Context.ConnectionId}: {message}");
        }

        // Method để join vào một group cụ thể
        public async Task JoinGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            await Clients.Group(groupName).SendAsync("UserJoinedGroup", new
            {
                ConnectionId = Context.ConnectionId,
                GroupName = groupName,
                JoinedAt = DateTime.Now
            });
            _logger.LogInformation($"Client {Context.ConnectionId} joined group {groupName}");
        }

        // Method để rời khỏi group
        public async Task LeaveGroup(string groupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
            await Clients.Group(groupName).SendAsync("UserLeftGroup", new
            {
                ConnectionId = Context.ConnectionId,
                GroupName = groupName,
                LeftAt = DateTime.Now
            });
            _logger.LogInformation($"Client {Context.ConnectionId} left group {groupName}");
        }

        // Method để gửi thông báo đến một group cụ thể
        public async Task SendNotificationToGroup(string groupName, string title, string message, string type = "info")
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

            await Clients.Group(groupName).SendAsync("ReceiveNotification", notification);
            _logger.LogInformation($"Notification sent to group {groupName}: {title} - {message}");
        }

        // Device-specific methods
        public async Task SendDeviceStatusUpdate(string deviceId, string deviceName, string status)
        {
            var notification = new
            {
                Title = "Device Status Update",
                Message = $"Device '{deviceName}' status changed to: {status}",
                Type = status == "Online" ? "success" : status == "Offline" ? "warning" : "info",
                Timestamp = DateTime.Now,
                From = "Device System",
                DeviceId = deviceId,
                DeviceName = deviceName,
                Status = status
            };

            await Clients.All.SendAsync("ReceiveDeviceStatusUpdate", notification);
            _logger.LogInformation($"Device status update sent: {deviceName} - {status}");
        }

        public async Task SendDeviceEvent(string deviceId, string deviceName, string eventType, string message, string severity = "info")
        {
            var notification = new
            {
                Title = $"Device Event: {eventType}",
                Message = $"[{deviceName}] {message}",
                Type = severity.ToLower(),
                Timestamp = DateTime.Now,
                From = "Device System",
                DeviceId = deviceId,
                DeviceName = deviceName,
                EventType = eventType,
                Severity = severity
            };

            await Clients.All.SendAsync("ReceiveDeviceEvent", notification);
            _logger.LogInformation($"Device event sent: {deviceName} - {eventType} - {message}");
        }

        public async Task SendDeviceHeartbeat(string deviceId, string deviceName, object? data = null)
        {
            var heartbeat = new
            {
                DeviceId = deviceId,
                DeviceName = deviceName,
                Timestamp = DateTime.Now,
                Data = data
            };

            await Clients.All.SendAsync("ReceiveDeviceHeartbeat", heartbeat);
            _logger.LogInformation($"Device heartbeat sent: {deviceName}");
        }

        public async Task JoinDeviceGroup(string deviceId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"device_{deviceId}");
            _logger.LogInformation($"Client {Context.ConnectionId} joined device group: {deviceId}");
        }

        public async Task LeaveDeviceGroup(string deviceId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"device_{deviceId}");
            _logger.LogInformation($"Client {Context.ConnectionId} left device group: {deviceId}");
        }

        public async Task SendNotificationToDeviceGroup(string deviceId, string title, string message, string type = "info")
        {
            var notification = new
            {
                Title = title,
                Message = message,
                Type = type,
                Timestamp = DateTime.Now,
                From = "Device System",
                DeviceId = deviceId
            };

            await Clients.Group($"device_{deviceId}").SendAsync("ReceiveNotification", notification);
            _logger.LogInformation($"Notification sent to device group {deviceId}: {title} - {message}");
        }
    }
}
