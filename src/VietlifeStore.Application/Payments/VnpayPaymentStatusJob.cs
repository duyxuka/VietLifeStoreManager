using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Threading.Tasks;
using VietlifeStore.Entity.DonHangs;
using Volo.Abp.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace VietlifeStore.Payments
{
    public class VnpayPaymentStatusJob
    {
        private readonly IRepository<DonHang, Guid> _donHangRepo;
        private readonly IConfiguration _configuration;

        public VnpayPaymentStatusJob(
            IRepository<DonHang, Guid> donHangRepo,
            IConfiguration configuration)
        {
            _donHangRepo = donHangRepo;
            _configuration = configuration;
        }

        /// <summary>
        /// Job này được Hangfire gọi tự động, kiểm tra đơn hàng quá hạn và cập nhật trạng thái.
        /// </summary>
        public async Task CheckPendingPaymentsAsync()
        {
            var vnPay = new VnPayLibrary();

            var now = DateTime.Now;
            var timeoutMinutes = 15;

            // Query trực tiếp DB
            var queryable = await _donHangRepo.GetQueryableAsync();

            var pendingOrders = await queryable
                .Where(o =>
                    o.PhuongThucThanhToan == "VNPAY" &&
                    o.TrangThai == 0 &&
                    o.CreationTime <= now.AddMinutes(-timeoutMinutes))
                .ToListAsync();

            if (!pendingOrders.Any())
                return;

            foreach (var order in pendingOrders)
            {
                try
                {
                    var result = await vnPay.QueryTransactionAsync(
                        txnRef: order.Ma,
                        transactionDate: order.CreationTime.ToString("yyyyMMddHHmmss"),
                        vnp_TmnCode: _configuration["Vnpay:TmnCode"],
                        vnp_HashSecret: _configuration["Vnpay:HashSecret"]
                    );

                    if (result == null)
                    {
                        order.TrangThai = 7; // Hủy
                    }
                    else if (result.vnp_TxnRef != order.Ma)
                    {
                        continue;
                    }
                    else if (result.vnp_TransactionStatus == "00" ||
                             result.vnp_ResponseCode == "00")
                    {
                        order.TrangThai = 1; // Thành công
                    }
                    else
                    {
                        order.TrangThai = 7; // Thất bại
                    }

                    await _donHangRepo.UpdateAsync(order);
                }
                catch
                {
                    order.TrangThai = 7;
                    await _donHangRepo.UpdateAsync(order);
                }
            }
        }
    }
}