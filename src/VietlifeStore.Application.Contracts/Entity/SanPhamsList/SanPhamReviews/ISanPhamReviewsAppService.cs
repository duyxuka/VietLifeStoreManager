using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VietlifeStore.Entity.SanPhamsList.SanPhamReviews;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace VietlifeStore.Entity.SanPhamsList.SanPhamReviews
{
    public interface ISanPhamReviewsAppService : ICrudAppService<
         SanPhamReviewDto,
         Guid,
         PagedResultRequestDto,
         CreateUpdateSanPhamReviewDto,
         CreateUpdateSanPhamReviewDto>
    {
        Task<PagedResultDto<SanPhamReviewInListDto>> GetListFilterAsync(BaseListFilterDto input);
        Task<List<SanPhamReviewInListDto>> GetListAllAsync();
        Task DeleteMultipleAsync(IEnumerable<Guid> ids);
    }
}
