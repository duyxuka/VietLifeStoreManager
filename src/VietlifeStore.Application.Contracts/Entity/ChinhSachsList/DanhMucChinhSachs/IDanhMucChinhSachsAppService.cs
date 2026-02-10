using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace VietlifeStore.Entity.ChinhSachsList.DanhMucChinhSachs
{
    public interface IDanhMucChinhSachsAppService : ICrudAppService<
         DanhMucChinhSachDto,
         Guid,
         PagedResultRequestDto,
         CreateUpdateDanhMucChinhSachDto,
         CreateUpdateDanhMucChinhSachDto>
    {
        Task<PagedResultDto<DanhMucChinhSachInListDto>> GetListFilterAsync(BaseListFilterDto input);
        Task<List<DanhMucChinhSachInListDto>> GetListAllAsync();
        Task DeleteMultipleAsync(IEnumerable<Guid> ids);
    }
}
