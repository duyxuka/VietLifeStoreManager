using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace VietlifeStore.ChucNang.DatLichs.Emails.EmailQueues
{
    public class EmailQueueDto : EntityDto<Guid>
    {
        public Guid CampaignId { get; set; }
        public Guid? UserId { get; set; }
        public string Email { get; set; }
        public string TieuDe { get; set; }
        public TrangThaiEmail TrangThai { get; set; }
        public int SoLanThu { get; set; }
        public DateTime? ThoiGianGui { get; set; }
    }
}
