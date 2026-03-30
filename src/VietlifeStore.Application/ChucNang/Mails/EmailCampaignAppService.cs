using AutoMapper.Internal.Mappers;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VietlifeStore.ChucNang.DatLichs.Emails.EmailCampaigns;
using VietlifeStore.ChucNang.DatLichs.Emails.EmailQueues;
using VietlifeStore.ChucNang.DatLichs.Emails;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Timing;
using Volo.Abp;
using Hangfire;
using Microsoft.Extensions.Logging;
using Volo.Abp.ObjectMapping;
using Microsoft.Extensions.Configuration;
using System.Net.Mail;
using System.Net;
using VietlifeStore.Entity.TaiKhoans;
using VietlifeStore.System.Users;

namespace VietlifeStore.ChucNang.Mails
{
    public class EmailCampaignAppService :
        CrudAppService<
            EmailCampaign,
            EmailCampaignDto,
            Guid,
            PagedResultRequestDto,
            CreateUpdateEmailCampaignDto,
            CreateUpdateEmailCampaignDto>,
        IEmailCampaignAppService
    {
        private readonly IRepository<EmailTemplate, Guid> _templateRepo;
        private readonly IRepository<EmailQueue, Guid> _queueRepo;
        private readonly IRepository<EmailUnsubscribe, Guid> _unsubRepo;
        private readonly IBackgroundJobClient _hangfire;
        private readonly IConfiguration _configuration;
        private readonly IRepository<TaiKhoan, Guid> _userRepo;

        public EmailCampaignAppService(
            IRepository<EmailCampaign, Guid> repository,
            IRepository<EmailTemplate, Guid> templateRepo,
            IRepository<EmailQueue, Guid> queueRepo,
            IRepository<EmailUnsubscribe, Guid> unsubRepo,
            IConfiguration configuration,
            IBackgroundJobClient hangfire,
            IRepository<TaiKhoan, Guid> userRepo)
            : base(repository)
        {
            _templateRepo = templateRepo;
            _queueRepo = queueRepo;
            _unsubRepo = unsubRepo;
            _configuration = configuration;
            _hangfire = hangfire;
            _userRepo = userRepo;

            //GetPolicyName = EmailPermissions.Campaign.Default;
            //GetListPolicyName = EmailPermissions.Campaign.Default;
            //CreatePolicyName = EmailPermissions.Campaign.Create;
            //UpdatePolicyName = EmailPermissions.Campaign.Update;
            //DeletePolicyName = EmailPermissions.Campaign.Delete;
        }

        // ================= CREATE =================
        //[Authorize(EmailPermissions.Campaign.Create)]
        public override async Task<EmailCampaignDto> CreateAsync(CreateUpdateEmailCampaignDto input)
        {
            if (!input.DanhSachEmail.Any())
                throw new UserFriendlyException("Danh sách email không được để trống.");

            var template = await _templateRepo.GetAsync(input.TemplateId);

            // Loại bỏ email đã unsubscribe
            var unsubSet = (await _unsubRepo.GetListAsync())
                .Select(x => x.Email.ToLower()).ToHashSet();

            var validEmails = input.DanhSachEmail
                .Where(e => !string.IsNullOrWhiteSpace(e))
                .Select(e => e.Trim().ToLower())
                .Distinct()
                .Where(e => !unsubSet.Contains(e))
                .ToList();

            if (!validEmails.Any())
                throw new UserFriendlyException("Không có địa chỉ email hợp lệ sau khi lọc unsubscribe.");

            var entity = new EmailCampaign
            {
                TenCampaign = input.TenCampaign,
                Subject = input.Subject,
                TemplateId = input.TemplateId,
                NgayGui = input.NgayGui,
                TrangThai = TrangThaiChienDich.NhapLieu,
                TongSoEmail = validEmails.Count
            };

            await Repository.InsertAsync(entity, autoSave: true);
            var users = await _userRepo.GetListAsync(x => validEmails.Contains(x.Email.ToLower()));
            var nameMap = users.ToDictionary(
                x => x.Email.ToLower(),
                x => x.Name ?? "Quý khách");
            // Tạo queue cho từng email
            var queues = validEmails.Select(email => new EmailQueue
            {
                CampaignId = entity.Id,
                Email = email,
                TenKhachHang = nameMap.GetValueOrDefault(email, "Quý khách"),
                TieuDe = input.Subject,
                NoiDung = template.NoiDungHtml,
                TrangThai = TrangThaiEmail.ChoGui,
                ThoiGianGui = input.NgayGui
            }).ToList();

            await _queueRepo.InsertManyAsync(queues, autoSave: true);

            var now = DateTime.Now;
            // Xử lý job tùy theo ngayGui
            if (input.NgayGui.HasValue && input.NgayGui > now)
            {
                // Lên lịch
                var delay = input.NgayGui.Value - now;
                var jobId = _hangfire.Schedule<EmailSendingJob>(
                    job => job.ExecuteAsync(entity.Id), delay);
                entity.SendJobId = jobId;
                entity.TrangThai = TrangThaiChienDich.DaLenLich;
            }
            else
            {
                // Gửi ngay
                var jobId = _hangfire.Enqueue<EmailSendingJob>(
                    job => job.ExecuteAsync(entity.Id));
                entity.SendJobId = jobId;
                entity.TrangThai = TrangThaiChienDich.DangGui;
                entity.NgayGui = now;
            }

            await Repository.UpdateAsync(entity, autoSave: true);

            return MapToGetOutputDto(entity);
        }

        // ================= UPDATE =================
        //[Authorize(EmailPermissions.Campaign.Update)]
        public override async Task<EmailCampaignDto> UpdateAsync(Guid id, CreateUpdateEmailCampaignDto input)
        {
            var entity = await Repository.GetAsync(id);

            if (entity.TrangThai == TrangThaiChienDich.DangGui ||
                entity.TrangThai == TrangThaiChienDich.HoanThanh)
                throw new UserFriendlyException("Không thể chỉnh sửa campaign đang gửi hoặc đã hoàn thành.");

            entity.TenCampaign = input.TenCampaign;
            entity.Subject = input.Subject;
            entity.TemplateId = input.TemplateId;
            entity.NgayGui = input.NgayGui;

            await Repository.UpdateAsync(entity, autoSave: true);
            return MapToGetOutputDto(entity);
        }

        // ================= DELETE SINGLE =================
        //[Authorize(EmailPermissions.Campaign.Delete)]
        public override async Task DeleteAsync(Guid id)
        {
            var entity = await Repository.GetAsync(id);

            if (entity.TrangThai == TrangThaiChienDich.DangGui)
                throw new UserFriendlyException("Không thể xóa campaign đang gửi.");

            // Hủy Hangfire job nếu có
            if (!string.IsNullOrWhiteSpace(entity.SendJobId))
            {
                try { _hangfire.Delete(entity.SendJobId); }
                catch (Exception ex)
                {
                    Logger.LogWarning(ex, $"Không thể hủy Hangfire job: {entity.SendJobId}");
                }
            }

            var queues = await _queueRepo.GetListAsync(x => x.CampaignId == id);
            await _queueRepo.DeleteManyAsync(queues);

            await base.DeleteAsync(id);
        }

        // ================= DELETE MULTIPLE =================
        //[Authorize(EmailPermissions.Campaign.Delete)]
        public async Task DeleteMultipleAsync(IEnumerable<Guid> ids)
        {
            var list = await Repository.GetListAsync(x => ids.Contains(x.Id));

            if (list.Any(x => x.TrangThai == TrangThaiChienDich.DangGui))
                throw new UserFriendlyException("Không thể xóa campaign đang gửi.");

            foreach (var item in list)
            {
                if (!string.IsNullOrWhiteSpace(item.SendJobId))
                {
                    try { _hangfire.Delete(item.SendJobId); }
                    catch (Exception ex)
                    {
                        Logger.LogWarning(ex, $"Không thể hủy Hangfire job: {item.SendJobId}");
                    }
                }
            }

            var queues = await _queueRepo.GetListAsync(
                x => ids.Contains(x.CampaignId));
            await _queueRepo.DeleteManyAsync(queues);

            await Repository.DeleteManyAsync(list);
        }

        // ================= FILTER + PAGING =================
        public async Task<PagedResultDto<EmailCampaignInListDto>> GetListFilterAsync(EmailCampaignFilterDto input)
        {
            var query = (await Repository.GetQueryableAsync())
                .WhereIf(!string.IsNullOrWhiteSpace(input.Keyword),
                    x => x.TenCampaign.Contains(input.Keyword) ||
                         x.Subject.Contains(input.Keyword))
                .WhereIf(input.TrangThai.HasValue,
                    x => x.TrangThai == input.TrangThai)
                .WhereIf(input.TuNgay.HasValue,
                    x => x.NgayGui >= input.TuNgay)
                .WhereIf(input.DenNgay.HasValue,
                    x => x.NgayGui <= input.DenNgay);

            var totalCount = await AsyncExecuter.LongCountAsync(query);

            var items = await AsyncExecuter.ToListAsync(
                query.OrderByDescending(x => x.CreationTime)
                     .Skip(input.SkipCount)
                     .Take(input.MaxResultCount)
            );

            // Gán tên template theo batch
            var templateIds = items.Select(x => x.TemplateId).Distinct().ToList();
            var templates = await _templateRepo.GetListAsync(x => templateIds.Contains(x.Id));

            var dtos = ObjectMapper.Map<List<EmailCampaign>, List<EmailCampaignInListDto>>(items);
            foreach (var dto in dtos)
            {
                var tmplId = items.First(i => i.Id == dto.Id).TemplateId;
                dto.TenTemplate = templates.FirstOrDefault(t => t.Id == tmplId)?.TenTemplate ?? "";
            }

            return new PagedResultDto<EmailCampaignInListDto>(totalCount, dtos);
        }
        public async Task<List<EmailUserDto>> GetListUserAsync()
        {
            var unsubSet = (await _unsubRepo.GetListAsync())
                .Select(x => x.Email.ToLower()).ToHashSet();

            var users = await AsyncExecuter.ToListAsync(
                (await _userRepo.GetQueryableAsync())
                    .Where(x => !string.IsNullOrEmpty(x.Email) &&
                                 x.IsActive &&
                                 !unsubSet.Contains(x.Email.ToLower()))
                    .OrderBy(x => x.Name)
            );

            return users.Select(x => new EmailUserDto
            {
                Id = x.Id,
                Email = x.Email,
                HoTen = $"{x.Name}".Trim()
            }).ToList();
        }

        // ================= SEND NOW =================
        //[Authorize(EmailPermissions.Campaign.Send)]
        public async Task SendNowAsync(Guid id)
        {
            var entity = await Repository.GetAsync(id);

            if (entity.TrangThai == TrangThaiChienDich.DangGui ||
                entity.TrangThai == TrangThaiChienDich.HoanThanh)
                throw new UserFriendlyException("Campaign này không thể gửi lại.");

            // Hủy lịch cũ nếu có
            if (!string.IsNullOrWhiteSpace(entity.SendJobId))
            {
                try { _hangfire.Delete(entity.SendJobId); }
                catch (Exception ex)
                {
                    Logger.LogWarning(ex, $"Không thể hủy job cũ: {entity.SendJobId}");
                }
            }

            entity.TrangThai = TrangThaiChienDich.DangGui;
            entity.NgayGui = DateTime.Now;

            // Enqueue Hangfire job ngay lập tức
            var jobId = _hangfire.Enqueue<EmailSendingJob>(
                job => job.ExecuteAsync(id));

            entity.SendJobId = jobId;
            await Repository.UpdateAsync(entity, autoSave: true);
        }

        // ================= SCHEDULE =================
        //[Authorize(EmailPermissions.Campaign.Send)]
        public async Task ScheduleAsync(Guid id, DateTime ngayGui)
        {
            if (ngayGui <= DateTime.Now)
                throw new UserFriendlyException("Ngày gửi phải lớn hơn thời điểm hiện tại.");

            var entity = await Repository.GetAsync(id);

            if (entity.TrangThai == TrangThaiChienDich.DangGui ||
                entity.TrangThai == TrangThaiChienDich.HoanThanh)
                throw new UserFriendlyException("Campaign này không thể lên lịch lại.");

            // Hủy lịch cũ nếu có
            if (!string.IsNullOrWhiteSpace(entity.SendJobId))
            {
                try { _hangfire.Delete(entity.SendJobId); }
                catch (Exception ex)
                {
                    Logger.LogWarning(ex, $"Không thể hủy job cũ: {entity.SendJobId}");
                }
            }

            entity.TrangThai = TrangThaiChienDich.DaLenLich;
            entity.NgayGui = ngayGui;

            // Schedule Hangfire job đúng giờ
            var delay = ngayGui - DateTime.Now;
            var jobId = _hangfire.Schedule<EmailSendingJob>(
                job => job.ExecuteAsync(id),
                delay > TimeSpan.Zero ? delay : TimeSpan.Zero);

            entity.SendJobId = jobId;
            await Repository.UpdateAsync(entity, autoSave: true);
        }

        // ================= PAUSE =================
        //[Authorize(EmailPermissions.Campaign.Send)]
        public async Task PauseAsync(Guid id)
        {
            var entity = await Repository.GetAsync(id);

            if (entity.TrangThai != TrangThaiChienDich.DaLenLich &&
                entity.TrangThai != TrangThaiChienDich.DangGui)
                throw new UserFriendlyException("Chỉ có thể tạm dừng campaign đang lên lịch hoặc đang gửi.");

            // Hủy Hangfire job
            if (!string.IsNullOrWhiteSpace(entity.SendJobId))
            {
                try { _hangfire.Delete(entity.SendJobId); }
                catch (Exception ex)
                {
                    Logger.LogWarning(ex, $"Không thể hủy job: {entity.SendJobId}");
                }
            }

            entity.TrangThai = TrangThaiChienDich.TamDung;
            entity.SendJobId = null;

            // Đánh dấu queue chưa gửi là HuyBo
            var pendingQueues = await _queueRepo.GetListAsync(
                x => x.CampaignId == id &&
                     x.TrangThai == TrangThaiEmail.ChoGui);

            foreach (var q in pendingQueues)
                q.TrangThai = TrangThaiEmail.HuyBo;

            await _queueRepo.UpdateManyAsync(pendingQueues);
            await Repository.UpdateAsync(entity, autoSave: true);
        }

        // ================= SEND DIRECT =================
        //[Authorize(EmailPermissions.Campaign.Send)]
        public async Task SendDirectAsync(SendDirectEmailDto input)
        {
            var isUnsub = await _unsubRepo.AnyAsync(
                x => x.Email.ToLower() == input.Email.Trim().ToLower());

            if (isUnsub)
                throw new UserFriendlyException($"Email {input.Email} đã hủy đăng ký nhận mail.");

            // Dùng chung SendEmailAsync logic — gọi qua SmtpClient trực tiếp
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
                new MailAddress(input.Email))
            {
                Subject = input.TieuDe,
                Body = input.NoiDungHtml,
                IsBodyHtml = true
            };

            await smtp.SendMailAsync(message);
        }

        // ================= GET QUEUE =================
        public async Task<PagedResultDto<EmailQueueDto>> GetQueueAsync(
            Guid campaignId, PagedResultRequestDto input)
        {
            var query = (await _queueRepo.GetQueryableAsync())
                .Where(x => x.CampaignId == campaignId);

            var totalCount = await AsyncExecuter.LongCountAsync(query);

            var items = await AsyncExecuter.ToListAsync(
                query.OrderByDescending(x => x.CreationTime)
                     .Skip(input.SkipCount)
                     .Take(input.MaxResultCount)
            );
            return new PagedResultDto<EmailQueueDto>(
                totalCount,
                ObjectMapper.Map<List<EmailQueue>, List<EmailQueueDto>>(items)
            );
        }

        public async Task<string> PreviewEmailAsync(Guid templateId, string tenKhachHang = "Nguyễn Văn A")
        {
            var template = await _templateRepo.GetAsync(templateId);

            return template.NoiDungHtml
                .Replace("{{tenKhachHang}}", tenKhachHang)
                .Replace("{{email}}", "preview@example.com")
                .Replace("{{ngayHienTai}}", DateTime.Now.ToString("dd/MM/yyyy"))
                .Replace("{{unsubscribeLink}}", "#unsubscribe-preview");
        }
    }
}
