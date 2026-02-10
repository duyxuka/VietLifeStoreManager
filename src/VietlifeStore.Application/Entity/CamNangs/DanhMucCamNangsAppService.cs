using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VietlifeStore.Entity.CamNangsList.DanhMucCamNangs;
using VietlifeStore.Permissions;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace VietlifeStore.Entity.CamNangs
{
    public class DanhMucCamNangsAppService :
        CrudAppService<
            DanhMucCamNang,
            DanhMucCamNangDto,
            Guid,
            PagedResultRequestDto,
            CreateUpdateDanhMucCamNangDto,
            CreateUpdateDanhMucCamNangDto>,
        IDanhMucCamNangsAppService
    {
        public DanhMucCamNangsAppService(
            IRepository<DanhMucCamNang, Guid> repository)
            : base(repository)
        {
            GetPolicyName = VietlifeStorePermissions.CamNang.View;
            GetListPolicyName = VietlifeStorePermissions.CamNang.View;
            CreatePolicyName = VietlifeStorePermissions.CamNang.Create;
            UpdatePolicyName = VietlifeStorePermissions.CamNang.Update;
            DeletePolicyName = VietlifeStorePermissions.CamNang.Delete;
        }

        // ================= CREATE =================
        [Authorize(VietlifeStorePermissions.CamNang.Create)]
        public override async Task<DanhMucCamNangDto> CreateAsync(CreateUpdateDanhMucCamNangDto input)
        {
            var entity = new DanhMucCamNang
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
        [Authorize(VietlifeStorePermissions.CamNang.Update)]
        public override async Task<DanhMucCamNangDto> UpdateAsync(Guid id, CreateUpdateDanhMucCamNangDto input)
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
        [Authorize(VietlifeStorePermissions.CamNang.Delete)]
        public async Task DeleteMultipleAsync(IEnumerable<Guid> ids)
        {
            await Repository.DeleteManyAsync(ids);
            await UnitOfWorkManager.Current.SaveChangesAsync();
        }

        // ================= GET ALL ACTIVE =================
        [Authorize(VietlifeStorePermissions.CamNang.View)]
        public async Task<List<DanhMucCamNangInListDto>> GetListAllAsync()
        {
            var list = await AsyncExecuter.ToListAsync(
                (await Repository.GetQueryableAsync())
                    .Where(x => x.TrangThai)
                    .OrderByDescending(x => x.CreationTime)
            );

            return ObjectMapper.Map<List<DanhMucCamNang>, List<DanhMucCamNangInListDto>>(list);
        }

        // ================= FILTER + PAGING =================
        [Authorize(VietlifeStorePermissions.CamNang.View)]
        public async Task<PagedResultDto<DanhMucCamNangInListDto>> GetListFilterAsync(BaseListFilterDto input)
        {
            var query = (await Repository.GetQueryableAsync())
                .WhereIf(!string.IsNullOrWhiteSpace(input.Keyword),
                    x => x.Ten.Contains(input.Keyword) || x.Slug.Contains(input.Keyword));

            var totalCount = await AsyncExecuter.LongCountAsync(query);

            var items = await AsyncExecuter.ToListAsync(
                query
                    .OrderByDescending(x => x.CreationTime)
                    .Skip(input.SkipCount)
                    .Take(input.MaxResultCount)
            );

            return new PagedResultDto<DanhMucCamNangInListDto>(
                totalCount,
                ObjectMapper.Map<List<DanhMucCamNang>, List<DanhMucCamNangInListDto>>(items)
            );
        }
    }
}
