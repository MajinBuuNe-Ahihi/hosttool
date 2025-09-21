using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace DeviceSimulator
{
    public class DeviceSimulator
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private readonly string _deviceId;
        private readonly Timer _heartbeatTimer;
        private readonly Random _random;
        private bool _isRunning;

        public DeviceSimulator(string baseUrl = "https://localhost:7000")
        {
            _httpClient = new HttpClient();
            _baseUrl = baseUrl;
            _deviceId = Guid.NewGuid().ToString();
            _random = new Random();
            _isRunning = false;

            // Create heartbeat timer (runs every 30 seconds)
            _heartbeatTimer = new Timer(SendHeartbeat, null, Timeout.Infinite, Timeout.Infinite);
        }

        public async Task StartAsync()
        {
            Console.WriteLine($"Starting Device Simulator - Device ID: {_deviceId}");
            
            // Register device
            var registrationSuccess = await RegisterDeviceAsync();
            if (!registrationSuccess)
            {
                Console.WriteLine("Failed to register device. Exiting...");
                return;
            }

            _isRunning = true;
            
            // Start heartbeat timer
            _heartbeatTimer.Change(TimeSpan.Zero, TimeSpan.FromSeconds(30));
            
            Console.WriteLine("Device simulator started successfully!");
            Console.WriteLine("Press 'q' to quit, 'h' to send heartbeat manually, 's' to change status");
            
            // Main loop
            while (_isRunning)
            {
                var key = Console.ReadKey(true);
                switch (key.KeyChar)
                {
                    case 'q':
                    case 'Q':
                        await StopAsync();
                        break;
                    case 'h':
                    case 'H':
                        await SendHeartbeatAsync();
                        break;
                    case 's':
                    case 'S':
                        await ChangeStatusAsync();
                        break;
                }
            }
        }

        public async Task StopAsync()
        {
            Console.WriteLine("Stopping device simulator...");
            _isRunning = false;
            _heartbeatTimer.Change(Timeout.Infinite, Timeout.Infinite);
            _httpClient.Dispose();
            Console.WriteLine("Device simulator stopped.");
        }

        private async Task<bool> RegisterDeviceAsync()
        {
            try
            {
                var registrationRequest = new
                {
                    deviceName = $"Simulated Device {_deviceId.Substring(0, 8)}",
                    description = "A simulated IoT device for testing",
                    deviceType = "IoT",
                    location = "Test Lab",
                    firmwareVersion = "1.0.0",
                    hardwareVersion = "1.0",
                    manufacturer = "Test Manufacturer",
                    model = "Test Model",
                    serialNumber = $"SN{_deviceId.Substring(0, 8)}",
                    macAddress = GenerateMacAddress(),
                    heartbeatInterval = 30
                };

                var json = JsonSerializer.Serialize(registrationRequest);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{_baseUrl}/api/device/register", content);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<JsonElement>(responseContent);
                    
                    if (result.TryGetProperty("success", out var success) && success.GetBoolean())
                    {
                        Console.WriteLine($"Device registered successfully!");
                        Console.WriteLine($"Device ID: {_deviceId}");
                        return true;
                    }
                }

                Console.WriteLine($"Failed to register device. Status: {response.StatusCode}");
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error: {errorContent}");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception during registration: {ex.Message}");
                return false;
            }
        }

        private async void SendHeartbeat(object? state)
        {
            if (_isRunning)
            {
                await SendHeartbeatAsync();
            }
        }

        private async Task SendHeartbeatAsync()
        {
            try
            {
                var heartbeatRequest = new
                {
                    deviceId = _deviceId,
                    status = "Online",
                    batteryLevel = _random.Next(20, 100),
                    temperature = Math.Round(20 + _random.NextDouble() * 15, 1),
                    signalStrength = _random.Next(-80, -30),
                    data = JsonSerializer.Serialize(new
                    {
                        cpuUsage = _random.Next(10, 90),
                        memoryUsage = _random.Next(30, 80),
                        diskUsage = _random.Next(40, 95)
                    })
                };

                var json = JsonSerializer.Serialize(heartbeatRequest);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{_baseUrl}/api/device/heartbeat", content);
                
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Heartbeat sent successfully");
                }
                else
                {
                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Heartbeat failed. Status: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Exception during heartbeat: {ex.Message}");
            }
        }

        private async Task ChangeStatusAsync()
        {
            try
            {
                Console.WriteLine("Enter new status (Online/Offline/Error/Maintenance):");
                var status = Console.ReadLine();
                
                if (string.IsNullOrEmpty(status))
                {
                    Console.WriteLine("Invalid status");
                    return;
                }

                var statusRequest = new { status = status };
                var json = JsonSerializer.Serialize(statusRequest);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync($"{_baseUrl}/api/device/{_deviceId}/status", content);
                
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Status changed to: {status}");
                }
                else
                {
                    Console.WriteLine($"Failed to change status. Status: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception during status change: {ex.Message}");
            }
        }

        private string GenerateMacAddress()
        {
            var macBytes = new byte[6];
            _random.NextBytes(macBytes);
            return string.Join(":", macBytes.Select(b => b.ToString("X2")));
        }
    }

    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("=== Device Simulator ===");
            Console.WriteLine("This simulator will register a device and send periodic heartbeats");
            Console.WriteLine();

            var baseUrl = args.Length > 0 ? args[0] : "https://localhost:7000";
            Console.WriteLine($"Connecting to: {baseUrl}");
            Console.WriteLine();

            var simulator = new DeviceSimulator(baseUrl);
            
            try
            {
                await simulator.StartAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}
