using HostTool.Domain;
using HostTool.Services;
using Microsoft.AspNetCore.Mvc;

namespace HostTool.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DeviceController : ControllerBase
    {
        private readonly IDeviceService _deviceService;
        private readonly INotificationService _notificationService;
        private readonly ILogger<DeviceController> _logger;

        public DeviceController(
            IDeviceService deviceService, 
            INotificationService notificationService,
            ILogger<DeviceController> logger)
        {
            _deviceService = deviceService;
            _notificationService = notificationService;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterDevice([FromBody] DeviceRegistrationRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrEmpty(request.DeviceName))
                {
                    return BadRequest("Device name is required");
                }

                var clientIP = GetClientIP();
                var device = await _deviceService.RegisterDeviceAsync(request, clientIP);

                if (device == null)
                {
                    return BadRequest("Failed to register device");
                }

                // Send real-time notification
                await _notificationService.SendNotificationToAllAsync(
                    "Device Registered", 
                    $"Device '{device.DeviceName}' has been registered and is now online!", 
                    "success"
                );

                _logger.LogInformation($"Device registered successfully: {device.DeviceName} ({device.DeviceId})");

                return Ok(new
                {
                    success = true,
                    deviceId = device.DeviceId,
                    message = "Device registered successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering device");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("heartbeat")]
        public async Task<IActionResult> UpdateHeartbeat([FromBody] DeviceHeartbeatRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrEmpty(request.DeviceId))
                {
                    return BadRequest("Device ID is required");
                }

                var clientIP = GetClientIP();
                var success = await _deviceService.UpdateDeviceHeartbeatAsync(request, clientIP);

                if (!success)
                {
                    return BadRequest("Device not found or heartbeat update failed");
                }

                return Ok(new
                {
                    success = true,
                    message = "Heartbeat updated successfully",
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating heartbeat for device: {request?.DeviceId}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllDevices()
        {
            try
            {
                var devices = await _deviceService.GetAllDevicesAsync();
                return Ok(devices);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all devices");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("{deviceId}")]
        public async Task<IActionResult> GetDevice(string deviceId)
        {
            try
            {
                if (string.IsNullOrEmpty(deviceId))
                {
                    return BadRequest("Device ID is required");
                }

                var device = await _deviceService.GetDeviceByIdAsync(deviceId);
                if (device == null)
                {
                    return NotFound("Device not found");
                }

                return Ok(device);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting device: {deviceId}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("online")]
        public async Task<IActionResult> GetOnlineDevices()
        {
            try
            {
                var devices = await _deviceService.GetOnlineDevicesAsync();
                return Ok(devices);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting online devices");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("offline")]
        public async Task<IActionResult> GetOfflineDevices()
        {
            try
            {
                var devices = await _deviceService.GetOfflineDevicesAsync();
                return Ok(devices);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting offline devices");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut("{deviceId}/status")]
        public async Task<IActionResult> UpdateDeviceStatus(string deviceId, [FromBody] UpdateStatusRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(deviceId) || request == null || string.IsNullOrEmpty(request.Status))
                {
                    return BadRequest("Device ID and status are required");
                }

                var success = await _deviceService.UpdateDeviceStatusAsync(deviceId, request.Status);
                if (!success)
                {
                    return NotFound("Device not found");
                }

                // Send real-time notification
                await _notificationService.SendNotificationToAllAsync(
                    "Device Status Updated", 
                    $"Device status has been updated to: {request.Status}", 
                    "info"
                );

                return Ok(new
                {
                    success = true,
                    message = "Device status updated successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating device status: {deviceId}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("{deviceId}")]
        public async Task<IActionResult> DeleteDevice(string deviceId)
        {
            try
            {
                if (string.IsNullOrEmpty(deviceId))
                {
                    return BadRequest("Device ID is required");
                }

                var device = await _deviceService.GetDeviceByIdAsync(deviceId);
                if (device == null)
                {
                    return NotFound("Device not found");
                }

                var success = await _deviceService.DeleteDeviceAsync(deviceId);
                if (!success)
                {
                    return BadRequest("Failed to delete device");
                }

                // Send real-time notification
                await _notificationService.SendNotificationToAllAsync(
                    "Device Deleted", 
                    $"Device '{device.DeviceName}' has been removed from the system", 
                    "warning"
                );

                return Ok(new
                {
                    success = true,
                    message = "Device deleted successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting device: {deviceId}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("{deviceId}/events")]
        public async Task<IActionResult> GetDeviceEvents(string deviceId, [FromQuery] int limit = 50)
        {
            try
            {
                if (string.IsNullOrEmpty(deviceId))
                {
                    return BadRequest("Device ID is required");
                }

                var events = await _deviceService.GetDeviceEventsAsync(deviceId, limit);
                return Ok(events);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting device events: {deviceId}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("{deviceId}/status")]
        public async Task<IActionResult> GetDeviceStatus(string deviceId)
        {
            try
            {
                if (string.IsNullOrEmpty(deviceId))
                {
                    return BadRequest("Device ID is required");
                }

                var isOnline = await _deviceService.IsDeviceOnlineAsync(deviceId);
                return Ok(new
                {
                    deviceId = deviceId,
                    isOnline = isOnline,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting device status: {deviceId}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("cleanup")]
        public async Task<IActionResult> CleanupOfflineDevices([FromQuery] int timeoutMinutes = 5)
        {
            try
            {
                await _deviceService.CleanupOfflineDevicesAsync(timeoutMinutes);
                
                // Send real-time notification
                await _notificationService.SendNotificationToAllAsync(
                    "System Maintenance", 
                    "Offline devices cleanup completed", 
                    "info"
                );

                return Ok(new
                {
                    success = true,
                    message = "Offline devices cleanup completed"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up offline devices");
                return StatusCode(500, "Internal server error");
            }
        }

        private string GetClientIP()
        {
            // Try to get the real IP address from headers (for reverse proxy scenarios)
            var forwardedFor = Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwardedFor))
            {
                return forwardedFor.Split(',')[0].Trim();
            }

            var realIP = Request.Headers["X-Real-IP"].FirstOrDefault();
            if (!string.IsNullOrEmpty(realIP))
            {
                return realIP;
            }

            // Fallback to connection remote IP
            return Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        }
    }

    public class UpdateStatusRequest
    {
        public string Status { get; set; } = string.Empty;
    }
}
