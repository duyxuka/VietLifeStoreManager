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

namespace VietlifeStore.System.Users
{
    public class AccountAppService : ApplicationService
    {
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
            ILookupNormalizer lookupNormalizer)
        {
            _taiKhoanManager = taiKhoanManager;
            _userRepository = userRepository;
            _guidGenerator = guidGenerator;
            _currentTenant = currentTenant;
            LookupNormalizer = lookupNormalizer;
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
    }
}
