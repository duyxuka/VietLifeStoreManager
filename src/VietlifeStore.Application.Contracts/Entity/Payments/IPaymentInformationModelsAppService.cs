using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VietlifeStore.Entity.DonHangsList.Vouchers;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace VietlifeStore.Entity.Payments
{
    public interface IPaymentInformationModelsAppService : ICrudAppService<
         PaymentInformationModelDto,
         Guid,
         PagedResultRequestDto,
         CreateUpdatePaymentInformationModelDto,
         CreateUpdatePaymentInformationModelDto>
    {
        Task<PagedResultDto<PaymentInformationModelInListDto>> GetListFilterAsync(BaseListFilterDto input);
        Task<List<PaymentInformationModelInListDto>> GetListAllAsync();
        Task DeleteMultipleAsync(IEnumerable<Guid> ids);
    }
}
