using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VietlifeStore.Entity.CamNangsList.CamNangs;
using Volo.Abp.Application.Dtos;

namespace VietlifeStore.Entity.CamNangsList.DanhMucCamNangs
{
    public class DanhMucCamNangDto : IEntityDto<Guid>
    {
        public Guid Id { get; set; }
        public string Ten { get; set; }
        public string Slug { get; set; }
        public bool TrangThai { get; set; } = true; // Trạng thái hiển thị
        public string TitleSEO { get; set; }
        public string Keyword { get; set; }
        public string DescriptionSEO { get; set; }
    }
}
