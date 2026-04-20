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
        public string VideoId { get; set; }
        public string Section { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }
    }
}
