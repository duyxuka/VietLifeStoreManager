using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace VietlifeStore.Entity.DonHangsList.DonHangs
{
    public interface IDonHangsAppService : ICrudAppService<
         DonHangDto,
         Guid,
         PagedResultRequestDto,
         CreateUpdateDonHangDto,
         CreateUpdateDonHangDto>
    {
        Task<PagedResultDto<DonHangInListDto>> GetListFilterAsync(BaseListFilterDto input);
        Task<List<DonHangInListDto>> GetListAllAsync();
        Task DeleteMultipleAsync(IEnumerable<Guid> ids);
    }
}
