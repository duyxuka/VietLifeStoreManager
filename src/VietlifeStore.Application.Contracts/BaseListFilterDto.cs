using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace VietlifeStore
{
    public class BaseListFilterDto : PagedResultRequestDto
    {
        public string? Keyword { get; set; }
    }
}
