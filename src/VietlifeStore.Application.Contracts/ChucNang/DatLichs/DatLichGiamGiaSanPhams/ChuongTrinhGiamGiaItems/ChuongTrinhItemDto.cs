using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VietlifeStore.ChucNang.DatLichs.DatLichGiamGiaSanPhams.ChuongTrinhGiamGiaItems
{
    public class ChuongTrinhItemDto
    {
        public Guid Id { get; set; }
        public Guid? SanPhamId { get; set; }
        public Guid? BienTheId { get; set; }
        public decimal GiaSauGiam { get; set; }
        public Guid? QuaTangId { get; set; }
        public string? TenSanPham { get; set; }
        public string? TenBienThe { get; set; }
        public decimal? GiaBanDau { get; set; }
        public string? TenQuaTang { get; set; }

    }
}
