using System.ComponentModel.DataAnnotations;

namespace HostTool.Domain
{
    public class Device
    {
        [Key]
        public string DeviceId { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(100)]
        public string DeviceName { get; set; } = string.Empty;
        
        [MaxLength(500)]
        public string? Description { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string DeviceType { get; set; } = string.Empty; // IoT, Sensor, Controller, etc.
        
        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "Offline"; // Online, Offline, Error, Maintenance
        
        [MaxLength(100)]
        public string? Location { get; set; }
        
        [MaxLength(50)]
        public string? FirmwareVersion { get; set; }
        
        [MaxLength(50)]
        public string? HardwareVersion { get; set; }
        
        public string? LastKnownIP { get; set; }
        
        public DateTime? LastSeen { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        // Device capabilities and configuration
        public string? Capabilities { get; set; } // JSON string for device capabilities
        public string? Configuration { get; set; } // JSON string for device configuration
        
        // Device health metrics
        public double? BatteryLevel { get; set; }
        public double? Temperature { get; set; }
        public double? SignalStrength { get; set; }
        
        // Device metadata
        public string? Manufacturer { get; set; }
        public string? Model { get; set; }
        public string? SerialNumber { get; set; }
        
        // Network information
        public string? MacAddress { get; set; }
        public int? Port { get; set; }
        
        // Device state
        public bool IsActive { get; set; } = true;
        public bool IsRegistered { get; set; } = false;
        
        // Heartbeat settings
        public int HeartbeatInterval { get; set; } = 30; // seconds
        public DateTime? LastHeartbeat { get; set; }
        
        // Error tracking
        public string? LastError { get; set; }
        public DateTime? LastErrorTime { get; set; }
        public int ErrorCount { get; set; } = 0;
    }
    
    public class DeviceEvent
    {
        [Key]
        public string EventId { get; set; } = string.Empty;
        
        [Required]
        public string DeviceId { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(50)]
        public string EventType { get; set; } = string.Empty; // Register, Heartbeat, StatusChange, Error, etc.
        
        [MaxLength(500)]
        public string? Message { get; set; }
        
        public string? Data { get; set; } // JSON string for additional event data
        
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        
        [MaxLength(20)]
        public string Severity { get; set; } = "Info"; // Info, Warning, Error, Critical
        
        public bool IsProcessed { get; set; } = false;
    }
    
    public class DeviceRegistrationRequest
    {
        public string DeviceName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string DeviceType { get; set; } = string.Empty;
        public string? Location { get; set; }
        public string? FirmwareVersion { get; set; }
        public string? HardwareVersion { get; set; }
        public string? Capabilities { get; set; }
        public string? Configuration { get; set; }
        public string? Manufacturer { get; set; }
        public string? Model { get; set; }
        public string? SerialNumber { get; set; }
        public string? MacAddress { get; set; }
        public int HeartbeatInterval { get; set; } = 30;
    }
    
    public class DeviceHeartbeatRequest
    {
        public string DeviceId { get; set; } = string.Empty;
        public string? Status { get; set; }
        public double? BatteryLevel { get; set; }
        public double? Temperature { get; set; }
        public double? SignalStrength { get; set; }
        public string? LastKnownIP { get; set; }
        public string? Data { get; set; } // Additional sensor data
    }
}
