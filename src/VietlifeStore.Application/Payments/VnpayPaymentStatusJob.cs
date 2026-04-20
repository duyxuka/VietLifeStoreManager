using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Threading.Tasks;
using VietlifeStore.Entity.DonHangs;
using Volo.Abp.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Uow;
using System.Collections.Generic;

namespace VietlifeStore.Payments
{
    public class VnpayPaymentStatusJob
    {
        private readonly IRepository<DonHang, Guid> _donHangRepo;
        private readonly IConfiguration _configuration;
        private readonly IUnitOfWorkManager _uowManager;

        public VnpayPaymentStatusJob(
            IRepository<DonHang, Guid> donHangRepo,
            IConfiguration configuration,
            IUnitOfWorkManager uowManager)
        {
            _donHangRepo = donHangRepo;
            _configuration = configuration;
            _uowManager = uowManager;
        }

        /// <summary>
        /// Job này được Hangfire gọi tự động, kiểm tra đơn hàng quá hạn và cập nhật trạng thái.
        /// </summary>
        public async Task CheckPendingPaymentsAsync()
        {
            var now = DateTime.Now;

            List<DonHang> expiredOrders;

            using (var uow = _uowManager.Begin(requiresNew: true, isTransactional: false))
            {
                var queryable = await _donHangRepo.GetQueryableAsync();
                expiredOrders = await queryable
                    .Where(o =>
                        o.PhuongThucThanhToan == "VNPAY" &&
                        o.TrangThai == 0 &&
                        o.CreationTime <= now.AddMinutes(-15))
                    .ToListAsync();

                await uow.CompleteAsync();
            }

            if (!expiredOrders.Any()) return;

            foreach (var order in expiredOrders)
            {
                using var uow = _uowManager.Begin(requiresNew: true, isTransactional: true);
                try
                {
                    order.TrangThai = 7; // Expired — không cần gọi VNPay QueryDR nữa
                    await _donHangRepo.UpdateAsync(order);
                    await uow.CompleteAsync();
                }
                catch { /* log và bỏ qua, job chạy lại sau 5 phút */ }
            }
        }
    }
}