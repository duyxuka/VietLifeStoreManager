using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities.Auditing;

namespace VietlifeStore.ChucNang.DatLichs.Emails
{
    public class EmailUnsubscribe : FullAuditedAggregateRoot<Guid>
    {
        public string Email { get; set; } = string.Empty;

        public string? LyDo { get; set; }

        public DateTime NgayHuy { get; set; }

        public string? DiaChiIP { get; set; }
    }
}
