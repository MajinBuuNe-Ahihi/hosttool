using HostTool.Domain;
using HostTool.Services;
using Microsoft.AspNetCore.Mvc;

namespace HostTool.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ToolController : ControllerBase
    {
        private readonly SchedulerServices _services;
        private readonly INotificationService _notificationService;
        
        public ToolController(INotificationService notificationService)
        {
            _services = new SchedulerServices();
            _notificationService = notificationService;
        }

        [HttpGet("GetSchedulers")]
        public IActionResult GetSchedulers()
        {
            return Ok(_services.GetSchedulers());
        }

        [HttpGet("GetSchedulerDays/{id}")]
        public IActionResult GetSchedulerDays(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest();
            }

            return Ok(_services.GetSchedulerDays(Guid.Parse(id)));
        }

        [HttpPost]
        public async Task<IActionResult> CreateScheduler([FromBody]SchedulerDTO dto)
        {
            if (dto == null)
            {
                return BadRequest();
            }
            var result = _services.CreateScheduler(dto);
            if(!result)
            {
                return BadRequest();
            }
            
            // Gửi thông báo real-time
            await _notificationService.NotifySchedulerCreatedAsync(dto.master.SchedulerName ?? "Unknown Scheduler");
            
            return Ok();
        }

        [HttpPut]
        public async Task<IActionResult> UpdateSchduler([FromBody]SchedulerDTO dto)
        {
            if (dto == null)
            {
                return BadRequest();
            }
            var result = _services.UpdateSchduler(dto);
            if (!result)
            {
                return BadRequest();
            }
            
            // Gửi thông báo real-time
            await _notificationService.NotifySchedulerUpdatedAsync(dto.master.SchedulerName ?? "Unknown Scheduler");
            
            return Ok();
        }

        [HttpGet("GetListScheduler")]
        public IActionResult GetListScheduler()
        {
            return Ok(_services.GetListScheduler());
        }

        [HttpPost("SendNotification")]
        public async Task<IActionResult> SendNotification([FromBody] NotificationRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.Title) || string.IsNullOrEmpty(request.Message))
            {
                return BadRequest("Title and Message are required");
            }

            await _notificationService.SendNotificationToAllAsync(request.Title, request.Message, request.Type ?? "info");
            return Ok(new { message = "Notification sent successfully" });
        }

        [HttpPost("SendSystemEvent")]
        public async Task<IActionResult> SendSystemEvent([FromBody] SystemEventRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.EventType) || string.IsNullOrEmpty(request.Description))
            {
                return BadRequest("EventType and Description are required");
            }

            await _notificationService.NotifySystemEventAsync(request.EventType, request.Description);
            return Ok(new { message = "System event notification sent successfully" });
        }
    }

    public class NotificationRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? Type { get; set; } = "info";
    }

    public class SystemEventRequest
    {
        public string EventType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
