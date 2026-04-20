using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace VietlifeStore.Entity.SEOs
{
    public interface ISeoConfigAppService : ICrudAppService<
        SeoConfigDto,
        Guid,
        PagedResultRequestDto,
        CreateUpdateSeoConfigDto,
        CreateUpdateSeoConfigDto>
    {
        Task<SeoConfigDto> GetByPageKeyAsync(string pageKey);
        Task<List<SeoConfigInListDto>> GetListAllAsync();
        Task<PagedResultDto<SeoConfigInListDto>> GetListFilterAsync(BaseListFilterDto input);
        Task DeleteMultipleAsync(IEnumerable<Guid> ids);
    }
}
