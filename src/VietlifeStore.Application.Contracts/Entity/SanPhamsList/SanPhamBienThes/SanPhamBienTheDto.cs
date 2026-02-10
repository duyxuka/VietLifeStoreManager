using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VietlifeStore.Entity.SanPhamsList.SanPhamBienTheThuocTinhs;
using Volo.Abp.Application.Dtos;

namespace VietlifeStore.Entity.SanPhamsList.SanPhamBienThes
{
    public class SanPhamBienTheDto : IEntityDto<Guid>
    {
        public Guid Id { get; set; }
        public Guid SanPhamId { get; set; }
        public string Ten { get; set; }
        public decimal Gia { get; set; }
        public decimal? GiaKhuyenMai { get; set; }

        public List<SanPhamBienTheThuocTinhDto> SanPhamBienTheThuocTinhDtos { get; set; }
    }
}
