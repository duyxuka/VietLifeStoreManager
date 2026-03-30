using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities.Auditing;

namespace VietlifeStore.ChucNang.DatLichs.Emails
{
    public class EmailOpenTracking : FullAuditedAggregateRoot<Guid>
    {
        public Guid EmailLogId { get; set; }

        public DateTime ThoiGianMo { get; set; }

        public string? DiaChiIP { get; set; }

        public string? UserAgent { get; set; }

        public EmailLog EmailLog { get; set; }
    }
}
