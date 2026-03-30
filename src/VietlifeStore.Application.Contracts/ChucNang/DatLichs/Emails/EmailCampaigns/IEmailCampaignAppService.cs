using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VietlifeStore.ChucNang.DatLichs.Emails.EmailQueues;
using VietlifeStore.System.Users;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace VietlifeStore.ChucNang.DatLichs.Emails.EmailCampaigns
{
    public interface IEmailCampaignAppService : ICrudAppService<
        EmailCampaignDto,
        Guid,
        PagedResultRequestDto,
        CreateUpdateEmailCampaignDto,
        CreateUpdateEmailCampaignDto>
    {
        Task<PagedResultDto<EmailCampaignInListDto>> GetListFilterAsync(EmailCampaignFilterDto input);
        Task DeleteMultipleAsync(IEnumerable<Guid> ids);
        Task SendNowAsync(Guid id);
        Task ScheduleAsync(Guid id, DateTime ngayGui);
        Task PauseAsync(Guid id);
        Task SendDirectAsync(SendDirectEmailDto input);
        Task<PagedResultDto<EmailQueueDto>> GetQueueAsync(Guid campaignId, PagedResultRequestDto input);
        Task<List<EmailUserDto>> GetListUserAsync();
    }
}
