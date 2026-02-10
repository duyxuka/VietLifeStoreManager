using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VietlifeStore.Entity.SanPhamsList.ThuocTinhs;
using VietlifeStore.Permissions;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace VietlifeStore.Entity.SanPhams
{
    public class ThuocTinhsAppService :
        CrudAppService<
            ThuocTinh,
            ThuocTinhDto,
            Guid,
            PagedResultRequestDto,
            CreateUpdateThuocTinhDto,
            CreateUpdateThuocTinhDto>,
        IThuocTinhsAppService
    {
        public ThuocTinhsAppService(IRepository<ThuocTinh, Guid> repository)
            : base(repository)
        {
            GetPolicyName = VietlifeStorePermissions.ThuocTinh.View;
            GetListPolicyName = VietlifeStorePermissions.ThuocTinh.View;
            CreatePolicyName = VietlifeStorePermissions.ThuocTinh.Create;
            UpdatePolicyName = VietlifeStorePermissions.ThuocTinh.Update;
            DeletePolicyName = VietlifeStorePermissions.ThuocTinh.Delete;
        }

        // ================= DELETE MULTIPLE =================
        [Authorize(VietlifeStorePermissions.ThuocTinh.Delete)]
        public async Task DeleteMultipleAsync(IEnumerable<Guid> ids)
        {
            await Repository.DeleteManyAsync(ids);
            await UnitOfWorkManager.Current.SaveChangesAsync();
        }

        // ================= GET ALL =================
        [Authorize(VietlifeStorePermissions.ThuocTinh.View)]
        public async Task<List<ThuocTinhInListDto>> GetListAllAsync()
        {
            var list = await AsyncExecuter.ToListAsync(
                (await Repository.GetQueryableAsync())
                    .OrderBy(x => x.Ten)
            );

            return ObjectMapper.Map<List<ThuocTinh>, List<ThuocTinhInListDto>>(list);
        }

        // ================= FILTER + PAGING =================
        [Authorize(VietlifeStorePermissions.ThuocTinh.View)]
        public async Task<PagedResultDto<ThuocTinhInListDto>> GetListFilterAsync(BaseListFilterDto input)
        {
            var query = (await Repository.GetQueryableAsync())
                .WhereIf(
                    !string.IsNullOrWhiteSpace(input.Keyword),
                    x => x.Ten.Contains(input.Keyword)
                );

            var totalCount = await AsyncExecuter.LongCountAsync(query);

            var items = await AsyncExecuter.ToListAsync(
                query
                    .OrderBy(x => x.Ten)
                    .Skip(input.SkipCount)
                    .Take(input.MaxResultCount)
            );

            return new PagedResultDto<ThuocTinhInListDto>(
                totalCount,
                ObjectMapper.Map<List<ThuocTinh>, List<ThuocTinhInListDto>>(items)
            );
        }
    }
}
