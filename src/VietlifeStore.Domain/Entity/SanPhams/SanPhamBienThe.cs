using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities;

namespace VietlifeStore.Entity.SanPhams
{
    public class SanPhamBienThe : Entity<Guid>
    {
        public Guid SanPhamId { get; set; }
        public virtual SanPham SanPham { get; set; }
        public string Ten { get; set; }
        public decimal Gia { get; set; }
        public decimal? GiaKhuyenMai { get; set; }

        public virtual ICollection<SanPhamBienTheThuocTinh> ThuocTinhs { get; set; }
            = new List<SanPhamBienTheThuocTinh>();
    }
}
