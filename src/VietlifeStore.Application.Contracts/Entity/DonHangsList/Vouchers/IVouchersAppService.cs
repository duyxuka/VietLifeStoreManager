using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace VietlifeStore.Entity.DonHangsList.Vouchers
{
    public interface IVouchersAppService : ICrudAppService<
         VoucherDto,
         Guid,
         PagedResultRequestDto,
         CreateUpdateVoucherDto,
         CreateUpdateVoucherDto>
    {
        Task<PagedResultDto<VoucherInListDto>> GetListFilterAsync(BaseListFilterDto input);
        Task<List<VoucherInListDto>> GetListAllAsync();
        Task DeleteMultipleAsync(IEnumerable<Guid> ids);
    }
}
