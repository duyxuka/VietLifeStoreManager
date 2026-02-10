using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VietlifeStore.Permissions;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace VietlifeStore.Entity.LienHes
{
    public class LienHesAppService :
        CrudAppService<
            LienHe,
            LienHeDto,
            Guid,
            PagedResultRequestDto,
            CreateUpdateLienHeDto,
            CreateUpdateLienHeDto>,
        ILienHesAppService
    {
        public LienHesAppService(
            IRepository<LienHe, Guid> repository)
            : base(repository)
        {
            GetPolicyName = VietlifeStorePermissions.LienHe.View;
            GetListPolicyName = VietlifeStorePermissions.LienHe.View;
            CreatePolicyName = VietlifeStorePermissions.LienHe.Create;
            UpdatePolicyName = VietlifeStorePermissions.LienHe.Update;
            DeletePolicyName = VietlifeStorePermissions.LienHe.Delete;
        }

        // ================= CREATE (PUBLIC) =================
        [AllowAnonymous]
        public override async Task<LienHeDto> CreateAsync(CreateUpdateLienHeDto input)
        {
            var entity = new LienHe
            {
                HoTen = input.HoTen,
                Email = input.Email,
                SoDienThoai = input.SoDienThoai,
                NoiDung = input.NoiDung,
                DaXuLy = false
            };

            await Repository.InsertAsync(entity, autoSave: true);
            return MapToGetOutputDto(entity);
        }

        // ================= UPDATE (CHỈ ĐÁNH DẤU XỬ LÝ) =================
        [Authorize(VietlifeStorePermissions.LienHe.Update)]
        public override async Task<LienHeDto> UpdateAsync(Guid id, CreateUpdateLienHeDto input)
        {
            var entity = await Repository.GetAsync(id);

            entity.DaXuLy = input.DaXuLy;

            await Repository.UpdateAsync(entity, autoSave: true);
            return MapToGetOutputDto(entity);
        }

        // ================= DELETE MULTIPLE =================
        [Authorize(VietlifeStorePermissions.LienHe.Delete)]
        public async Task DeleteMultipleAsync(IEnumerable<Guid> ids)
        {
            await Repository.DeleteManyAsync(ids);
            await UnitOfWorkManager.Current.SaveChangesAsync();
        }

        // ================= GET ALL =================
        [Authorize(VietlifeStorePermissions.LienHe.View)]
        public async Task<List<LienHeInListDto>> GetListAllAsync()
        {
            var list = await AsyncExecuter.ToListAsync(
                (await Repository.GetQueryableAsync())
                    .OrderByDescending(x => x.CreationTime)
            );

            return ObjectMapper.Map<List<LienHe>, List<LienHeInListDto>>(list);
        }

        // ================= FILTER + PAGING =================
        [Authorize(VietlifeStorePermissions.LienHe.View)]
        public async Task<PagedResultDto<LienHeInListDto>> GetListFilterAsync(BaseListFilterDto input)
        {
            var query = (await Repository.GetQueryableAsync())
                .WhereIf(!string.IsNullOrWhiteSpace(input.Keyword),
                    x => x.HoTen.Contains(input.Keyword)
                      || x.Email.Contains(input.Keyword)
                      || x.SoDienThoai.Contains(input.Keyword));

            var totalCount = await AsyncExecuter.LongCountAsync(query);

            var items = await AsyncExecuter.ToListAsync(
                query.OrderByDescending(x => x.CreationTime)
                     .Skip(input.SkipCount)
                     .Take(input.MaxResultCount)
            );

            return new PagedResultDto<LienHeInListDto>(
                totalCount,
                ObjectMapper.Map<List<LienHe>, List<LienHeInListDto>>(items)
            );
        }

        // ================= MARK AS PROCESSED =================
        [Authorize(VietlifeStorePermissions.LienHe.Update)]
        public async Task MarkAsProcessedAsync(Guid id)
        {
            var entity = await Repository.GetAsync(id);
            entity.DaXuLy = true;

            await Repository.UpdateAsync(entity, autoSave: true);
        }
    }
}
