using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VietlifeStore.Entity.LienHes;
using VietlifeStore.Entity.MediaContainers;
using VietlifeStore.Entity.SanPhamsList.SanPhamBienThes;
using VietlifeStore.Entity.SanPhamsList.SanPhams;
using VietlifeStore.Entity.SanPhamsList.ThuocTinhs;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.BlobStoring;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Uow;

namespace VietlifeStore.Entity.SanPhams
{
    [UnitOfWork]
    public class SanPhamsAppService :
        CrudAppService<SanPham, SanPhamDto, Guid, PagedResultRequestDto,
            CreateUpdateSanPhamDto, CreateUpdateSanPhamDto>,
        ISanPhamsAppService
    {
        private readonly IRepository<AnhSanPham, Guid> _anhRepo;
        private readonly IRepository<SanPhamBienThe, Guid> _bienTheRepo;
        private readonly IRepository<SanPhamBienTheThuocTinh, Guid> _bienTheThuocTinhRepo;
        private readonly IRepository<ThuocTinh, Guid> _thuocTinhRepo;
        private readonly IRepository<GiaTriThuocTinh, Guid> _giaTriRepo;
        private readonly IBlobContainer<MediaContainer> _media;

        public SanPhamsAppService(
            IRepository<SanPham, Guid> repository,
            IRepository<AnhSanPham, Guid> anhRepo,
            IRepository<SanPhamBienThe, Guid> bienTheRepo,
            IRepository<SanPhamBienTheThuocTinh, Guid> bienTheThuocTinhRepo,
            IRepository<ThuocTinh, Guid> thuocTinhRepo,
            IRepository<GiaTriThuocTinh, Guid> giaTriRepo,
            IBlobContainer<MediaContainer> media)
            : base(repository)
        {
            _anhRepo = anhRepo;
            _bienTheRepo = bienTheRepo;
            _bienTheThuocTinhRepo = bienTheThuocTinhRepo;
            _thuocTinhRepo = thuocTinhRepo;
            _giaTriRepo = giaTriRepo;
            _media = media;
        }

        // ======================================================
        // GET - Trả về đầy đủ thông tin để hiển thị khi edit
        // ======================================================
        public override async Task<SanPhamDto> GetAsync(Guid id)
        {
            var entity = await Repository.GetAsync(id);
            var dto = ObjectMapper.Map<SanPham, SanPhamDto>(entity);

            // Ảnh phụ - danh sách tên file
            var anhPhuList = await _anhRepo.GetListAsync(x => x.SanPhamId == id);
            dto.AnhPhu = anhPhuList.Select(x => x.Anh).ToList();

            // Thuộc tính
            var bienThes = await _bienTheRepo.GetListAsync(x => x.SanPhamId == id);
            var bienTheIds = bienThes.Select(x => x.Id).ToList();

            var mappings = await _bienTheThuocTinhRepo.GetListAsync(x => bienTheIds.Contains(x.SanPhamBienTheId));
            var giaTriIds = mappings.Select(x => x.GiaTriThuocTinhId).Distinct().ToList();
            var giaTris = await _giaTriRepo.GetListAsync(x => giaTriIds.Contains(x.Id));
            var thuocTinhIds = giaTris.Select(x => x.ThuocTinhId).Distinct().ToList();
            var thuocTinhs = await _thuocTinhRepo.GetListAsync(x => thuocTinhIds.Contains(x.Id));

            var thuocTinhResult = new List<ThuocTinhDto>();
            foreach (var tt in thuocTinhs)
            {
                var values = giaTris
                    .Where(gt => gt.ThuocTinhId == tt.Id)
                    .Select(gt => gt.GiaTri)
                    .Distinct()
                    .OrderBy(v => v)
                    .ToList();

                thuocTinhResult.Add(new ThuocTinhDto
                {
                    Ten = tt.Ten,
                    GiaTris = values
                });
            }
            dto.ThuocTinhs = thuocTinhResult;

            // Biến thể - đầy đủ thông tin
            dto.BienThes = bienThes.Select(b => new SanPhamBienTheDto
            {
                Id = b.Id,
                Ten = b.Ten,
                Gia = b.Gia,
                GiaKhuyenMai = b.GiaKhuyenMai,
            }).ToList();

            return dto;
        }

        // ================= CREATE =================
        public override async Task<SanPhamDto> CreateAsync(CreateUpdateSanPhamDto input)
        {
            if (string.IsNullOrWhiteSpace(input.Slug))
                input.Slug = GenerateSlug(input.Ten);

            var sanPham = ObjectMapper.Map<CreateUpdateSanPhamDto, SanPham>(input);
            await Repository.InsertAsync(sanPham, autoSave: true);

            // Ảnh đại diện
            if (!string.IsNullOrWhiteSpace(input.AnhDaiDienContent))
            {
                var avatar = $"avatar_{Guid.NewGuid()}_{input.AnhDaiDienName}";
                await SaveImageAsync(avatar, input.AnhDaiDienContent);
                sanPham.Anh = avatar;
            }

            // Ảnh phụ
            if (input.AnhPhu?.Any() == true)
            {
                await SaveSubImagesAsync(sanPham.Id, input.AnhPhu);
            }

            // Thuộc tính + biến thể
            if (input.ThuocTinhs?.Any() == true)
            {
                await RebuildVariantsAsync(sanPham.Id, input.ThuocTinhs, input.Gia);
            }
            // Nếu có biến thể gửi riêng (ít dùng khi tạo mới)
            else if (input.BienThes?.Any() == true)
            {
                await UpdateOrCreateVariantsAsync(sanPham.Id, input.BienThes);
            }

            await Repository.UpdateAsync(sanPham, autoSave: true);

            return await GetAsync(sanPham.Id);
        }

        // ================= UPDATE =================
        public override async Task<SanPhamDto> UpdateAsync(Guid id, CreateUpdateSanPhamDto input)
        {
            var entity = await Repository.GetAsync(id);

            if (!string.IsNullOrWhiteSpace(input.Slug) && input.Slug != entity.Slug)
            {
                if (await Repository.AnyAsync(x => x.Slug == input.Slug && x.Id != id))
                    throw new UserFriendlyException("Slug đã tồn tại");
                entity.Slug = input.Slug;
            }

            ObjectMapper.Map(input, entity);

            // Ảnh đại diện
            if (!string.IsNullOrWhiteSpace(input.AnhDaiDienContent))
            {
                if (!string.IsNullOrWhiteSpace(entity.Anh))
                    await _media.DeleteAsync(entity.Anh);

                var avatar = $"avatar_{Guid.NewGuid()}_{input.AnhDaiDienName}";
                await SaveImageAsync(avatar, input.AnhDaiDienContent);
                entity.Anh = avatar;
            }

            // Ảnh phụ - chỉ thay thế nếu frontend gửi ảnh mới
            if (input.AnhPhu?.Any() == true)
            {
                await ReplaceSubImagesAsync(id, input.AnhPhu); // Thay thế hết nếu có ảnh mới
            }
            else if (input.AnhPhuGiuLai?.Any() == true)
            {
                // Xóa những ảnh không nằm trong danh sách giữ lại
                var old = await _anhRepo.GetListAsync(x => x.SanPhamId == id);
                var toDelete = old.Where(x => !input.AnhPhuGiuLai.Contains(x.Anh)).ToList();
                foreach (var img in toDelete)
                    await _media.DeleteAsync(img.Anh);
                await _anhRepo.DeleteManyAsync(toDelete);
            }
            // Nếu input.AnhPhu == null hoặc rỗng → giữ nguyên ảnh cũ

            // Biến thể
            if (input.BienThes?.Any() == true)
            {
                await UpdateVariantsAsync(id, input.BienThes);
            }
            else if (input.ThuocTinhs != null)
            {
                await RebuildVariantsAsync(id, input.ThuocTinhs, input.Gia);
            }

            await Repository.UpdateAsync(entity, autoSave: true);
            return await GetAsync(id);
        }
        private async Task UpdateVariantsAsync(Guid sanPhamId, List<CreateUpdateSanPhamBienTheDto> inputBienThes)
        {
            var existing = await _bienTheRepo.GetListAsync(x => x.SanPhamId == sanPhamId);

            foreach (var inputBt in inputBienThes)
            {
                var exist = existing.FirstOrDefault(x => x.Ten == inputBt.Ten);
                if (exist != null)
                {
                    exist.Gia = inputBt.Gia;
                    exist.GiaKhuyenMai = inputBt.GiaKhuyenMai;
                    await _bienTheRepo.UpdateAsync(exist);
                }
            }
        }

        // ======================================================
        // Cập nhật hoặc tạo biến thể (khi frontend gửi danh sách)
        // ======================================================
        private async Task UpdateOrCreateVariantsAsync(Guid sanPhamId, List<CreateUpdateSanPhamBienTheDto> inputBienThes)
        {
            var existing = await _bienTheRepo.GetListAsync(x => x.SanPhamId == sanPhamId);
            var existingDict = existing.ToDictionary(x => x.Ten.Trim(), x => x);

            foreach (var inputBt in inputBienThes)
            {
                var key = inputBt.Ten.Trim();

                if (existingDict.TryGetValue(key, out var exist))
                {
                    // Cập nhật biến thể hiện có
                    exist.Gia = inputBt.Gia;
                    exist.GiaKhuyenMai = inputBt.GiaKhuyenMai;
                    await _bienTheRepo.UpdateAsync(exist);
                }
                else
                {
                    // Nếu có biến thể mới (ít xảy ra khi edit) - có thể insert hoặc bỏ qua
                    // Ở đây ta tạm thời bỏ qua để tránh phức tạp
                    // Nếu muốn hỗ trợ thêm biến thể khi edit → thêm logic insert + mapping
                }
            }

            // Có thể xóa biến thể không còn trong danh sách nếu cần
            // var toDelete = existing.Where(e => !inputBienThes.Any(i => i.Ten.Trim() == e.Ten.Trim())).ToList();
            // if (toDelete.Any())
            // {
            //     await _bienTheRepo.DeleteManyAsync(toDelete);
            //     await _bienTheThuocTinhRepo.DeleteAsync(x => toDelete.Select(d => d.Id).Contains(x.SanPhamBienTheId));
            // }
        }

        // ======================================================
        // Các phương thức cũ giữ nguyên hoặc nhỏ chỉnh sửa
        // ======================================================

        private async Task RebuildVariantsAsync(
            Guid sanPhamId,
            List<CreateUpdateThuocTinhWithGiaTriDto> thuocTinhs,
            decimal giaMacDinh)
        {
            // Xóa biến thể cũ
            var old = await _bienTheRepo.GetListAsync(x => x.SanPhamId == sanPhamId);
            await _bienTheRepo.DeleteManyAsync(old);
            await _bienTheThuocTinhRepo.DeleteAsync(x => old.Select(o => o.Id).Contains(x.SanPhamBienTheId));

            var allGiaTri = new List<List<GiaTriThuocTinh>>();

            foreach (var tt in thuocTinhs)
            {
                var thuocTinh = await _thuocTinhRepo.InsertAsync(new ThuocTinh { Ten = tt.Ten }, true);

                var giaTris = new List<GiaTriThuocTinh>();
                foreach (var val in tt.GiaTris)
                {
                    var gt = await _giaTriRepo.InsertAsync(
                        new GiaTriThuocTinh
                        {
                            ThuocTinhId = thuocTinh.Id,
                            GiaTri = val
                        }, true);
                    giaTris.Add(gt);
                }
                allGiaTri.Add(giaTris);
            }

            var combos = GenerateCombinations(allGiaTri);

            foreach (var combo in combos)
            {
                var bienThe = await _bienTheRepo.InsertAsync(
                    new SanPhamBienThe
                    {
                        SanPhamId = sanPhamId,
                        Ten = string.Join(" - ", combo.Select(x => x.GiaTri)),
                        Gia = giaMacDinh,
                    }, true);

                foreach (var gt in combo)
                {
                    await _bienTheThuocTinhRepo.InsertAsync(
                        new SanPhamBienTheThuocTinh
                        {
                            SanPhamBienTheId = bienThe.Id,
                            GiaTriThuocTinhId = gt.Id
                        }, true);
                }
            }
        }

        private async Task SaveSubImagesAsync(Guid productId, List<AnhUploadDto> images)
        {
            foreach (var img in images)
            {
                var name = $"sp_{Guid.NewGuid()}_{img.FileName}";
                await SaveImageAsync(name, img.Base64);
                await _anhRepo.InsertAsync(new AnhSanPham
                {
                    SanPhamId = productId,
                    Anh = name
                }, autoSave: true);
            }
        }

        private async Task ReplaceSubImagesAsync(Guid productId, List<AnhUploadDto> images)
        {
            await DeleteAllSubImagesAsync(productId);
            await SaveSubImagesAsync(productId, images);
        }

        private async Task DeleteAllSubImagesAsync(Guid productId)
        {
            var old = await _anhRepo.GetListAsync(x => x.SanPhamId == productId);
            foreach (var img in old)
                await _media.DeleteAsync(img.Anh);
            await _anhRepo.DeleteManyAsync(old);
        }

        private async Task SaveImageAsync(string fileName, string base64)
        {
            base64 = Regex.Replace(base64, @"^[\w/\:.-]+;base64,", "");
            await _media.SaveAsync(fileName, Convert.FromBase64String(base64), true);
        }

        private List<List<GiaTriThuocTinh>> GenerateCombinations(List<List<GiaTriThuocTinh>> lists)
        {
            var result = new List<List<GiaTriThuocTinh>> { new() };
            foreach (var list in lists)
            {
                result = result.SelectMany(r => list, (r, i) =>
                {
                    var n = new List<GiaTriThuocTinh>(r) { i };
                    return n;
                }).ToList();
            }
            return result;
        }

        private string GenerateSlug(string input)
        {
            var slug = input.ToLowerInvariant();
            slug = slug.Replace("áàảãạăắằẳẵặâấầẩẫậ", "a")
                       .Replace("éèẻẽẹêếềểễệ", "e")
                       .Replace("íìỉĩị", "i")
                       .Replace("óòỏõọôốồổỗộơớờởỡợ", "o")
                       .Replace("úùủũụưứừửữự", "u")
                       .Replace("ýỳỷỹỵ", "y")
                       .Replace("đ", "d");
            slug = Regex.Replace(slug, @"[^a-z0-9\s-]", "");
            return Regex.Replace(slug, @"\s+", "-").Trim('-');
        }

        public async Task<string> GetImageAsync(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return null;

            var bytes = await _media.GetAllBytesOrNullAsync(fileName);
            return bytes == null ? null : Convert.ToBase64String(bytes);
        }
        public async Task<string> GetThumbnailAsync(string fileName)
        {
            return await GetImageAsync(fileName);
        }
        public async Task<PagedResultDto<SanPhamInListDto>> GetListFilterAsync(BaseListFilterDto input)
        {
            var query = await Repository.GetQueryableAsync();
            query = query
                .WhereIf(!string.IsNullOrWhiteSpace(input.Keyword),
                    x => x.Ten.Contains(input.Keyword) || x.Slug.Contains(input.Keyword));

            var totalCount = await AsyncExecuter.CountAsync(query);
            var items = await AsyncExecuter.ToListAsync(
                query
                    .OrderByDescending(x => x.CreationTime)
                    .Skip(input.SkipCount)
                    .Take(input.MaxResultCount)
            );

            var result = ObjectMapper.Map<List<SanPham>, List<SanPhamInListDto>>(items);
            return new PagedResultDto<SanPhamInListDto>(totalCount, result);
        }
        public async Task<List<SanPhamInListDto>> GetListAllAsync()
        {
            var list = await Repository.GetListAsync();
            return ObjectMapper.Map<List<SanPham>, List<SanPhamInListDto>>(list);
        }

        public async Task DeleteMultipleAsync(IEnumerable<Guid> ids)
        {
            foreach (var id in ids)
            {
                await DeleteAsync(id);
            }
        }

        [AllowAnonymous]
        public async Task<List<SanPhamInListDto>> GetByDanhMucAsync(Guid danhMucId)
        {
            var entities = await Repository.GetListAsync(
                x => x.DanhMucId == danhMucId && x.TrangThai
            );

            var result = ObjectMapper.Map<
                List<SanPham>,
                List<SanPhamInListDto>
            >(entities);

            foreach (var item in result)
            {
                if (!string.IsNullOrWhiteSpace(item.Anh))
                {
                    var bytes = await _media.GetAllBytesOrNullAsync(item.Anh);
                    item.AnhDaiDienContent = bytes == null
                        ? null
                        : $"data:image/png;base64,{Convert.ToBase64String(bytes)}";
                }
            }

            return result;
        }

    }
}
