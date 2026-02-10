using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Guids;
using Volo.Abp.Identity;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Uow;
using VietlifeStore.Entity.TaiKhoans;

namespace VietlifeStore.Seeding
{
    public class IdentityDataSeeder : ITransientDependency, IIdentityDataSeeder
    {
        protected IGuidGenerator GuidGenerator { get; }
        protected IIdentityRoleRepository RoleRepository { get; }
        protected IIdentityUserRepository UserRepository { get; }
        protected ILookupNormalizer LookupNormalizer { get; }
        protected TaiKhoanManager UserManager { get; }
        protected IdentityRoleManager RoleManager { get; }
        protected ICurrentTenant CurrentTenant { get; }
        protected IOptions<IdentityOptions> IdentityOptions { get; }

        public IdentityDataSeeder(
            IGuidGenerator guidGenerator,
            IIdentityRoleRepository roleRepository,
            IIdentityUserRepository userRepository,
            ILookupNormalizer lookupNormalizer,
            TaiKhoanManager userManager,
            IdentityRoleManager roleManager,
            ICurrentTenant currentTenant,
            IOptions<IdentityOptions> identityOptions)
        {
            GuidGenerator = guidGenerator;
            RoleRepository = roleRepository;
            UserRepository = userRepository;
            LookupNormalizer = lookupNormalizer;
            UserManager = userManager;
            RoleManager = roleManager;
            CurrentTenant = currentTenant;
            IdentityOptions = identityOptions;
        }

        [UnitOfWork]
        public virtual async Task<IdentityDataSeedResult> SeedAsync(
            string adminEmail,
            string adminPassword,
            Guid? tenantId = null,
            string? roleName = null)
        {
            using (CurrentTenant.Change(tenantId))
            {
                await IdentityOptions.SetAsync();
                var result = new IdentityDataSeedResult();

                // ===== ADMIN USER =====
                var normalizedUserName = LookupNormalizer.NormalizeName(adminEmail);
                var adminUser = await UserRepository.FindByNormalizedUserNameAsync(normalizedUserName);

                if (adminUser == null)
                {
                    adminUser = new TaiKhoan(
                        GuidGenerator.Create(),
                        adminEmail,
                        adminEmail,
                        tenantId
                    )
                    {
                        Name = "Admin",
                        IsCustomer = false,
                        Status = true
                    };

                    (await UserManager.CreateAsync(adminUser, adminPassword, validatePassword: false))
                        .CheckErrors();

                    result.CreatedAdminUser = true;
                }

                // ===== ADMIN ROLE =====
                var adminRoleName = roleName ?? "Admin";
                await CreateRoleIfNotExistsAsync(adminRoleName, tenantId, result);

                // ===== CUSTOMER ROLE =====
                await CreateRoleIfNotExistsAsync("Customer", tenantId, result);

                // ===== ASSIGN ADMIN ROLE =====
                if (!await UserManager.IsInRoleAsync(adminUser, adminRoleName))
                {
                    (await UserManager.AddToRoleAsync(adminUser, adminRoleName)).CheckErrors();
                }

                return result;
            }
        }

        private async Task CreateRoleIfNotExistsAsync(
            string roleName,
            Guid? tenantId,
            IdentityDataSeedResult result)
        {
            var normalizedRoleName = LookupNormalizer.NormalizeName(roleName);
            var role = await RoleRepository.FindByNormalizedNameAsync(normalizedRoleName);

            if (role == null)
            {
                role = new IdentityRole(
                    GuidGenerator.Create(),
                    roleName,
                    tenantId
                )
                {
                    IsStatic = roleName == "Admin",
                    IsPublic = true
                };

                (await RoleManager.CreateAsync(role)).CheckErrors();

                if (roleName == "Admin")
                {
                    result.CreatedAdminRole = true;
                }
            }
        }
    }
}
