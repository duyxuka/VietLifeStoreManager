using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace VietlifeStore.Entity.SanPhamsList.SanPhamBienThes
{
    public interface ISanPhamBienThesAppService : ICrudAppService<
         SanPhamBienTheDto,
         Guid,
         PagedResultRequestDto,
         CreateUpdateSanPhamBienTheDto,
         CreateUpdateSanPhamBienTheDto>
    {
        Task<PagedResultDto<SanPhamBienTheInListDto>> GetListFilterAsync(BaseListFilterDto input);
        Task<List<SanPhamBienTheInListDto>> GetListAllAsync();
        Task DeleteMultipleAsync(IEnumerable<Guid> ids);
    }
}
