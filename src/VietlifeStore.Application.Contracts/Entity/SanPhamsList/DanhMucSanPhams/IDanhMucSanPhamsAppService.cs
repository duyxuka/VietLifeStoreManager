using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace VietlifeStore.Entity.SanPhamsList.DanhMucSanPhams
{
    public interface IDanhMucSanPhamsAppService : ICrudAppService<
         DanhMucSanPhamDto,
         Guid,
         PagedResultRequestDto,
         CreateUpdateDanhMucSanPhamDto,
         CreateUpdateDanhMucSanPhamDto>
    {
        Task<PagedResultDto<DanhMucSanPhamInListDto>> GetListFilterAsync(BaseListFilterDto input);
        Task<List<DanhMucSanPhamInListDto>> GetListAllAsync();
        Task DeleteMultipleAsync(IEnumerable<Guid> ids);
    }
}
