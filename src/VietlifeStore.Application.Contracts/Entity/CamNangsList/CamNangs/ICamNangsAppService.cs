using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace VietlifeStore.Entity.CamNangsList.CamNangs
{
    public interface ICamNangsAppService : ICrudAppService<
         CamNangDto,
         Guid,
         PagedResultRequestDto,
         CreateUpdateCamNangDto,
         CreateUpdateCamNangDto>
    {
        Task<PagedResultDto<CamNangInListDto>> GetListFilterAsync(BaseListFilterDto input);
        Task<List<CamNangInListDto>> GetListAllAsync();
        Task DeleteMultipleAsync(IEnumerable<Guid> ids);
    }
}
