using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VietlifeStore.ChucNang.DatLichs.Emails.EmailCampaigns
{
    public class EmailCampaignFilterDto : BaseListFilterDto
    {
        public TrangThaiChienDich? TrangThai { get; set; }
        public DateTime? TuNgay { get; set; }
        public DateTime? DenNgay { get; set; }
    }
}
