using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VietlifeStore.Entity.CamNangsList.CamNangComments
{
    public class CreateUpdateCamNangCommentDto
    {
        public Guid CamNangId { get; set; }
        public string TenNguoiDung { get; set; }

        public string Email { get; set; }

        public string NoiDung { get; set; }

        public Guid? ParentId { get; set; }
    }
}
