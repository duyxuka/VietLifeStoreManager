using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace VietlifeStore.Entity.LienHes
{
    public interface ILienHesAppService : ICrudAppService<
         LienHeDto,
         Guid,
         PagedResultRequestDto,
         CreateUpdateLienHeDto,
         CreateUpdateLienHeDto>
    {
        Task<PagedResultDto<LienHeInListDto>> GetListFilterAsync(BaseListFilterDto input);
        Task<List<LienHeInListDto>> GetListAllAsync();
        Task DeleteMultipleAsync(IEnumerable<Guid> ids);
    }
}
