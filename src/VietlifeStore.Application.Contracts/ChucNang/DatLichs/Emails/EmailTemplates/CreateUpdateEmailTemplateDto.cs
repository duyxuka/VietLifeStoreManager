using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VietlifeStore.ChucNang.DatLichs.Emails.EmailTemplates
{
    public class CreateUpdateEmailTemplateDto
    {
        public string TenTemplate { get; set; }
        public string TieuDe { get; set; }
        public string NoiDungHtml { get; set; }

        public string MoTa { get; set; }

        public bool TrangThai { get; set; }
    }
}
