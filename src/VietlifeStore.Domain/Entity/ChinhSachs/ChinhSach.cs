using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities.Auditing;

namespace VietlifeStore.Entity.ChinhSachs
{
    public class ChinhSach : FullAuditedAggregateRoot<Guid>
    {
        public string TieuDe { get; set; }
        public string NoiDung { get; set; }
        public bool TrangThai { get; set; } = true;
        public Guid DanhMucChinhSachId { get; set; } // FK đến DanhMucChinhSach
        public virtual DanhMucChinhSach DanhMucChinhSach { get; set; }
    }
}
