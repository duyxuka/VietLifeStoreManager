using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VietlifeStore.ChucNang.DatLichs.DatLichVouchers;
using Volo.Abp.Domain.Entities.Auditing;

namespace VietlifeStore.Entity.DonHangs
{
    public class VoucherDoiTuong : FullAuditedAggregateRoot<Guid>
    {
        public Guid VoucherId { get; set; }
        public LoaiDoiTuong LoaiDoiTuong { get; set; }
        public Guid DoiTuongId { get; set; }             // SanPhamId / DanhMucId / NhomId...

        public virtual Voucher Voucher { get; set; }
    }
}
