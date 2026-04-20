using AutoMapper.Internal.Mappers;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VietlifeStore.Permissions;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp;

namespace VietlifeStore.Entity.SEOs
{
    public class SeoConfigAppService :
        CrudAppService<
            SeoConfig,
            SeoConfigDto,
            Guid,
            PagedResultRequestDto,
            CreateUpdateSeoConfigDto,
            CreateUpdateSeoConfigDto>,
        ISeoConfigAppService
    {
        public SeoConfigAppService(IRepository<SeoConfig, Guid> repository)
            : base(repository)
        {
            // Quyền theo Permission (bạn có thể tạo riêng Permission cho SEO)
            GetPolicyName = VietlifeStorePermissions.SeoConfig.View;           // cần tạo permission này
            GetListPolicyName = VietlifeStorePermissions.SeoConfig.View;
            CreatePolicyName = VietlifeStorePermissions.SeoConfig.Create;
            UpdatePolicyName = VietlifeStorePermissions.SeoConfig.Update;
            DeletePolicyName = VietlifeStorePermissions.SeoConfig.Delete;
        }

        // ====================== GET BY PAGE KEY (Quan trọng nhất) ======================
        [AllowAnonymous]
        public async Task<SeoConfigDto> GetByPageKeyAsync(string pageKey)
        {
            if (string.IsNullOrWhiteSpace(pageKey))
                throw new UserFriendlyException("PageKey không được để trống");

            var entity = await Repository.FirstOrDefaultAsync(x => x.PageKey == pageKey);

            if (entity == null)
            {
                // Trả về object rỗng thay vì lỗi để Angular không bị crash
                return new SeoConfigDto
                {
                    PageKey = pageKey,
                    SeoTitle = string.Empty,
                    SeoDescription = string.Empty,
                    Robots = "index, follow"
                };
            }

            return MapToGetOutputDto(entity);
        }

        // ====================== GET LIST ALL ======================
        public async Task<List<SeoConfigInListDto>> GetListAllAsync()
        {
            var list = await AsyncExecuter.ToListAsync(
                (await Repository.GetQueryableAsync())
                    .OrderBy(x => x.PageKey)
            );

            return ObjectMapper.Map<List<SeoConfig>, List<SeoConfigInListDto>>(list);
        }

        // ====================== GET LIST WITH FILTER & PAGING ======================
        [Authorize(VietlifeStorePermissions.SeoConfig.View)]
        public async Task<PagedResultDto<SeoConfigInListDto>> GetListFilterAsync(BaseListFilterDto input)
        {
            var query = await Repository.GetQueryableAsync();

            query = query
                .WhereIf(!string.IsNullOrWhiteSpace(input.Keyword),
                    x => x.PageKey.Contains(input.Keyword) ||
                         x.SeoTitle.Contains(input.Keyword) ||
                         (x.SeoDescription != null && x.SeoDescription.Contains(input.Keyword)));

            var totalCount = await AsyncExecuter.LongCountAsync(query);

            var items = await AsyncExecuter.ToListAsync(
                query
                    .OrderBy(x => x.PageKey)
                    .Skip(input.SkipCount)
                    .Take(input.MaxResultCount)
            );

            return new PagedResultDto<SeoConfigInListDto>(
                totalCount,
                ObjectMapper.Map<List<SeoConfig>, List<SeoConfigInListDto>>(items)
            );
        }

        // ====================== CREATE ======================
        [Authorize(VietlifeStorePermissions.SeoConfig.Create)]
        public override async Task<SeoConfigDto> CreateAsync(CreateUpdateSeoConfigDto input)
        {
            // Kiểm tra PageKey đã tồn tại chưa
            if (await Repository.AnyAsync(x => x.PageKey == input.PageKey))
            {
                throw new UserFriendlyException($"PageKey '{input.PageKey}' đã tồn tại. Vui lòng chọn PageKey khác.");
            }

            return await base.CreateAsync(input);
        }

        // ====================== UPDATE ======================
        [Authorize(VietlifeStorePermissions.SeoConfig.Update)]
        public override async Task<SeoConfigDto> UpdateAsync(Guid id, CreateUpdateSeoConfigDto input)
        {
            var entity = await Repository.GetAsync(id);

            // Nếu thay đổi PageKey thì kiểm tra trùng
            if (entity.PageKey != input.PageKey)
            {
                if (await Repository.AnyAsync(x => x.PageKey == input.PageKey))
                {
                    throw new UserFriendlyException($"PageKey '{input.PageKey}' đã tồn tại.");
                }
            }

            return await base.UpdateAsync(id, input);
        }

        // ====================== DELETE MULTIPLE ======================
        [Authorize(VietlifeStorePermissions.SeoConfig.Delete)]
        public async Task DeleteMultipleAsync(IEnumerable<Guid> ids)
        {
            await Repository.DeleteManyAsync(ids);
        }

        // Override nếu bạn muốn tùy chỉnh Map thêm
        protected override SeoConfigDto MapToGetOutputDto(SeoConfig entity)
        {
            return base.MapToGetOutputDto(entity);
        }
    }
}
