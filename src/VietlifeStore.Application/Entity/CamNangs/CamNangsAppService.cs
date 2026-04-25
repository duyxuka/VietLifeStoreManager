using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VietlifeStore.Entity.CamNangs;
using VietlifeStore.Entity.CamNangsList.CamNangs;
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

namespace VietlifeStore.Entity.CamNangs
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
            if (string.IsNullOrWhiteSpace(input.Ten))
                throw new UserFriendlyException("Tên cẩm nang không được để trống");

            // Tự động sinh slug nếu chưa có
            if (string.IsNullOrWhiteSpace(input.Slug))
            {
                input.Slug = await GenerateUniqueSlugAsync(input.Ten);
            }
            else
            {
                // Kiểm tra slug tồn tại khi người dùng nhập thủ công
                if (await Repository.AnyAsync(x => x.Slug == input.Slug))
                    throw new UserFriendlyException("Slug đã tồn tại, vui lòng chọn slug khác.");
            }
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
            entity.Mota = input.Mota;
            entity.DanhMucCamNangId = input.DanhMucCamNangId;
            entity.TrangThai = input.TrangThai;
            entity.TitleSEO = input.TitleSEO;
            entity.Keyword = input.Keyword;
            entity.DescriptionSEO = input.DescriptionSEO;
            // Xử lý slug
            if (!string.IsNullOrWhiteSpace(input.Slug) && input.Slug != entity.Slug)
            {
                if (await Repository.AnyAsync(x => x.Slug == input.Slug && x.Id != id))
                    throw new UserFriendlyException("Slug đã tồn tại, vui lòng chọn slug khác.");

                entity.Slug = input.Slug;
            }
            else if (string.IsNullOrWhiteSpace(input.Slug) && !string.IsNullOrWhiteSpace(input.Ten))
            {
                entity.Slug = await GenerateUniqueSlugAsync(input.Ten);
            }

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
        [Authorize(VietlifeStorePermissions.CamNang.View)]
        public async Task<List<CamNangSelectDto>> GetListSelectAsync()
        {
            var query = await Repository.GetQueryableAsync();

            var result = await AsyncExecuter.ToListAsync(
                query
                .AsNoTracking()
                .Where(x => x.TrangThai)
                .OrderBy(x => x.Ten)
                .Select(x => new CamNangSelectDto
                {
                    Id = x.Id,
                    Ten = x.Ten
                })
            );

            return result;
        }

        // ================= FILTER + PAGING =================
        [AllowAnonymous]
        public async Task<PagedResultDto<CamNangInListDto>> GetListFilterAsync(BaseListFilterDto input)
        {
            var camNangQueryable = (await Repository.GetQueryableAsync())
                .AsNoTracking();

            var danhMucQueryable = (await _danhMucRepo.GetQueryableAsync())
                .AsNoTracking();

            // ================= FILTER KEYWORD =================
            if (!string.IsNullOrWhiteSpace(input.Keyword))
            {
                camNangQueryable = camNangQueryable.Where(x =>
                    x.Ten.Contains(input.Keyword) ||
                    x.Slug.Contains(input.Keyword));
            }

            // ================= JOIN + FILTER DANH MỤC =================
            var query =
                from cn in camNangQueryable
                join dm in danhMucQueryable
                    on cn.DanhMucCamNangId equals dm.Id
                select new
                {
                    cn.Id,
                    cn.Ten,
                    cn.Slug,
                    cn.Anh,
                    cn.CreationTime,
                    cn.TrangThai,
                    TenDanhMuc = dm.Ten,
                    DanhMucSlug = dm.Slug,
                    DescriptionSEO = cn.DescriptionSEO
                };

            if (!string.IsNullOrWhiteSpace(input.DanhMucSlug))
            {
                query = query.Where(x => x.DanhMucSlug == input.DanhMucSlug);
            }

            var total = await AsyncExecuter.LongCountAsync(query);

            var items = await AsyncExecuter.ToListAsync(
                query
                .OrderByDescending(x => x.CreationTime)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount)
            );

            var result = items.Select(x => new CamNangInListDto
            {
                Id = x.Id,
                Ten = x.Ten,
                Slug = x.Slug,
                Anh = x.Anh,
                CreationTime = x.CreationTime,
                TrangThai = x.TrangThai,
                TenDanhMuc = x.TenDanhMuc,
                SlugDanhMuc = x.DanhMucSlug,
                DescriptionSEO = x.DescriptionSEO
            }).ToList();

            return new PagedResultDto<CamNangInListDto>(total, result);
        }

        // ================= GET LATEST FOR HOME =================
        [AllowAnonymous]
        public async Task<List<CamNangInListDto>> GetLatestCamNangHomeAsync(int take = 4)
        {
            var camNangQueryable = (await Repository.GetQueryableAsync())
                .AsNoTracking()
                .Where(x => x.TrangThai);

            var danhMucQueryable = (await _danhMucRepo.GetQueryableAsync())
                .AsNoTracking();

            var query =
                from cn in camNangQueryable
                join dm in danhMucQueryable
                    on cn.DanhMucCamNangId equals dm.Id
                orderby cn.CreationTime descending
                select new CamNangInListDto
                {
                    Id = cn.Id,
                    Ten = cn.Ten,
                    Slug = cn.Slug,
                    Anh = cn.Anh,
                    CreationTime = cn.CreationTime,
                    TenDanhMuc = dm.Ten,
                    SlugDanhMuc = dm.Slug
                };

            return await AsyncExecuter.ToListAsync(
                query.Take(take)
            );
        }

        [AllowAnonymous]
        public async Task<List<CamNangInListDto>> GetByDanhMucAsync(string slug)
        {
            if (string.IsNullOrWhiteSpace(slug))
                throw new UserFriendlyException("Slug danh mục không hợp lệ");

            var camNangQueryable = await Repository.GetQueryableAsync();
            var danhMucQueryable = await _danhMucRepo.GetQueryableAsync();

            var query = from cn in camNangQueryable
                        join dm in danhMucQueryable on cn.DanhMucCamNangId equals dm.Id
                        where dm.Slug == slug && cn.TrangThai
                        orderby cn.CreationTime descending
                        select new CamNangInListDto
                        {
                            Id = cn.Id,
                            Ten = cn.Ten,
                            Slug = cn.Slug,
                            Anh = cn.Anh,
                            CreationTime = cn.CreationTime,
                            TenDanhMuc = dm.Ten,      // Bổ sung thêm tên danh mục nếu cần
                            SlugDanhMuc = dm.Slug     // Đã có SlugDanhMuc
                        };

            return await AsyncExecuter.ToListAsync(query.Take(5));
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
        // ================= HÀM SINH SLUG UNIQUE =================
        private async Task<string> GenerateUniqueSlugAsync(string input)
        {
            var baseSlug = RemoveVietnamese(input)
                .ToLowerInvariant()
                .Replace(" ", "-")
                .Replace("--", "-")
                .Trim('-');

            baseSlug = Regex.Replace(baseSlug, @"[^a-z0-9\-]", "");

            if (string.IsNullOrEmpty(baseSlug))
                baseSlug = "cam-nang-" + DateTime.UtcNow.Ticks;

            var slug = baseSlug;
            int counter = 1;
            while (await Repository.AnyAsync(x => x.Slug == slug))
            {
                slug = $"{baseSlug}-{counter++}";
            }
            return slug;
        }

        private static string RemoveVietnamese(string text)
        {
            string[] vietnameseSigns = new string[]
            {
                "aAeEoOuUiIdDyY",
                "áàạảãâấầậẩẫăắằặẳẵ",
                "ÁÀẠẢÃÂẤẦẬẨẪĂẮẰẶẲẴ",
                "éèẹẻẽêếềệểễ",
                "ÉÈẸẺẼÊẾỀỆỂỄ",
                "óòọỏõôốồộổỗơớờợởỡ",
                "ÓÒỌỎÕÔỐỒỘỔỖƠỚỜỢỞỠ",
                "úùụủũưứừựửữ",
                "ÚÙỤỦŨƯỨỪỰỬỮ",
                "íìịỉĩ",
                "ÍÌỊỈĨ",
                "đ",
                "Đ",
                "ýỳỵỷỹ",
                "ÝỲỴỶỸ"
            };

            for (int i = 1; i < vietnameseSigns.Length; i++)
            {
                for (int j = 0; j < vietnameseSigns[i].Length; j++)
                {
                    text = text.Replace(vietnameseSigns[i][j], vietnameseSigns[0][i - 1]);
                }
            }
            return text;
        }
    }
}
