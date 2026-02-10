using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace VietlifeStore.Entity.SanPhamsList.QuaTangs
{
    public interface IQuaTangsAppService : ICrudAppService<
         QuaTangDto,
         Guid,
         PagedResultRequestDto,
         CreateUpdateQuaTangDto,
         CreateUpdateQuaTangDto>
    {
        Task<PagedResultDto<QuaTangInListDto>> GetListFilterAsync(BaseListFilterDto input);
        Task<List<QuaTangInListDto>> GetListAllAsync();
        Task DeleteMultipleAsync(IEnumerable<Guid> ids);
    }
}
