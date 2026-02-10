using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities.Auditing;

namespace VietlifeStore.Entity.SanPhams
{
    public class QuaTang : FullAuditedAggregateRoot<Guid>
    {
        public string Ten { get; set; }
        public decimal Gia { get; set; }
        public bool TrangThai { get; set; } = true;
        public ICollection<SanPham> SanPhams { get; set; }
    }
}
