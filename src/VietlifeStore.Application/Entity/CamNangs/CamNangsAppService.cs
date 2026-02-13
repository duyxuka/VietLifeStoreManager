using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VietlifeStore.Entity.CamNangs;
using VietlifeStore.Entity.MediaContainers;
using VietlifeStore.Entity.UploadFile;
using VietlifeStore.Permissions;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.BlobStoring;
using Volo.Abp.Content;
using Volo.Abp.Domain.Repositories;

namespace VietlifeStore.Entity.CamNangsList.CamNangs
{
    public class CamNangsAppService :
        CrudAppService<
            CamNang,
            CamNangDto,
            Guid,
            PagedResultRequestDto,
            CreateUpdateCamNangDto,
            CreateUpdateCamNangDto>,
        ICamNangsAppService
    {
        private readonly IMediaAppService _mediaAppService;

        public CamNangsAppService(
            IRepository<CamNang, Guid> repository,
            IMediaAppService mediaAppService)
            : base(repository)
        {
            _mediaAppService = mediaAppService;

            GetPolicyName = VietlifeStorePermissions.CamNang.View;
            GetListPolicyName = VietlifeStorePermissions.CamNang.View;
            CreatePolicyName = VietlifeStorePermissions.CamNang.Create;
            UpdatePolicyName = VietlifeStorePermissions.CamNang.Update;
            DeletePolicyName = VietlifeStorePermissions.CamNang.Delete;
        }


        // ================= CREATE =================
        [Authorize(VietlifeStorePermissions.CamNang.Create)]
        public override async Task<CamNangDto> CreateAsync(CreateUpdateCamNangDto input)
        {
            if (string.IsNullOrWhiteSpace(input.Anh))
                throw new UserFriendlyException("Không thấy file ảnh");

            var entity = new CamNang
            {
                Ten = input.Ten,
                Slug = input.Slug,
                Mota = input.Mota,
                DanhMucCamNangId = input.DanhMucCamNangId,
                TrangThai = input.TrangThai,
                TitleSEO = input.TitleSEO,
                Keyword = input.Keyword,
                DescriptionSEO = input.DescriptionSEO,
                Anh = input.Anh
            };

            var created = await Repository.InsertAsync(entity, autoSave: true);
            return MapToGetOutputDto(created);
        }

        // ================= UPDATE =================
        [Authorize(VietlifeStorePermissions.CamNang.Update)]
        public override async Task<CamNangDto> UpdateAsync(Guid id, CreateUpdateCamNangDto input)
        {
            var entity = await Repository.GetAsync(id);
            var oldImage = entity.Anh;
            entity.Ten = input.Ten;
            entity.Slug = input.Slug;
            entity.Mota = input.Mota;
            entity.DanhMucCamNangId = input.DanhMucCamNangId;
            entity.TrangThai = input.TrangThai;
            entity.TitleSEO = input.TitleSEO;
            entity.Keyword = input.Keyword;
            entity.DescriptionSEO = input.DescriptionSEO;
            if (!string.IsNullOrWhiteSpace(input.Anh) && input.Anh != oldImage)
            {
                entity.Anh = input.Anh;

                // ✅ Xóa ảnh cũ
                if (!string.IsNullOrWhiteSpace(oldImage))
                {
                    try
                    {
                        await _mediaAppService.DeleteAsync(oldImage);
                    }
                    catch (Exception ex)
                    {
                        // Log nhưng không throw - không block update nếu xóa file thất bại
                        Logger.LogWarning(ex, $"Không thể xóa ảnh cũ: {oldImage}");
                    }
                }
            }

            await Repository.UpdateAsync(entity, autoSave: true);
            return MapToGetOutputDto(entity);
        }

        [Authorize(VietlifeStorePermissions.CamNang.Delete)]
        public override async Task DeleteAsync(Guid id)
        {
            var entity = await Repository.GetAsync(id);

            if (!string.IsNullOrWhiteSpace(entity.Anh))
            {
                await _mediaAppService.DeleteAsync(entity.Anh);
            }

            await base.DeleteAsync(id);
        }
        // ================= DELETE MULTIPLE =================
        [Authorize(VietlifeStorePermissions.CamNang.Delete)]
        public async Task DeleteMultipleAsync(IEnumerable<Guid> ids)
        {
            var list = await Repository.GetListAsync(x => ids.Contains(x.Id));

            foreach (var item in list)
            {
                if (!string.IsNullOrWhiteSpace(item.Anh))
                {
                    await _mediaAppService.DeleteAsync(item.Anh);
                }
            }

            await Repository.DeleteManyAsync(list);
        }


        // ================= GET ALL ACTIVE =================
        [Authorize(VietlifeStorePermissions.CamNang.View)]
        public async Task<List<CamNangInListDto>> GetListAllAsync()
        {
            var list = await AsyncExecuter.ToListAsync(
                (await Repository.GetQueryableAsync())
                    .Where(x => x.TrangThai)
                    .OrderByDescending(x => x.CreationTime)
            );

            return ObjectMapper.Map<List<CamNang>, List<CamNangInListDto>>(list);
        }

        // ================= FILTER + PAGING =================
        [Authorize(VietlifeStorePermissions.CamNang.View)]
        public async Task<PagedResultDto<CamNangInListDto>> GetListFilterAsync(BaseListFilterDto input)
        {
            var query = (await Repository.GetQueryableAsync())
                .WhereIf(!string.IsNullOrWhiteSpace(input.Keyword),
                    x => x.Ten.Contains(input.Keyword) || x.Slug.Contains(input.Keyword));

            var totalCount = await AsyncExecuter.LongCountAsync(query);

            var items = await AsyncExecuter.ToListAsync(
                query
                    .OrderByDescending(x => x.CreationTime)
                    .Skip(input.SkipCount)
                    .Take(input.MaxResultCount)
            );

            return new PagedResultDto<CamNangInListDto>(
                totalCount,
                ObjectMapper.Map<List<CamNang>, List<CamNangInListDto>>(items)
            );
        }

        // ================= GET LATEST FOR HOME =================
        [AllowAnonymous]
        public async Task<List<CamNangInListDto>> GetLatestCamNangHomeAsync(int take = 4)
        {
            var query = await Repository.GetQueryableAsync();

            var items = await AsyncExecuter.ToListAsync(
                query
                    .Where(x => x.TrangThai)
                    .OrderByDescending(x => x.CreationTime)
                    .Take(take)
            );

            return ObjectMapper.Map<List<CamNang>, List<CamNangInListDto>>(items);
        }
    }
}
