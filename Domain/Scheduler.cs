
namespace HostTool.Domain
{
    public class Scheduler
    {
        public Guid SchedulerId { get; set; }
        public string SchedulerName { get; set; } = string.Empty;
        public string SchedulerDescription { get; set; }
        public string SchedulerPath { get; set; }
        public bool Active { get; set; }
        public bool RunAll { get; set; }
    }

    public class SchedulerDay
    {
        public Guid SchedulerDayId { get; set; }
        public Guid SchedulerId { get; set; }
        public int Day { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public bool Active { get; set; }
    }

    public class SchedulerDTO
    {
        public Scheduler? master { get; set; }
        public  List<SchedulerDay>? detail { get; set; }

        //0 --- edit ---1 xóa
        public int EditMode { get; set; } = 0;

    }
}
