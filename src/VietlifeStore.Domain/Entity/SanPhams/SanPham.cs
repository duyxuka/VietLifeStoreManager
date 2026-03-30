using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VietlifeStore.Entity.DonHangs;
using Volo.Abp.Domain.Entities.Auditing;

namespace VietlifeStore.Entity.SanPhams
{
    public class SanPham : FullAuditedAggregateRoot<Guid>
    {
        public string Ten { get; set; } // Tên sản phẩm
        public string Slug { get; set; }
        public string MoTaNgan { get; set; } // Mô tả ngắn
        public string MoTa { get; set; } // Mô tả chi tiết
        public string HuongDanSuDung { get; set; } // Mô tả chi tiết
        public string ThongSoKyThuat { get; set; } // Mô tả chi tiết
        public decimal Gia { get; set; } // Giá gốc
        public decimal GiaKhuyenMai { get; set; } // Giá khuyen mãi (nếu có)
        public Guid DanhMucId { get; set; } // FK đến DanhMucSanPham
        public virtual DanhMucSanPham DanhMucSanPham { get; set; }
        public Guid? QuaTangId { get; set; } // FK đến QuaTang
        public virtual QuaTang? QuaTang { get; set; }
        public string? Anh { get; set; } // Danh sách URL hình ảnh
        public int? ThuTu { get; set; }
        public int? LuotXem { get; set; }
        public int? LuotMua { get; set; }
        public string TitleSEO { get; set; }
        public string Keyword { get; set; }
        public string DescriptionSEO { get; set; }
        public bool TrangThai { get; set; } = true; // Trạng thái hiển thị

        // Variants: Liên kết với các tùy chọn và biến thể
        public virtual ICollection<SanPhamBienThe> SanPhamBienThes { get; set; } = new List<SanPhamBienThe>();
        public virtual ICollection<AnhSanPham> AnhSanPham { get; set; } = new List<AnhSanPham>();
        public virtual ICollection<SanPhamReview> Reviews { get; set; } = new List<SanPhamReview>();
    }
}
