using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace VietlifeStore.ChucNang.DatLichs.Emails.EmailCampaigns
{
    public class EmailCampaignInListDto : EntityDto<Guid>
    {
        public string TenCampaign { get; set; }
        public string Subject { get; set; }
        public string TenTemplate { get; set; }
        public DateTime? NgayGui { get; set; }
        public TrangThaiChienDich TrangThai { get; set; }
        public int TongSoEmail { get; set; }
        public int SoDaGui { get; set; }
        public int SoMo { get; set; }
        public DateTime CreationTime { get; set; }
    }
}
