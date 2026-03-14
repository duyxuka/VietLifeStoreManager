using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VietlifeStore.Entity.TaiKhoans;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.Identity;

namespace VietlifeStore.Entity.DonHangs
{
    public class DonHang : FullAuditedAggregateRoot<Guid>
    {
        public Guid TaiKhoanKhachHangId { get; set; } // FK đến IdentityUser (tài khoản khách)
        public virtual TaiKhoan TaiKhoanKhachHang { get; set; }
        public string Ten { get; set; }
        public string Ma { get; set; }
        public string DiaChi { get; set; }
        public string Email { get; set; }
        public string SoDienThoai { get; set; }
        public string GhiChu { get; set; } = null;
        public string PhuongThucThanhToan { get; set; }
        public decimal TongSoLuong { get; set; }
        public decimal TongTien { get; set; }
        public decimal? GiamGiaVoucher { get; set; }
        public byte TrangThai { get; set; } // Ví dụ: "Chờ xác nhận", "Đang giao", "Hoàn thành"
        public DateTime NgayDat { get; set; } = DateTime.Now;
        public virtual ICollection<ChiTietDonHang> ChiTietDonHangs { get; set; } = new List<ChiTietDonHang>();
    }
}
