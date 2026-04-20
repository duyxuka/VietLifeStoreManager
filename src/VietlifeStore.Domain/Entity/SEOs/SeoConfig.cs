using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities.Auditing;

namespace VietlifeStore.Entity.SEOs
{
    public class SeoConfig : FullAuditedAggregateRoot<Guid>
    {
        /// <summary>
        /// Khóa duy nhất để xác định trang tĩnh
        /// Ví dụ: "Home", "ProductList", "GuideList", "About", "Contact", ...
        /// </summary>
        public string PageKey { get; set; } = string.Empty;

        /// <summary>
        /// SEO Title (title tag)
        /// </summary>
        public string SeoTitle { get; set; } = string.Empty;

        /// <summary>
        /// SEO Keywords (có thể là chuỗi cách nhau dấu phẩy)
        /// </summary>
        public string? SeoKeywords { get; set; }

        /// <summary>
        /// SEO Meta Description
        /// </summary>
        public string? SeoDescription { get; set; }

        // Các trường mở rộng rất hay dùng cho SEO hiện đại
        public string? OgTitle { get; set; }
        public string? OgDescription { get; set; }
        public string? OgImageUrl { get; set; }
        public string? CanonicalUrl { get; set; }
        public string? Robots { get; set; } = "index, follow"; // index, noindex, follow, nofollow
    }
}
