using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace VietlifeStore.Entity.DonHangsList.Vouchers
{
    public class VoucherInListDto : EntityDto<Guid>
    {
        public string MaVoucher { get; set; }
        public decimal GiamGia { get; set; } // Giá trị giảm (phần trăm hoặc cố định)
        public bool LaPhanTram { get; set; } = false; // True nếu giảm phần trăm
        public decimal DonHangToiThieu { get; set; } // Đơn hàng tối thiểu áp dụng
        public DateTime? ThoiHanBatDau { get; set; }
        public DateTime? ThoiHanKetThuc { get; set; }
        public int SoLuong { get; set; } // Số lượng voucher có sẵn
        public bool LaDatLich { get; set; } = false;
        public bool TrangThai { get; set; } = false;
    }
}
