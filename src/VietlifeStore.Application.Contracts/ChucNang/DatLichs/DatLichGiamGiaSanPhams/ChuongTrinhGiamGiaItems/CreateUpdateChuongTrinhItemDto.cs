using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VietlifeStore.ChucNang.DatLichs.DatLichGiamGiaSanPhams.ChuongTrinhGiamGiaItems
{
    public class CreateUpdateChuongTrinhItemDto
    {
        public Guid? SanPhamId { get; set; }
        public Guid? BienTheId { get; set; }
        public Guid? QuaTangId { get; set; }

        public decimal GiaSauGiam { get; set; }
    }
}
