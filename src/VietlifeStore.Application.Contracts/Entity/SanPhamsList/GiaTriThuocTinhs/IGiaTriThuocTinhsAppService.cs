using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace VietlifeStore.Entity.SanPhamsList.GiaTriThuocTinhs
{
    public interface IGiaTriThuocTinhsAppService : ICrudAppService<
         GiaTriThuocTinhDto,
         Guid,
         PagedResultRequestDto,
         CreateUpdateGiaTriThuocTinhDto,
         CreateUpdateGiaTriThuocTinhDto>
    {
        Task<PagedResultDto<GiaTriThuocTinhInListDto>> GetListFilterAsync(BaseListFilterDto input);
        Task<List<GiaTriThuocTinhInListDto>> GetListAllAsync();
        Task DeleteMultipleAsync(IEnumerable<Guid> ids);
    }
}
