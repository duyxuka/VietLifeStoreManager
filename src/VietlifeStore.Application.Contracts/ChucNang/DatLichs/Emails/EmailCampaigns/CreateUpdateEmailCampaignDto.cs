using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VietlifeStore.ChucNang.DatLichs.Emails.EmailCampaigns
{
    public class CreateUpdateEmailCampaignDto
    {
        public string TenCampaign { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public Guid TemplateId { get; set; }
        public DateTime? NgayGui { get; set; }
        public List<string> DanhSachEmail { get; set; } = new();
    }
}
