using AutoMapper.Internal.Mappers;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VietlifeStore.ChucNang.DatLichs.Emails.EmailTemplates;
using VietlifeStore.ChucNang.DatLichs.Emails;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.ObjectMapping;
using Volo.Abp;
using VietlifeStore.Permissions;

namespace VietlifeStore.ChucNang.Mails
{
    public class EmailTemplateAppService :
        CrudAppService<
            EmailTemplate,
            EmailTemplateDto,
            Guid,
            PagedResultRequestDto,
            CreateUpdateEmailTemplateDto,
            CreateUpdateEmailTemplateDto>,
        IEmailTemplateAppService
    {
        public EmailTemplateAppService(IRepository<EmailTemplate, Guid> repository)
            : base(repository)
        {
            GetPolicyName = VietlifeStorePermissions.EmailTemplate.Default;
            GetListPolicyName = VietlifeStorePermissions.EmailTemplate.View;
            CreatePolicyName = VietlifeStorePermissions.EmailTemplate.Create;
            UpdatePolicyName = VietlifeStorePermissions.EmailTemplate.Update;
            DeletePolicyName = VietlifeStorePermissions.EmailTemplate.Delete;
        }

        // ================= CREATE =================
        [Authorize(VietlifeStorePermissions.EmailTemplate.Create)]
        public override async Task<EmailTemplateDto> CreateAsync(CreateUpdateEmailTemplateDto input)
        {
            if (string.IsNullOrWhiteSpace(input.NoiDungHtml))
                throw new UserFriendlyException("Nội dung HTML không được để trống.");

            var entity = new EmailTemplate
            {
                TenTemplate = input.TenTemplate,
                TieuDe = input.TieuDe,
                NoiDungHtml = input.NoiDungHtml,
                MoTa = input.MoTa,
                TrangThai = input.TrangThai
            };

            await Repository.InsertAsync(entity, autoSave: true);
            return MapToGetOutputDto(entity);
        }

        // ================= UPDATE =================
        [Authorize(VietlifeStorePermissions.EmailTemplate.Update)]
        public override async Task<EmailTemplateDto> UpdateAsync(Guid id, CreateUpdateEmailTemplateDto input)
        {
            var entity = await Repository.GetAsync(id);

            entity.TenTemplate = input.TenTemplate;
            entity.TieuDe = input.TieuDe;
            entity.NoiDungHtml = input.NoiDungHtml;
            entity.MoTa = input.MoTa;
            entity.TrangThai = input.TrangThai;

            await Repository.UpdateAsync(entity, autoSave: true);
            return MapToGetOutputDto(entity);
        }

        // ================= DELETE SINGLE =================
        [Authorize(VietlifeStorePermissions.EmailTemplate.Delete)]
        public override async Task DeleteAsync(Guid id)
        {
            await base.DeleteAsync(id);
        }

        // ================= DELETE MULTIPLE =================
        [Authorize(VietlifeStorePermissions.EmailTemplate.Delete)]
        public async Task DeleteMultipleAsync(IEnumerable<Guid> ids)
        {
            var list = await Repository.GetListAsync(x => ids.Contains(x.Id));
            await Repository.DeleteManyAsync(list);
        }

        // ================= GET ALL ACTIVE =================
        [Authorize(VietlifeStorePermissions.EmailTemplate.View)]
        public async Task<List<EmailTemplateInListDto>> GetListAllActiveAsync()
        {
            var list = await AsyncExecuter.ToListAsync(
                (await Repository.GetQueryableAsync())
                    .Where(x => x.TrangThai)
                    .OrderByDescending(x => x.CreationTime)
            );
            return ObjectMapper.Map<List<EmailTemplate>, List<EmailTemplateInListDto>>(list);
        }

        // ================= FILTER + PAGING =================
        [Authorize(VietlifeStorePermissions.EmailTemplate.View)]
        public async Task<PagedResultDto<EmailTemplateInListDto>> GetListFilterAsync(BaseListFilterDto input)
        {
            var query = (await Repository.GetQueryableAsync())
                .WhereIf(!string.IsNullOrWhiteSpace(input.Keyword),
                    x => x.TenTemplate.Contains(input.Keyword) ||
                         x.TieuDe.Contains(input.Keyword));

            var totalCount = await AsyncExecuter.LongCountAsync(query);

            var items = await AsyncExecuter.ToListAsync(
                query.OrderByDescending(x => x.CreationTime)
                     .Skip(input.SkipCount)
                     .Take(input.MaxResultCount)
            );

            return new PagedResultDto<EmailTemplateInListDto>(
            totalCount,
                ObjectMapper.Map<List<EmailTemplate>, List<EmailTemplateInListDto>>(items)
            );
        }
    }
}
