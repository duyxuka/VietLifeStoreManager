using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VietlifeStore.Entity.SanPhamsList.GiaTriThuocTinhs;
using VietlifeStore.Permissions;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace VietlifeStore.Entity.SanPhams
{
    public class GiaTriThuocTinhsAppService :
        CrudAppService<
            GiaTriThuocTinh,
            GiaTriThuocTinhDto,
            Guid,
            PagedResultRequestDto,
            CreateUpdateGiaTriThuocTinhDto,
            CreateUpdateGiaTriThuocTinhDto>,
        IGiaTriThuocTinhsAppService
    {
        public GiaTriThuocTinhsAppService(IRepository<GiaTriThuocTinh, Guid> repository)
            : base(repository)
        {
            GetPolicyName = VietlifeStorePermissions.GiaTriThuocTinh.View;
            GetListPolicyName = VietlifeStorePermissions.GiaTriThuocTinh.View;
            CreatePolicyName = VietlifeStorePermissions.GiaTriThuocTinh.Create;
            UpdatePolicyName = VietlifeStorePermissions.GiaTriThuocTinh.Update;
            DeletePolicyName = VietlifeStorePermissions.GiaTriThuocTinh.Delete;
        }

        // ================= DELETE MULTIPLE =================
        [Authorize(VietlifeStorePermissions.GiaTriThuocTinh.Delete)]
        public async Task DeleteMultipleAsync(IEnumerable<Guid> ids)
        {
            await Repository.DeleteManyAsync(ids);
            await UnitOfWorkManager.Current.SaveChangesAsync();
        }

        // ================= GET ALL =================
        [Authorize(VietlifeStorePermissions.GiaTriThuocTinh.View)]
        public async Task<List<GiaTriThuocTinhInListDto>> GetListAllAsync()
        {
            var list = await AsyncExecuter.ToListAsync(
                (await Repository.GetQueryableAsync())
                    .OrderBy(x => x.GiaTri)
            );

            return ObjectMapper.Map<List<GiaTriThuocTinh>, List<GiaTriThuocTinhInListDto>>(list);
        }

        // ================= FILTER + PAGING =================
        [Authorize(VietlifeStorePermissions.GiaTriThuocTinh.View)]
        public async Task<PagedResultDto<GiaTriThuocTinhInListDto>> GetListFilterAsync(BaseListFilterDto input)
        {
            var query = (await Repository.GetQueryableAsync())
                .WhereIf(
                    !string.IsNullOrWhiteSpace(input.Keyword),
                    x => x.GiaTri.Contains(input.Keyword)
                );

            var totalCount = await AsyncExecuter.LongCountAsync(query);

            var items = await AsyncExecuter.ToListAsync(
                query
                    .OrderBy(x => x.GiaTri)
                    .Skip(input.SkipCount)
                    .Take(input.MaxResultCount)
            );

            return new PagedResultDto<GiaTriThuocTinhInListDto>(
                totalCount,
                ObjectMapper.Map<List<GiaTriThuocTinh>, List<GiaTriThuocTinhInListDto>>(items)
            );
        }
    }
}
