using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VietlifeStore.Entity.Banners
{
    public class CreateUpdateBannerDto
    {
        public string TieuDe { get; set; }
        public string MoTa { get; set; }
        public string? Anh { get; set; } // URL ảnh
        public string? AnhMobile { get; set; } // URL ảnh
        public string? LienKet { get; set; } // URL link
        public bool TrangThai { get; set; } = true;
    }
}
