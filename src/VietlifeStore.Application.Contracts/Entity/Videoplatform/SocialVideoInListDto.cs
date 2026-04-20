using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace VietlifeStore.Entity.Videoplatform
{
    public class SocialVideoInListDto : EntityDto<Guid>
    {
        public string Title { get; set; }
        public string VideoId { get; set; }
        public string Section { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }
    }
}
