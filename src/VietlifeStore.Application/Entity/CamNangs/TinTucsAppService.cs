using AutoMapper.Internal.Mappers;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VietlifeStore.Permissions;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp;
using Microsoft.EntityFrameworkCore;
using VietlifeStore.Entity.CamNangsList.TinTucs;

namespace VietlifeStore.Entity.CamNangs
{
    public class TinTucsAppService :
        CrudAppService<
            TinTuc,
            TinTucDto,
            Guid,
            PagedResultRequestDto,
            CreateUpdateTinTucDto,
            CreateUpdateTinTucDto>,
        ITinTucsAppService
    {
        public TinTucsAppService(IRepository<TinTuc, Guid> repository)
            : base(repository)
        {
            GetPolicyName = VietlifeStorePermissions.TinTuc.View;
            GetListPolicyName = VietlifeStorePermissions.TinTuc.View;
            CreatePolicyName = VietlifeStorePermissions.TinTuc.Create;
            UpdatePolicyName = VietlifeStorePermissions.TinTuc.Update;
            DeletePolicyName = VietlifeStorePermissions.TinTuc.Delete;
        }

        // ================= CREATE =================
        public override async Task<TinTucDto> CreateAsync(CreateUpdateTinTucDto input)
        {
            if (string.IsNullOrWhiteSpace(input.Ten))
                throw new UserFriendlyException("Tên tin tức không được để trống");

            if (string.IsNullOrWhiteSpace(input.Slug))
                input.Slug = await GenerateUniqueSlugAsync(input.Ten);
            else if (await Repository.AnyAsync(x => x.Slug == input.Slug))
                throw new UserFriendlyException("Slug đã tồn tại");

            var entity = ObjectMapper.Map<CreateUpdateTinTucDto, TinTuc>(input);

            var created = await Repository.InsertAsync(entity, autoSave: true);
            return MapToGetOutputDto(created);
        }

        // ================= UPDATE =================
        public override async Task<TinTucDto> UpdateAsync(Guid id, CreateUpdateTinTucDto input)
        {
            var entity = await Repository.GetAsync(id);

            entity.Ten = input.Ten;
            entity.Mota = input.Mota;
            entity.TrangThai = input.TrangThai;
            entity.TitleSEO = input.TitleSEO;
            entity.Keyword = input.Keyword;
            entity.DescriptionSEO = input.DescriptionSEO;

            // SLUG
            if (!string.IsNullOrWhiteSpace(input.Slug) && input.Slug != entity.Slug)
            {
                if (await Repository.AnyAsync(x => x.Slug == input.Slug && x.Id != id))
                    throw new UserFriendlyException("Slug đã tồn tại");

                entity.Slug = input.Slug;
            }
            else if (string.IsNullOrWhiteSpace(input.Slug))
            {
                entity.Slug = await GenerateUniqueSlugAsync(input.Ten);
            }

            // ẢNH
            entity.Anh = input.Anh;

            await Repository.UpdateAsync(entity, autoSave: true);
            return MapToGetOutputDto(entity);
        }

        // ================= DELETE MULTIPLE =================
        public async Task DeleteMultipleAsync(IEnumerable<Guid> ids)
        {
            var list = await Repository.GetListAsync(x => ids.Contains(x.Id));
            await Repository.DeleteManyAsync(list);
        }

        // ================= GET ALL =================
        public async Task<List<TinTucInListDto>> GetListAllAsync()
        {
            var list = await AsyncExecuter.ToListAsync(
                (await Repository.GetQueryableAsync())
                .Where(x => x.TrangThai)
                .OrderByDescending(x => x.CreationTime)
            );

            return ObjectMapper.Map<List<TinTuc>, List<TinTucInListDto>>(list);
        }

        // ================= FILTER + PAGING =================
        [AllowAnonymous]
        public async Task<PagedResultDto<TinTucInListDto>> GetListFilterAsync(BaseListFilterDto input)
        {
            var query = (await Repository.GetQueryableAsync())
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(input.Keyword))
            {
                query = query.Where(x =>
                    x.Ten.Contains(input.Keyword) ||
                    x.Slug.Contains(input.Keyword));
            }

            var total = await AsyncExecuter.LongCountAsync(query);

            var items = await AsyncExecuter.ToListAsync(
                query.OrderByDescending(x => x.CreationTime)
                     .Skip(input.SkipCount)
                     .Take(input.MaxResultCount)
            );

            var result = items.Select(x => new TinTucInListDto
            {
                Id = x.Id,
                Ten = x.Ten,
                Slug = x.Slug,
                Anh = x.Anh,
                CreationTime = x.CreationTime,
                TrangThai = x.TrangThai,
                Mota = GetShortDescription(x.Mota, 200)
            }).ToList();

            return new PagedResultDto<TinTucInListDto>(total, result);
        }

        // ================= GET BY SLUG =================
        [AllowAnonymous]
        public async Task<TinTucDto> GetBySlugAsync(string slug)
        {
            var entity = await AsyncExecuter.FirstOrDefaultAsync(
                (await Repository.GetQueryableAsync())
                .Where(x => x.Slug == slug && x.TrangThai)
            );

            if (entity == null)
                throw new UserFriendlyException("Tin tức không tồn tại");

            return ObjectMapper.Map<TinTuc, TinTucDto>(entity);
        }

        // ================= HELPER =================
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
                baseSlug = "tin-tuc-" + DateTime.UtcNow.Ticks;

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
