using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VietlifeStore.Entity.SanPhams;
using Volo.Abp.Domain.Entities;

namespace VietlifeStore.Entity.DonHangs
{
    public class ChiTietDonHang : Entity<Guid>
    {
        public Guid DonHangId { get; set; } // FK đến DonHang
        public virtual DonHang DonHang { get; set; }
        public Guid SanPhamId { get; set; } // FK đến SanPham
        public virtual SanPham SanPham { get; set; }
        public string SanPhamBienThe { get; set; }
        public string QuaTang { get; set; }
        public int SoLuong { get; set; }
        public decimal Gia { get; set; }
        public bool TrangThai { get; set; }
    }
}
