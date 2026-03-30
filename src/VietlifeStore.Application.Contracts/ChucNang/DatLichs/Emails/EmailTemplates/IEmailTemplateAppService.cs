using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace VietlifeStore.ChucNang.DatLichs.Emails.EmailTemplates
{
    public interface IEmailTemplateAppService : ICrudAppService<
        EmailTemplateDto,
        Guid,
        PagedResultRequestDto,
        CreateUpdateEmailTemplateDto,
        CreateUpdateEmailTemplateDto>
    {
        Task<PagedResultDto<EmailTemplateInListDto>> GetListFilterAsync(BaseListFilterDto input);
        Task<List<EmailTemplateInListDto>> GetListAllActiveAsync();
        Task DeleteMultipleAsync(IEnumerable<Guid> ids);
    }
}
