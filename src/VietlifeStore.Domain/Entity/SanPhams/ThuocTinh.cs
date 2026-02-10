using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities.Auditing;

namespace VietlifeStore.Entity.SanPhams
{
    public class ThuocTinh : FullAuditedEntity<Guid>
    {
        public string Ten { get; set; }
    }
}
