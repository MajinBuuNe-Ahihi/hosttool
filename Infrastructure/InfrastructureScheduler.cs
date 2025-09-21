using HostTool.Domain;
using Microsoft.EntityFrameworkCore;
using System;

namespace HostTool.Infrastructure
{
    public class InfrastructureScheduler
    {
        private AppDataContext _datacontext;
        public InfrastructureScheduler()
        {
            _datacontext = new AppDataContext();
            try
            {
                // Kiểm tra xem database có tồn tại và có bảng chưa
                if (!_datacontext.Database.CanConnect())
                {
                    // Database chưa tồn tại, tạo mới
                    _datacontext.Database.EnsureCreated();
                }
                else
                {
                    // Database đã tồn tại, kiểm tra xem có cần migrate không
                    var pendingMigrations = _datacontext.Database.GetPendingMigrations();
                    if (pendingMigrations.Any())
                    {
                        // Có migration chưa được áp dụng, thực hiện migrate
                        _datacontext.Database.Migrate();
                    }
                    // Nếu không có pending migrations, không cần làm gì
                }
            }
            catch (Exception ex)
            {
                // Nếu có lỗi, thử tạo database từ đầu
                try
                {
                    _datacontext.Database.EnsureCreated();
                }
                catch (Exception ensureCreatedEx)
                {
                    // Log lỗi và xử lý theo cách phù hợp
                    throw new InvalidOperationException($"Failed to initialize database: {ensureCreatedEx.Message}", ensureCreatedEx);
                }
            }
        }

        public List<Scheduler> GetSchedulers()
        {
            return _datacontext.Schedulers.ToList<Scheduler>();
        }

        public List<SchedulerDay> GetSchedulerDays(Guid scheid)
        {
            return _datacontext.SchedulerDays.Where(x => x.SchedulerId == scheid).ToList<SchedulerDay>();
        }

        public List<SchedulerDTO> GetListScheduler()
        {
            var result = new List<SchedulerDTO>();
            var lst = _datacontext.Schedulers.ToList<Scheduler>();
            if (lst != null && lst.Count > 0)
            {
                lst.ForEach(x =>
                {
                    List<SchedulerDay> lstdetail = _datacontext?.SchedulerDays?.Where(x => x.SchedulerId == x.SchedulerId)?.ToList<SchedulerDay>();
                    result.Add(new SchedulerDTO
                    {
                        master = x,
                        detail = lstdetail
                    });
                });
            }
            return result;
        }

        public bool CreateScheduler(SchedulerDTO schedulerDTO)
        {
            using var transaction = _datacontext.Database.BeginTransaction();
            try
            {
                var master = schedulerDTO.master;
                _datacontext.Schedulers.Add(master);
                _datacontext.SaveChanges();

                _datacontext.SchedulerDays.AddRange(schedulerDTO.detail);
                _datacontext.SaveChanges();

                // Commit transaction nếu tất cả thành công
                transaction.Commit();
            }
            catch
            {
                // Rollback nếu có lỗi
                transaction.Rollback();
                return false;
            }
            finally { transaction.Dispose(); }
            return true;
        }

        public bool UpdateSchduler(SchedulerDTO schedulerDTO)
        {
            using var transaction = _datacontext.Database.BeginTransaction();
            try
            {
                Scheduler master = schedulerDTO.master;
                if (schedulerDTO.EditMode == 1)
                {
                    var obj = _datacontext.Schedulers.Where(x => x.SchedulerId == master.SchedulerId).ToList<Scheduler>().FirstOrDefault();
                    if (obj != null)
                    {
                        _datacontext.Schedulers.Remove(obj);
                    }
                    _datacontext.SaveChanges();
                    _datacontext.SchedulerDays.RemoveRange(_datacontext.SchedulerDays.Where(x => x.SchedulerId == obj.SchedulerId).ToList<SchedulerDay>());
                    _datacontext.SaveChanges();

                    transaction.Commit();
                }
                else
                {

                    _datacontext.Schedulers.RemoveRange(_datacontext.Schedulers.Where(x => x.SchedulerId == master.SchedulerId).ToList<Scheduler>());
                    _datacontext.Add(master);
                    _datacontext.SaveChanges();
                    _datacontext.SchedulerDays.RemoveRange(_datacontext.SchedulerDays.Where(x => x.SchedulerId == master.SchedulerId).ToList<SchedulerDay>());
                    _datacontext.SaveChanges();
                    _datacontext.SchedulerDays.AddRange(schedulerDTO.detail);
                    _datacontext.SaveChanges();
                    transaction.Commit();

                }
            }
            catch
            {
                // Rollback nếu có lỗi
                transaction.Rollback();
                return false;
            }
            finally { transaction.Dispose(); }
            return true;
        }
    }
}
