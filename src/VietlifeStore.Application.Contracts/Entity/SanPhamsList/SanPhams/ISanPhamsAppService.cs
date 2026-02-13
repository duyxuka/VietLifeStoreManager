using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace VietlifeStore.Entity.SanPhamsList.SanPhams
{
    public interface ISanPhamsAppService : ICrudAppService<
         SanPhamDto,
         Guid,
         PagedResultRequestDto,
         CreateUpdateSanPhamDto,
         CreateUpdateSanPhamDto>
    {
        Task<PagedResultDto<SanPhamInListDto>> GetListFilterAsync(BaseListFilterDto input);
        Task<List<SanPhamInListDto>> GetListAllAsync();
        Task DeleteMultipleAsync(IEnumerable<Guid> ids);
        Task<List<SanPhamInListDto>> GetTopBanChayAsync(int top = 6);
    }
}
