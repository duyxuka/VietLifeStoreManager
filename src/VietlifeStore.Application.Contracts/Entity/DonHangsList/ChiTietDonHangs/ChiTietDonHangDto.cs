using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace VietlifeStore.Entity.DonHangsList.ChiTietDonHangs
{
    public class ChiTietDonHangDto : IEntityDto<Guid>
    {
        public Guid Id { get; set; }
        public Guid DonHangId { get; set; } // FK đến DonHang
        public Guid SanPhamId { get; set; } // FK đến SanPham
        public string SanPhamBienThe { get; set; }
        public string TenSanPham { get; set; }
        public string QuaTang { get; set; }
        public int SoLuong { get; set; }
        public decimal Gia { get; set; }
        public bool TrangThai { get; set; }
    }
}
