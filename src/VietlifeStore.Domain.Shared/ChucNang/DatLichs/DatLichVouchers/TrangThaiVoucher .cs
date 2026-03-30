using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VietlifeStore.ChucNang.DatLichs.DatLichVouchers
{
    public enum TrangThaiVoucher { ChuaKichHoat = 0, DangHoatDong = 1, HetHan = 2, VoHieu = 3, HetSoLuong = 4 }
    public enum LoaiVoucher { GiamDonHang = 1, GiamVanChuyen = 2, HoanTien = 3 }
    public enum PhamViVoucher { ToanShop = 1, SanPhamCuThe = 2, DanhMucCuThe = 3, NhomKhachHang = 4 }
    public enum LoaiDoiTuong { SanPham = 1, DanhMuc = 2, Shop = 3, NhomKhachHang = 4 }
    public enum LoaiJobVoucher { KichHoat = 1, VoHieuHoa = 2, CanhBaoHetHan = 3 }
    public enum TrangThaiJob { ChoXuLy = 0, ThanhCong = 1, ThatBai = 2, DaHuy = 3 }
}
