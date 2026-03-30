using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VietlifeStore.ChucNang.DatLichs.DatLichVouchers;
using Volo.Abp.Domain.Entities.Auditing;

namespace VietlifeStore.Entity.DonHangs
{
    public class VoucherSchedule : FullAuditedAggregateRoot<Guid>
    {
        public Guid VoucherId { get; set; }
        public string HangfireJobId { get; set; }
        public LoaiJobVoucher LoaiJob { get; set; }
        public DateTime ThoiGianDuKien { get; set; }
        public DateTime? ThoiGianThucThi { get; set; }
        public TrangThaiJob TrangThai { get; set; } = TrangThaiJob.ChoXuLy;
        public string GhiChu { get; set; }               // Stack trace nếu lỗi

        public virtual Voucher Voucher { get; set; }
    }
}
