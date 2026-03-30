using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace VietlifeStore.ChucNang.DatLichs.Emails.EmailCampaigns
{
    public class EmailCampaignDto : IEntityDto<Guid>
    {
        public Guid Id { get; set; }
        public string TenCampaign { get; set; }
        public string Subject { get; set; }
        public Guid TemplateId { get; set; }
        public string TenTemplate { get; set; }
        public DateTime? NgayGui { get; set; }
        public TrangThaiChienDich TrangThai { get; set; }
        public int TongSoEmail { get; set; }
        public int SoDaGui { get; set; }
        public int SoMo { get; set; }
        public int SoClick { get; set; }
        public string? HangfireJobId { get; set; }
    }
}
