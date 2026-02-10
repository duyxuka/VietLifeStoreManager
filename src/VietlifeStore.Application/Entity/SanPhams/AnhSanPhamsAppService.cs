using AutoMapper.Internal.Mappers;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VietlifeStore.Entity.MediaContainers;
using VietlifeStore.Entity.SanPhamsList.AnhSanPhams;
using VietlifeStore.Permissions;
using Volo.Abp.Application.Services;
using Volo.Abp.BlobStoring;
using Volo.Abp.Domain.Repositories;

namespace VietlifeStore.Entity.SanPhams
{
    public class AnhSanPhamsAppService : ApplicationService
    {
        private readonly IRepository<AnhSanPham, Guid> _repository;
        private readonly IBlobContainer<MediaContainer> _mediaContainer;

        public AnhSanPhamsAppService(
            IRepository<AnhSanPham, Guid> repository,
            IBlobContainer<MediaContainer> mediaContainer)
        {
            _repository = repository;
            _mediaContainer = mediaContainer;
        }

        // ================= CREATE =================
        [Authorize(VietlifeStorePermissions.SanPham.Update)]
        public async Task<AnhSanPhamDto> CreateAsync(CreateUpdateAnhSanPhamDto input)
        {
            var fileName = $"{Guid.NewGuid()}_{input.AnhName}";

            await SaveImageAsync(fileName, input.AnhContent);

            var entity = new AnhSanPham
            {
                SanPhamId = input.SanPhamId,
                Anh = fileName, // chỉ tên file
                Status = true
            };

            await _repository.InsertAsync(entity, autoSave: true);

            return ObjectMapper.Map<AnhSanPham, AnhSanPhamDto>(entity);
        }

        // ================= DELETE =================
        [Authorize(VietlifeStorePermissions.SanPham.Update)]
        public async Task DeleteAsync(Guid id)
        {
            var entity = await _repository.GetAsync(id);

            if (!string.IsNullOrWhiteSpace(entity.Anh))
            {
                await _mediaContainer.DeleteAsync(entity.Anh);
            }

            await _repository.DeleteAsync(entity);
        }

        // ================= DELETE BY PRODUCT =================
        [Authorize(VietlifeStorePermissions.SanPham.Update)]
        public async Task DeleteBySanPhamAsync(Guid sanPhamId)
        {
            var list = await _repository.GetListAsync(x => x.SanPhamId == sanPhamId);

            foreach (var item in list)
            {
                if (!string.IsNullOrWhiteSpace(item.Anh))
                {
                    await _mediaContainer.DeleteAsync(item.Anh);
                }
            }

            await _repository.DeleteManyAsync(list);
        }

        // ================= GET LIST =================
        [Authorize(VietlifeStorePermissions.SanPham.View)]
        public async Task<List<AnhSanPhamDto>> GetListBySanPhamAsync(Guid sanPhamId)
        {
            var list = await _repository.GetListAsync(
                x => x.SanPhamId == sanPhamId && x.Status
            );

            return ObjectMapper.Map<List<AnhSanPham>, List<AnhSanPhamDto>>(list);
        }

        // ================= GET IMAGE =================
        public async Task<string> GetImageAsync(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return null;

            var bytes = await _mediaContainer.GetAllBytesOrNullAsync(fileName);
            return bytes == null ? null : Convert.ToBase64String(bytes);
        }

        // ================= PRIVATE =================
        private async Task SaveImageAsync(string fileName, string base64)
        {
            var regex = new Regex(@"^[\w/\:.-]+;base64,");
            base64 = regex.Replace(base64, string.Empty);

            var bytes = Convert.FromBase64String(base64);

            await _mediaContainer.SaveAsync(fileName, bytes, overrideExisting: true);
        }
    }
}
