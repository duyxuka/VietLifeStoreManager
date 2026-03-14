using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VietlifeStore.Entity.MediaContainers;
using VietlifeStore.Entity.SanPhamsList.DanhMucSanPhams;
using VietlifeStore.Entity.UploadFile;
using VietlifeStore.Permissions;
using Volo.Abp;
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
        private readonly IRepository<SanPham, Guid> _sanPhamRepository;
        private readonly IMediaAppService _mediaAppService;

        public DanhMucSanPhamsAppService(
            IRepository<DanhMucSanPham, Guid> repository,
            IRepository<SanPham, Guid> sanPhamRepository,
            IMediaAppService mediaAppService)
            : base(repository)
        {
            _sanPhamRepository = sanPhamRepository;
            _mediaAppService = mediaAppService;

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
            if (string.IsNullOrWhiteSpace(input.AnhThumbnail))
                throw new UserFriendlyException("Không thấy file ảnh danh mục sản phẩm thumbnail");
            if (string.IsNullOrWhiteSpace(input.AnhBanner))
                throw new UserFriendlyException("Không thấy file ảnh danh mục sản phẩm banner");

            // Tự động sinh slug nếu trống
            if (string.IsNullOrWhiteSpace(input.Slug) && !string.IsNullOrWhiteSpace(input.Ten))
            {
                input.Slug = await GenerateUniqueSlugAsync(input.Ten);
            }
            else if (!string.IsNullOrWhiteSpace(input.Slug))
            {
                // Kiểm tra slug đã tồn tại (khi người dùng nhập thủ công)
                if (await Repository.AnyAsync(x => x.Slug == input.Slug))
                    throw new UserFriendlyException("Slug đã tồn tại, vui lòng chọn slug khác.");
            }
            var entity = new DanhMucSanPham
            {
                Ten = input.Ten,
                Slug = input.Slug,
                TrangThai = input.TrangThai,
                TitleSEO = input.TitleSEO,
                Keyword = input.Keyword,
                DescriptionSEO = input.DescriptionSEO,
                AnhThumbnail = input.AnhThumbnail,
                AnhBanner = input.AnhBanner
            };

            var created = await Repository.InsertAsync(entity, autoSave: true);
            return MapToGetOutputDto(created);
        }

        // ================= UPDATE =================
        [Authorize(VietlifeStorePermissions.DanhMucSanPham.Update)]
        public override async Task<DanhMucSanPhamDto> UpdateAsync(Guid id, CreateUpdateDanhMucSanPhamDto input)
        {
            var entity = await Repository.GetAsync(id);
            var oldThumbnail = entity.AnhThumbnail;
            var oldBanner = entity.AnhBanner;
            entity.Ten = input.Ten;
            entity.TrangThai = input.TrangThai;
            entity.TitleSEO = input.TitleSEO;
            entity.Keyword = input.Keyword;
            entity.DescriptionSEO = input.DescriptionSEO;

            if (!string.IsNullOrWhiteSpace(input.Slug) && input.Slug != entity.Slug)
            {
                // Slug thay đổi → kiểm tra unique
                if (await Repository.AnyAsync(x => x.Slug == input.Slug && x.Id != id))
                    throw new UserFriendlyException("Slug đã tồn tại, vui lòng chọn slug khác.");

                entity.Slug = input.Slug;
            }
            // Nếu slug trống và có tên → sinh mới (ít dùng trường hợp này khi update)
            else if (string.IsNullOrWhiteSpace(input.Slug) && !string.IsNullOrWhiteSpace(input.Ten))
            {
                entity.Slug = await GenerateUniqueSlugAsync(input.Ten);
            }

            // Nếu có ảnh mới → xoá ảnh cũ trước khi cập nhật
            if (!string.IsNullOrWhiteSpace(input.AnhThumbnail) && input.AnhThumbnail != oldThumbnail)
            {
                entity.AnhThumbnail = input.AnhThumbnail;

                // Xóa ảnh cũ
                if (!string.IsNullOrWhiteSpace(oldThumbnail))
                {
                    try
                    {
                        await _mediaAppService.DeleteAsync(oldThumbnail);
                    }
                    catch (Exception ex)
                    {
                        // Log nhưng không throw - không block update nếu xóa file thất bại
                        Logger.LogWarning(ex, $"Không thể xóa ảnh cũ: {oldThumbnail}");
                    }
                }
            }

            // ✅ Xử lý Banner
            if (!string.IsNullOrWhiteSpace(input.AnhBanner) && input.AnhBanner != oldBanner)
            {
                entity.AnhBanner = input.AnhBanner;

                // Xóa ảnh cũ
                if (!string.IsNullOrWhiteSpace(oldBanner))
                {
                    try
                    {
                        await _mediaAppService.DeleteAsync(oldBanner);
                    }
                    catch (Exception ex)
                    {
                        // Log nhưng không throw - không block update nếu xóa file thất bại
                        Logger.LogWarning(ex, $"Không thể xóa ảnh cũ: {oldBanner}");
                    }
                }
            }

            await Repository.UpdateAsync(entity, autoSave: true);
            return MapToGetOutputDto(entity);
        }

        // ================= DELETE SINGLE =================
        [Authorize(VietlifeStorePermissions.DanhMucSanPham.Delete)]
        public override async Task DeleteAsync(Guid id)
        {
            var entity = await Repository.GetAsync(id);

            if (!string.IsNullOrWhiteSpace(entity.AnhThumbnail))
                await _mediaAppService.DeleteAsync(entity.AnhThumbnail);

            if (!string.IsNullOrWhiteSpace(entity.AnhBanner))
                await _mediaAppService.DeleteAsync(entity.AnhBanner);

            await base.DeleteAsync(id);
        }

        // ================= DELETE MULTIPLE =================
        [Authorize(VietlifeStorePermissions.DanhMucSanPham.Delete)]
        public async Task DeleteMultipleAsync(IEnumerable<Guid> ids)
        {
            var list = await Repository.GetListAsync(x => ids.Contains(x.Id));

            foreach (var item in list)
            {
                if (!string.IsNullOrWhiteSpace(item.AnhThumbnail))
                    await _mediaAppService.DeleteAsync(item.AnhThumbnail);

                if (!string.IsNullOrWhiteSpace(item.AnhBanner))
                    await _mediaAppService.DeleteAsync(item.AnhBanner);
            }

            await Repository.DeleteManyAsync(list);
        }

        // ================= GET ALL =================
        [AllowAnonymous]
        public async Task<List<DanhMucSanPhamInListDto>> GetListAllAsync()
        {
            var danhMucQueryable = await Repository.GetQueryableAsync();
            var sanPhamQueryable = await _sanPhamRepository.GetQueryableAsync();

            var query =
                from dm in danhMucQueryable
                where dm.TrangThai
                join sp in sanPhamQueryable
                    on dm.Id equals sp.DanhMucId into sanPhamGroup
                select new DanhMucSanPhamInListDto
                {
                    Id = dm.Id,
                    Ten = dm.Ten,
                    Slug = dm.Slug,
                    AnhThumbnail = dm.AnhThumbnail,
                    AnhBanner = dm.AnhBanner,
                    TrangThai = dm.TrangThai,
                    SoLuongSanPham = sanPhamGroup.Count(x => x.TrangThai)
                };

            return await AsyncExecuter.ToListAsync(query);
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
        // Hàm sinh slug unique (tương tự SanPham)
        private async Task<string> GenerateUniqueSlugAsync(string input)
        {
            var baseSlug = RemoveVietnamese(input)
                .ToLowerInvariant()
                .Replace(" ", "-")
                .Replace("--", "-")
                .Trim('-');

            // Loại bỏ ký tự không hợp lệ
            baseSlug = Regex.Replace(baseSlug, @"[^a-z0-9\-]", "");

            if (string.IsNullOrEmpty(baseSlug))
                baseSlug = "danh-muc-" + DateTime.UtcNow.Ticks;

            var slug = baseSlug;
            int counter = 1;
            while (await Repository.AnyAsync(x => x.Slug == slug))
            {
                slug = $"{baseSlug}-{counter++}";
            }
            return slug;
        }

        // Hàm loại bỏ dấu tiếng Việt (copy từ SanPham nếu chưa có)
        private static string RemoveVietnamese(string text)
        {
            // Giống hệt hàm RemoveVietnamese trong SanPhamsAppService
            // (bạn có thể copy nguyên hoặc di chuyển ra class helper chung)
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
