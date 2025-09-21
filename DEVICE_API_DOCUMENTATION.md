# Device Integration API Documentation

## Tổng quan
API này cho phép các device IoT đăng ký, gửi heartbeat và quản lý trạng thái trong hệ thống. Tất cả các thay đổi sẽ được gửi real-time đến Vue frontend thông qua SignalR.

## Base URL
```
https://localhost:7000/api/device
```

## Authentication
Hiện tại API không yêu cầu authentication. Trong production, nên thêm API key hoặc JWT token.

## Endpoints

### 1. Device Registration
Đăng ký device mới hoặc cập nhật thông tin device đã tồn tại.

**POST** `/api/device/register`

**Request Body:**
```json
{
  "deviceName": "My IoT Device",
  "description": "Temperature sensor in living room",
  "deviceType": "Sensor",
  "location": "Living Room",
  "firmwareVersion": "1.2.3",
  "hardwareVersion": "2.1",
  "capabilities": "{\"sensors\":[\"temperature\",\"humidity\"],\"actuators\":[]}",
  "configuration": "{\"samplingRate\":30,\"threshold\":25}",
  "manufacturer": "Acme Corp",
  "model": "TempSensor Pro",
  "serialNumber": "TS001234567",
  "macAddress": "AA:BB:CC:DD:EE:FF",
  "heartbeatInterval": 30
}
```

**Response:**
```json
{
  "success": true,
  "deviceId": "550e8400-e29b-41d4-a716-446655440000",
  "message": "Device registered successfully"
}
```

**Real-time Notification:**
Khi device đăng ký thành công, tất cả client Vue sẽ nhận được thông báo:
```json
{
  "title": "Device Registered",
  "message": "Device 'My IoT Device' has been registered and is now online!",
  "type": "success",
  "timestamp": "2024-01-20T10:30:00Z",
  "from": "System"
}
```

### 2. Device Heartbeat
Gửi heartbeat để duy trì trạng thái online và cập nhật thông tin device.

**POST** `/api/device/heartbeat`

**Request Body:**
```json
{
  "deviceId": "550e8400-e29b-41d4-a716-446655440000",
  "status": "Online",
  "batteryLevel": 85,
  "temperature": 23.5,
  "signalStrength": -45,
  "lastKnownIP": "192.168.1.100",
  "data": "{\"cpuUsage\":45,\"memoryUsage\":60,\"sensorReadings\":{\"temp\":23.5,\"humidity\":65}}"
}
```

**Response:**
```json
{
  "success": true,
  "message": "Heartbeat updated successfully",
  "timestamp": "2024-01-20T10:30:30Z"
}
```

**Real-time Notification:**
Heartbeat sẽ trigger SignalR event đến tất cả client:
```json
{
  "deviceId": "550e8400-e29b-41d4-a716-446655440000",
  "deviceName": "My IoT Device",
  "timestamp": "2024-01-20T10:30:30Z",
  "data": {
    "batteryLevel": 85,
    "temperature": 23.5,
    "signalStrength": -45
  }
}
```

### 3. Get All Devices
Lấy danh sách tất cả devices.

**GET** `/api/device`

**Response:**
```json
[
  {
    "deviceId": "550e8400-e29b-41d4-a716-446655440000",
    "deviceName": "My IoT Device",
    "description": "Temperature sensor in living room",
    "deviceType": "Sensor",
    "status": "Online",
    "location": "Living Room",
    "firmwareVersion": "1.2.3",
    "hardwareVersion": "2.1",
    "lastKnownIP": "192.168.1.100",
    "lastSeen": "2024-01-20T10:30:30Z",
    "createdAt": "2024-01-20T09:00:00Z",
    "updatedAt": "2024-01-20T10:30:30Z",
    "batteryLevel": 85,
    "temperature": 23.5,
    "signalStrength": -45,
    "manufacturer": "Acme Corp",
    "model": "TempSensor Pro",
    "serialNumber": "TS001234567",
    "macAddress": "AA:BB:CC:DD:EE:FF",
    "isActive": true,
    "isRegistered": true,
    "heartbeatInterval": 30,
    "lastHeartbeat": "2024-01-20T10:30:30Z",
    "errorCount": 0
  }
]
```

### 4. Get Device by ID
Lấy thông tin chi tiết của một device.

**GET** `/api/device/{deviceId}`

**Response:**
```json
{
  "deviceId": "550e8400-e29b-41d4-a716-446655440000",
  "deviceName": "My IoT Device",
  // ... (same as above)
}
```

### 5. Get Online Devices
Lấy danh sách devices đang online.

**GET** `/api/device/online`

**Response:**
```json
[
  // Array of online devices
]
```

### 6. Get Offline Devices
Lấy danh sách devices đang offline.

**GET** `/api/device/offline`

**Response:**
```json
[
  // Array of offline devices
]
```

### 7. Update Device Status
Cập nhật trạng thái device.

**PUT** `/api/device/{deviceId}/status`

**Request Body:**
```json
{
  "status": "Maintenance"
}
```

**Response:**
```json
{
  "success": true,
  "message": "Device status updated successfully"
}
```

**Real-time Notification:**
```json
{
  "title": "Device Status Updated",
  "message": "Device status has been updated to: Maintenance",
  "type": "info",
  "timestamp": "2024-01-20T10:35:00Z",
  "from": "System"
}
```

### 8. Delete Device
Xóa device khỏi hệ thống.

**DELETE** `/api/device/{deviceId}`

**Response:**
```json
{
  "success": true,
  "message": "Device deleted successfully"
}
```

**Real-time Notification:**
```json
{
  "title": "Device Deleted",
  "message": "Device 'My IoT Device' has been removed from the system",
  "type": "warning",
  "timestamp": "2024-01-20T10:40:00Z",
  "from": "System"
}
```

### 9. Get Device Events
Lấy lịch sử events của device.

**GET** `/api/device/{deviceId}/events?limit=50`

**Response:**
```json
[
  {
    "eventId": "event-123",
    "deviceId": "550e8400-e29b-41d4-a716-446655440000",
    "eventType": "Heartbeat",
    "message": "Heartbeat received from IP: 192.168.1.100",
    "data": "{\"batteryLevel\":85}",
    "timestamp": "2024-01-20T10:30:30Z",
    "severity": "Info",
    "isProcessed": true
  }
]
```

### 10. Get Device Status
Kiểm tra trạng thái online/offline của device.

**GET** `/api/device/{deviceId}/status`

**Response:**
```json
{
  "deviceId": "550e8400-e29b-41d4-a716-446655440000",
  "isOnline": true,
  "timestamp": "2024-01-20T10:30:30Z"
}
```

### 11. Cleanup Offline Devices
Đánh dấu devices offline dựa trên timeout.

**POST** `/api/device/cleanup?timeoutMinutes=5`

**Response:**
```json
{
  "success": true,
  "message": "Offline devices cleanup completed"
}
```

## SignalR Events

### Connection
```javascript
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/notificationHub")
    .build();

await connection.start();
```

### Event Handlers

#### 1. Device Status Updates
```javascript
connection.on("ReceiveDeviceStatusUpdate", (notification) => {
    console.log("Device status update:", notification);
    // notification contains: deviceId, deviceName, status, etc.
});
```

#### 2. Device Events
```javascript
connection.on("ReceiveDeviceEvent", (notification) => {
    console.log("Device event:", notification);
    // notification contains: deviceId, deviceName, eventType, message, severity
});
```

#### 3. Device Heartbeats
```javascript
connection.on("ReceiveDeviceHeartbeat", (heartbeat) => {
    console.log("Device heartbeat:", heartbeat);
    // heartbeat contains: deviceId, deviceName, timestamp, data
});
```

#### 4. General Notifications
```javascript
connection.on("ReceiveNotification", (notification) => {
    console.log("Notification:", notification);
    // notification contains: title, message, type, timestamp, from
});
```

## Device Implementation Examples

### C# Device Client
```csharp
public class IoTDevice
{
    private readonly HttpClient _httpClient;
    private readonly string _deviceId;
    private readonly Timer _heartbeatTimer;

    public async Task RegisterAsync()
    {
        var request = new
        {
            deviceName = "My IoT Device",
            deviceType = "Sensor",
            manufacturer = "Acme Corp",
            model = "TempSensor Pro",
            serialNumber = "TS001234567",
            macAddress = GetMacAddress(),
            heartbeatInterval = 30
        };

        var response = await _httpClient.PostAsJsonAsync("/api/device/register", request);
        var result = await response.Content.ReadFromJsonAsync<RegistrationResponse>();
        
        if (result.Success)
        {
            _deviceId = result.DeviceId;
            StartHeartbeat();
        }
    }

    private void StartHeartbeat()
    {
        _heartbeatTimer = new Timer(async _ => await SendHeartbeat(), null, 
            TimeSpan.Zero, TimeSpan.FromSeconds(30));
    }

    private async Task SendHeartbeat()
    {
        var request = new
        {
            deviceId = _deviceId,
            status = "Online",
            batteryLevel = GetBatteryLevel(),
            temperature = GetTemperature(),
            signalStrength = GetSignalStrength(),
            data = JsonSerializer.Serialize(GetSensorData())
        };

        await _httpClient.PostAsJsonAsync("/api/device/heartbeat", request);
    }
}
```

### Python Device Client
```python
import requests
import json
import time
import threading

class IoTDevice:
    def __init__(self, base_url):
        self.base_url = base_url
        self.device_id = None
        self.session = requests.Session()
        
    def register(self):
        data = {
            "deviceName": "My IoT Device",
            "deviceType": "Sensor",
            "manufacturer": "Acme Corp",
            "model": "TempSensor Pro",
            "serialNumber": "TS001234567",
            "macAddress": self.get_mac_address(),
            "heartbeatInterval": 30
        }
        
        response = self.session.post(f"{self.base_url}/api/device/register", json=data)
        result = response.json()
        
        if result["success"]:
            self.device_id = result["deviceId"]
            self.start_heartbeat()
            
    def start_heartbeat(self):
        def heartbeat_loop():
            while True:
                self.send_heartbeat()
                time.sleep(30)
                
        thread = threading.Thread(target=heartbeat_loop)
        thread.daemon = True
        thread.start()
        
    def send_heartbeat(self):
        data = {
            "deviceId": self.device_id,
            "status": "Online",
            "batteryLevel": self.get_battery_level(),
            "temperature": self.get_temperature(),
            "signalStrength": self.get_signal_strength(),
            "data": json.dumps(self.get_sensor_data())
        }
        
        self.session.post(f"{self.base_url}/api/device/heartbeat", json=data)
```

### JavaScript/Node.js Device Client
```javascript
const axios = require('axios');

class IoTDevice {
    constructor(baseUrl) {
        this.baseUrl = baseUrl;
        this.deviceId = null;
        this.heartbeatInterval = null;
    }

    async register() {
        const data = {
            deviceName: 'My IoT Device',
            deviceType: 'Sensor',
            manufacturer: 'Acme Corp',
            model: 'TempSensor Pro',
            serialNumber: 'TS001234567',
            macAddress: this.getMacAddress(),
            heartbeatInterval: 30
        };

        const response = await axios.post(`${this.baseUrl}/api/device/register`, data);
        
        if (response.data.success) {
            this.deviceId = response.data.deviceId;
            this.startHeartbeat();
        }
    }

    startHeartbeat() {
        this.heartbeatInterval = setInterval(() => {
            this.sendHeartbeat();
        }, 30000);
    }

    async sendHeartbeat() {
        const data = {
            deviceId: this.deviceId,
            status: 'Online',
            batteryLevel: this.getBatteryLevel(),
            temperature: this.getTemperature(),
            signalStrength: this.getSignalStrength(),
            data: JSON.stringify(this.getSensorData())
        };

        await axios.post(`${this.baseUrl}/api/device/heartbeat`, data);
    }
}
```

## Error Handling

### HTTP Status Codes
- `200 OK` - Request successful
- `400 Bad Request` - Invalid request data
- `404 Not Found` - Device not found
- `500 Internal Server Error` - Server error

### Error Response Format
```json
{
  "error": "Device not found",
  "message": "The specified device ID does not exist",
  "timestamp": "2024-01-20T10:30:00Z"
}
```

## Best Practices

### 1. Device Registration
- Gọi register endpoint khi device khởi động
- Sử dụng serial number hoặc MAC address để tránh duplicate
- Implement retry logic nếu registration fails

### 2. Heartbeat
- Gửi heartbeat theo interval đã đăng ký
- Implement exponential backoff nếu heartbeat fails
- Gửi heartbeat ngay cả khi không có thay đổi data

### 3. Error Handling
- Log tất cả errors và retry nếu cần
- Implement circuit breaker pattern cho network calls
- Graceful degradation khi server không available

### 4. Data Format
- Sử dụng JSON format cho tất cả data
- Validate data trước khi gửi
- Sử dụng consistent naming conventions

### 5. Security
- Implement API key authentication trong production
- Sử dụng HTTPS cho tất cả communications
- Validate và sanitize tất cả input data

## Testing

### Device Simulator
Sử dụng DeviceSimulator project để test:
```bash
cd DeviceSimulator
dotnet run
```

### Manual Testing với curl
```bash
# Register device
curl -X POST https://localhost:7000/api/device/register \
  -H "Content-Type: application/json" \
  -d '{
    "deviceName": "Test Device",
    "deviceType": "IoT",
    "manufacturer": "Test Corp",
    "model": "Test Model",
    "serialNumber": "TEST123456"
  }'

# Send heartbeat
curl -X POST https://localhost:7000/api/device/heartbeat \
  -H "Content-Type: application/json" \
  -d '{
    "deviceId": "your-device-id",
    "status": "Online",
    "batteryLevel": 85
  }'
```

## Monitoring và Debugging

### 1. Logs
- Tất cả device events được log trong database
- Sử dụng `/api/device/{deviceId}/events` để xem lịch sử

### 2. Real-time Monitoring
- Vue frontend hiển thị real-time device status
- SignalR notifications cho tất cả device events

### 3. Health Checks
- Sử dụng `/api/device/{deviceId}/status` để check device health
- Implement monitoring cho offline devices

Hệ thống này cung cấp một giải pháp hoàn chỉnh cho việc quản lý IoT devices với real-time monitoring và notifications!
