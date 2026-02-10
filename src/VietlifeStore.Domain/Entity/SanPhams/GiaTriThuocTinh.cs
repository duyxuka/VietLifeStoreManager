using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities;

namespace VietlifeStore.Entity.SanPhams
{
    public class GiaTriThuocTinh : Entity<Guid>
    {
        public Guid ThuocTinhId { get; set; }
        public virtual ThuocTinh ThuocTinh { get; set; }

        public string GiaTri { get; set; } // Đỏ, Xanh, S, M
    }
}
