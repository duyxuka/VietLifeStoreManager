using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VietlifeStore.Entity.DonHangs;
using Volo.Abp.Identity;

namespace VietlifeStore.Entity.TaiKhoans
{
    public class TaiKhoan : IdentityUser
    {
        public bool IsCustomer { get; set; } = true;
        public bool Status { get; set; } = true;
        public virtual ICollection<DonHang> DonHangs { get; set; }
        protected TaiKhoan()
        {
            // Required by EF Core
        }

        public TaiKhoan(
            Guid id,
            string userName,
            string email,
            Guid? tenantId
        ) : base(id, userName, email, tenantId)
        {
        }
    }
}
