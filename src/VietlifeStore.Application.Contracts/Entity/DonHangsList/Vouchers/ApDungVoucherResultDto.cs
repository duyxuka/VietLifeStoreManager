using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VietlifeStore.ChucNang.DatLichs.DatLichVouchers;

namespace VietlifeStore.Entity.DonHangsList.Vouchers
{
    public class ApDungVoucherResultDto
    {
        public Guid VoucherId { get; set; }
        public string MaVoucher { get; set; }
        public string TenVoucher { get; set; }
        public decimal GiaTriGiam { get; set; }
        public decimal GiaSauGiam { get; set; }
        public LoaiVoucher LoaiVoucher { get; set; }
    }
}
