using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VietlifeStore.Entity.TaiKhoans;
using Volo.Abp.Application.Services;
using Volo.Abp.Guids;
using Volo.Abp.Identity;
using Volo.Abp.MultiTenancy;
using Volo.Abp;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.Emailing;
using System.Net.Mail;
using System.Net;
using Microsoft.Extensions.Configuration;
using Volo.Abp.Users;
using System.ComponentModel.DataAnnotations;
using RegisterDto = VietlifeStore.Entity.TaiKhoans.RegisterDto;
using Volo.Abp.Account;

namespace VietlifeStore.System.Users
{
    public class AccountAppService : ApplicationService
    {
        private readonly IdentityUserManager _identityUserManager;
        private readonly TaiKhoanManager _taiKhoanManager;
        private readonly IIdentityUserRepository _userRepository;
        private readonly IGuidGenerator _guidGenerator;
        private readonly ICurrentTenant _currentTenant;
        private readonly ILookupNormalizer LookupNormalizer;
        private readonly IConfiguration _configuration;

        public AccountAppService(
            TaiKhoanManager taiKhoanManager,
            IIdentityUserRepository userRepository,
            IGuidGenerator guidGenerator,
            ICurrentTenant currentTenant,
            ILookupNormalizer lookupNormalizer,
            IdentityUserManager identityUserManager,
            IConfiguration configuration)
        {
            _taiKhoanManager = taiKhoanManager;
            _userRepository = userRepository;
            _guidGenerator = guidGenerator;
            _currentTenant = currentTenant;
            LookupNormalizer = lookupNormalizer;
            _identityUserManager = identityUserManager;
            _configuration = configuration;
        }

        /// <summary>
        /// Đăng ký tài khoản khách hàng
        /// </summary>
        [AllowAnonymous]
        public async Task RegisterAsync(RegisterDto input)
        {
            var normalizedUserName = LookupNormalizer.NormalizeName(input.UserName);
            var normalizedEmail = LookupNormalizer.NormalizeEmail(input.Email);

            // Check username
            if (await _userRepository.FindByNormalizedUserNameAsync(normalizedUserName) != null)
            {
                throw new UserFriendlyException("Tên đăng nhập đã tồn tại");
            }

            // Check email
            if (await _userRepository.FindByNormalizedEmailAsync(normalizedEmail) != null)
            {
                throw new UserFriendlyException("Email đã tồn tại");
            }

            var user = new TaiKhoan(
                _guidGenerator.Create(),
                input.UserName,
                input.Email,
                _currentTenant.Id
            )
            {
                Name = input.Name,
                IsCustomer = true,
                Status = true
            };

            user.SetPhoneNumber(input.PhoneNumber, true);

            (await _taiKhoanManager.CreateAsync(user, input.Password))
                .CheckErrors();

            await _taiKhoanManager.AddToRoleAsync(user, "Customer");
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<string> ResolveUserNameAsync(string emailOrPhone)
        {
            if (string.IsNullOrWhiteSpace(emailOrPhone))
                throw new UserFriendlyException("Vui lòng nhập Email hoặc SĐT");

            Volo.Abp.Identity.IdentityUser user = null;

            if (emailOrPhone.Contains("@"))
            {
                user = await _identityUserManager.FindByEmailAsync(emailOrPhone);
            }
            else
            {
                user = (await _userRepository.GetListAsync()).FirstOrDefault(x => x.PhoneNumber == emailOrPhone);
            }

            if (user == null)
                throw new UserFriendlyException("Tài khoản không tồn tại");

            return user.UserName;
        }
        
        [AllowAnonymous]
        public async Task ForgotPasswordAsync(string email)
        {
            var user = await _identityUserManager.FindByEmailAsync(email);
            if (user == null)
                throw new UserFriendlyException("Email không tồn tại");

            var token = await _identityUserManager.GeneratePasswordResetTokenAsync(user);

            var resetLink = $"https://mayhutsua.com.vn/reset-password?userId={user.Id}&token={Uri.EscapeDataString(token)}";

            var body = $@"
                <!DOCTYPE html>
                <html>
                <head>
                <meta charset='UTF-8'>
                </head>
                <body style='margin:0;padding:0;background:#f4f6f8;font-family:Arial,sans-serif;'>

                <table width='100%' cellpadding='0' cellspacing='0' style='background:#f4f6f8;padding:30px 0;'>
                <tr>
                <td align='center'>

                <table width='600' cellpadding='0' cellspacing='0' style='background:#ffffff;border-radius:8px;overflow:hidden;box-shadow:0 2px 8px rgba(0,0,0,0.05);'>

                <tr>
                <td style='background:#ff4d94;padding:20px;text-align:center;color:white;font-size:22px;font-weight:bold;'>
                Vietlife Store
                </td>
                </tr>

                <tr>
                <td style='padding:30px;color:#333;'>

                <h2 style='margin-top:0;'>Đặt lại mật khẩu</h2>

                <p>
                Chúng tôi nhận được yêu cầu đặt lại mật khẩu cho tài khoản của bạn.
                Nhấn nút bên dưới để tiếp tục.
                </p>

                <div style='text-align:center;margin:30px 0;'>
                <a href='{resetLink}'
                style='background:#ff4d94;
                color:white;
                padding:14px 28px;
                text-decoration:none;
                border-radius:6px;
                font-weight:bold;
                display:inline-block;'>
                Đặt lại mật khẩu
                </a>
                </div>

                <p>
                Link này sẽ hết hạn sau <b>15 phút</b>. Nếu bạn không yêu cầu đặt lại mật khẩu, hãy bỏ qua email này.
                </p>

                <hr style='border:none;border-top:1px solid #eee;margin:30px 0;'>

                <p style='font-size:13px;color:#888;margin:0;'>
                Nếu nút không hoạt động, hãy copy link sau vào trình duyệt:
                </p>

                <p style='word-break:break-all;font-size:13px;color:#ff4d94;'>
                {resetLink}
                </p>

                </td>
                </tr>

                <tr>
                <td style='background:#fafafa;padding:15px;text-align:center;font-size:12px;color:#999;'>

                © {DateTime.Now.Year} Vietlife Store  
                <br>
                Email này được gửi tự động, vui lòng không trả lời.

                </td>
                </tr>

                </table>

                </td>
                </tr>
                </table>

                </body>
                </html>
                ";

            var smtpSection = _configuration.GetSection("Abp:Mailing:Smtp");

            var host = smtpSection["Host"];
            var port = int.Parse(smtpSection["Port"]);
            var username = smtpSection["UserName"];
            var password = smtpSection["Password"];
            var enableSsl = bool.Parse(smtpSection["EnableSsl"]);

            var senderEmail = new MailAddress(username, "Vietlife Store");
            var receiverEmail = new MailAddress(email);

            using (var smtp = new SmtpClient(host, port))
            {
                smtp.Credentials = new NetworkCredential(username, password);
                smtp.EnableSsl = enableSsl;

                using (var message = new MailMessage(senderEmail, receiverEmail)
                {
                    Subject = "Yêu cầu đặt lại mật khẩu",
                    Body = body,
                    IsBodyHtml = true
                })
                {
                    await smtp.SendMailAsync(message);
                }
            }
        }

        [AllowAnonymous]
        public async Task ResetPasswordAsync(Guid userId, string token, string newPassword)
        {
            var user = await _identityUserManager.FindByIdAsync(userId.ToString());
            if (user == null)
                throw new UserFriendlyException("Tài khoản không tồn tại");

            var result = await _identityUserManager.ResetPasswordAsync(user, token, newPassword);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(x => x.Description));
                throw new UserFriendlyException(errors);
            }
        }

        [Authorize]
        [HttpGet]
        public async Task<UserDto> GetProfileAsync()
        {
            var userId = CurrentUser.GetId();

            var user = await _identityUserManager.FindByIdAsync(userId.ToString());

            if (user == null)
                throw new UserFriendlyException("Không tìm thấy tài khoản");

            var roles = await _identityUserManager.GetRolesAsync(user);

            return new UserDto
            {
                Id = user.Id,
                Name = user.Name,
                UserName = user.UserName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Roles = roles,
                IsActive = user.IsActive,
                IsCustomer = (user as TaiKhoan)?.IsCustomer ?? false,
                Status = (user as TaiKhoan)?.Status ?? false
            };
        }

        [Authorize]
        public async Task ChangePasswordAsync(ChangePasswordDto input)
        {
            var userId = CurrentUser.GetId();

            var user = await _identityUserManager.FindByIdAsync(userId.ToString());

            if (user == null)
                throw new UserFriendlyException("Không tìm thấy tài khoản");

            var result = await _identityUserManager.ChangePasswordAsync(
                user,
                input.CurrentPassword,
                input.NewPassword
            );

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(x => x.Description));
                throw new UserFriendlyException(errors);
            }
        }
        public class ChangePasswordDto
        {
            [Required]
            public string CurrentPassword { get; set; }

            [Required]
            public string NewPassword { get; set; }
        }

        [Authorize]
        [HttpPost]
        public async Task<UserDto> UpdateProfileAsync(UpdateProfileDto input)
        {
            var userId = CurrentUser.GetId();

            var user = await _identityUserManager.FindByIdAsync(userId.ToString());

            if (user == null)
                throw new UserFriendlyException("Không tìm thấy tài khoản");

            user.Name = input.Name;

            if (!string.IsNullOrWhiteSpace(input.PhoneNumber))
            {
                user.SetPhoneNumber(input.PhoneNumber, true);
            }

            var result = await _identityUserManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(x => x.Description));
                throw new UserFriendlyException(errors);
            }

            var roles = await _identityUserManager.GetRolesAsync(user);

            return new UserDto
            {
                Id = user.Id,
                Name = user.Name,
                UserName = user.UserName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Roles = roles,
                IsActive = user.IsActive,
                IsCustomer = (user as TaiKhoan)?.IsCustomer ?? false,
                Status = (user as TaiKhoan)?.Status ?? false
            };
        }

        public class UpdateProfileDto
        {
            [Required]
            public string Name { get; set; }

            public string PhoneNumber { get; set; }
        }
    }
}
