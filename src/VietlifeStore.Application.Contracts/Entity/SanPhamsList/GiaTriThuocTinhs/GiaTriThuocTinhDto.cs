using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace VietlifeStore.Entity.SanPhamsList.GiaTriThuocTinhs
{
    public class GiaTriThuocTinhDto : IEntityDto<Guid>
    {
        public Guid Id { get; set; }
        public Guid ThuocTinhId { get; set; }
        public string GiaTri { get; set; } // Đỏ, Xanh, S, M
    }
}
