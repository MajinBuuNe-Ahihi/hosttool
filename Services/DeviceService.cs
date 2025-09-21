using HostTool.Domain;
using HostTool.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace HostTool.Services
{
    public interface IDeviceService
    {
        Task<Device?> RegisterDeviceAsync(DeviceRegistrationRequest request, string clientIP);
        Task<bool> UpdateDeviceHeartbeatAsync(DeviceHeartbeatRequest request, string clientIP);
        Task<List<Device>> GetAllDevicesAsync();
        Task<Device?> GetDeviceByIdAsync(string deviceId);
        Task<bool> UpdateDeviceStatusAsync(string deviceId, string status);
        Task<bool> DeleteDeviceAsync(string deviceId);
        Task<List<DeviceEvent>> GetDeviceEventsAsync(string deviceId, int limit = 50);
        Task<List<Device>> GetOnlineDevicesAsync();
        Task<List<Device>> GetOfflineDevicesAsync();
        Task<bool> IsDeviceOnlineAsync(string deviceId);
        Task CleanupOfflineDevicesAsync(int timeoutMinutes = 5);
    }

    public class DeviceService : IDeviceService
    {
        private readonly AppDataContext _context;
        private readonly ILogger<DeviceService> _logger;

        public DeviceService(AppDataContext context, ILogger<DeviceService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Device?> RegisterDeviceAsync(DeviceRegistrationRequest request, string clientIP)
        {
            try
            {
                // Check if device already exists
                var existingDevice = await _context.Devices
                    .FirstOrDefaultAsync(d => d.SerialNumber == request.SerialNumber || 
                                            d.MacAddress == request.MacAddress);

                if (existingDevice != null)
                {
                    // Update existing device
                    existingDevice.DeviceName = request.DeviceName;
                    existingDevice.Description = request.Description;
                    existingDevice.DeviceType = request.DeviceType;
                    existingDevice.Location = request.Location;
                    existingDevice.FirmwareVersion = request.FirmwareVersion;
                    existingDevice.HardwareVersion = request.HardwareVersion;
                    existingDevice.Capabilities = request.Capabilities;
                    existingDevice.Configuration = request.Configuration;
                    existingDevice.Manufacturer = request.Manufacturer;
                    existingDevice.Model = request.Model;
                    existingDevice.SerialNumber = request.SerialNumber;
                    existingDevice.MacAddress = request.MacAddress;
                    existingDevice.HeartbeatInterval = request.HeartbeatInterval;
                    existingDevice.Status = "Online";
                    existingDevice.LastSeen = DateTime.UtcNow;
                    existingDevice.LastKnownIP = clientIP;
                    existingDevice.IsRegistered = true;
                    existingDevice.UpdatedAt = DateTime.UtcNow;

                    await _context.SaveChangesAsync();

                    // Log device re-registration event
                    await LogDeviceEventAsync(existingDevice.DeviceId, "ReRegister", 
                        $"Device re-registered from IP: {clientIP}", "Info");

                    _logger.LogInformation($"Device re-registered: {existingDevice.DeviceName} ({existingDevice.DeviceId})");
                    return existingDevice;
                }
                else
                {
                    // Create new device
                    var device = new Device
                    {
                        DeviceId = Guid.NewGuid().ToString(),
                        DeviceName = request.DeviceName,
                        Description = request.Description,
                        DeviceType = request.DeviceType,
                        Location = request.Location,
                        FirmwareVersion = request.FirmwareVersion,
                        HardwareVersion = request.HardwareVersion,
                        Capabilities = request.Capabilities,
                        Configuration = request.Configuration,
                        Manufacturer = request.Manufacturer,
                        Model = request.Model,
                        SerialNumber = request.SerialNumber,
                        MacAddress = request.MacAddress,
                        HeartbeatInterval = request.HeartbeatInterval,
                        Status = "Online",
                        LastSeen = DateTime.UtcNow,
                        LastKnownIP = clientIP,
                        IsRegistered = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    _context.Devices.Add(device);
                    await _context.SaveChangesAsync();

                    // Log device registration event
                    await LogDeviceEventAsync(device.DeviceId, "Register", 
                        $"Device registered from IP: {clientIP}", "Info");

                    _logger.LogInformation($"New device registered: {device.DeviceName} ({device.DeviceId})");
                    return device;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error registering device: {request.DeviceName}");
                return null;
            }
        }

        public async Task<bool> UpdateDeviceHeartbeatAsync(DeviceHeartbeatRequest request, string clientIP)
        {
            try
            {
                var device = await _context.Devices
                    .FirstOrDefaultAsync(d => d.DeviceId == request.DeviceId);

                if (device == null)
                {
                    _logger.LogWarning($"Device not found for heartbeat: {request.DeviceId}");
                    return false;
                }

                // Update device status
                device.LastSeen = DateTime.UtcNow;
                device.LastHeartbeat = DateTime.UtcNow;
                device.LastKnownIP = clientIP;
                device.UpdatedAt = DateTime.UtcNow;

                if (!string.IsNullOrEmpty(request.Status))
                {
                    device.Status = request.Status;
                }

                if (request.BatteryLevel.HasValue)
                {
                    device.BatteryLevel = request.BatteryLevel.Value;
                }

                if (request.Temperature.HasValue)
                {
                    device.Temperature = request.Temperature.Value;
                }

                if (request.SignalStrength.HasValue)
                {
                    device.SignalStrength = request.SignalStrength.Value;
                }

                // Update device status to Online if it was offline
                if (device.Status == "Offline")
                {
                    device.Status = "Online";
                }

                await _context.SaveChangesAsync();

                // Log heartbeat event
                await LogDeviceEventAsync(device.DeviceId, "Heartbeat", 
                    $"Heartbeat received from IP: {clientIP}", "Info", request.Data);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating device heartbeat: {request.DeviceId}");
                return false;
            }
        }

        public async Task<List<Device>> GetAllDevicesAsync()
        {
            try
            {
                return await _context.Devices
                    .OrderByDescending(d => d.LastSeen)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all devices");
                return new List<Device>();
            }
        }

        public async Task<Device?> GetDeviceByIdAsync(string deviceId)
        {
            try
            {
                return await _context.Devices
                    .FirstOrDefaultAsync(d => d.DeviceId == deviceId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting device by ID: {deviceId}");
                return null;
            }
        }

        public async Task<bool> UpdateDeviceStatusAsync(string deviceId, string status)
        {
            try
            {
                var device = await _context.Devices
                    .FirstOrDefaultAsync(d => d.DeviceId == deviceId);

                if (device == null)
                {
                    return false;
                }

                var oldStatus = device.Status;
                device.Status = status;
                device.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                // Log status change event
                await LogDeviceEventAsync(deviceId, "StatusChange", 
                    $"Status changed from {oldStatus} to {status}", "Info");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating device status: {deviceId}");
                return false;
            }
        }

        public async Task<bool> DeleteDeviceAsync(string deviceId)
        {
            try
            {
                var device = await _context.Devices
                    .FirstOrDefaultAsync(d => d.DeviceId == deviceId);

                if (device == null)
                {
                    return false;
                }

                _context.Devices.Remove(device);
                await _context.SaveChangesAsync();

                // Log device deletion event
                await LogDeviceEventAsync(deviceId, "Delete", 
                    $"Device deleted: {device.DeviceName}", "Warning");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting device: {deviceId}");
                return false;
            }
        }

        public async Task<List<DeviceEvent>> GetDeviceEventsAsync(string deviceId, int limit = 50)
        {
            try
            {
                return await _context.DeviceEvents
                    .Where(e => e.DeviceId == deviceId)
                    .OrderByDescending(e => e.Timestamp)
                    .Take(limit)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting device events: {deviceId}");
                return new List<DeviceEvent>();
            }
        }

        public async Task<List<Device>> GetOnlineDevicesAsync()
        {
            try
            {
                return await _context.Devices
                    .Where(d => d.Status == "Online" && d.IsActive)
                    .OrderByDescending(d => d.LastSeen)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting online devices");
                return new List<Device>();
            }
        }

        public async Task<List<Device>> GetOfflineDevicesAsync()
        {
            try
            {
                return await _context.Devices
                    .Where(d => d.Status == "Offline" && d.IsActive)
                    .OrderByDescending(d => d.LastSeen)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting offline devices");
                return new List<Device>();
            }
        }

        public async Task<bool> IsDeviceOnlineAsync(string deviceId)
        {
            try
            {
                var device = await _context.Devices
                    .FirstOrDefaultAsync(d => d.DeviceId == deviceId);

                if (device == null)
                {
                    return false;
                }

                // Check if device is online based on last seen time
                var timeoutMinutes = device.HeartbeatInterval / 60.0 * 3; // 3x heartbeat interval
                var isOnline = device.LastSeen.HasValue && 
                              device.LastSeen.Value.AddMinutes(timeoutMinutes) > DateTime.UtcNow;

                return isOnline && device.Status == "Online";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking device online status: {deviceId}");
                return false;
            }
        }

        public async Task CleanupOfflineDevicesAsync(int timeoutMinutes = 5)
        {
            try
            {
                var cutoffTime = DateTime.UtcNow.AddMinutes(-timeoutMinutes);
                var offlineDevices = await _context.Devices
                    .Where(d => d.LastSeen.HasValue && 
                               d.LastSeen.Value < cutoffTime && 
                               d.Status == "Online")
                    .ToListAsync();

                foreach (var device in offlineDevices)
                {
                    device.Status = "Offline";
                    device.UpdatedAt = DateTime.UtcNow;

                    // Log offline event
                    await LogDeviceEventAsync(device.DeviceId, "Offline", 
                        $"Device went offline (last seen: {device.LastSeen})", "Warning");
                }

                if (offlineDevices.Any())
                {
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"Marked {offlineDevices.Count} devices as offline");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up offline devices");
            }
        }

        private async Task LogDeviceEventAsync(string deviceId, string eventType, string message, string severity, string? data = null)
        {
            try
            {
                var deviceEvent = new DeviceEvent
                {
                    EventId = Guid.NewGuid().ToString(),
                    DeviceId = deviceId,
                    EventType = eventType,
                    Message = message,
                    Data = data,
                    Timestamp = DateTime.UtcNow,
                    Severity = severity
                };

                _context.DeviceEvents.Add(deviceEvent);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error logging device event: {deviceId} - {eventType}");
            }
        }
    }
}
