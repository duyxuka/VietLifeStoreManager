using AutoMapper.Internal.Mappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VietlifeStore.Permissions;
using VietlifeStore.Roles;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.PermissionManagement;
using Volo.Abp.SimpleStateChecking;
using Volo.Abp.Uow;
using Volo.Abp;

namespace VietlifeStore.System.Roles
{
    public class RolesAppService : CrudAppService<
        IdentityRole,
        RoleDto,
        Guid,
        PagedResultRequestDto,
        CreateUpdateRoleDto,
        CreateUpdateRoleDto>, IRolesAppService
    {
        protected PermissionManagementOptions Options { get; }
        protected IPermissionManager PermissionManager { get; }
        protected IPermissionDefinitionManager PermissionDefinitionManager { get; }
        protected ISimpleStateCheckerManager<PermissionDefinition> SimpleStateCheckerManager { get; }

        public RolesAppService(IRepository<IdentityRole, Guid> repository,
            IPermissionManager permissionManager,
        IPermissionDefinitionManager permissionDefinitionManager,
        IOptions<PermissionManagementOptions> options,
        ISimpleStateCheckerManager<PermissionDefinition> simpleStateCheckerManager)
            : base(repository)
        {
            Options = options.Value;
            PermissionManager = permissionManager;
            PermissionDefinitionManager = permissionDefinitionManager;
            SimpleStateCheckerManager = simpleStateCheckerManager;

            GetPolicyName = ExtendedIdentityPermissions.Roles.View;
            GetListPolicyName = ExtendedIdentityPermissions.Roles.View;
            CreatePolicyName = IdentityPermissions.Roles.Create;
            UpdatePolicyName = IdentityPermissions.Roles.Update;
            DeletePolicyName = IdentityPermissions.Roles.Delete;
        }

        [Authorize(IdentityPermissions.Roles.Delete)]
        public async Task DeleteMultipleAsync(IEnumerable<Guid> ids)
        {
            await Repository.DeleteManyAsync(ids);
            await UnitOfWorkManager.Current.SaveChangesAsync();
        }

        [Authorize(ExtendedIdentityPermissions.Roles.View)]
        public async Task<List<RoleInListDto>> GetListAllAsync()
        {
            var query = await Repository.GetQueryableAsync();
            var data = await AsyncExecuter.ToListAsync(query);

            return ObjectMapper.Map<List<IdentityRole>, List<RoleInListDto>>(data);

        }

        [Authorize(ExtendedIdentityPermissions.Roles.View)]
        public async Task<PagedResultDto<RoleInListDto>> GetListFilterAsync(BaseListFilterDto input)
        {
            var query = await Repository.GetQueryableAsync();
            query = query.WhereIf(!string.IsNullOrWhiteSpace(input.Keyword), x => x.Name.Contains(input.Keyword));

            var totalCount = await AsyncExecuter.LongCountAsync(query);
            var data = await AsyncExecuter.ToListAsync(query.Skip(input.SkipCount).Take(input.MaxResultCount));

            return new PagedResultDto<RoleInListDto>(totalCount, ObjectMapper.Map<List<IdentityRole>, List<RoleInListDto>>(data));
        }

        [Authorize(IdentityPermissions.Roles.Create)]
        public async override Task<RoleDto> CreateAsync(CreateUpdateRoleDto input)
        {
            var query = await Repository.GetQueryableAsync();
            var isNameExisted = query.Any(x => x.Name == input.Name);
            if (isNameExisted)
            {
                throw new BusinessException(VietlifeStoreDomainErrorCodes.RoleNameAlreadyExists)
                    .WithData("Name", input.Name);
            }
            var role = new IdentityRole(Guid.NewGuid(), input.Name);
            role.ExtraProperties[RoleConsts.DescriptionFieldName] = input.Description;
            var data = await Repository.InsertAsync(role);
            await UnitOfWorkManager.Current.SaveChangesAsync();
            return ObjectMapper.Map<IdentityRole, RoleDto>(data);
        }

        [Authorize(IdentityPermissions.Roles.Update)]
        public async override Task<RoleDto> UpdateAsync(Guid id, CreateUpdateRoleDto input)
        {
            var role = await Repository.GetAsync(id);
            if (role == null)
            {
                throw new EntityNotFoundException(typeof(IdentityRole), id);
            }
            var query = await Repository.GetQueryableAsync();
            var isNameExisted = query.Any(x => x.Name == input.Name && x.Id != id);
            if (isNameExisted)
            {
                throw new BusinessException(VietlifeStoreDomainErrorCodes.RoleNameAlreadyExists)
                    .WithData("Name", input.Name);
            }
            role.ExtraProperties[RoleConsts.DescriptionFieldName] = input.Description;
            var data = await Repository.UpdateAsync(role);
            await UnitOfWorkManager.Current.SaveChangesAsync();
            return ObjectMapper.Map<IdentityRole, RoleDto>(data);
        }

        [Authorize(ExtendedIdentityPermissions.Roles.View)]
        public async Task<GetPermissionListResultDto> GetPermissionsAsync(string providerName, string providerKey)
        {
            //await CheckProviderPolicy(providerName);

            var result = new GetPermissionListResultDto
            {
                EntityDisplayName = providerKey,
                Groups = new List<PermissionGroupDto>()
            };

            var groups = await PermissionDefinitionManager.GetGroupsAsync();

            foreach (var group in groups)
            {
                var groupDto = CreatePermissionGroupDto(group);
                var permissions = group.GetPermissionsWithChildren()
                    .Where(p => p.IsEnabled && (!p.Providers.Any() || p.Providers.Contains(providerName)))
                    .ToList();

                var enabledPermissions = new List<PermissionDefinition>();
                foreach (var p in permissions)
                {
                    if (await SimpleStateCheckerManager.IsEnabledAsync(p))
                        enabledPermissions.Add(p);
                }

                if (!enabledPermissions.Any()) continue;

                var grantInfoDtos = enabledPermissions.Select(CreatePermissionGrantInfoDto).ToList();
                var multipleGrantInfo = await PermissionManager.GetAsync(enabledPermissions.Select(x => x.Name).ToArray(), providerName, providerKey);

                foreach (var grantInfo in multipleGrantInfo.Result)
                {
                    var dto = grantInfoDtos.First(x => x.Name == grantInfo.Name);
                    dto.IsGranted = grantInfo.IsGranted;
                    dto.GrantedProviders = grantInfo.Providers.Select(p => new ProviderInfoDto
                    {
                        ProviderName = p.Name,
                        ProviderKey = p.Key
                    }).ToList();
                }

                // Gắn vào groupDto
                groupDto.Permissions.AddRange(grantInfoDtos);
                if (groupDto.Permissions.Any())
                    result.Groups.Add(groupDto);
            }

            return result;
        }

        private PermissionGrantInfoDto CreatePermissionGrantInfoDto(PermissionDefinition permission)
        {
            return new PermissionGrantInfoDto
            {
                Name = permission.Name,
                DisplayName = permission.DisplayName?.Localize(StringLocalizerFactory),
                ParentName = permission.Parent?.Name,
                AllowedProviders = permission.Providers,
                GrantedProviders = new List<ProviderInfoDto>()
            };
        }

        private PermissionGroupDto CreatePermissionGroupDto(PermissionGroupDefinition group)
        {
            return new PermissionGroupDto
            {
                Name = group.Name,
                DisplayName = group.DisplayName?.Localize(StringLocalizerFactory),
                Permissions = new List<PermissionGrantInfoDto>(),
            };
        }

        [Authorize(IdentityPermissions.Roles.Update)]
        public virtual async Task UpdatePermissionsAsync(string providerName, string providerKey, UpdatePermissionsDto input)
        {
            // await CheckProviderPolicy(providerName);

            foreach (var permissionDto in input.Permissions)
            {
                await PermissionManager.SetAsync(permissionDto.Name, providerName, providerKey, permissionDto.IsGranted);
            }
        }

        protected virtual async Task CheckProviderPolicy(string providerName)
        {
            var policyName = Options.ProviderPolicies.GetOrDefault(providerName);
            if (policyName.IsNullOrEmpty())
            {
                throw new AbpException($"No policy defined to get/set permissions for the provider '{providerName}'. Use {nameof(PermissionManagementOptions)} to map the policy.");
            }

            await AuthorizationService.CheckAsync(policyName);
        }
    }
}
