using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VietlifeStore.Entity.SanPhams;
using Volo.Abp.Application.Dtos;

namespace VietlifeStore.Entity.SanPhamsList.SanPhamReviews
{
    public class SanPhamReviewDto : IEntityDto<Guid>
    {
        public Guid Id { get; set; }
        public Guid SanPhamId { get; set; }

        public Guid UserId { get; set; }

        public string TenNguoiDung { get; set; }

        public string Email { get; set; }

        public int SoSao { get; set; }   // 1 -> 5

        public string NoiDung { get; set; }

        public bool TrangThai { get; set; } = true;

        public virtual SanPham SanPham { get; set; }
    }
}
