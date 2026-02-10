using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities;

namespace VietlifeStore.Entity.SanPhams
{
    public class SanPhamBienTheThuocTinh : Entity<Guid>
    {
        public Guid SanPhamBienTheId { get; set; }
        public virtual SanPhamBienThe SanPhamBienThe { get; set; }

        public Guid GiaTriThuocTinhId { get; set; }
        public virtual GiaTriThuocTinh GiaTriThuocTinh { get; set; }
    }
}
