using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VietlifeStore.Entity.SanPhamsList.GiaTriThuocTinhs
{
    public class CreateUpdateGiaTriThuocTinhDto
    {
        public Guid ThuocTinhId { get; set; }
        public string GiaTri { get; set; } // Đỏ, Xanh, S, M
    }
}
