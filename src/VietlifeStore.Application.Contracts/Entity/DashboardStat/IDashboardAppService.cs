using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VietlifeStore.Entity.DashboardStat
{
    public interface IDashboardAppService
    {
        Task<DashboardStatsDto> GetStatsAsync();
    }
}
