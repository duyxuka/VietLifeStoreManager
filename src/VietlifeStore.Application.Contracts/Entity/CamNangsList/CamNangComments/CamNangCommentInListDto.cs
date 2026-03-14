using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace VietlifeStore.Entity.CamNangsList.CamNangComments
{
    public class CamNangCommentInListDto : EntityDto<Guid>
    {
        public Guid CamNangId { get; set; }

        public string TenNguoiDung { get; set; }

        public string Email { get; set; }

        public string NoiDung { get; set; }

        public Guid? ParentId { get; set; }

        public bool TrangThai { get; set; }
        public string CamNangTen { get; set; }

        public DateTime CreationTime { get; set; }
    }
}
