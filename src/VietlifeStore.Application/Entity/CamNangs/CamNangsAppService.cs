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
using VietlifeStore.Entity.SanPhamsList.SanPhams;
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
        private readonly IRepository<DanhMucCamNang, Guid> _danhMucRepo;

        public CamNangsAppService(
            IRepository<CamNang, Guid> repository,
            IRepository<DanhMucCamNang, Guid> danhMucRepo,
            IMediaAppService mediaAppService)
            : base(repository)
        {
            _mediaAppService = mediaAppService;
            _danhMucRepo = danhMucRepo;

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
        [AllowAnonymous]
        public async Task<PagedResultDto<CamNangInListDto>> GetListFilterAsync(BaseListFilterDto input)
        {
            var camNangQueryable = await Repository.GetQueryableAsync();
            var danhMucQueryable = await _danhMucRepo.GetQueryableAsync();

            var query =
                from cn in camNangQueryable
                join dm in danhMucQueryable
                    on cn.DanhMucCamNangId equals dm.Id
                where cn.TrangThai
                select new { cn, dm };

            // ================= LỌC DANH MỤC =================
            if (!string.IsNullOrWhiteSpace(input.DanhMucSlug))
            {
                query = query.Where(x => x.dm.Slug == input.DanhMucSlug);
            }

            // ================= LỌC KEYWORD =================
            if (!string.IsNullOrWhiteSpace(input.Keyword))
            {
                query = query.Where(x =>
                    x.cn.Ten.Contains(input.Keyword) ||
                    x.cn.Slug.Contains(input.Keyword));
            }

            var total = await AsyncExecuter.LongCountAsync(query);

            var items = await AsyncExecuter.ToListAsync(
                query
                    .Skip(input.SkipCount)
                    .Take(input.MaxResultCount)
            );

            var result = items.Select(x => new CamNangInListDto
            {
                Id = x.cn.Id,
                Ten = x.cn.Ten,
                Slug = x.cn.Slug,
                Anh = x.cn.Anh,
                Mota = GetShortDescription(x.cn.Mota, 200),
                CreationTime = x.cn.CreationTime,
                TrangThai = x.cn.TrangThai,
                TenDanhMuc = x.dm.Ten
            }).ToList();

            return new PagedResultDto<CamNangInListDto>(total, result);
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

        [AllowAnonymous]
        public async Task<List<CamNangInListDto>> GetByDanhMucAsync(string slug)
        {
            if (string.IsNullOrWhiteSpace(slug))
            {
                throw new UserFriendlyException("Slug danh mục không hợp lệ");
            }
            var danhMuc = await _danhMucRepo.FirstOrDefaultAsync(x => x.Slug == slug);
            if (danhMuc == null)
            {
                throw new UserFriendlyException("Danh mục không tồn tại");
            }
            var entities = await Repository.GetListAsync(
                x => x.DanhMucCamNangId == danhMuc.Id && x.TrangThai
            );
            var result = ObjectMapper.Map<List<CamNang>, List<CamNangInListDto>>(entities);

            return result;
        }

        [AllowAnonymous]
        public async Task<CamNangDto> GetBySlugAsync(string slug)
        {
            if (string.IsNullOrWhiteSpace(slug))
            {
                throw new UserFriendlyException("Slug không hợp lệ");
            }

            var camNangQueryable = await Repository.GetQueryableAsync();
            var danhMucQueryable = await _danhMucRepo.GetQueryableAsync();

            var query =
                from cn in camNangQueryable
                join dm in danhMucQueryable
                    on cn.DanhMucCamNangId equals dm.Id
                where cn.Slug == slug && cn.TrangThai
                select new CamNangDto
                {
                    Id = cn.Id,
                    Ten = cn.Ten,
                    Slug = cn.Slug,
                    Anh = cn.Anh,
                    Mota = cn.Mota,
                    DescriptionSEO = cn.DescriptionSEO,
                    Keyword = cn.Keyword,
                    TitleSEO = cn.TitleSEO,
                    CreationTime = cn.CreationTime,
                    TrangThai = cn.TrangThai,
                    TenDanhMuc = dm.Ten,
                    SlugDanhMuc = dm.Slug
                };

            var result = await AsyncExecuter.FirstOrDefaultAsync(query);

            if (result == null)
            {
                throw new UserFriendlyException("Cẩm nang không tồn tại");
            }

            return result;
        }
        private string GetShortDescription(string? html, int length)
        {
            if (string.IsNullOrWhiteSpace(html))
                return string.Empty;

            // Loại bỏ HTML tags
            var plainText = Regex.Replace(html, "<.*?>", string.Empty);

            if (plainText.Length <= length)
                return plainText;

            return plainText.Substring(0, length) + "...";
        }
    }
}
