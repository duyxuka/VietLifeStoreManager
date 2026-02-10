using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VietlifeStore.Entity.SanPhamsList.AnhSanPhams
{
    public class CreateUpdateAnhSanPhamDto
    {
        public string Anh { get; set; }
        public bool Status { get; set; } = true;
        public Guid SanPhamId { get; set; }
        public string AnhName { get; set; }

        public string AnhContent { get; set; } // base64
    }
}
