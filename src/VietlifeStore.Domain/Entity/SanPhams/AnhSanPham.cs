using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities.Auditing;

namespace VietlifeStore.Entity.SanPhams
{
    public class AnhSanPham : FullAuditedAggregateRoot<Guid>
    {
        public string? Anh { get; set; }
        public bool Status { get; set; } = true;
        public Guid SanPhamId { get; set; }
        public virtual SanPham SanPham { get; set; }
    }
}
