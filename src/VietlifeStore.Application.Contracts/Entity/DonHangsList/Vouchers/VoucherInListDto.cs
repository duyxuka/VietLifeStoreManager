using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VietlifeStore.ChucNang.DatLichs.DatLichVouchers;
using Volo.Abp.Application.Dtos;

namespace VietlifeStore.Entity.DonHangsList.Vouchers
{
    public class VoucherInListDto : EntityDto<Guid>
    {
        public Guid Id { get; set; }
        public string MaVoucher { get; set; }
        public string TenVoucher { get; set; }
        public decimal GiamGia { get; set; }
        public bool LaPhanTram { get; set; }
        public decimal? GiamToiDa { get; set; }
        public decimal DonHangToiThieu { get; set; }
        public string? HangfireActivateJobId { get; set; }
        public string? HangfireExpireJobId { get; set; }
        public int TongSoLuong { get; set; }
        public int DaDung { get; set; }
        public DateTime? ThoiHanBatDau { get; set; }
        public DateTime? ThoiHanKetThuc { get; set; }
        public TrangThaiVoucher TrangThai { get; set; }
        public LoaiVoucher LoaiVoucher { get; set; }
        public PhamViVoucher PhamVi { get; set; }
        public int? PhanTramDaDung { get; set; }
    }
}
