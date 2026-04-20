using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace VietlifeStore.Entity.SanPhamsList.AnhSanPhams
{
    public class AnhSanPhamInListDto : EntityDto<Guid>
    {
        public string Anh { get; set; }
        public bool Status { get; set; } = true;
        public Guid SanPhamId { get; set; }
        public int? ThuTu { get; set; }
    }
}
