using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities.Auditing;

namespace VietlifeStore.Entity.SanPhams
{
    public class SanPhamReview : FullAuditedAggregateRoot<Guid>
    {
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
