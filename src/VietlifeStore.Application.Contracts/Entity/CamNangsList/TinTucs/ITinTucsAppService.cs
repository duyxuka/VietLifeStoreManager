using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace VietlifeStore.Entity.CamNangsList.TinTucs
{
    public interface ITinTucsAppService : ICrudAppService<
         TinTucDto,
         Guid,
         PagedResultRequestDto,
         CreateUpdateTinTucDto,
         CreateUpdateTinTucDto>
    {
        Task<PagedResultDto<TinTucInListDto>> GetListFilterAsync(BaseListFilterDto input);
        Task<List<TinTucInListDto>> GetListAllAsync();
        Task DeleteMultipleAsync(IEnumerable<Guid> ids);
    }
}
