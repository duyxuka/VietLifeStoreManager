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
using VietlifeStore.Entity.UploadFile;
using VietlifeStore.Permissions;
using Volo.Abp.Application.Services;
using Volo.Abp.BlobStoring;
using Volo.Abp.Domain.Repositories;

namespace VietlifeStore.Entity.SanPhams
{
    public class AnhSanPhamsAppService : ApplicationService
    {
        private readonly IRepository<AnhSanPham, Guid> _repository;
        private readonly IMediaAppService _mediaAppService;

        public AnhSanPhamsAppService(
            IRepository<AnhSanPham, Guid> repository,
            IMediaAppService mediaAppService)
        {
            _repository = repository;
            _mediaAppService = mediaAppService;
        }

        // ================= CREATE =================
        [Authorize(VietlifeStorePermissions.SanPham.Update)]
        public async Task<AnhSanPhamDto> CreateAsync(CreateUpdateAnhSanPhamDto input)
        {

            var entity = new AnhSanPham
            {
                SanPhamId = input.SanPhamId,
                Anh = input.Anh,
                Status = true,
                ThuTu = input.ThuTu
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
                await _mediaAppService.DeleteAsync(entity.Anh);
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
                    await _mediaAppService.DeleteAsync(item.Anh);
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
    }
}
