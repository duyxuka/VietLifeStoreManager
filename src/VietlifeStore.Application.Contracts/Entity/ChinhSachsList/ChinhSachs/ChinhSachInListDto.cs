using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace VietlifeStore.Entity.ChinhSachsList.ChinhSachs
{
    public class ChinhSachInListDto : EntityDto<Guid>
    {
        public string TieuDe { get; set; }
        public string NoiDung { get; set; }
        public bool TrangThai { get; set; } = true;
        public Guid DanhMucChinhSachId { get; set; } // FK đến DanhMucChinhSach
    }
}
