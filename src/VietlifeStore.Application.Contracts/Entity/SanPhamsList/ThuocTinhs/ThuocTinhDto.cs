using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace VietlifeStore.Entity.SanPhamsList.ThuocTinhs
{
    public class ThuocTinhDto : IEntityDto<Guid>
    {
        public Guid Id { get; set; }
        public string Ten { get; set; }
        public List<string> GiaTris { get; set; } = new();
    }
}
