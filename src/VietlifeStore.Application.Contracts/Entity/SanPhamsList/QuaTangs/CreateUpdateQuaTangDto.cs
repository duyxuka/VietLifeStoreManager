using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VietlifeStore.Entity.SanPhamsList.QuaTangs
{
    public class CreateUpdateQuaTangDto
    {
        public string Ten { get; set; }
        public decimal Gia { get; set; }
        public bool TrangThai { get; set; } = true;
    }
}
