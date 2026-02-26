using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VietlifeStore.Entity.ChinhSachsList.DanhMucChinhSachs;
using VietlifeStore.Permissions;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace VietlifeStore.Entity.ChinhSachs
{
    public class DanhMucChinhSachsAppService :
        CrudAppService<
            DanhMucChinhSach,
            DanhMucChinhSachDto,
            Guid,
            PagedResultRequestDto,
            CreateUpdateDanhMucChinhSachDto,
            CreateUpdateDanhMucChinhSachDto>,
        IDanhMucChinhSachsAppService
    {
        public DanhMucChinhSachsAppService(
            IRepository<DanhMucChinhSach, Guid> repository)
            : base(repository)
        {
            GetPolicyName = VietlifeStorePermissions.DanhMucChinhSach.View;
            GetListPolicyName = VietlifeStorePermissions.DanhMucChinhSach.View;
            CreatePolicyName = VietlifeStorePermissions.DanhMucChinhSach.Create;
            UpdatePolicyName = VietlifeStorePermissions.DanhMucChinhSach.Update;
            DeletePolicyName = VietlifeStorePermissions.DanhMucChinhSach.Delete;
        }

        // ================= CREATE =================
        [Authorize(VietlifeStorePermissions.DanhMucChinhSach.Create)]
        public override async Task<DanhMucChinhSachDto> CreateAsync(CreateUpdateDanhMucChinhSachDto input)
        {
            var entity = new DanhMucChinhSach
            {
                Ten = input.Ten,
                Slug = input.Slug,
                TrangThai = input.TrangThai,
                TitleSEO = input.TitleSEO,
                Keyword = input.Keyword,
                DescriptionSEO = input.DescriptionSEO
            };

            await Repository.InsertAsync(entity, autoSave: true);
            return MapToGetOutputDto(entity);
        }

        // ================= UPDATE =================
        [Authorize(VietlifeStorePermissions.DanhMucChinhSach.Update)]
        public override async Task<DanhMucChinhSachDto> UpdateAsync(Guid id, CreateUpdateDanhMucChinhSachDto input)
        {
            var entity = await Repository.GetAsync(id);

            entity.Ten = input.Ten;
            entity.Slug = input.Slug;
            entity.TrangThai = input.TrangThai;
            entity.TitleSEO = input.TitleSEO;
            entity.Keyword = input.Keyword;
            entity.DescriptionSEO = input.DescriptionSEO;

            await Repository.UpdateAsync(entity, autoSave: true);
            return MapToGetOutputDto(entity);
        }

        // ================= DELETE MULTIPLE =================
        [Authorize(VietlifeStorePermissions.DanhMucChinhSach.Delete)]
        public async Task DeleteMultipleAsync(IEnumerable<Guid> ids)
        {
            await Repository.DeleteManyAsync(ids);
            await UnitOfWorkManager.Current.SaveChangesAsync();
        }

        // ================= GET ALL ACTIVE =================
        [AllowAnonymous]
        public async Task<List<DanhMucChinhSachInListDto>> GetListAllAsync()
        {
            var list = await AsyncExecuter.ToListAsync(
                (await Repository.GetQueryableAsync())
                    .Where(x => x.TrangThai)
                    .OrderByDescending(x => x.CreationTime)
            );

            return ObjectMapper.Map<List<DanhMucChinhSach>, List<DanhMucChinhSachInListDto>>(list);
        }

        // ================= FILTER + PAGING =================
        [Authorize(VietlifeStorePermissions.DanhMucChinhSach.View)]
        public async Task<PagedResultDto<DanhMucChinhSachInListDto>> GetListFilterAsync(BaseListFilterDto input)
        {
            var query = (await Repository.GetQueryableAsync())
                .WhereIf(!string.IsNullOrWhiteSpace(input.Keyword),
                    x => x.Ten.Contains(input.Keyword) || x.Slug.Contains(input.Keyword));

            var totalCount = await AsyncExecuter.LongCountAsync(query);

            var items = await AsyncExecuter.ToListAsync(
                query.OrderByDescending(x => x.CreationTime)
                     .Skip(input.SkipCount)
                     .Take(input.MaxResultCount)
            );

            return new PagedResultDto<DanhMucChinhSachInListDto>(
                totalCount,
                ObjectMapper.Map<List<DanhMucChinhSach>, List<DanhMucChinhSachInListDto>>(items)
            );
        }
    }
}
