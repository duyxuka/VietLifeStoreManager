using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace VietlifeStore.Entity.DonHangsList.ChiTietDonHangs
{
    public interface IChiTietDonHangsAppService : ICrudAppService<
        ChiTietDonHangDto,
        Guid,
        PagedResultRequestDto,
        CreateUpdateChiTietDonHangDto,
        CreateUpdateChiTietDonHangDto>
    {
        Task<List<ChiTietDonHangDto>> GetByDonHangIdAsync(Guid donHangId);
    }
}
