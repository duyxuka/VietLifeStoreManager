using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace VietlifeStore.Entity.ChinhSachsList.DanhMucChinhSachs
{
    public class DanhMucChinhSachDto : IEntityDto<Guid>
    {
        public Guid Id { get; set; }
        public string Ten { get; set; }
        public string Slug { get; set; }
        public bool TrangThai { get; set; } = true;
        public string TitleSEO { get; set; }
        public string Keyword { get; set; }
        public string DescriptionSEO { get; set; }
    }
}
