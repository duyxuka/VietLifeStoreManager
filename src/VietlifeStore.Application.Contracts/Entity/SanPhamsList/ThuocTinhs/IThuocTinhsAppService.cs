using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace VietlifeStore.Entity.SanPhamsList.ThuocTinhs
{
    public interface IThuocTinhsAppService : ICrudAppService<
         ThuocTinhDto,
         Guid,
         PagedResultRequestDto,
         CreateUpdateThuocTinhDto,
         CreateUpdateThuocTinhDto>
    {
        Task<PagedResultDto<ThuocTinhInListDto>> GetListFilterAsync(BaseListFilterDto input);
        Task<List<ThuocTinhInListDto>> GetListAllAsync();
        Task DeleteMultipleAsync(IEnumerable<Guid> ids);
    }
}
