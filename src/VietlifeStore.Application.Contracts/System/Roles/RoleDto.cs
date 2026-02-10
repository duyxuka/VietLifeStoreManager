using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace VietlifeStore.System.Roles
{
    public class RoleDto : EntityDto<Guid>
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
