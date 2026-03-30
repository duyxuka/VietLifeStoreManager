using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities.Auditing;

namespace VietlifeStore.ChucNang.DatLichs.Emails
{
    public class EmailQueue : FullAuditedAggregateRoot<Guid>
    {
        public Guid CampaignId { get; set; }

        public Guid? UserId { get; set; }

        public string Email { get; set; } = string.Empty;
        public string TenKhachHang { get; set; } = string.Empty;

        public string TieuDe { get; set; } = string.Empty;

        public string NoiDung { get; set; } = string.Empty;

        public TrangThaiEmail TrangThai { get; set; }

        public int SoLanThu { get; set; }

        public DateTime? ThoiGianGui { get; set; }

        public EmailCampaign Campaign { get; set; }

        public EmailLog? Log { get; set; }
        public bool DaMo { get; set; } = false;
        public DateTime? ThoiGianMo { get; set; }
    }
}
