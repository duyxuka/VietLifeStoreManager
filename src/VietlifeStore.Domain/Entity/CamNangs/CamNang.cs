using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities.Auditing;

namespace VietlifeStore.Entity.CamNangs
{
    public class CamNang : FullAuditedAggregateRoot<Guid>
    {
        public string Ten { get; set; }
        public string Slug { get; set; }
        public string Mota { get; set; } // Nội dung HTML hoặc Markdown
        public string? Anh { get; set; }
        public Guid DanhMucCamNangId { get; set; } // FK đến DanhMucCamNang
        public virtual DanhMucCamNang DanhMucCamNang { get; set; }
        public bool TrangThai { get; set; } = true; // Trạng thái hiển thị
        public string TitleSEO { get; set; }
        public string Keyword { get; set; }
        public string DescriptionSEO { get; set; }
    }
}
