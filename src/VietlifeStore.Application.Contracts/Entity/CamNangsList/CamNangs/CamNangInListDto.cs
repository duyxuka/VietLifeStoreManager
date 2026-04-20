using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace VietlifeStore.Entity.CamNangsList.CamNangs
{
    public class CamNangInListDto : EntityDto<Guid>
    {
        public string Ten { get; set; }
        public string Slug { get; set; }
        public string Mota { get; set; } // Nội dung HTML hoặc Markdown
        public string Anh { get; set; }
        public Guid DanhMucCamNangId { get; set; } // FK đến DanhMucCamNang
        public bool TrangThai { get; set; } = true; // Trạng thái hiển thị
        public DateTime CreationTime { get; set; }
        public string TenDanhMuc { get; set; }
        public string SlugDanhMuc { get; set; }
    }
}
