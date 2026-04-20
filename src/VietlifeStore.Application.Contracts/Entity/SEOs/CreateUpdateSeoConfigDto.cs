using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VietlifeStore.Entity.SEOs
{
    public class CreateUpdateSeoConfigDto
    {
        public string PageKey { get; set; } = string.Empty;
        public string SeoTitle { get; set; } = string.Empty;
        public string? SeoKeywords { get; set; }
        public string? SeoDescription { get; set; }

        // Open Graph
        public string? OgTitle { get; set; }
        public string? OgDescription { get; set; }
        public string? OgImageUrl { get; set; }

        public string? CanonicalUrl { get; set; }
        public string? Robots { get; set; }
    }
}
