using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace VietlifeStore.Entity.SanPhamsList.SanPhamBienThes
{
    public class SanPhamBienTheInListDto : EntityDto<Guid>
    {
        public Guid SanPhamId { get; set; }
        public decimal Gia { get; set; }
        public decimal? GiaKhuyenMai { get; set; }
    }
}
