using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VietlifeStore.Entity.ChinhSachsList.ChinhSachs
{
    public class CreateUpdateChinhSachDto
    {
        public string TieuDe { get; set; }
        public string NoiDung { get; set; }
        public bool TrangThai { get; set; } = true;
        public Guid DanhMucChinhSachId { get; set; } // FK đến DanhMucChinhSach
    }
}
