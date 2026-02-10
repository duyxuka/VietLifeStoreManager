using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VietlifeStore.Entity.SanPhamsList.SanPhamBienTheThuocTinhs
{
    public class CreateUpdateSanPhamBienTheThuocTinhDto
    {
        public Guid SanPhamBienTheId { get; set; }

        public Guid GiaTriThuocTinhId { get; set; }
    }
}
