using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VietlifeStore.Entity.DashboardStat
{
    public class DashboardStatsDto
    {
        // ===== TỔNG QUAN =====
        public long TongDonHang { get; set; }
        public long TongSanPham { get; set; }
        public long TongKhachHang { get; set; }
        public decimal TongDoanhThu { get; set; }

        // ===== ĐƠN HÀNG THEO TRẠNG THÁI =====
        public long DonChoXacNhan { get; set; }   // TrangThai = 0
        public long DonDangXuLy { get; set; }   // TrangThai = 1
        public long DonDangGiao { get; set; }   // TrangThai = 2
        public long DonHoanThanh { get; set; }   // TrangThai = 3
        public long DonDaHuy { get; set; }   // TrangThai = 4

        // ===== DOANH THU =====
        public decimal DoanhThuHomNay { get; set; }
        public decimal DoanhThuThangNay { get; set; }
        public decimal DoanhThuNamNay { get; set; }

        // ===== THANH TOÁN =====
        public long SoDonCOD { get; set; }
        public long SoDonViDienTu { get; set; }
        public decimal DoanhThuCOD { get; set; }
        public decimal DoanhThuViDienTu { get; set; }

        // ===== BIỂU ĐỒ =====
        public List<DoanhThuTheoThangDto> DoanhThuTheoThang { get; set; } = new();
        public List<DonHangTheoThangDto> DonHangTheoThang { get; set; } = new();

        // ===== TOP 5 =====
        public List<TopSanPhamDto> TopSanPham { get; set; } = new();
        public List<TopKhachHangDto> TopKhachHang { get; set; } = new();

        // ===== TRUY CẬP =====
        public long LuotTruyCapHomNay { get; set; }
        public long NguoiDungOnline { get; set; }
        public List<TopSanPhamXemDto> TopSanPhamXemNhieu { get; set; } = new();
    }

    public class DoanhThuTheoThangDto
    {
        public int Thang { get; set; }
        public string Label { get; set; } = "";
        public decimal DoanhThu { get; set; }
    }

    public class DonHangTheoThangDto
    {
        public int Thang { get; set; }
        public string Label { get; set; } = "";
        public int SoDon { get; set; }
        public int SoHuy { get; set; }
    }

    public class TopSanPhamDto
    {
        public string Ten { get; set; } = "";
        public string Anh { get; set; } = "";
        public string Slug { get; set; } = "";
        public int SoLuongBan { get; set; }
        public decimal DoanhThu { get; set; }
    }

    public class TopKhachHangDto
    {
        public string HoTen { get; set; } = "";
        public string SoDienThoai { get; set; } = "";
        public string Email { get; set; } = "";
        public int SoDon { get; set; }
        public decimal TongChiTieu { get; set; }
    }

    public class TopSanPhamXemDto
    {
        public string Ten { get; set; } = "";
        public string Slug { get; set; } = "";
        public int? LuotXem { get; set; }
    }
}
