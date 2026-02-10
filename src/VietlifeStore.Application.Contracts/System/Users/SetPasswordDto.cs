using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VietlifeStore.System.Users
{
    public class SetPasswordDto
    {
        public string NewPassword { get; set; }
        public string ConfirmNewPassword { get; set; }
    }
}
