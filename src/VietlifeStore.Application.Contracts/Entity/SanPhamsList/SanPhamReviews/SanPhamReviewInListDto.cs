using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace VietlifeStore.Entity.SanPhamsList.SanPhamReviews
{
    public class SanPhamReviewInListDto : EntityDto<Guid>
    {
        public Guid SanPhamId { get; set; }
        public Guid UserId { get; set; }
        public string TenNguoiDung { get; set; }
        public string Email { get; set; }
        public int SoSao { get; set; }   // 1 -> 5
        public string NoiDung { get; set; }
        public bool TrangThai { get; set; } = true;
        public DateTime CreationTime { get; set; }
        public string TenSanPham { get; set; }
    }
}
