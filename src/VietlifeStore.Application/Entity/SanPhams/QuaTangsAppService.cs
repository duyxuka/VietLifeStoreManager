using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VietlifeStore.Entity.SanPhams;
using VietlifeStore.Entity.SanPhamsList.QuaTangs;
using VietlifeStore.Permissions;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Linq;

namespace VietlifeStore.Entity.SanPhams
{
    public class QuaTangsAppService :
        CrudAppService<
            QuaTang,
            QuaTangDto,
            Guid,
            PagedResultRequestDto,
            CreateUpdateQuaTangDto,
            CreateUpdateQuaTangDto>,
        IQuaTangsAppService
    {
        public QuaTangsAppService(IRepository<QuaTang, Guid> repository)
            : base(repository)
        {
            GetPolicyName = VietlifeStorePermissions.QuaTang.View;
            GetListPolicyName = VietlifeStorePermissions.QuaTang.View;
            CreatePolicyName = VietlifeStorePermissions.QuaTang.Create;
            UpdatePolicyName = VietlifeStorePermissions.QuaTang.Update;
            DeletePolicyName = VietlifeStorePermissions.QuaTang.Delete;
        }

        // ================= GET ALL =================
        [Authorize(VietlifeStorePermissions.QuaTang.View)]
        public async Task<List<QuaTangInListDto>> GetListAllAsync()
        {
            var list = await Repository.GetListAsync(x => x.TrangThai);
            return ObjectMapper.Map<List<QuaTang>, List<QuaTangInListDto>>(list);
        }

        // ================= FILTER + PAGING =================
        [Authorize(VietlifeStorePermissions.QuaTang.View)]
        public async Task<PagedResultDto<QuaTangInListDto>> GetListFilterAsync(BaseListFilterDto input)
        {
            var query = await Repository.GetQueryableAsync();

            query = query
                .WhereIf(!string.IsNullOrWhiteSpace(input.Keyword),
                    x => x.Ten.Contains(input.Keyword));

            var totalCount = await AsyncExecuter.CountAsync(query);

            var items = await AsyncExecuter.ToListAsync(
                query
                    .OrderByDescending(x => x.CreationTime)
                    .Skip(input.SkipCount)
                    .Take(input.MaxResultCount)
            );

            return new PagedResultDto<QuaTangInListDto>(
                totalCount,
                ObjectMapper.Map<List<QuaTang>, List<QuaTangInListDto>>(items)
            );
        }

        // ================= DELETE MULTIPLE =================
        [Authorize(VietlifeStorePermissions.QuaTang.Delete)]
        public async Task DeleteMultipleAsync(IEnumerable<Guid> ids)
        {
            await Repository.DeleteManyAsync(ids);
            await UnitOfWorkManager.Current.SaveChangesAsync();
        }
    }
}
