using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VietlifeStore.Entity.UploadFile;
using VietlifeStore.Permissions;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
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
        private readonly IMediaAppService _mediaAppService;

        public BannersAppService(
            IRepository<Banner, Guid> repository,
            IMediaAppService mediaAppService)
            : base(repository)
        {
            _mediaAppService = mediaAppService;

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
            if (string.IsNullOrWhiteSpace(input.Anh))
                throw new UserFriendlyException("Không thấy file ảnh");

            if (string.IsNullOrWhiteSpace(input.AnhMobile))
                throw new UserFriendlyException("Không thấy file ảnh Mobile");

            var entity = new Banner
            {
                TieuDe = input.TieuDe,
                MoTa = input.MoTa,
                LienKet = input.LienKet,
                TrangThai = input.TrangThai,
                Anh = input.Anh, // chỉ lưu fileName
                AnhMobile = input.AnhMobile // chỉ lưu fileName
            };

            await Repository.InsertAsync(entity, autoSave: true);
            return MapToGetOutputDto(entity);
        }

        // ================= UPDATE =================
        [Authorize(VietlifeStorePermissions.Banner.Update)]
        public override async Task<BannerDto> UpdateAsync(Guid id, CreateUpdateBannerDto input)
        {
            var entity = await Repository.GetAsync(id);

            var oldImage = entity.Anh;
            var oldImageMobile = entity.AnhMobile;

            entity.TieuDe = input.TieuDe;
            entity.MoTa = input.MoTa;
            entity.LienKet = input.LienKet;
            entity.TrangThai = input.TrangThai;

            // ===== Ảnh desktop =====
            if (!string.IsNullOrWhiteSpace(input.Anh) && input.Anh != oldImage)
            {
                entity.Anh = input.Anh;

                if (!string.IsNullOrWhiteSpace(oldImage))
                {
                    try
                    {
                        await _mediaAppService.DeleteAsync(oldImage);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogWarning(ex, $"Không thể xóa ảnh cũ: {oldImage}");
                    }
                }
            }

            // ===== Ảnh mobile =====
            if (!string.IsNullOrWhiteSpace(input.AnhMobile) && input.AnhMobile != oldImageMobile)
            {
                entity.AnhMobile = input.AnhMobile;

                if (!string.IsNullOrWhiteSpace(oldImageMobile))
                {
                    try
                    {
                        await _mediaAppService.DeleteAsync(oldImageMobile);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogWarning(ex, $"Không thể xóa ảnh mobile cũ: {oldImageMobile}");
                    }
                }
            }

            await Repository.UpdateAsync(entity, autoSave: true);

            return MapToGetOutputDto(entity);
        }

        // ================= DELETE SINGLE =================
        [Authorize(VietlifeStorePermissions.Banner.Delete)]
        public override async Task DeleteAsync(Guid id)
        {
            var entity = await Repository.GetAsync(id);

            if (!string.IsNullOrWhiteSpace(entity.Anh))
            {
                await _mediaAppService.DeleteAsync(entity.Anh);
            }

            if (!string.IsNullOrWhiteSpace(entity.AnhMobile))
            {
                await _mediaAppService.DeleteAsync(entity.AnhMobile);
            }

            await base.DeleteAsync(id);
        }

        // ================= DELETE MULTIPLE =================
        [Authorize(VietlifeStorePermissions.Banner.Delete)]
        [Authorize(VietlifeStorePermissions.Banner.Delete)]
        public async Task DeleteMultipleAsync(IEnumerable<Guid> ids)
        {
            var list = await Repository.GetListAsync(x => ids.Contains(x.Id));

            foreach (var item in list)
            {
                if (!string.IsNullOrWhiteSpace(item.Anh))
                {
                    await _mediaAppService.DeleteAsync(item.Anh);
                }

                if (!string.IsNullOrWhiteSpace(item.AnhMobile))
                {
                    await _mediaAppService.DeleteAsync(item.AnhMobile);
                }
            }

            await Repository.DeleteManyAsync(list);
        }

        // ================= GET ALL ACTIVE =================
        public async Task<List<BannerInListDto>> GetListAllAsync()
        {
            var list = await AsyncExecuter.ToListAsync(
                (await Repository.GetQueryableAsync())
                    .Where(x => x.TrangThai)
                    .OrderByDescending(x => x.CreationTime)
            );

            return ObjectMapper.Map<List<Banner>, List<BannerInListDto>>(list);
        }

        // ================= FILTER + PAGING =================
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
    }
}
