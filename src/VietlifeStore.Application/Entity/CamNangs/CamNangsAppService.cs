using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VietlifeStore.Entity.CamNangs;
using VietlifeStore.Entity.MediaContainers;
using VietlifeStore.Permissions;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.BlobStoring;
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
        private readonly IBlobContainer<MediaContainer> _mediaContainer;

        public CamNangsAppService(
            IRepository<CamNang, Guid> repository,
            IBlobContainer<MediaContainer> mediaContainer)
            : base(repository)
        {
            _mediaContainer = mediaContainer;

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
            var entity = new CamNang
            {
                Ten = input.Ten,
                Slug = input.Slug,
                Mota = input.Mota,
                DanhMucCamNangId = input.DanhMucCamNangId,
                TrangThai = input.TrangThai,
                TitleSEO = input.TitleSEO,
                Keyword = input.Keyword,
                DescriptionSEO = input.DescriptionSEO
            };

            await SaveImageAsync(entity, input);

            var created = await Repository.InsertAsync(entity, autoSave: true);
            return MapToGetOutputDto(created);
        }

        // ================= UPDATE =================
        [Authorize(VietlifeStorePermissions.CamNang.Update)]
        public override async Task<CamNangDto> UpdateAsync(Guid id, CreateUpdateCamNangDto input)
        {
            var entity = await Repository.GetAsync(id);

            entity.Ten = input.Ten;
            entity.Slug = input.Slug;
            entity.Mota = input.Mota;
            entity.DanhMucCamNangId = input.DanhMucCamNangId;
            entity.TrangThai = input.TrangThai;
            entity.TitleSEO = input.TitleSEO;
            entity.Keyword = input.Keyword;
            entity.DescriptionSEO = input.DescriptionSEO;

            await SaveImageAsync(entity, input);

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

        // ================= GET ALL =================
        [Authorize(VietlifeStorePermissions.CamNang.View)]
        public async Task<List<CamNangInListDto>> GetListAllAsync()
        {
            var list = await AsyncExecuter.ToListAsync(
                (await Repository.GetQueryableAsync())
                    .Where(x => x.TrangThai)
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
                query.Skip(input.SkipCount)
                     .Take(input.MaxResultCount)
            );

            return new PagedResultDto<CamNangInListDto>(
                totalCount,
                ObjectMapper.Map<List<CamNang>, List<CamNangInListDto>>(items)
            );
        }

        // ================= IMAGE =================
        private async Task SaveImageAsync(CamNang entity, CreateUpdateCamNangDto input)
        {
            if (string.IsNullOrWhiteSpace(input.AnhContent))
                return;

            await SaveImageAsync(input.AnhName, input.AnhContent);
            entity.Anh = input.AnhName;
        }

        private async Task SaveImageAsync(string fileName, string base64)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return;

            var regex = new Regex(@"^[\w/\:.-]+;base64,");
            base64 = regex.Replace(base64, string.Empty);

            var bytes = Convert.FromBase64String(base64);
            await _mediaContainer.SaveAsync(fileName, bytes, overrideExisting: true);
        }

        // ================= GET IMAGE =================
        public async Task<string?> GetImageAsync(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return null;

            var bytes = await _mediaContainer.GetAllBytesOrNullAsync(fileName);
            return bytes == null ? null : Convert.ToBase64String(bytes);
        }
    }
}
