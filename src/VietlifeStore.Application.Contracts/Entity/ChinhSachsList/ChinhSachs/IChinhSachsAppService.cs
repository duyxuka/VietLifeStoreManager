using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace VietlifeStore.Entity.ChinhSachsList.ChinhSachs
{
    public interface IChinhSachsAppService : ICrudAppService<
         ChinhSachDto,
         Guid,
         PagedResultRequestDto,
         CreateUpdateChinhSachDto,
         CreateUpdateChinhSachDto>
    {
        Task<PagedResultDto<ChinhSachInListDto>> GetListFilterAsync(BaseListFilterDto input);
        Task<List<ChinhSachInListDto>> GetListAllAsync();
        Task DeleteMultipleAsync(IEnumerable<Guid> ids);
    }
}
