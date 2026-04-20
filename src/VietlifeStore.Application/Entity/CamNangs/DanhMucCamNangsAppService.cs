using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VietlifeStore.Entity.CamNangsList.DanhMucCamNangs;
using VietlifeStore.Permissions;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace VietlifeStore.Entity.CamNangs
{
    public class DanhMucCamNangsAppService :
        CrudAppService<
            DanhMucCamNang,
            DanhMucCamNangDto,
            Guid,
            PagedResultRequestDto,
            CreateUpdateDanhMucCamNangDto,
            CreateUpdateDanhMucCamNangDto>,
        IDanhMucCamNangsAppService
    {
        private readonly IRepository<CamNang, Guid> _camNangRepo;
        public DanhMucCamNangsAppService(
            IRepository<DanhMucCamNang, Guid> repository,
            IRepository<CamNang, Guid> camNangRepo)
            : base(repository)
        {
            GetPolicyName = VietlifeStorePermissions.CamNang.View;
            GetListPolicyName = VietlifeStorePermissions.CamNang.View;
            CreatePolicyName = VietlifeStorePermissions.CamNang.Create;
            UpdatePolicyName = VietlifeStorePermissions.CamNang.Update;
            DeletePolicyName = VietlifeStorePermissions.CamNang.Delete;
            _camNangRepo = camNangRepo;
        }

        // ================= CREATE =================
        [Authorize(VietlifeStorePermissions.CamNang.Create)]
        public override async Task<DanhMucCamNangDto> CreateAsync(CreateUpdateDanhMucCamNangDto input)
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
            var entity = new DanhMucCamNang
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
        [Authorize(VietlifeStorePermissions.CamNang.Update)]
        public override async Task<DanhMucCamNangDto> UpdateAsync(Guid id, CreateUpdateDanhMucCamNangDto input)
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
                // Slug thay đổi → kiểm tra unique
                if (await Repository.AnyAsync(x => x.Slug == input.Slug && x.Id != id))
                    throw new UserFriendlyException("Slug đã tồn tại, vui lòng chọn slug khác.");

                entity.Slug = input.Slug;
            }
            // Nếu slug bị xóa trống → sinh lại từ tên (tùy chọn)
            else if (string.IsNullOrWhiteSpace(input.Slug) && !string.IsNullOrWhiteSpace(input.Ten))
            {
                entity.Slug = await GenerateUniqueSlugAsync(input.Ten);
            }
            await Repository.UpdateAsync(entity, autoSave: true);
            return MapToGetOutputDto(entity);
        }

        // ================= DELETE MULTIPLE =================
        [Authorize(VietlifeStorePermissions.CamNang.Delete)]
        public async Task DeleteMultipleAsync(IEnumerable<Guid> ids)
        {
            await Repository.DeleteManyAsync(ids);
            await UnitOfWorkManager.Current.SaveChangesAsync();
        }

        // ================= GET ALL ACTIVE =================
        [AllowAnonymous]
        public async Task<List<DanhMucCamNangInListDto>> GetListAllAsync()
        {
            var danhMucQueryable = await Repository.GetQueryableAsync();
            var camNangQueryable = await _camNangRepo.GetQueryableAsync(); // cần inject repo

            var query =
                from dm in danhMucQueryable
                where dm.TrangThai
                join cn in camNangQueryable
                    on dm.Id equals cn.DanhMucCamNangId into camNangGroup
                select new DanhMucCamNangInListDto
                {
                    Id = dm.Id,
                    Ten = dm.Ten,
                    Slug = dm.Slug,
                    SoLuongCamNang = camNangGroup.Count(x => x.TrangThai),
                    TitleSEO = dm.TitleSEO,
                    Keyword = dm.Keyword,
                    DescriptionSEO = dm.DescriptionSEO
                };

            return await AsyncExecuter.ToListAsync(query);
        }

        // ================= FILTER + PAGING =================
        [Authorize(VietlifeStorePermissions.CamNang.View)]
        public async Task<PagedResultDto<DanhMucCamNangInListDto>> GetListFilterAsync(BaseListFilterDto input)
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

            return new PagedResultDto<DanhMucCamNangInListDto>(
                totalCount,
                ObjectMapper.Map<List<DanhMucCamNang>, List<DanhMucCamNangInListDto>>(items)
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
                baseSlug = "danh-muc-" + DateTime.UtcNow.Ticks;

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
