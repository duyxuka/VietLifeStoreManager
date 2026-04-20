using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace VietlifeStore.Entity.SanPhamsList.DanhMucSanPhams
{
    public class DanhMucSanPhamInListDto : EntityDto<Guid>
    {
        public string Ten { get; set; } // Tên danh mục
        public string Slug { get; set; }
        public string? AnhThumbnail { get; set; } // Ảnh 1 (URL)
        public string? AnhBanner { get; set; } // Ảnh 2 (URL)
        public bool TrangThai { get; set; } = true;
        public int SoLuongSanPham { get; set; }
        public int? ThuTu { get; set; }
        public string TitleSEO { get; set; }
        public string Keyword { get; set; }
        public string DescriptionSEO { get; set; }
    }
}
