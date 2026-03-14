using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VietlifeStore.Entity.CamNangsList.DanhMucCamNangs;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace VietlifeStore.Entity.CamNangsList.CamNangComments
{
    public interface ICamNangCommentAppService :
        ICrudAppService<
            CamNangCommentDto,
            Guid,
            PagedResultRequestDto,
            CreateUpdateCamNangCommentDto,
            CreateUpdateCamNangCommentDto>
    {
        Task<PagedResultDto<CamNangCommentInListDto>> GetListFilterAsync(BaseListFilterDto input);
        Task<List<CamNangCommentInListDto>> GetListAllAsync();
        Task<List<CamNangCommentDto>> GetListByCamNangAsync(Guid camNangId);

        Task DeleteMultipleAsync(IEnumerable<Guid> ids);
    }
}
