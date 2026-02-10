using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VietlifeStore.Entity.SanPhamsList.DanhMucSanPhams
{
    public class CreateUpdateDanhMucSanPhamDto
    {
        public string Ten { get; set; } // Tên danh mục
        public string Slug { get; set; }
        public string? AnhThumbnail { get; set; } // Ảnh 1 (URL)
        public string? AnhBanner { get; set; } // Ảnh 2 (URL)
        public bool TrangThai { get; set; } = true;
        public string TitleSEO { get; set; }
        public string Keyword { get; set; }
        public string DescriptionSEO { get; set; }

        public string? AnhThumbnailName { get; set; }
        public string? AnhThumbnailContent { get; set; }

        public string? AnhBannerName { get; set; }
        public string? AnhBannerContent { get; set; }
    }
}
