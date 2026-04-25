using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using VietlifeStore.Entity.SanPhams;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Uow;

namespace VietlifeStore.Entity.DonHangs
{
    public class DonHangEmailJob : ITransientDependency
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IConfiguration _configuration;

        public DonHangEmailJob(
            IServiceScopeFactory scopeFactory,
            IConfiguration configuration)
        {
            _scopeFactory = scopeFactory;
            _configuration = configuration;
        }

        // ✅ Gửi mail xác nhận đặt hàng thành công
        public async Task SendOrderSuccessAsync(Guid donHangId)
        {
            using var scope = _scopeFactory.CreateScope();

            // ✅ Bắt buộc phải có UnitOfWork khi dùng ABP repository ngoài HTTP request
            var uowManager = scope.ServiceProvider.GetRequiredService<IUnitOfWorkManager>();

            using var uow = uowManager.Begin(requiresNew: true, isTransactional: false);

            var donHangRepo = scope.ServiceProvider.GetRequiredService<IRepository<DonHang, Guid>>();
            var chiTietRepo = scope.ServiceProvider.GetRequiredService<IRepository<ChiTietDonHang, Guid>>();
            var sanPhamRepo = scope.ServiceProvider.GetRequiredService<IRepository<SanPham, Guid>>();

            var order = await donHangRepo.GetAsync(donHangId);
            var (details, products) = await GetDetailsAndProductsAsync(donHangId, chiTietRepo, sanPhamRepo);

            await uow.CompleteAsync();

            string productDetails =
                "<table style='width:100%;border-collapse:collapse;'>"
                + "<thead><tr style='background:#f2f2f2;'>"
                + "<th style='padding:10px;border:1px solid #ddd;'>Tên sản phẩm</th>"
                + "<th style='padding:10px;border:1px solid #ddd;'>Ảnh</th>"
                + "<th style='padding:10px;border:1px solid #ddd;'>Biến thể</th>"
                + "<th style='padding:10px;border:1px solid #ddd;'>Quà tặng</th>"
                + "<th style='padding:10px;border:1px solid #ddd;'>Số lượng</th>"
                + "<th style='padding:10px;border:1px solid #ddd;'>Giá</th>"
                + "<th style='padding:10px;border:1px solid #ddd;'>Tổng</th>"
                + "</tr></thead><tbody>";

            foreach (var item in details)
            {
                var product = products.FirstOrDefault(x => x.Id == item.SanPhamId);
                var image = product?.Anh ?? "";

                productDetails +=
                    "<tr>"
                    + $"<td style='padding:10px;border:1px solid #ddd'>{product?.Ten}</td>"
                    + $"<td style='padding:10px;border:1px solid #ddd'><img src='https://mayhutsua.com.vn/files/{image}' width='80'/></td>"
                    + $"<td style='padding:10px;border:1px solid #ddd'>{item.SanPhamBienThe}</td>"
                    + $"<td style='padding:10px;border:1px solid #ddd'>{item.QuaTang}</td>"
                    + $"<td style='padding:10px;border:1px solid #ddd'>{item.SoLuong}</td>"
                    + $"<td style='padding:10px;border:1px solid #ddd'>{item.Gia:#,##0} VNĐ</td>"
                    + $"<td style='padding:10px;border:1px solid #ddd'>{(item.Gia * item.SoLuong):#,##0} VNĐ</td>"
                    + "</tr>";
            }

            productDetails +=
                "<tr style='text-align:right'>"
                + $"<td colspan='6' style='padding:15px;border:1px solid #ddd'><b>Tổng tiền:</b></td>"
                + $"<td style='padding:15px;border:1px solid #ddd'><b>{order.TongTien:#,##0} VNĐ</b></td>"
                + "</tr></tbody></table>";

            string accountInfo =
                "<table style='width:100%;border-collapse:collapse;'>"
                + "<tr style='background:#f2f2f2'>"
                + "<th style='padding:10px;border:1px solid #ddd'>Khách hàng</th>"
                + "<th style='padding:10px;border:1px solid #ddd'>SĐT</th>"
                + "<th style='padding:10px;border:1px solid #ddd'>Địa chỉ</th>"
                + "<th style='padding:10px;border:1px solid #ddd'>Thanh toán</th>"
                + "</tr><tr>"
                + $"<td style='padding:10px;border:1px solid #ddd'>{order.Ten}</td>"
                + $"<td style='padding:10px;border:1px solid #ddd'>{order.SoDienThoai}</td>"
                + $"<td style='padding:10px;border:1px solid #ddd'>{order.DiaChi}</td>"
                + $"<td style='padding:10px;border:1px solid #ddd'>{order.PhuongThucThanhToan}</td>"
                + "</tr></table>";

            string body =
                "<div style='font-family:Arial;font-size:14px'>"
                + "<h2 style='color:#10cb04'>Bạn đã đặt hàng thành công tại Vietlife Store</h2>"
                + $"<p>Mã đơn hàng: <b>{order.Ma}</b></p>"
                + $"<p>Ngày đặt: {order.NgayDat:dd/MM/yyyy HH:mm}</p>"
                + "<h3>Thông tin khách hàng</h3>"
                + accountInfo
                + "<h3>Chi tiết đơn hàng</h3>"
                + productDetails
                + "<p>Chúng tôi sẽ liên hệ xác nhận đơn hàng sớm.</p>"
                + "</div>";

            await SendEmailAsync(order.Email, $"Xác nhận đơn hàng #{order.Ma}", body);

            var notifyEmails = _configuration
            .GetSection("NotificationEmails")
            .Get<List<string>>();

            if (notifyEmails != null && notifyEmails.Any())
            {
                string adminSubject = $"[CÓ ĐƠN HÀNG MỚI] #{order.Ma}";

                string adminBody =
                    "<div style='font-family:Arial;font-size:14px'>"
                    + "<h2 style='color:#1890ff'>Có đơn hàng mới</h2>"
                    + $"<p><b>Mã đơn:</b> {order.Ma}</p>"
                    + $"<p><b>Khách:</b> {order.Ten}</p>"
                    + $"<p><b>SĐT:</b> {order.SoDienThoai}</p>"
                    + $"<p><b>Tổng tiền:</b> {order.TongTien:#,##0} VNĐ</p>"
                    + $"<p><b>Thanh toán:</b> {order.PhuongThucThanhToan}</p>"
                    + "<hr/>"
                    + productDetails
                    + "</div>";

                foreach (var email in notifyEmails)
                {
                    await SendEmailAsync(email, adminSubject, adminBody);
                }
            }
        }

        // ✅ Gửi mail thông báo hủy đơn hàng
        public async Task SendCancelOrderAsync(Guid donHangId)
        {
            using var scope = _scopeFactory.CreateScope();

            // ✅ Bắt buộc phải có UnitOfWork khi dùng ABP repository ngoài HTTP request
            var uowManager = scope.ServiceProvider.GetRequiredService<IUnitOfWorkManager>();

            using var uow = uowManager.Begin(requiresNew: true, isTransactional: false);

            var donHangRepo = scope.ServiceProvider.GetRequiredService<IRepository<DonHang, Guid>>();
            var chiTietRepo = scope.ServiceProvider.GetRequiredService<IRepository<ChiTietDonHang, Guid>>();
            var sanPhamRepo = scope.ServiceProvider.GetRequiredService<IRepository<SanPham, Guid>>();

            var order = await donHangRepo.GetAsync(donHangId);
            var (details, products) = await GetDetailsAndProductsAsync(donHangId, chiTietRepo, sanPhamRepo);

            await uow.CompleteAsync();

            string productList = "";

            foreach (var item in details)
            {
                var product = products.FirstOrDefault(x => x.Id == item.SanPhamId);

                productList +=
                    "<tr>"
                    + $"<td style='padding:8px;border:1px solid #ddd'>{product?.Ten}-{item.SanPhamBienThe}</td>"
                    + $"<td style='padding:8px;border:1px solid #ddd'>{item.SoLuong}</td>"
                    + $"<td style='padding:8px;border:1px solid #ddd'>{item.Gia:#,##0} VNĐ</td>"
                    + "</tr>";
            }

            string body =
                "<div style='font-family:Arial;font-size:14px'>"
                + "<h2 style='color:#ff4d4f'>Đơn hàng đã được hủy</h2>"
                + $"<p>Mã đơn hàng: <b>{order.Ma}</b></p>"
                + $"<p>Khách hàng: <b>{order.Ten}</b></p>"
                + $"<p>Số điện thoại: {order.SoDienThoai}</p>"
                + $"<p>Địa chỉ: {order.DiaChi}</p>"
                + "<h3>Danh sách sản phẩm</h3>"
                + "<table style='width:100%;border-collapse:collapse'>"
                + "<tr style='background:#f5f5f5'>"
                + "<th style='padding:8px;border:1px solid #ddd'>Sản phẩm</th>"
                + "<th style='padding:8px;border:1px solid #ddd'>Số lượng</th>"
                + "<th style='padding:8px;border:1px solid #ddd'>Giá</th>"
                + "</tr>"
                + productList
                + "</table>"
                + $"<p style='margin-top:20px'>Tổng tiền đơn hàng: <b>{order.TongTien:#,##0} VNĐ</b></p>"
                + "<p style='color:#999'>Nếu đây là nhầm lẫn vui lòng đặt lại đơn hàng.</p>"
                + "<p>Cảm ơn bạn đã quan tâm đến Vietlife Store.</p>"
                + "</div>";

            await SendEmailAsync(order.Email, $"Đơn hàng #{order.Ma} đã bị hủy", body);
            var notifyEmails = _configuration
                .GetSection("NotificationEmails")
                .Get<List<string>>();

            if (notifyEmails != null && notifyEmails.Any())
            {
                string adminSubject = $"[CÓ ĐƠN HÀNG BỊ HỦY] #{order.Ma}";

                string adminBody =
                    "<div style='font-family:Arial;font-size:14px'>"
                    + "<h2 style='color:#ff4d4f'>Đơn hàng đã bị hủy</h2>"
                    + $"<p><b>Mã đơn:</b> {order.Ma}</p>"
                    + $"<p><b>Khách:</b> {order.Ten}</p>"
                    + $"<p><b>SĐT:</b> {order.SoDienThoai}</p>"
                    + $"<p><b>Địa chỉ:</b> {order.DiaChi}</p>"
                    + $"<p><b>Tổng tiền:</b> {order.TongTien:#,##0} VNĐ</p>"
                    + $"<p><b>Thanh toán:</b> {order.PhuongThucThanhToan}</p>"
                    + "<hr/>"
                    + "<h3>Chi tiết đơn hàng</h3>"
                    + "<table style='width:100%;border-collapse:collapse'>"
                    + "<tr style='background:#f5f5f5'>"
                    + "<th style='padding:8px;border:1px solid #ddd'>Sản phẩm</th>"
                    + "<th style='padding:8px;border:1px solid #ddd'>Số lượng</th>"
                    + "<th style='padding:8px;border:1px solid #ddd'>Giá</th>"
                    + "</tr>"
                    + productList
                    + "</table>"
                    + "</div>";

                foreach (var email in notifyEmails)
                {
                    await SendEmailAsync(email, adminSubject, adminBody);
                }
            }
        }

        // ================= PRIVATE =================
        private async Task<(List<ChiTietDonHang> details, List<SanPham> products)> GetDetailsAndProductsAsync(
            Guid donHangId,
            IRepository<ChiTietDonHang, Guid> chiTietRepo,
            IRepository<SanPham, Guid> sanPhamRepo)
        {
            var details = await chiTietRepo.GetListAsync(x => x.DonHangId == donHangId);

            var productIds = details.Select(x => x.SanPhamId).ToList();

            var products = (await sanPhamRepo.GetQueryableAsync())
                .Where(x => productIds.Contains(x.Id))
                .ToList();

            return (details, products);
        }

        private async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var smtpSection = _configuration.GetSection("Abp:Mailing:Smtp");

            var host = smtpSection["Host"];
            var port = int.Parse(smtpSection["Port"]);
            var username = smtpSection["UserName"];
            var password = smtpSection["Password"];
            var enableSsl = bool.Parse(smtpSection["EnableSsl"]);

            using var smtp = new SmtpClient(host, port)
            {
                Credentials = new NetworkCredential(username, password),
                EnableSsl = enableSsl
            };

            using var message = new MailMessage(
                new MailAddress(username, "Vietlife Store"),
                new MailAddress(toEmail))
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            await smtp.SendMailAsync(message);
        }
    }
}
