using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace VietlifeStore.Entity.SanPhamsList.AnhSanPhams
{
    public interface IAnhSanPhamsAppService : ICrudAppService<
         AnhSanPhamDto,
         Guid,
         PagedResultRequestDto,
         CreateUpdateAnhSanPhamDto,
         CreateUpdateAnhSanPhamDto>
    {
        Task<PagedResultDto<AnhSanPhamInListDto>> GetListFilterAsync(BaseListFilterDto input);
        Task<List<AnhSanPhamInListDto>> GetListAllAsync();
        Task DeleteMultipleAsync(IEnumerable<Guid> ids);
    }
}
