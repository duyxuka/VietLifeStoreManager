using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities.Auditing;

namespace VietlifeStore.Entity.DonHangs
{
    public class VoucherDaSuDung : FullAuditedAggregateRoot<Guid>
    {
        public Guid VoucherId { get; set; }
        public Guid UserId { get; set; }
        public Guid DonHangId { get; set; }

        public DateTime NgaySuDung { get; set; } = DateTime.Now;
    }
}
