using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace VietlifeStore.Entity.CamNangsList.DanhMucCamNangs
{
    public interface IDanhMucCamNangsAppService : ICrudAppService<
         DanhMucCamNangDto,
         Guid,
         PagedResultRequestDto,
         CreateUpdateDanhMucCamNangDto,
         CreateUpdateDanhMucCamNangDto>
    {
        Task<PagedResultDto<DanhMucCamNangInListDto>> GetListFilterAsync(BaseListFilterDto input);
        Task<List<DanhMucCamNangInListDto>> GetListAllAsync();
        Task DeleteMultipleAsync(IEnumerable<Guid> ids);
    }
}
