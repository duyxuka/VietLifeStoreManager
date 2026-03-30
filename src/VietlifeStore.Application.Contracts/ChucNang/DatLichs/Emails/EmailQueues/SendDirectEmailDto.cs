using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VietlifeStore.ChucNang.DatLichs.Emails.EmailQueues
{
    public class SendDirectEmailDto
    {
        public string Email { get; set; }
        public string TieuDe { get; set; }
        public string NoiDungHtml { get; set; }
    }
}
