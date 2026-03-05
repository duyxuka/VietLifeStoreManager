using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VietlifeStore.Entity.Videoplatform
{
    public class CreateUpdateSocialVideoDto
    {
        public string Title { get; set; }
        public string Description { get; set; }

        public string Platform { get; set; }
        public string VideoId { get; set; }
        public string VideoUrl { get; set; }
        public string ThumbnailUrl { get; set; }

        public string Section { get; set; }

        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }
    }
}
