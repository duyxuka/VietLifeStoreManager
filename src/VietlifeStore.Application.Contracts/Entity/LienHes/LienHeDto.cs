using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace VietlifeStore.Entity.LienHes
{
    public class LienHeDto : IEntityDto<Guid>
    {
        public Guid Id { get; set; }
        public string HoTen { get; set; }
        public string Email { get; set; }
        public string SoDienThoai { get; set; }
        public string NoiDung { get; set; }
        public bool DaXuLy { get; set; } = false; // Trạng thái xử lý
    }
}
