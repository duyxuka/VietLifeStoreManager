using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VietlifeStore.Entity.MediaContainers;
using VietlifeStore.Entity.SanPhamsList.DanhMucSanPhams;
using VietlifeStore.Permissions;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.BlobStoring;
using Volo.Abp.Domain.Repositories;

namespace VietlifeStore.Entity.SanPhams
{
    public class DanhMucSanPhamsAppService :
        CrudAppService<
            DanhMucSanPham,
            DanhMucSanPhamDto,
            Guid,
            PagedResultRequestDto,
            CreateUpdateDanhMucSanPhamDto,
            CreateUpdateDanhMucSanPhamDto>,
        IDanhMucSanPhamsAppService
    {
        private readonly IBlobContainer<MediaContainer> _mediaContainer;

        public DanhMucSanPhamsAppService(
            IRepository<DanhMucSanPham, Guid> repository,
            IBlobContainer<MediaContainer> mediaContainer)
            : base(repository)
        {
            _mediaContainer = mediaContainer;

            GetPolicyName = VietlifeStorePermissions.DanhMucSanPham.View;
            GetListPolicyName = VietlifeStorePermissions.DanhMucSanPham.View;
            CreatePolicyName = VietlifeStorePermissions.DanhMucSanPham.Create;
            UpdatePolicyName = VietlifeStorePermissions.DanhMucSanPham.Update;
            DeletePolicyName = VietlifeStorePermissions.DanhMucSanPham.Delete;
        }

        // ================= CREATE =================
        [Authorize(VietlifeStorePermissions.DanhMucSanPham.Create)]
        public override async Task<DanhMucSanPhamDto> CreateAsync(CreateUpdateDanhMucSanPhamDto input)
        {
            var entity = new DanhMucSanPham
            {
                Ten = input.Ten,
                Slug = input.Slug,
                TrangThai = input.TrangThai,
                TitleSEO = input.TitleSEO,
                Keyword = input.Keyword,
                DescriptionSEO = input.DescriptionSEO
            };

            await SaveImagesAsync(entity, input);

            var created = await Repository.InsertAsync(entity, autoSave: true);
            return MapToGetOutputDto(created);
        }

        // ================= UPDATE =================
        [Authorize(VietlifeStorePermissions.DanhMucSanPham.Update)]
        public override async Task<DanhMucSanPhamDto> UpdateAsync(Guid id, CreateUpdateDanhMucSanPhamDto input)
        {
            var entity = await Repository.GetAsync(id);

            entity.Ten = input.Ten;
            entity.Slug = input.Slug;
            entity.TrangThai = input.TrangThai;
            entity.TitleSEO = input.TitleSEO;
            entity.Keyword = input.Keyword;
            entity.DescriptionSEO = input.DescriptionSEO;

            await SaveImagesAsync(entity, input);

            await Repository.UpdateAsync(entity, autoSave: true);
            return MapToGetOutputDto(entity);
        }

        // ================= DELETE MULTIPLE =================
        [Authorize(VietlifeStorePermissions.DanhMucSanPham.Delete)]
        public async Task DeleteMultipleAsync(IEnumerable<Guid> ids)
        {
            await Repository.DeleteManyAsync(ids);
            await UnitOfWorkManager.Current.SaveChangesAsync();
        }

        // ================= GET ALL =================
        [AllowAnonymous]
        public async Task<List<DanhMucSanPhamInListDto>> GetListAllAsync()
        {
            var entities = await AsyncExecuter.ToListAsync(
                (await Repository.GetQueryableAsync())
                    .Where(x => x.TrangThai)
                    .OrderBy(x => x.Ten)
            );

            var result = ObjectMapper.Map<
                List<DanhMucSanPham>,
                List<DanhMucSanPhamInListDto>
            >(entities);

            foreach (var item in result)
            {
                // Thumbnail
                if (!string.IsNullOrWhiteSpace(item.AnhThumbnail))
                {
                    var bytes = await _mediaContainer.GetAllBytesOrNullAsync(item.AnhThumbnail);
                    item.AnhThumbnailContent = bytes == null
                        ? null
                        : $"data:image/png;base64,{Convert.ToBase64String(bytes)}";
                }

                // Banner
                if (!string.IsNullOrWhiteSpace(item.AnhBanner))
                {
                    var bytes = await _mediaContainer.GetAllBytesOrNullAsync(item.AnhBanner);
                    item.AnhBannerContent = bytes == null
                        ? null
                        : $"data:image/png;base64,{Convert.ToBase64String(bytes)}";
                }
            }

            return result;
        }


        // ================= FILTER + PAGING =================
        [Authorize(VietlifeStorePermissions.DanhMucSanPham.View)]
        public async Task<PagedResultDto<DanhMucSanPhamInListDto>> GetListFilterAsync(BaseListFilterDto input)
        {
            var query = (await Repository.GetQueryableAsync())
                .WhereIf(!string.IsNullOrWhiteSpace(input.Keyword),
                    x => x.Ten.Contains(input.Keyword) || x.Slug.Contains(input.Keyword));

            var totalCount = await AsyncExecuter.LongCountAsync(query);

            var items = await AsyncExecuter.ToListAsync(
                query.Skip(input.SkipCount).Take(input.MaxResultCount)
            );

            return new PagedResultDto<DanhMucSanPhamInListDto>(
                totalCount,
                ObjectMapper.Map<List<DanhMucSanPham>, List<DanhMucSanPhamInListDto>>(items)
            );
        }

        // ================= IMAGE =================
        private async Task SaveImagesAsync(DanhMucSanPham entity, CreateUpdateDanhMucSanPhamDto input)
        {
            if (!string.IsNullOrWhiteSpace(input.AnhThumbnailContent))
            {
                await SaveImageAsync(input.AnhThumbnailName, input.AnhThumbnailContent);
                entity.AnhThumbnail = input.AnhThumbnailName;
            }

            if (!string.IsNullOrWhiteSpace(input.AnhBannerContent))
            {
                await SaveImageAsync(input.AnhBannerName, input.AnhBannerContent);
                entity.AnhBanner = input.AnhBannerName;
            }
        }

        private async Task SaveImageAsync(string fileName, string base64)
        {
            var regex = new Regex(@"^[\w/\:.-]+;base64,");
            base64 = regex.Replace(base64, string.Empty);

            var bytes = Convert.FromBase64String(base64);
            await _mediaContainer.SaveAsync(fileName, bytes, overrideExisting: true);
        }

        public async Task<string> GetImageAsync(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return null;

            var bytes = await _mediaContainer.GetAllBytesOrNullAsync(fileName);
            return bytes == null ? null : Convert.ToBase64String(bytes);
        }
    }
}
