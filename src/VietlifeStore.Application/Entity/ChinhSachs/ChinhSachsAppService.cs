using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VietlifeStore.Entity.ChinhSachsList.ChinhSachs;
using VietlifeStore.Permissions;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace VietlifeStore.Entity.ChinhSachs
{
    public class ChinhSachsAppService :
        CrudAppService<
            ChinhSach,
            ChinhSachDto,
            Guid,
            PagedResultRequestDto,
            CreateUpdateChinhSachDto,
            CreateUpdateChinhSachDto>,
        IChinhSachsAppService
    {
        public ChinhSachsAppService(
            IRepository<ChinhSach, Guid> repository)
            : base(repository)
        {
            GetPolicyName = VietlifeStorePermissions.ChinhSach.View;
            GetListPolicyName = VietlifeStorePermissions.ChinhSach.View;
            CreatePolicyName = VietlifeStorePermissions.ChinhSach.Create;
            UpdatePolicyName = VietlifeStorePermissions.ChinhSach.Update;
            DeletePolicyName = VietlifeStorePermissions.ChinhSach.Delete;
        }

        // ================= CREATE =================
        [Authorize(VietlifeStorePermissions.ChinhSach.Create)]
        public override async Task<ChinhSachDto> CreateAsync(CreateUpdateChinhSachDto input)
        {
            var entity = new ChinhSach
            {
                TieuDe = input.TieuDe,
                NoiDung = input.NoiDung,
                DanhMucChinhSachId = input.DanhMucChinhSachId,
                TrangThai = input.TrangThai
            };

            await Repository.InsertAsync(entity, autoSave: true);
            return MapToGetOutputDto(entity);
        }

        // ================= UPDATE =================
        [Authorize(VietlifeStorePermissions.ChinhSach.Update)]
        public override async Task<ChinhSachDto> UpdateAsync(Guid id, CreateUpdateChinhSachDto input)
        {
            var entity = await Repository.GetAsync(id);

            entity.TieuDe = input.TieuDe;
            entity.NoiDung = input.NoiDung;
            entity.DanhMucChinhSachId = input.DanhMucChinhSachId;
            entity.TrangThai = input.TrangThai;

            await Repository.UpdateAsync(entity, autoSave: true);
            return MapToGetOutputDto(entity);
        }

        // ================= DELETE MULTIPLE =================
        [Authorize(VietlifeStorePermissions.ChinhSach.Delete)]
        public async Task DeleteMultipleAsync(IEnumerable<Guid> ids)
        {
            await Repository.DeleteManyAsync(ids);
            await UnitOfWorkManager.Current.SaveChangesAsync();
        }

        // ================= GET ALL ACTIVE =================
        [Authorize(VietlifeStorePermissions.ChinhSach.View)]
        public async Task<List<ChinhSachInListDto>> GetListAllAsync()
        {
            var list = await AsyncExecuter.ToListAsync(
                (await Repository.GetQueryableAsync())
                    .Where(x => x.TrangThai)
                    .OrderByDescending(x => x.CreationTime)
            );

            return ObjectMapper.Map<List<ChinhSach>, List<ChinhSachInListDto>>(list);
        }

        // ================= FILTER + PAGING =================
        [Authorize(VietlifeStorePermissions.ChinhSach.View)]
        public async Task<PagedResultDto<ChinhSachInListDto>> GetListFilterAsync(BaseListFilterDto input)
        {
            var query = (await Repository.GetQueryableAsync())
                .WhereIf(!string.IsNullOrWhiteSpace(input.Keyword),
                    x => x.TieuDe.Contains(input.Keyword));

            var totalCount = await AsyncExecuter.LongCountAsync(query);

            var items = await AsyncExecuter.ToListAsync(
                query.OrderByDescending(x => x.CreationTime)
                     .Skip(input.SkipCount)
                     .Take(input.MaxResultCount)
            );

            return new PagedResultDto<ChinhSachInListDto>(
                totalCount,
                ObjectMapper.Map<List<ChinhSach>, List<ChinhSachInListDto>>(items)
            );
        }

        [AllowAnonymous]
        public async Task<List<ChinhSachInListDto>> GetByDanhMucIdAsync(Guid danhMucId)
        {
            var list = await AsyncExecuter.ToListAsync(
                (await Repository.GetQueryableAsync())
                    .Where(x => x.DanhMucChinhSachId == danhMucId && x.TrangThai)
                    .OrderByDescending(x => x.CreationTime)
            );

            return ObjectMapper.Map<List<ChinhSach>, List<ChinhSachInListDto>>(list);
        }
    }
}
