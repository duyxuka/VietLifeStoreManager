using Hangfire;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using VietlifeStore.ChucNang.DatLichs.Emails;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Uow;

namespace VietlifeStore.ChucNang.Mails
{
    public class EmailSendingJob : ITransientDependency
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailSendingJob> _logger;

        public EmailSendingJob(
            IServiceScopeFactory scopeFactory,
            IConfiguration configuration,
            ILogger<EmailSendingJob> logger)
        {
            _scopeFactory = scopeFactory;
            _configuration = configuration;
            _logger = logger;
        }

        [JobDisplayName("Gửi Email Campaign: {0}")]
        [AutomaticRetry(Attempts = 3, DelaysInSeconds = new[] { 60, 300, 600 })]
        public async Task ExecuteAsync(Guid campaignId)
        {
            using var scope = _scopeFactory.CreateScope();

            var uowManager = scope.ServiceProvider.GetRequiredService<IUnitOfWorkManager>();
            var campaignRepo = scope.ServiceProvider.GetRequiredService<IRepository<EmailCampaign, Guid>>();
            var queueRepo = scope.ServiceProvider.GetRequiredService<IRepository<EmailQueue, Guid>>();
            var logRepo = scope.ServiceProvider.GetRequiredService<IRepository<EmailLog, Guid>>();

            // ===== Load dữ liệu =====
            EmailCampaign campaign;
            List<EmailQueue> pendingQueues;

            using (var uow = uowManager.Begin(requiresNew: true, isTransactional: false))
            {
                campaign = await campaignRepo.FindAsync(campaignId);

                if (campaign == null ||
                    campaign.TrangThai == TrangThaiChienDich.TamDung ||
                    campaign.TrangThai == TrangThaiChienDich.HoanThanh)
                    return;

                campaign.TrangThai = TrangThaiChienDich.DangGui;
                await campaignRepo.UpdateAsync(campaign);

                pendingQueues = await queueRepo.GetListAsync(
                    x => x.CampaignId == campaignId &&
                         x.TrangThai == TrangThaiEmail.ChoGui);

                await uow.CompleteAsync();
            }

            int sent = 0, failed = 0;

            // ===== Gửi từng email =====
            foreach (var queue in pendingQueues)
            {
                // Kiểm tra bị pause giữa chừng
                using (var uowCheck = uowManager.Begin(requiresNew: true, isTransactional: false))
                {
                    var check = await campaignRepo.GetAsync(campaignId);
                    await uowCheck.CompleteAsync();

                    if (check.TrangThai == TrangThaiChienDich.TamDung)
                    {
                        _logger.LogInformation($"Campaign {campaignId} bị tạm dừng, dừng gửi.");
                        break;
                    }
                }

                var log = new EmailLog
                {
                    QueueId = queue.Id,
                    Email = queue.Email,
                    ThoiGianGui = DateTime.Now
                };

                try
                {
                    var processedBody = ProcessEmailBody(queue.NoiDung, queue);
                    await SendEmailAsync(queue.Email, queue.TieuDe, processedBody);

                    queue.TrangThai = TrangThaiEmail.ThanhCong;
                    queue.SoLanThu++;
                    log.TrangThai = TrangThaiEmail.ThanhCong;
                    sent++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Gửi thất bại tới: {queue.Email}");
                    queue.TrangThai = TrangThaiEmail.ThatBai;
                    queue.SoLanThu++;
                    log.TrangThai = TrangThaiEmail.ThatBai;
                    log.ThongBaoLoi = ex.Message;
                    failed++;
                }

                // Lưu từng email một, tránh mất dữ liệu nếu bị lỗi giữa chừng
                using (var uowSave = uowManager.Begin(requiresNew: true, isTransactional: false))
                {
                    await queueRepo.UpdateAsync(queue);
                    await logRepo.InsertAsync(log);
                    await uowSave.CompleteAsync();
                }
            }

            // ===== Cập nhật thống kê campaign =====
            using (var uowFinal = uowManager.Begin(requiresNew: true, isTransactional: false))
            {
                var final = await campaignRepo.GetAsync(campaignId);

                if (final.TrangThai != TrangThaiChienDich.TamDung)
                {
                    final.SoDaGui += sent;
                    final.TrangThai = TrangThaiChienDich.HoanThanh;
                    await campaignRepo.UpdateAsync(final);
                }

                await uowFinal.CompleteAsync();
            }

            _logger.LogInformation(
                $"Campaign {campaignId} hoàn thành — Thành công: {sent}, Thất bại: {failed}");
        }

        // ================= PRIVATE =================
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
        private string ProcessEmailBody(string html, EmailQueue queue)
        {
            // Tracking pixel
            var trackingPixel = $"<img src='https://vietlifebaby.vn/api/email/track/open/{queue.Id}' " +
                                $"width='1' height='1' style='display:none' />";

            // Unsubscribe link
            var unsubLink = $"https://vietlifebaby.vn/unsubscribe?email={Uri.EscapeDataString(queue.Email)}";

            return html
                .Replace("{{tenKhachHang}}", queue.TenKhachHang ?? "Quý khách")
                .Replace("{{email}}", queue.Email)
                .Replace("{{ngayHienTai}}", DateTime.Now.ToString("dd/MM/yyyy"))
                .Replace("{{unsubscribeLink}}", unsubLink)
                + trackingPixel; // Append pixel cuối body
        }
    }
}
