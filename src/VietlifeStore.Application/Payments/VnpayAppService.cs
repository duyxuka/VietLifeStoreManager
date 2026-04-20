using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System;
using VietlifeStore.Entity.Payments;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using VietlifeStore.Entity.DonHangs;
using Microsoft.Extensions.Configuration;
using System.Linq;
using VietlifeStore.Payments;
using Volo.Abp.Settings;
using Microsoft.AspNetCore.Mvc;
using VietlifeStore.Entity.DonHangsList.DonHangs;
using Hangfire;
using Microsoft.AspNetCore.Http.HttpResults;

public class VnPayAppService : ApplicationService, IVnPayAppService
{
    private readonly IConfiguration _configuration;
    private readonly IRepository<DonHang, Guid> _donHangRepo;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IBackgroundJobClient _backgroundJobClient;

    public VnPayAppService(IConfiguration configuration, IRepository<DonHang, Guid> donHangRepo,
        IHttpContextAccessor httpContextAccessor, IBackgroundJobClient backgroundJobClient)
    {
        _configuration = configuration;
        _donHangRepo = donHangRepo;
        _backgroundJobClient = backgroundJobClient;
        _httpContextAccessor = httpContextAccessor;
    }
    public string CreatePaymentUrl(DonHang model)
    {
        var context = _httpContextAccessor.HttpContext;
        var timeZoneById = TimeZoneInfo.FindSystemTimeZoneById(_configuration["Vnpay:TimeZoneId"]);
        var timeNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZoneById);
        var tick = DateTime.Now.Ticks.ToString();
        var pay = new VnPayLibrary();
        var urlCallBack = _configuration["PaymentCallBack:ReturnUrl"];
        var amount = ((long)(model.TongTien * 100)).ToString();


        pay.AddRequestData("vnp_Version", _configuration["Vnpay:Version"]);
        pay.AddRequestData("vnp_Command", _configuration["Vnpay:Command"]);
        pay.AddRequestData("vnp_TmnCode", _configuration["Vnpay:TmnCode"]);
        pay.AddRequestData("vnp_Amount", amount);
        pay.AddRequestData("vnp_CreateDate", timeNow.ToString("yyyyMMddHHmmss"));
        pay.AddRequestData("vnp_CurrCode", _configuration["Vnpay:CurrCode"]);
        pay.AddRequestData("vnp_IpAddr", pay.GetIpAddress(context));
        pay.AddRequestData("vnp_Locale", _configuration["Vnpay:Locale"]);
        pay.AddRequestData("vnp_OrderInfo", $"Mã order: {model.Ma}-{model.Ten}-{model.Email}-{model.SoDienThoai}-{(long)model.TongTien}");
        pay.AddRequestData("vnp_OrderType", "Khác");
        pay.AddRequestData("vnp_ReturnUrl", urlCallBack);
        pay.AddRequestData("vnp_TxnRef", model.Ma);
        pay.AddRequestData("vnp_ExpireDate", timeNow.AddMinutes(15).ToString("yyyyMMddHHmmss"));


        var paymentUrl = pay.CreateRequestUrl(_configuration["Vnpay:BaseUrl"], _configuration["Vnpay:HashSecret"]);

        return paymentUrl;
    }

    [Produces("application/json")]
    public PaymentResponseModel PaymentExecute(IQueryCollection collections)
    {
        var pay = new VnPayLibrary();
        var response = pay.GetFullResponseData(collections, _configuration["Vnpay:HashSecret"]);

        return response;
    }

    public async Task<PaymentIPN> ResponsepayAsync(IQueryCollection collections)
    {
        var pay = new VnPayLibrary();
        var result = await pay.ResponsepayAsync(collections, _configuration["Vnpay:HashSecret"]);

        // Parse thất bại → trả lỗi luôn
        if (result.RspCode != "00") return result;

        // Tìm đơn hàng theo mã
        var order = await _donHangRepo.FirstOrDefaultAsync(x => x.Ma == result.OrderId);

        if (order == null)
        {
            result.Set("01", "Order not found");
            return result;
        }

        if (order.TongTien != result.Amount)
        {
            result.Set("04", "Invalid amount");
            return result;
        }

        if (order.TrangThai != 0)
        {
            result.Set("02", "Order already confirmed");
            return result;
        }

        // Thanh toán thành công
        if (result.VnpResponseCode == "00" && result.VnpTransactionStatus == "00")
        {
            order.TrangThai = 1;
            await _donHangRepo.UpdateAsync(order, autoSave: true);

            // Gửi mail xác nhận SAU KHI thanh toán thành công
            _backgroundJobClient.Enqueue<DonHangEmailJob>(
                job => job.SendOrderSuccessAsync(order.Id));
        }
        else
        {
            // Thanh toán thất bại hoặc bị hủy trên VNPay
            order.TrangThai = 7;
            await _donHangRepo.UpdateAsync(order, autoSave: true);
            // Không gửi mail khi thất bại
        }

        result.Set("00", "Confirm Success");
        return result;
    }
}