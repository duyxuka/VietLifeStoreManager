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

        public AccountAppService(
            TaiKhoanManager taiKhoanManager,
            IIdentityUserRepository userRepository,
            IGuidGenerator guidGenerator,
            ICurrentTenant currentTenant,
            ILookupNormalizer lookupNormalizer,
            IdentityUserManager identityUserManager)
        {
            _taiKhoanManager = taiKhoanManager;
            _userRepository = userRepository;
            _guidGenerator = guidGenerator;
            _currentTenant = currentTenant;
            LookupNormalizer = lookupNormalizer;
            _identityUserManager = identityUserManager;
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

            IdentityUser user = null;

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
    }
}
