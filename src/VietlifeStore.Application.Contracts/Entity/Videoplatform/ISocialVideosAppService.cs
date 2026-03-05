using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VietlifeStore.Entity.LienHes;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace VietlifeStore.Entity.Videoplatform
{
    public interface ISocialVideosAppService : ICrudAppService<
         SocialVideoDto,
         Guid,
         PagedResultRequestDto,
         CreateUpdateSocialVideoDto,
         CreateUpdateSocialVideoDto>
    {
        Task<PagedResultDto<SocialVideoInListDto>> GetListFilterAsync(BaseListFilterDto input);
        Task<List<SocialVideoInListDto>> GetListAllAsync();
        Task DeleteMultipleAsync(IEnumerable<Guid> ids);
    }
}
