using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace VietlifeStore.Entity.SanPhamsList.SanPhamBienTheThuocTinhs
{
    public class SanPhamBienTheThuocTinhDto : IEntityDto<Guid>
    {
        public Guid Id { get; set; }
        public Guid SanPhamBienTheId { get; set; }

        public Guid GiaTriThuocTinhId { get; set; }
    }
}
