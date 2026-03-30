using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VietlifeStore.ChucNang.DatLichs.DatLichGiamGiaSanPhams.ChuongTrinhGiamGiaItems;
using Volo.Abp.Application.Dtos;

namespace VietlifeStore.ChucNang.DatLichs.DatLichGiamGiaSanPhams.ChuongTrinhGiamGias
{
    public class ChuongTrinhDto : EntityDto<Guid>
    {
        public string TenChuongTrinh { get; set; }
        public DateTime ThoiGianBatDau { get; set; }
        public DateTime ThoiGianKetThuc { get; set; }
        public LichGiamGiaTrangThai TrangThai { get; set; }
        public string? MoTa { get; set; }

        public List<ChuongTrinhItemDto> Items { get; set; }
    }
}
