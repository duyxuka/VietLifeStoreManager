using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VietlifeStore.ChucNang.DatLichs.DatLichVouchers;

namespace VietlifeStore.Entity.DonHangsList.Vouchers
{
    public class CreateUpdateVoucherDto
    {
        public string MaVoucher { get; set; }
        public string TenVoucher { get; set; }
        public string MoTa { get; set; }

        public LoaiVoucher LoaiVoucher { get; set; }
        public PhamViVoucher PhamVi { get; set; }

        public decimal GiamGia { get; set; }
        public bool LaPhanTram { get; set; }
        public decimal? GiamToiDa { get; set; }
        public decimal DonHangToiThieu { get; set; }

        public int TongSoLuong { get; set; }
        public int GioiHanMoiUser { get; set; } = 1;

        public DateTime? ThoiHanBatDau { get; set; }
        public DateTime? ThoiHanKetThuc { get; set; }
        public bool ChiPhatHanhCuThe { get; set; } = false;
        public List<Guid> SanPhamIds { get; set; } = new();
        public List<Guid> DanhMucIds { get; set; } = new();
    }
}
