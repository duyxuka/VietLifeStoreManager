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

public class VnPayAppService : ApplicationService, IVnPayAppService
{
    private readonly IConfiguration _configuration;
    private readonly IRepository<DonHang, Guid> _donHangRepo;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public VnPayAppService(IConfiguration configuration, IRepository<DonHang, Guid> donHangRepo,
        IHttpContextAccessor httpContextAccessor)
    {
        _configuration = configuration;
        _donHangRepo = donHangRepo;
        _httpContextAccessor = httpContextAccessor;
    }
    public string CreatePaymentUrl(DonHang model)
    {
        var context = _httpContextAccessor.HttpContext;
        var timeZoneById = TimeZoneInfo.FindSystemTimeZoneById(_configuration["Vnpay:TimeZoneId"]);
        var timeNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZoneById);
        var tick = DateTime.Now.Ticks.ToString();
        var pay = new VnPayLibrary(_donHangRepo);
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


        var paymentUrl =
            pay.CreateRequestUrl(_configuration["Vnpay:BaseUrl"], _configuration["Vnpay:HashSecret"]);

        return paymentUrl;
    }

    [Produces("application/json")]
    public PaymentResponseModel PaymentExecute(IQueryCollection collections)
    {
        var pay = new VnPayLibrary(_donHangRepo);
        var response = pay.GetFullResponseData(collections, _configuration["Vnpay:HashSecret"]);

        return response;
    }

    public PaymentIPN Responsepay(IQueryCollection collections)
    {
        var pay = new VnPayLibrary(_donHangRepo);
        var response = pay.ResponsepayAsync(collections, _configuration["Vnpay:HashSecret"]).Result;

        return response;
    }
}