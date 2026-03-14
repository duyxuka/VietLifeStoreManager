using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VietlifeStore.Entity.ChinhSachsList.DanhMucChinhSachs;
using VietlifeStore.Permissions;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace VietlifeStore.Entity.ChinhSachs
{
    public class DanhMucChinhSachsAppService :
        CrudAppService<
            DanhMucChinhSach,
            DanhMucChinhSachDto,
            Guid,
            PagedResultRequestDto,
            CreateUpdateDanhMucChinhSachDto,
            CreateUpdateDanhMucChinhSachDto>,
        IDanhMucChinhSachsAppService
    {
        public DanhMucChinhSachsAppService(
            IRepository<DanhMucChinhSach, Guid> repository)
            : base(repository)
        {
            GetPolicyName = VietlifeStorePermissions.DanhMucChinhSach.View;
            GetListPolicyName = VietlifeStorePermissions.DanhMucChinhSach.View;
            CreatePolicyName = VietlifeStorePermissions.DanhMucChinhSach.Create;
            UpdatePolicyName = VietlifeStorePermissions.DanhMucChinhSach.Update;
            DeletePolicyName = VietlifeStorePermissions.DanhMucChinhSach.Delete;
        }

        // ================= CREATE =================
        [Authorize(VietlifeStorePermissions.DanhMucChinhSach.Create)]
        public override async Task<DanhMucChinhSachDto> CreateAsync(CreateUpdateDanhMucChinhSachDto input)
        {
            if (string.IsNullOrWhiteSpace(input.Ten))
                throw new UserFriendlyException("Tên danh mục không được để trống");

            // Tự động sinh slug nếu chưa có
            if (string.IsNullOrWhiteSpace(input.Slug))
            {
                input.Slug = await GenerateUniqueSlugAsync(input.Ten);
            }
            else
            {
                // Kiểm tra slug đã tồn tại khi người dùng nhập thủ công
                if (await Repository.AnyAsync(x => x.Slug == input.Slug))
                    throw new UserFriendlyException("Slug đã tồn tại, vui lòng chọn slug khác.");
            }
            var entity = new DanhMucChinhSach
            {
                Ten = input.Ten,
                Slug = input.Slug,
                TrangThai = input.TrangThai,
                TitleSEO = input.TitleSEO,
                Keyword = input.Keyword,
                DescriptionSEO = input.DescriptionSEO
            };

            await Repository.InsertAsync(entity, autoSave: true);
            return MapToGetOutputDto(entity);
        }

        // ================= UPDATE =================
        [Authorize(VietlifeStorePermissions.DanhMucChinhSach.Update)]
        public override async Task<DanhMucChinhSachDto> UpdateAsync(Guid id, CreateUpdateDanhMucChinhSachDto input)
        {
            var entity = await Repository.GetAsync(id);

            entity.Ten = input.Ten;
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
            // Nếu slug bị xóa trống → sinh lại từ tên
            else if (string.IsNullOrWhiteSpace(input.Slug) && !string.IsNullOrWhiteSpace(input.Ten))
            {
                entity.Slug = await GenerateUniqueSlugAsync(input.Ten);
            }

            await Repository.UpdateAsync(entity, autoSave: true);
            return MapToGetOutputDto(entity);
        }

        // ================= DELETE MULTIPLE =================
        [Authorize(VietlifeStorePermissions.DanhMucChinhSach.Delete)]
        public async Task DeleteMultipleAsync(IEnumerable<Guid> ids)
        {
            await Repository.DeleteManyAsync(ids);
            await UnitOfWorkManager.Current.SaveChangesAsync();
        }

        // ================= GET ALL ACTIVE =================
        [AllowAnonymous]
        public async Task<List<DanhMucChinhSachInListDto>> GetListAllAsync()
        {
            var list = await AsyncExecuter.ToListAsync(
                (await Repository.GetQueryableAsync())
                    .Where(x => x.TrangThai)
                    .OrderByDescending(x => x.CreationTime)
            );

            return ObjectMapper.Map<List<DanhMucChinhSach>, List<DanhMucChinhSachInListDto>>(list);
        }

        // ================= FILTER + PAGING =================
        [Authorize(VietlifeStorePermissions.DanhMucChinhSach.View)]
        public async Task<PagedResultDto<DanhMucChinhSachInListDto>> GetListFilterAsync(BaseListFilterDto input)
        {
            var query = (await Repository.GetQueryableAsync())
                .WhereIf(!string.IsNullOrWhiteSpace(input.Keyword),
                    x => x.Ten.Contains(input.Keyword) || x.Slug.Contains(input.Keyword));

            var totalCount = await AsyncExecuter.LongCountAsync(query);

            var items = await AsyncExecuter.ToListAsync(
                query.OrderByDescending(x => x.CreationTime)
                     .Skip(input.SkipCount)
                     .Take(input.MaxResultCount)
            );

            return new PagedResultDto<DanhMucChinhSachInListDto>(
                totalCount,
                ObjectMapper.Map<List<DanhMucChinhSach>, List<DanhMucChinhSachInListDto>>(items)
            );
        }
        // ================= HÀM SINH SLUG UNIQUE =================
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
                baseSlug = "chinh-sach-" + DateTime.UtcNow.Ticks;

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
