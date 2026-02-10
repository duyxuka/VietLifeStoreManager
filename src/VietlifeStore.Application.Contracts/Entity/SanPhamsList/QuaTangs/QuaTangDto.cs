using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace VietlifeStore.Entity.SanPhamsList.QuaTangs
{
    public class QuaTangDto : IEntityDto<Guid>
    {
        public Guid Id { get; set; }
        public string Ten { get; set; }
        public decimal Gia { get; set; }
        public bool TrangThai { get; set; } = true;
    }
}
