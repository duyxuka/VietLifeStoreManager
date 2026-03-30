using System;
using System.Threading.Tasks;
using VietlifeStore.ChucNang.DatLichs.DatLichGiamGiaSanPhams;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace VietlifeStore.ChucNang.DatLichs
{
    public interface IJobIdStore
    {
        Task SaveAsync(Guid ctId, string activateJobId, string? expireJobId);
        Task<GiamGiaJobIds?> GetAsync(Guid ctId);
        Task RemoveAsync(Guid ctId);
    }

    public class GiamGiaJobIds
    {
        public string ActivateJobId { get; set; } = "";
        public string? ExpireJobId { get; set; }
    }

    public class DbJobIdStore : IJobIdStore, ITransientDependency
    {
        private readonly IRepository<ChuongTrinhGiamGia, Guid> _repo;

        public DbJobIdStore(IRepository<ChuongTrinhGiamGia, Guid> repo)
        {
            _repo = repo;
        }

        public async Task SaveAsync(Guid ctId, string activateJobId, string? expireJobId)
        {
            var ct = await _repo.FindAsync(ctId).ConfigureAwait(false);
            if (ct == null) return;
            ct.ActivateJobId = activateJobId;
            ct.ExpireJobId = expireJobId;
            await _repo.UpdateAsync(ct, autoSave: true).ConfigureAwait(false);
        }

        public async Task<GiamGiaJobIds?> GetAsync(Guid ctId)
        {
            var ct = await _repo.FindAsync(ctId).ConfigureAwait(false);
            if (ct == null) return null;
            return new GiamGiaJobIds
            {
                ActivateJobId = ct.ActivateJobId ?? "",
                ExpireJobId = ct.ExpireJobId
            };
        }

        public async Task RemoveAsync(Guid ctId)
        {
            var ct = await _repo.FindAsync(ctId).ConfigureAwait(false);
            if (ct == null) return;
            ct.ActivateJobId = null;
            ct.ExpireJobId = null;
            await _repo.UpdateAsync(ct, autoSave: true).ConfigureAwait(false);
        }
    }
}