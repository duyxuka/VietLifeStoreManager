using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using VietlifeStore.Entity.DonHangs;
using VietlifeStore.Entity.DonHangsList.DonHangs;
using VietlifeStore.Entity.Payments;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Domain.Repositories;

namespace VietlifeStore.Controllers
{
    [ApiController]
    [Route("api/payment")]
    public class PaymentController : AbpControllerBase
    {
        private readonly IVnPayAppService _vnPayService;
        private readonly IRepository<DonHang, Guid> _donHangRepository;

        public PaymentController(IVnPayAppService vnPayService, IRepository<DonHang, Guid> donHangRepository)
        {
            _vnPayService = vnPayService;
            _donHangRepository = donHangRepository;
        }

        /// <summary>
        /// Tạo URL thanh toán VNPay, trả về cho frontend redirect
        /// </summary>
        [HttpPost("create-url")]
        [Produces("application/json")]
        public async Task<IActionResult> CreatePaymentUrl(Guid orderId)
        {
            var order = await _donHangRepository.GetAsync(orderId);

            var url = _vnPayService.CreatePaymentUrl(order);

            return Ok(url);
        }

        /// <summary>
        /// VNPay redirect về sau thanh toán (dùng để hiển thị kết quả cho user)
        /// </summary>
        [HttpGet("callback")]
        [Produces("application/json")]
        public async Task<IActionResult> PaymentCallback()
        {
            var response = _vnPayService.ResponsepayAsync(Request.Query);

            return Ok(response);
        }

        [HttpGet("pending")]
        public async Task<IActionResult> GetPendingOrder([FromQuery] Guid userId)
        {
            var cutoff = DateTime.Now.AddMinutes(-15);

            var order = await _donHangRepository
                .FirstOrDefaultAsync(o =>
                    o.TaiKhoanKhachHangId == userId &&
                    o.PhuongThucThanhToan == "VNPAY" &&
                    o.TrangThai == 0 &&
                    o.CreationTime >= cutoff); // còn trong 15 phút

            if (order == null) return Ok(null);

            // Tính số giây còn lại
            var expiresAt = order.CreationTime.AddMinutes(15);
            var secondsLeft = (int)(expiresAt - DateTime.Now).TotalSeconds;

            return Ok(new
            {
                order.Id,
                order.Ma,
                order.TongTien,
                order.CreationTime,
                ExpiresAt = expiresAt,
                SecondsLeft = Math.Max(secondsLeft, 0)
            });
        }

        [HttpPost("cancel-pending")]
        public async Task<IActionResult> CancelPendingOrder([FromQuery] Guid orderId)
        {
            var order = await _donHangRepository.GetAsync(orderId);

            if (order.TrangThai != 0)
                return BadRequest("Đơn hàng không ở trạng thái chờ thanh toán");

            order.TrangThai = 7; // Đã hủy
            await _donHangRepository.UpdateAsync(order, autoSave: true);

            return Ok(new { message = "Đã hủy đơn hàng" });
        }
    }
}