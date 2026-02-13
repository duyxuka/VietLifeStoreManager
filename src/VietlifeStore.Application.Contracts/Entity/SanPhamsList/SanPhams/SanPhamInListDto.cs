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
        public decimal GiaKhuyenMai { get; set; } // Giá khuyen mãi (nếu có)
        public Guid DanhMucId { get; set; } // FK đến DanhMucSanPham
        public string Anh { get; set; } // Danh sách URL hình ảnh
        public string Slug { get; set; }
        public string MoTaNgan { get; set; } // Danh sách URL hình ảnh
        public bool TrangThai { get; set; } = true; // Trạng thái hiển thị
        public bool LaDatLich { get; set; } = false; // Sản phẩm có phải là dịch vụ đặt lịch không
        public DateTime? ThoiHanBatDau { get; set; }
        public DateTime? ThoiHanKetThuc { get; set; }
        public int? PhanTramGiamGia { get; set; }
        public int? SoLuongDaBan { get; set; }
    }
}
