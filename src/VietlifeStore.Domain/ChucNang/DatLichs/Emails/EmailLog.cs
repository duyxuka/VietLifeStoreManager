using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities.Auditing;

namespace VietlifeStore.ChucNang.DatLichs.Emails
{
    public class EmailLog : FullAuditedAggregateRoot<Guid>
    {
        public Guid QueueId { get; set; }

        public string Email { get; set; } = string.Empty;

        public TrangThaiEmail TrangThai { get; set; }

        public string? ThongBaoLoi { get; set; }

        public DateTime ThoiGianGui { get; set; }

        public EmailQueue Queue { get; set; }

        public ICollection<EmailOpenTracking> Opens { get; set; }
        public bool DaMo { get; set; } = false;
        public DateTime? ThoiGianMo { get; set; }
    }
}
