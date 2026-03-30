using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities.Auditing;

namespace VietlifeStore.ChucNang.DatLichs.Emails
{
    public class EmailCampaign : FullAuditedAggregateRoot<Guid>
    {
        public string TenCampaign { get; set; } = string.Empty;

        public string Subject { get; set; } = string.Empty;

        public Guid TemplateId { get; set; }

        public DateTime? NgayGui { get; set; }

        public TrangThaiChienDich TrangThai { get; set; }

        public int TongSoEmail { get; set; }

        public int SoDaGui { get; set; }

        public int SoMo { get; set; }

        public int SoClick { get; set; }
        public string? SendJobId { get; set; }

        public EmailTemplate Template { get; set; }

        public ICollection<EmailQueue> Queues { get; set; }
    }
}
