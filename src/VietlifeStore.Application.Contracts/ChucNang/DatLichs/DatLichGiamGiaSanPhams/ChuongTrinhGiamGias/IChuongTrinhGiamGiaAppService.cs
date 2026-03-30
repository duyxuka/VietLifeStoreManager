using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace VietlifeStore.ChucNang.DatLichs.DatLichGiamGiaSanPhams.ChuongTrinhGiamGias
{
    public interface IChuongTrinhGiamGiaAppService : ICrudAppService<
    ChuongTrinhDto,
    Guid,
    PagedResultRequestDto,
    CreateUpdateChuongTrinhDto,
    CreateUpdateChuongTrinhDto>
    {
        Task<PagedResultDto<ChuongTrinhInListDto>> GetListFilterAsync(BaseListFilterDto input);
        Task<List<ChuongTrinhInListDto>> GetListAllAsync();
        Task DeleteMultipleAsync(IEnumerable<Guid> ids);

        Task ActivateAsync();
        Task ExpireAsync();
        Task CancelAsync(Guid id);
        Task ActivateSingleAsync(Guid id);
        Task ExpireSingleAsync(Guid id);
    }
}
