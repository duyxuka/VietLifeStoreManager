using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace VietlifeStore.ChucNang.DatLichs.Emails.EmailTemplates
{
    public class EmailTemplateInListDto : EntityDto<Guid>
    {
        public string TenTemplate { get; set; }
        public string TieuDe { get; set; }
        public string MoTa { get; set; }
        public bool TrangThai { get; set; }
        public DateTime CreationTime { get; set; }
    }
}
