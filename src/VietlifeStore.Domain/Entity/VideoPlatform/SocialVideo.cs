using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities.Auditing;

namespace VietlifeStore.Entity.VideoPlatform
{
    public class SocialVideo : FullAuditedAggregateRoot<Guid>
    {
        public string Title { get; set; }
        public string Description { get; set; }

        public string Platform { get; set; } // TikTok
        public string VideoId { get; set; }
        public string VideoUrl { get; set; }
        public string ThumbnailUrl { get; set; }

        public string Section { get; set; }

        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }
    }
}
