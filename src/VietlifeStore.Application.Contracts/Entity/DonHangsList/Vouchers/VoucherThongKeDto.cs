using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VietlifeStore.ChucNang.DatLichs.DatLichVouchers;

namespace VietlifeStore.Entity.DonHangsList.Vouchers
{
    public class VoucherThongKeDto
    {
        public Guid VoucherId { get; set; }
        public string MaVoucher { get; set; }
        public int TongSoLuong { get; set; }
        public int DaDung { get; set; }
        public int ConLai { get; set; }
        public decimal TongGiaTriGiam { get; set; }
        public int SoNguoiDung { get; set; }
        public TrangThaiVoucher TrangThai { get; set; }
    }
}
