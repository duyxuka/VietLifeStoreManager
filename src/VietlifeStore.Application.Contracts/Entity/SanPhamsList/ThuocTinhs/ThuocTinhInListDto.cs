using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace VietlifeStore.Entity.SanPhamsList.ThuocTinhs
{
    public class ThuocTinhInListDto : EntityDto<Guid>
    {
        public string Ten { get; set; }
    }
}
