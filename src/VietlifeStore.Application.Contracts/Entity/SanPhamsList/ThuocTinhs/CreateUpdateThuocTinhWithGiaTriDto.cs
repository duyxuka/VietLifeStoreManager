using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VietlifeStore.Entity.SanPhamsList.ThuocTinhs
{
    public class CreateUpdateThuocTinhWithGiaTriDto
    {
        public string Ten { get; set; }              // Màu, Size
        public List<string> GiaTris { get; set; }    // Đỏ, Xanh / S, M
    }

}
