using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace VietlifeStore.Entity.CamNangsList.DanhMucCamNangs
{
    public class DanhMucCamNangInListDto : EntityDto<Guid>
    {
        public string Ten { get; set; }
        public string Slug { get; set; }
        public bool TrangThai { get; set; } = true; // Trạng thái hiển thị
    }
}
