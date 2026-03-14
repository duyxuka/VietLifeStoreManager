using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities.Auditing;

namespace VietlifeStore.Entity.CamNangs
{
    public class CamNangComment : FullAuditedAggregateRoot<Guid>
    {
        public Guid CamNangId { get; set; }

        public string TenNguoiDung { get; set; }

        public string Email { get; set; }

        public string NoiDung { get; set; }

        public bool TrangThai { get; set; } = true;

        // reply comment
        public Guid? ParentId { get; set; }

        public virtual CamNang CamNang { get; set; }
        public virtual CamNangComment Parent { get; set; }

        public virtual ICollection<CamNangComment> Replies { get; set; }
    }
}
