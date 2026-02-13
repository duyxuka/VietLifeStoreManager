using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VietlifeStore.Entity.SanPhamsList.SanPhamBienThes;
using VietlifeStore.Entity.SanPhamsList.ThuocTinhs;

namespace VietlifeStore.Entity.SanPhamsList.SanPhams
{
    public class CreateUpdateSanPhamDto
    {
        public string Ten { get; set; } // Tên sản phẩm
        public string? Slug { get; set; }
        public string MoTaNgan { get; set; } // Mô tả ngắn
        public string MoTa { get; set; } // Mô tả chi tiết
        public string HuongDanSuDung { get; set; } // Mô tả chi tiết
        public string ThongSoKyThuat { get; set; } // Mô tả chi tiết
        public decimal Gia { get; set; } // Giá gốc
        public decimal GiaKhuyenMai { get; set; } // Giá khuyen mãi (nếu có)
        public Guid DanhMucId { get; set; } // FK đến DanhMucSanPham
        public Guid QuaTangId { get; set; } // FK đến QuaTang
        public string? Anh { get; set; } // Danh sách URL hình ảnh
        public int? ThuTu { get; set; }
        public int? LuotXem { get; set; }
        public int? LuotMua { get; set; }
        public string TitleSEO { get; set; }
        public string Keyword { get; set; }
        public string DescriptionSEO { get; set; }
        public bool TrangThai { get; set; } = true; // Trạng thái hiển thị
        public string? JobId { get; set; }
        public bool LaDatLich { get; set; } = false; // Sản phẩm có phải là dịch vụ đặt lịch không
        public DateTime? ThoiHanBatDau { get; set; }
        public DateTime? ThoiHanKetThuc { get; set; }

        // Thuộc tính kiểu Shopee
        public List<CreateUpdateThuocTinhWithGiaTriDto> ThuocTinhs { get; set; } = new();

        // Biến thể được sinh ra (FE có thể sửa giá từng dòng)
        public List<CreateUpdateSanPhamBienTheDto> BienThes { get; set; } = new();

        // Ảnh
        public List<string> AnhPhu { get; set; } = new();
        public List<string> AnhPhuGiuLai { get; set; } = new List<string>();
        public decimal? PhanTramKhuyenMai { get; set; }
    }
}
