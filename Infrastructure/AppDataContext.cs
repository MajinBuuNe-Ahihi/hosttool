using HostTool.Domain;
using Microsoft.EntityFrameworkCore;

namespace HostTool.Infrastructure
{
    public class AppDataContext:DbContext
    {
        public DbSet<Scheduler> Schedulers { get; set; }

        public DbSet<SchedulerDay> SchedulerDays { get; set; }
        
        public DbSet<Device> Devices { get; set; }
        
        public DbSet<DeviceEvent> DeviceEvents { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite("Data Source=app.db"); 
    }
}
