using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities.Auditing;

namespace VietlifeStore.Entity.DonHangs
{
    public class VoucherNguoiDung : FullAuditedAggregateRoot<Guid>
    {
        public Guid VoucherId { get; set; }
        public Guid UserId { get; set; }

        public int SoLuongNhan { get; set; } = 1;        // Số lượng nhận (thường là 1)
        public int DaDung { get; set; } = 0;             // Đã dùng bao nhiêu
        public DateTime NgayNhan { get; set; } = DateTime.UtcNow;
        public bool DaHetHan { get; set; } = false;      // Cache trạng thái

        public int ConLai => SoLuongNhan - DaDung;       // Computed

        public virtual Voucher Voucher { get; set; }
    }
}
