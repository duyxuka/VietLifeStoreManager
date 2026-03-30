using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace VietlifeStore.Entity.SanPhamsList.SanPhams
{
    public class SanPhamInListDto : EntityDto<Guid>
    {
        public string Ten { get; set; } // Tên sản phẩm
        public decimal Gia { get; set; } // Giá gốc
        public decimal? GiaKhuyenMai { get; set; } // Giá khuyen mãi (nếu có)
        public Guid DanhMucId { get; set; } // FK đến DanhMucSanPham
        public string Anh { get; set; } // Danh sách URL hình ảnh
        public string Slug { get; set; }
        public int? ThuTu { get; set; }
        public int? LuotXem { get; set; }
        public int? LuotMua { get; set; }
        public string MoTaNgan { get; set; } // Danh sách URL hình ảnh
        public bool TrangThai { get; set; } = true; // Trạng thái hiển thị
        public bool LaDatLich { get; set; } = false; // Sản phẩm có phải là dịch vụ đặt lịch không
        public DateTime? ThoiHanBatDau { get; set; }
        public DateTime? ThoiHanKetThuc { get; set; }
        public int? PhanTramGiamGia { get; set; }
        public int? SoLuongDaBan { get; set; }
        public string? DanhMucSlug { get; set; }
        public string? QuaTangTen { get; set; }
        public decimal? QuaTangGia { get; set; }
        public bool HasVariants { get; set; }
        public decimal? GiaKhuyenMaiBienTheMin { get; set; }
        public decimal? GiaKhuyenMaiBienTheMax { get; set; }
        public decimal? GiaBienTheMin { get; set; }
        public decimal? GiaBienTheMax { get; set; }
        public int? PhanTramGiamGiaBienThe { get; set; }
    }
}
