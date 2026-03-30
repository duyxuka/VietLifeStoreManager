using Hangfire;
using System;
using Volo.Abp.DependencyInjection;

namespace VietlifeStore.ChucNang.DatLichs
{
    public interface IGiamGiaScheduler
    {
        void Schedule(Guid ctId, DateTime batDau, DateTime ketThuc);
        void ScheduleOnlyExpire(Guid ctId, DateTime ketThuc);
        void Cancel(Guid ctId);
    }

    public class GiamGiaScheduler : IGiamGiaScheduler, ITransientDependency
    {
        private readonly IJobIdStore _jobIdStore;

        public GiamGiaScheduler(IJobIdStore jobIdStore)
        {
            _jobIdStore = jobIdStore;
        }

        public void Schedule(Guid ctId, DateTime batDau, DateTime ketThuc)
        {
            Cancel(ctId);

            var now = DateTime.Now;

            string activateJobId;
            var activateDelay = batDau - now;
            if (activateDelay > TimeSpan.FromSeconds(10))
            {
                activateJobId = BackgroundJob.Schedule<IGiamGiaSingleJob>(
                    job => job.ActivateSingleAsync(ctId), activateDelay);
            }
            else
            {
                activateJobId = BackgroundJob.Enqueue<IGiamGiaSingleJob>(
                    job => job.ActivateSingleAsync(ctId));
            }

            string? expireJobId = null;
            var expireDelay = ketThuc - now;
            if (expireDelay > TimeSpan.FromSeconds(10))
            {
                expireJobId = BackgroundJob.Schedule<IGiamGiaSingleJob>(
                    job => job.ExpireSingleAsync(ctId), expireDelay);
            }

            _jobIdStore.SaveAsync(ctId, activateJobId, expireJobId)
                       .GetAwaiter().GetResult();
        }

        public void ScheduleOnlyExpire(Guid ctId, DateTime ketThuc)
        {
            // Cancel dọn sạch job cũ trong Hangfire (đã gọi trước đó)
            // Chỉ cần tạo expire job mới và lưu lại
            var now = DateTime.Now;
            var expireDelay = ketThuc - now;
            string? expireJobId = null;

            if (expireDelay > TimeSpan.FromSeconds(10))
            {
                expireJobId = BackgroundJob.Schedule<IGiamGiaSingleJob>(
                    job => job.ExpireSingleAsync(ctId), expireDelay);
            }
            else
            {
                expireJobId = BackgroundJob.Enqueue<IGiamGiaSingleJob>(
                    job => job.ExpireSingleAsync(ctId));
            }

            // Lưu vào DB — activateJobId rỗng vì đã activate trực tiếp
            _jobIdStore.SaveAsync(ctId, string.Empty, expireJobId)
                       .GetAwaiter().GetResult();
        }

        public void Cancel(Guid ctId)
        {
            var ids = _jobIdStore.GetAsync(ctId).GetAwaiter().GetResult();
            if (ids == null) return;

            if (!string.IsNullOrEmpty(ids.ActivateJobId))
                BackgroundJob.Delete(ids.ActivateJobId);

            if (!string.IsNullOrEmpty(ids.ExpireJobId))
                BackgroundJob.Delete(ids.ExpireJobId);

            _jobIdStore.RemoveAsync(ctId).GetAwaiter().GetResult();
        }
    }
}