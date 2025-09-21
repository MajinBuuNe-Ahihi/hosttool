# Hệ thống SignalR Real-time Notifications

## Tổng quan
Hệ thống này cung cấp khả năng gửi thông báo real-time giữa server và client sử dụng SignalR. Khi có thay đổi dữ liệu hoặc có client kết nối/ngắt kết nối, hệ thống sẽ tự động gửi thông báo đến tất cả client đang kết nối.

## Tính năng chính

### 1. Thông báo Real-time
- **Thông báo khi tạo scheduler**: Khi tạo scheduler mới, tất cả client sẽ nhận được thông báo
- **Thông báo khi cập nhật scheduler**: Khi cập nhật scheduler, tất cả client sẽ nhận được thông báo
- **Thông báo khi có client kết nối**: Khi có client mới kết nối, các client khác sẽ nhận được thông báo
- **Thông báo khi có client ngắt kết nối**: Khi có client ngắt kết nối, các client khác sẽ nhận được thông báo

### 2. Chat Real-time
- Gửi tin nhắn đến tất cả client đang kết nối
- Hiển thị tin nhắn chat real-time

### 3. Quản lý Groups
- Tham gia/rời khỏi group
- Gửi thông báo đến group cụ thể

### 4. Quản lý kết nối
- Hiển thị trạng thái kết nối
- Hiển thị danh sách client đang kết nối
- Tự động kết nối lại khi mất kết nối

## Cấu trúc Backend

### 1. SignalR Hub (`Hubs/NotificationHub.cs`)
```csharp
public class NotificationHub : Hub
{
    // Xử lý kết nối/ngắt kết nối
    public override async Task OnConnectedAsync()
    public override async Task OnDisconnectedAsync(Exception? exception)
    
    // Gửi thông báo
    public async Task SendNotificationToAll(string title, string message, string type)
    public async Task SendNotificationToClient(string connectionId, string title, string message, string type)
    public async Task SendNotificationToGroup(string groupName, string title, string message, string type)
    
    // Chat
    public async Task SendMessageToAll(string message)
    
    // Quản lý groups
    public async Task JoinGroup(string groupName)
    public async Task LeaveGroup(string groupName)
    
    // Quản lý kết nối
    public async Task GetConnectedClients()
}
```

### 2. Notification Service (`Services/NotificationService.cs`)
```csharp
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
```

### 3. Controller Updates (`Controllers/ToolController.cs`)
- Tích hợp `INotificationService` vào controller
- Gửi thông báo khi tạo/cập nhật scheduler
- Thêm endpoints để gửi thông báo tùy chỉnh

## Cấu trúc Frontend

### 1. SignalR Service (`view/src/api/signalr.ts`)
```typescript
class SignalRService {
    // Kết nối
    async startConnection(): Promise<void>
    async stopConnection(): Promise<void>
    
    // Gửi thông báo
    async sendNotificationToAll(title: string, message: string, type: string)
    async sendMessageToAll(message: string)
    
    // Quản lý groups
    async joinGroup(groupName: string)
    async leaveGroup(groupName: string)
    
    // Event handlers
    setOnNotification(callback: (notification: Notification) => void)
    setOnClientConnected(callback: (client: ClientConnection) => void)
    // ... các event handlers khác
}
```

### 2. Notification Center Component (`view/src/components/NotificationCenter.vue`)
- Hiển thị trạng thái kết nối
- Form để gửi thông báo tùy chỉnh
- Hiển thị danh sách thông báo
- Chat interface
- Quản lý groups
- Hiển thị danh sách client đang kết nối

## Cách sử dụng

### 1. Khởi động Backend
```bash
cd HostTool
dotnet run
```

### 2. Khởi động Frontend
```bash
cd view
npm install
npm run dev
```

### 3. Truy cập ứng dụng
- Mở trình duyệt và truy cập `http://localhost:5173`
- Hệ thống sẽ tự động kết nối SignalR
- Bạn sẽ thấy trạng thái kết nối và có thể gửi thông báo

### 4. Test thông báo
1. **Tạo scheduler mới**: Tạo scheduler mới sẽ gửi thông báo đến tất cả client
2. **Cập nhật scheduler**: Cập nhật scheduler sẽ gửi thông báo đến tất cả client
3. **Gửi thông báo tùy chỉnh**: Sử dụng form trong Notification Center
4. **Chat**: Gửi tin nhắn đến tất cả client
5. **Groups**: Tham gia group và gửi thông báo đến group

## API Endpoints

### SignalR Hub
- **URL**: `/notificationHub`
- **Transport**: WebSockets

### REST API
- `POST /api/tool/SendNotification` - Gửi thông báo tùy chỉnh
- `POST /api/tool/SendSystemEvent` - Gửi thông báo sự kiện hệ thống

## Cấu hình

### Backend (`Program.cs`)
```csharp
// Add SignalR
builder.Services.AddSignalR();

// Add Notification Service
builder.Services.AddScoped<INotificationService, NotificationService>();

// Map SignalR Hub
app.MapHub<NotificationHub>("/notificationHub");
```

### Frontend (`view/package.json`)
```json
{
  "dependencies": {
    "@microsoft/signalr": "^8.0.0"
  }
}
```

## Troubleshooting

### 1. Lỗi kết nối SignalR
- Kiểm tra CORS configuration
- Kiểm tra firewall/network
- Kiểm tra console log để xem lỗi chi tiết

### 2. Thông báo không hiển thị
- Kiểm tra kết nối SignalR
- Kiểm tra event handlers
- Kiểm tra console log

### 3. Lỗi build frontend
- Chạy `npm install` để cài đặt dependencies
- Kiểm tra version Node.js (yêu cầu >= 20.19.0)

## Mở rộng

### 1. Thêm loại thông báo mới
- Thêm method mới trong `INotificationService`
- Cập nhật controller để gọi method mới
- Cập nhật frontend để xử lý loại thông báo mới

### 2. Thêm authentication
- Thêm JWT authentication cho SignalR
- Xác thực user trước khi gửi thông báo

### 3. Thêm database logging
- Lưu lịch sử thông báo vào database
- Thêm pagination cho danh sách thông báo

### 4. Thêm push notifications
- Tích hợp với service push notification
- Gửi thông báo đến mobile app

## Kết luận
Hệ thống SignalR này cung cấp một giải pháp hoàn chỉnh cho việc gửi thông báo real-time. Nó có thể dễ dàng mở rộng và tùy chỉnh theo nhu cầu cụ thể của dự án.
