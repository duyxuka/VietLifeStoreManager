using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VietlifeStore.Entity.SanPhamsList.SanPhamReviews
{
    public class CreateUpdateSanPhamReviewDto
    {
        public Guid SanPhamId { get; set; }

        public int SoSao { get; set; }

        public string NoiDung { get; set; }
        public Guid? UserId { get; set; }

        public string? TenNguoiDung { get; set; }

        public string? Email { get; set; }
    }
}
