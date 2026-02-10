using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace VietlifeStore.Entity.Banners
{
    public interface IBannersAppService : ICrudAppService<
         BannerDto,
         Guid,
         PagedResultRequestDto,
         CreateUpdateBannerDto,
         CreateUpdateBannerDto>
    {
        Task<PagedResultDto<BannerInListDto>> GetListFilterAsync(BaseListFilterDto input);
        Task<List<BannerInListDto>> GetListAllAsync();
        Task DeleteMultipleAsync(IEnumerable<Guid> ids);
    }
}
