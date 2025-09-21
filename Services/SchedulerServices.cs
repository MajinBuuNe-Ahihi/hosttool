using HostTool.Domain;
using HostTool.Infrastructure;

namespace HostTool.Services
{
    public class SchedulerServices
    {  private InfrastructureScheduler _scheduler;
        public SchedulerServices() { 
            _scheduler = new InfrastructureScheduler();
        }
        public  List<Scheduler> GetSchedulers()
        {
            return _scheduler.GetSchedulers();
        }

        public List<SchedulerDay> GetSchedulerDays(Guid id)
        {
            return _scheduler.GetSchedulerDays(id);
        }

        public bool CreateScheduler(SchedulerDTO dto)
        {
            return _scheduler.CreateScheduler(dto);
        }

        public bool UpdateSchduler(SchedulerDTO dto)
        {
            return _scheduler.UpdateSchduler(dto);
        }


        public List<SchedulerDTO> GetListScheduler()
        {
            return _scheduler.GetListScheduler();
        }
    }
}
