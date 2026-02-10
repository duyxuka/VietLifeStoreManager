using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Caching;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.Identity;
using Volo.Abp.Security.Claims;
using Volo.Abp.Settings;
using Volo.Abp.Threading;

namespace VietlifeStore.Entity.TaiKhoans
{
    public class TaiKhoanManager : IdentityUserManager
    {
        public TaiKhoanManager(
            IdentityUserStore store,
            IIdentityRoleRepository roleRepository,
            IIdentityUserRepository userRepository,
            IOptions<IdentityOptions> optionsAccessor,
            IPasswordHasher<IdentityUser> passwordHasher,
            IEnumerable<IUserValidator<IdentityUser>> userValidators,
            IEnumerable<IPasswordValidator<IdentityUser>> passwordValidators,
            ILookupNormalizer keyNormalizer,
            IdentityErrorDescriber errors,
            IServiceProvider serviceProvider,
            ILogger<IdentityUserManager> logger,
            ICancellationTokenProvider cancellationTokenProvider,
            IOrganizationUnitRepository organizationUnitRepository,
            ISettingProvider settingProvider,
            IDistributedEventBus distributedEventBus,
            IIdentityLinkUserRepository linkUserRepository,
            IDistributedCache<AbpDynamicClaimCacheItem> dynamicClaimCache)
            : base(
                store,
                roleRepository,
                userRepository,
                optionsAccessor,
                passwordHasher,
                userValidators,
                passwordValidators,
                keyNormalizer,
                errors,
                serviceProvider,
                logger,
                cancellationTokenProvider,
                organizationUnitRepository,
                settingProvider,
                distributedEventBus,
                linkUserRepository,
                dynamicClaimCache)
        {
        }
    }
}
