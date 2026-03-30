using Hangfire;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using VietlifeStore.ChucNang.DatLichs.DatLichVouchers;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Uow;

namespace VietlifeStore.Entity.DonHangs
{
    public class VoucherJobHandler
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public VoucherJobHandler(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        [DisableConcurrentExecution(60)]
        public async Task KichHoatAsync(Guid voucherId)
        {
            using var scope = _scopeFactory.CreateScope();
            var voucherRepo = scope.ServiceProvider.GetRequiredService<IRepository<Voucher, Guid>>();
            var scheduleRepo = scope.ServiceProvider.GetRequiredService<IRepository<VoucherSchedule, Guid>>();
            var uowManager = scope.ServiceProvider.GetRequiredService<IUnitOfWorkManager>();

            using var uow = uowManager.Begin(requiresNew: true, isTransactional: true);

            var voucher = await voucherRepo.GetAsync(voucherId);
            if (voucher.TrangThai != TrangThaiVoucher.ChuaKichHoat)
            {
                await uow.CompleteAsync();
                return;
            }

            voucher.TrangThai = TrangThaiVoucher.DangHoatDong;
            await voucherRepo.UpdateAsync(voucher, autoSave: false);
            await CapNhatScheduleAsync(scheduleRepo, voucherId, LoaiJobVoucher.KichHoat);

            await uow.CompleteAsync();
        }

        [DisableConcurrentExecution(60)]
        public async Task HetHanAsync(Guid voucherId)
        {
            using var scope = _scopeFactory.CreateScope();
            var voucherRepo = scope.ServiceProvider.GetRequiredService<IRepository<Voucher, Guid>>();
            var scheduleRepo = scope.ServiceProvider.GetRequiredService<IRepository<VoucherSchedule, Guid>>();
            var uowManager = scope.ServiceProvider.GetRequiredService<IUnitOfWorkManager>();

            using var uow = uowManager.Begin(requiresNew: true, isTransactional: true);

            var voucher = await voucherRepo.GetAsync(voucherId);
            if (voucher.TrangThai == TrangThaiVoucher.HetHan ||
                voucher.TrangThai == TrangThaiVoucher.VoHieu)
            {
                await uow.CompleteAsync();
                return;
            }

            voucher.TrangThai = TrangThaiVoucher.HetHan;
            await voucherRepo.UpdateAsync(voucher, autoSave: false);
            await CapNhatScheduleAsync(scheduleRepo, voucherId, LoaiJobVoucher.VoHieuHoa);

            await uow.CompleteAsync();
        }

        [DisableConcurrentExecution(60)]
        public async Task CanhBaoHetHanAsync(Guid voucherId)
        {
            using var scope = _scopeFactory.CreateScope();
            var scheduleRepo = scope.ServiceProvider.GetRequiredService<IRepository<VoucherSchedule, Guid>>();
            var uowManager = scope.ServiceProvider.GetRequiredService<IUnitOfWorkManager>();

            using var uow = uowManager.Begin(requiresNew: true, isTransactional: true);

            // TODO: gửi push notification / email cho user đang giữ voucher
            await CapNhatScheduleAsync(scheduleRepo, voucherId, LoaiJobVoucher.CanhBaoHetHan);

            await uow.CompleteAsync();
        }

        private static async Task CapNhatScheduleAsync(
            IRepository<VoucherSchedule, Guid> scheduleRepo,
            Guid voucherId,
            LoaiJobVoucher loai)
        {
            var schedule = await scheduleRepo
                .FirstOrDefaultAsync(x =>
                    x.VoucherId == voucherId &&
                    x.LoaiJob == loai &&
                    x.TrangThai == TrangThaiJob.ChoXuLy);

            if (schedule == null) return;

            schedule.TrangThai = TrangThaiJob.ThanhCong;
            schedule.ThoiGianThucThi = DateTime.Now;
            await scheduleRepo.UpdateAsync(schedule, autoSave: false);
        }
    }
}