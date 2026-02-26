using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VietlifeStore.Entity.SanPhamsList.SanPhamBienThes;
using VietlifeStore.Entity.SanPhamsList.ThuocTinhs;
using Volo.Abp.Application.Dtos;

namespace VietlifeStore.Entity.SanPhamsList.SanPhams
{
    public class SanPhamDto : IEntityDto<Guid>
    {
        public Guid Id { get; set; }
        public string Ten { get; set; } // Tên sản phẩm
        public string Slug { get; set; }
        public string MoTaNgan { get; set; } // Mô tả ngắn
        public string MoTa { get; set; } // Mô tả chi tiết
        public string HuongDanSuDung { get; set; } // Mô tả chi tiết
        public string ThongSoKyThuat { get; set; } // Mô tả chi tiết
        public decimal Gia { get; set; } // Giá gốc
        public decimal GiaKhuyenMai { get; set; } // Giá khuyen mãi (nếu có)
        public Guid DanhMucId { get; set; } // FK đến DanhMucSanPham
        public string DanhMucSlug { get; set; }
        public string? QuaTangTen { get; set; }
        public decimal? QuaTangGia { get; set; }
        public Guid QuaTangId { get; set; } // FK đến QuaTang
        public string Anh { get; set; } // Danh sách URL hình ảnh
        public int ThuTu { get; set; }
        public int LuotXem { get; set; }
        public int LuotMua { get; set; }
        public string TitleSEO { get; set; }
        public string Keyword { get; set; }
        public string DescriptionSEO { get; set; }
        public bool TrangThai { get; set; } = true; // Trạng thái hiển thị
        public string JobId { get; set; }
        public bool LaDatLich { get; set; } = false; // Sản phẩm có phải là dịch vụ đặt lịch không
        public DateTime? ThoiHanBatDau { get; set; }
        public DateTime? ThoiHanKetThuc { get; set; }
        public int? PhanTramGiamGia { get; set; }
        public List<ThuocTinhDto> ThuocTinhs { get; set; } = new();
        public List<string> AnhPhu { get; set; } = new();
        public List<SanPhamBienTheDto> BienThes { get; set; } = new();
    }
}
