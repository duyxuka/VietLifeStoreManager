using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VietlifeStore.Entity.MediaContainers;
using VietlifeStore.Permissions;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.BlobStoring;
using Volo.Abp.Domain.Repositories;

namespace VietlifeStore.Entity.Banners
{
    public class BannersAppService :
        CrudAppService<
            Banner,
            BannerDto,
            Guid,
            PagedResultRequestDto,
            CreateUpdateBannerDto,
            CreateUpdateBannerDto>,
        IBannersAppService
    {
        private readonly IBlobContainer<MediaContainer> _mediaContainer;

        public BannersAppService(
            IRepository<Banner, Guid> repository,
            IBlobContainer<MediaContainer> mediaContainer)
            : base(repository)
        {
            _mediaContainer = mediaContainer;

            GetPolicyName = VietlifeStorePermissions.Banner.View;
            GetListPolicyName = VietlifeStorePermissions.Banner.View;
            CreatePolicyName = VietlifeStorePermissions.Banner.Create;
            UpdatePolicyName = VietlifeStorePermissions.Banner.Update;
            DeletePolicyName = VietlifeStorePermissions.Banner.Delete;
        }

        // ================= CREATE =================
        [Authorize(VietlifeStorePermissions.Banner.Create)]
        public override async Task<BannerDto> CreateAsync(CreateUpdateBannerDto input)
        {
            var entity = new Banner
            {
                TieuDe = input.TieuDe,
                MoTa = input.MoTa,
                LienKet = input.LienKet,
                TrangThai = input.TrangThai
            };

            await SaveImageAsync(entity, input);

            await Repository.InsertAsync(entity, autoSave: true);
            return MapToGetOutputDto(entity);
        }

        // ================= UPDATE =================
        [Authorize(VietlifeStorePermissions.Banner.Update)]
        public override async Task<BannerDto> UpdateAsync(Guid id, CreateUpdateBannerDto input)
        {
            var entity = await Repository.GetAsync(id);

            entity.TieuDe = input.TieuDe;
            entity.MoTa = input.MoTa;
            entity.LienKet = input.LienKet;
            entity.TrangThai = input.TrangThai;

            await SaveImageAsync(entity, input);

            await Repository.UpdateAsync(entity, autoSave: true);
            return MapToGetOutputDto(entity);
        }

        // ================= DELETE MULTIPLE =================
        [Authorize(VietlifeStorePermissions.Banner.Delete)]
        public async Task DeleteMultipleAsync(IEnumerable<Guid> ids)
        {
            await Repository.DeleteManyAsync(ids);
            await UnitOfWorkManager.Current.SaveChangesAsync();
        }

        // ================= GET ALL ACTIVE =================
        [AllowAnonymous]
        public async Task<List<BannerInListDto>> GetListAllAsync()
        {
            var list = await AsyncExecuter.ToListAsync(
                (await Repository.GetQueryableAsync())
                    .Where(x => x.TrangThai)
                    .OrderByDescending(x => x.CreationTime)
            );

            var result = ObjectMapper.Map<List<Banner>, List<BannerInListDto>>(list);

            foreach (var item in result)
            {
                if (!string.IsNullOrWhiteSpace(item.Anh))
                {
                    var bytes = await _mediaContainer.GetAllBytesOrNullAsync(item.Anh);
                    item.AnhContent = bytes == null
                        ? null
                        : $"data:image/png;base64,{Convert.ToBase64String(bytes)}";
                }
            }

            return result;
        }


        // ================= FILTER + PAGING =================
        [Authorize(VietlifeStorePermissions.Banner.View)]
        public async Task<PagedResultDto<BannerInListDto>> GetListFilterAsync(BaseListFilterDto input)
        {
            var query = (await Repository.GetQueryableAsync())
                .WhereIf(!string.IsNullOrWhiteSpace(input.Keyword),
                    x => x.TieuDe.Contains(input.Keyword));

            var totalCount = await AsyncExecuter.LongCountAsync(query);

            var items = await AsyncExecuter.ToListAsync(
                query
                    .OrderByDescending(x => x.CreationTime)
                    .Skip(input.SkipCount)
                    .Take(input.MaxResultCount)
            );

            return new PagedResultDto<BannerInListDto>(
                totalCount,
                ObjectMapper.Map<List<Banner>, List<BannerInListDto>>(items)
            );
        }

        // ================= IMAGE =================
        private async Task SaveImageAsync(Banner entity, CreateUpdateBannerDto input)
        {
            if (!string.IsNullOrWhiteSpace(input.AnhContent))
            {
                await SaveImageAsync(input.AnhName, input.AnhContent);
                entity.Anh = input.AnhName;
            }
        }

        private async Task SaveImageAsync(string fileName, string base64)
        {
            var regex = new Regex(@"^[\w/\:.-]+;base64,");
            base64 = regex.Replace(base64, string.Empty);

            var bytes = Convert.FromBase64String(base64);
            await _mediaContainer.SaveAsync(fileName, bytes, overrideExisting: true);
        }

        public async Task<string?> GetImageAsync(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return null;

            var bytes = await _mediaContainer.GetAllBytesOrNullAsync(fileName);
            return bytes == null ? null : Convert.ToBase64String(bytes);
        }
    }
}
