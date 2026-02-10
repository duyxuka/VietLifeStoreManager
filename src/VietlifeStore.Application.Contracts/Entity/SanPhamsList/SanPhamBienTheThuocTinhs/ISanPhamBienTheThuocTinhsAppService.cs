using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace VietlifeStore.Entity.SanPhamsList.SanPhamBienTheThuocTinhs
{
    public interface ISanPhamBienTheThuocTinhsAppService : ICrudAppService<
         SanPhamBienTheThuocTinhDto,
         Guid,
         PagedResultRequestDto,
         CreateUpdateSanPhamBienTheThuocTinhDto,
         CreateUpdateSanPhamBienTheThuocTinhDto>
    {
        Task<PagedResultDto<SanPhamBienTheThuocTinhInListDto>> GetListFilterAsync(BaseListFilterDto input);
        Task<List<SanPhamBienTheThuocTinhInListDto>> GetListAllAsync();
        Task DeleteMultipleAsync(IEnumerable<Guid> ids);
    }
}
