using AutoMapper.Internal.Mappers;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VietlifeStore.Entity.LienHes;
using VietlifeStore.Entity.Videoplatform;
using VietlifeStore.Entity.VideoPlatform;
using VietlifeStore.Permissions;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Uow;

namespace VietlifeStore.Entity.SocialVideos
{
    public class SocialVideosAppService :
        CrudAppService<
            SocialVideo,
            SocialVideoDto,
            Guid,
            PagedResultRequestDto,
            CreateUpdateSocialVideoDto,
            CreateUpdateSocialVideoDto>,
        ISocialVideosAppService
    {
        public SocialVideosAppService(
            IRepository<SocialVideo, Guid> repository)
            : base(repository)
        {
            GetPolicyName = VietlifeStorePermissions.SocialVideo.View;
            GetListPolicyName = VietlifeStorePermissions.SocialVideo.View;
            CreatePolicyName = VietlifeStorePermissions.SocialVideo.Create;
            UpdatePolicyName = VietlifeStorePermissions.SocialVideo.Update;
            DeletePolicyName = VietlifeStorePermissions.SocialVideo.Delete;
        }

        // ================= CREATE =================
        [Authorize(VietlifeStorePermissions.SocialVideo.Create)]
        public override async Task<SocialVideoDto> CreateAsync(CreateUpdateSocialVideoDto input)
        {
            var entity = ObjectMapper.Map<CreateUpdateSocialVideoDto, SocialVideo>(input);

            await Repository.InsertAsync(entity, autoSave: true);
            return MapToGetOutputDto(entity);
        }

        // ================= UPDATE =================
        [Authorize(VietlifeStorePermissions.SocialVideo.Update)]
        public override async Task<SocialVideoDto> UpdateAsync(Guid id, CreateUpdateSocialVideoDto input)
        {
            var entity = await Repository.GetAsync(id);

            ObjectMapper.Map(input, entity);

            await Repository.UpdateAsync(entity, autoSave: true);
            return MapToGetOutputDto(entity);
        }

        // ================= DELETE MULTIPLE =================
        [Authorize(VietlifeStorePermissions.SocialVideo.Delete)]
        public async Task DeleteMultipleAsync(IEnumerable<Guid> ids)
        {
            await Repository.DeleteManyAsync(ids);
            await UnitOfWorkManager.Current.SaveChangesAsync();
        }

        // ================= GET ALL ACTIVE (Frontend) =================
        [AllowAnonymous]
        public async Task<List<SocialVideoInListDto>> GetListBySectionAsync(string section)
        {
            var query = (await Repository.GetQueryableAsync())
                .Where(x => x.IsActive && x.Section == section)
                .OrderBy(x => x.DisplayOrder);

            var list = await AsyncExecuter.ToListAsync(query);

            return ObjectMapper.Map<List<SocialVideo>, List<SocialVideoInListDto>>(list);
        }

        // ================= FILTER + PAGING (Admin) =================
        [Authorize(VietlifeStorePermissions.SocialVideo.View)]
        public async Task<PagedResultDto<SocialVideoInListDto>> GetListFilterAsync(BaseListFilterDto input)
        {
            var query = (await Repository.GetQueryableAsync())
                .WhereIf(!string.IsNullOrWhiteSpace(input.Keyword),
                    x => x.Title.Contains(input.Keyword) ||
                         x.Platform.Contains(input.Keyword) ||
                         x.Section.Contains(input.Keyword));

            var totalCount = await AsyncExecuter.LongCountAsync(query);

            var items = await AsyncExecuter.ToListAsync(
                query.OrderByDescending(x => x.CreationTime)
                     .Skip(input.SkipCount)
                     .Take(input.MaxResultCount)
            );

            return new PagedResultDto<SocialVideoInListDto>(
                totalCount,
                ObjectMapper.Map<List<SocialVideo>, List<SocialVideoInListDto>>(items)
            );
        }

        public async Task<List<SocialVideoInListDto>> GetListAllAsync()
        {
            var list = await AsyncExecuter.ToListAsync(
                (await Repository.GetQueryableAsync())
                    .OrderByDescending(x => x.CreationTime)
            );

            return ObjectMapper.Map<List<SocialVideo>, List<SocialVideoInListDto>>(list);
        }
    }
}
