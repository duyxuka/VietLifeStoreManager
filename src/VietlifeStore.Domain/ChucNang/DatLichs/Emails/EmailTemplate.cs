using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities.Auditing;

namespace VietlifeStore.ChucNang.DatLichs.Emails
{
    public class EmailTemplate : FullAuditedAggregateRoot<Guid>
    {
        public string TenTemplate { get; set; } = string.Empty;

        public string TieuDe { get; set; } = string.Empty;

        public string NoiDungHtml { get; set; } = string.Empty;

        public string MoTa { get; set; } = string.Empty;

        public bool TrangThai { get; set; }

        public ICollection<EmailCampaign> Campaigns { get; set; }
    }
}
