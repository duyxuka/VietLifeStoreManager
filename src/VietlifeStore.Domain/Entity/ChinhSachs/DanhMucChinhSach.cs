using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities.Auditing;

namespace VietlifeStore.Entity.ChinhSachs
{
    public class DanhMucChinhSach : FullAuditedAggregateRoot<Guid>
    {
        public string Ten { get; set; }
        public string Slug { get; set; }
        public bool TrangThai { get; set; } = true;
        public string TitleSEO { get; set; }
        public string Keyword { get; set; }
        public string DescriptionSEO { get; set; }
        public virtual ICollection<ChinhSach> ChinhSachs { get; set; }
    }
}
