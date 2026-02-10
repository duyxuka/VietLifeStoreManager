using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities.Auditing;

namespace VietlifeStore.Entity.Banners
{
    public class Banner : FullAuditedAggregateRoot<Guid>
    {
        public string TieuDe { get; set; }
        public string MoTa { get; set; }
        public string? Anh { get; set; } // URL ảnh
        public string LienKet { get; set; } // URL link
        public bool TrangThai { get; set; } = true;
    }
}
