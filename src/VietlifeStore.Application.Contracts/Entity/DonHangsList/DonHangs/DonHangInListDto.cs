using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VietlifeStore.Entity.DonHangsList.ChiTietDonHangs;
using Volo.Abp.Application.Dtos;

namespace VietlifeStore.Entity.DonHangsList.DonHangs
{
    public class DonHangInListDto : EntityDto<Guid>
    {
        public string Ten { get; set; }
        public string Email { get; set; }
        public string SoDienThoai { get; set; }
        public string PhuongThucThanhToan { get; set; }
        public decimal TongTien { get; set; }
        public byte TrangThai { get; set; } // Ví dụ: "Chờ xác nhận", "Đang giao", "Hoàn thành"
        public DateTime NgayDat { get; set; } = DateTime.Now;
        public List<ChiTietDonHangDto> ChiTietDonHangDtos { get; set; }
    }
}
