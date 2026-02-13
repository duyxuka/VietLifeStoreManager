using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VietlifeStore.Entity.SanPhamsList.SanPhamBienTheThuocTinhs;

namespace VietlifeStore.Entity.SanPhamsList.SanPhamBienThes
{
    public class CreateUpdateSanPhamBienTheDto
    {
        public Guid? Id { get; set; }
        public decimal Gia { get; set; }
        public string Ten { get; set; }
        public decimal? GiaKhuyenMai { get; set; }
        public List<SanPhamBienTheThuocTinhDto>? SanPhamBienTheThuocTinhDtos { get; set; }
    }
}
