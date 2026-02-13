using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VietlifeStore.Entity.DonHangs;
using VietlifeStore.Entity.LienHes;
using VietlifeStore.Entity.MediaContainers;
using VietlifeStore.Entity.SanPhamsList.SanPhamBienThes;
using VietlifeStore.Entity.SanPhamsList.SanPhams;
using VietlifeStore.Entity.SanPhamsList.ThuocTinhs;
using VietlifeStore.Entity.UploadFile;
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
        private readonly IMediaAppService _mediaAppService;
        private readonly IRepository<ChiTietDonHang, Guid> _chiTietDonHangRepository;
        private readonly IRepository<DanhMucSanPham, Guid> _danhMucRepo;

        public SanPhamsAppService(
        IRepository<SanPham, Guid> repository,
        IRepository<AnhSanPham, Guid> anhRepo,
        IRepository<SanPhamBienThe, Guid> bienTheRepo,
        IRepository<SanPhamBienTheThuocTinh, Guid> bienTheThuocTinhRepo,
        IRepository<ThuocTinh, Guid> thuocTinhRepo,
        IRepository<GiaTriThuocTinh, Guid> giaTriRepo,
        IRepository<ChiTietDonHang, Guid> chiTietDonHangRepository,
        IRepository<DanhMucSanPham, Guid> danhMucRepo,
        IMediaAppService mediaAppService)
        : base(repository)
        {
            _anhRepo = anhRepo;
            _bienTheRepo = bienTheRepo;
            _bienTheThuocTinhRepo = bienTheThuocTinhRepo;
            _thuocTinhRepo = thuocTinhRepo;
            _giaTriRepo = giaTriRepo;
            _chiTietDonHangRepository = chiTietDonHangRepository;
            _danhMucRepo = danhMucRepo;
            _mediaAppService = mediaAppService;
        }


        public override async Task<SanPhamDto> GetAsync(Guid id)
        {
            var entity = await Repository.GetAsync(id);
            var dto = ObjectMapper.Map<SanPham, SanPhamDto>(entity);
            var anhPhuList = await _anhRepo.GetListAsync(x => x.SanPhamId == id);
            dto.AnhPhu = anhPhuList.Select(x => x.Anh).ToList();
            var bienThes = await _bienTheRepo.GetListAsync(x => x.SanPhamId == id);
            var bienTheIds = bienThes.Select(x => x.Id).ToList();
            if (bienTheIds.Any())
            {
                var mappings = await _bienTheThuocTinhRepo.GetListAsync(x => bienTheIds.Contains(x.SanPhamBienTheId));
                var giaTriIds = mappings.Select(x => x.GiaTriThuocTinhId).Distinct().ToList();
                var giaTris = await _giaTriRepo.GetListAsync(x => giaTriIds.Contains(x.Id));
                var thuocTinhIds = giaTris.Select(x => x.ThuocTinhId).Distinct().ToList();
                var thuocTinhs = await _thuocTinhRepo.GetListAsync(x => thuocTinhIds.Contains(x.Id));
                dto.ThuocTinhs = thuocTinhs.Select(tt => new ThuocTinhDto
                {
                    Ten = tt.Ten,
                    GiaTris = giaTris
                        .Where(gt => gt.ThuocTinhId == tt.Id)
                        .Select(gt => gt.GiaTri)
                        .Distinct()
                        .OrderBy(v => v)
                        .ToList()
                }).ToList();
            }
            dto.BienThes = bienThes.Select(b => new SanPhamBienTheDto
            {
                Id = b.Id,
                Ten = b.Ten,
                Gia = b.Gia,
                GiaKhuyenMai = b.GiaKhuyenMai,
            }).ToList();
            return dto;
        }

        public override async Task<SanPhamDto> CreateAsync(CreateUpdateSanPhamDto input)
        {
            if (string.IsNullOrWhiteSpace(input.Ten))
                throw new UserFriendlyException("Tên sản phẩm không được để trống");
            if (string.IsNullOrWhiteSpace(input.Anh))
                throw new UserFriendlyException("Không thấy file ảnh");
            if (string.IsNullOrWhiteSpace(input.Slug))
                input.Slug = GenerateUniqueSlug(input.Ten);

            var sanPham = ObjectMapper.Map<CreateUpdateSanPhamDto, SanPham>(input);
            sanPham.Anh = input.Anh;

            await Repository.InsertAsync(sanPham, autoSave: true);

            if (input.AnhPhu?.Any() == true)
            {
                foreach (var fileName in input.AnhPhu)
                {
                    await _anhRepo.InsertAsync(new AnhSanPham
                    {
                        SanPhamId = sanPham.Id,
                        Anh = fileName
                    }, autoSave: true);
                }
            }
            if (input.ThuocTinhs != null && input.ThuocTinhs.Any())
            {
                await SyncVariantsFromAttributesAsync(
                    sanPham.Id,
                    input.ThuocTinhs,
                    input.Gia,
                    input.GiaKhuyenMai,
                    input.PhanTramKhuyenMai,
                    input.BienThes
                );
            }
            await Repository.UpdateAsync(sanPham, autoSave: true);
            return await GetAsync(sanPham.Id);
        }

        public override async Task<SanPhamDto> UpdateAsync(Guid id, CreateUpdateSanPhamDto input)
        {
            var entity = await Repository.GetAsync(id);

            var oldMainImage = entity.Anh;

            if (!string.IsNullOrWhiteSpace(input.Slug) && input.Slug != entity.Slug)
            {
                if (await Repository.AnyAsync(x => x.Slug == input.Slug && x.Id != id))
                    throw new UserFriendlyException("Slug đã tồn tại");
                entity.Slug = input.Slug;
            }
            ObjectMapper.Map(input, entity);
            // ===== Ảnh đại diện =====
            if (!string.IsNullOrWhiteSpace(input.Anh) && input.Anh != oldMainImage)
            {
                entity.Anh = input.Anh;

                // Xóa ảnh cũ
                if (!string.IsNullOrWhiteSpace(oldMainImage))
                {
                    try
                    {
                        await _mediaAppService.DeleteAsync(oldMainImage);
                    }
                    catch (Exception ex)
                    {
                        // Log nhưng không throw - không block update nếu xóa file thất bại
                        Logger.LogWarning(ex, $"Không thể xóa ảnh cũ: {oldMainImage}");
                    }
                }
            }

            // ===== Ảnh phụ =====
            var oldImages = await _anhRepo.GetListAsync(x => x.SanPhamId == id);
            var keepList = input.AnhPhuGiuLai ?? new List<string>();

            var imagesToDelete = oldImages
                .Where(x => !keepList.Contains(x.Anh))
                .ToList();

            foreach (var img in imagesToDelete)
                await _mediaAppService.DeleteAsync(img.Anh);

            await _anhRepo.DeleteManyAsync(imagesToDelete);

            // Thêm ảnh mới
            if (input.AnhPhu?.Any() == true)
            {
                foreach (var fileName in input.AnhPhu)
                {
                    await _anhRepo.InsertAsync(new AnhSanPham
                    {
                        SanPhamId = id,
                        Anh = fileName
                    }, autoSave: true);
                }
            }
            if (input.ThuocTinhs != null)
            {
                if (input.ThuocTinhs.Any())
                {
                    await SyncVariantsFromAttributesAsync(
                        id,
                        input.ThuocTinhs,
                        input.Gia,
                        input.GiaKhuyenMai,
                        input.PhanTramKhuyenMai,
                        input.BienThes
                    );
                }
                else
                {
                    // ⚡ XÓA HẾT biến thể nếu thuộc tính bị xóa toàn bộ
                    var variants = await _bienTheRepo.GetListAsync(x => x.SanPhamId == id);
                    if (variants.Any())
                    {
                        var variantIds = variants.Select(v => v.Id).ToList();
                        await _bienTheThuocTinhRepo.DeleteAsync(x => variantIds.Contains(x.SanPhamBienTheId));
                        await _bienTheRepo.DeleteManyAsync(variants);
                    }
                }
            }
            await Repository.UpdateAsync(entity, autoSave: true);
            return await GetAsync(id);
        }

        public override async Task DeleteAsync(Guid id)
        {
            var hasOrders = await _chiTietDonHangRepository.AnyAsync(x => x.SanPhamId == id);
            if (hasOrders)
                throw new UserFriendlyException("Không thể xóa sản phẩm vì đã có đơn hàng.");

            var product = await Repository.GetAsync(id);

            if (!string.IsNullOrWhiteSpace(product.Anh))
                await _mediaAppService.DeleteAsync(product.Anh);

            var subImages = await _anhRepo.GetListAsync(x => x.SanPhamId == id);

            foreach (var img in subImages)
                await _mediaAppService.DeleteAsync(img.Anh);

            await _anhRepo.DeleteManyAsync(subImages);

            await base.DeleteAsync(id);
        }


        private async Task SyncVariantsFromAttributesAsync(
            Guid sanPhamId,
            List<CreateUpdateThuocTinhWithGiaTriDto> thuocTinhInputs,
            decimal giaMacDinh,
            decimal? giaKhuyenMaiMacDinh = null, 
            decimal? phanTramKhuyenMai = null,
            List<CreateUpdateSanPhamBienTheDto>? inputBienThes = null)
        {
            var inputThuocTinhNames = thuocTinhInputs
                .Where(t => !string.IsNullOrWhiteSpace(t.Ten))
                .Select(t => t.Ten.Trim())
                .Distinct()
                .ToList();
            var existingThuocTinhs = await _thuocTinhRepo.GetListAsync(x => inputThuocTinhNames.Contains(x.Ten));
            var existingThuocTinhIds = existingThuocTinhs.Select(x => x.Id).ToList();
            var existingGiaTris = new List<GiaTriThuocTinh>();
            if (existingThuocTinhIds.Any())
            {
                existingGiaTris = await _giaTriRepo.GetListAsync(x => existingThuocTinhIds.Contains(x.ThuocTinhId));
            }
            var targetGiaTriByThuocTinh = new Dictionary<string, List<GiaTriThuocTinh>>();
            foreach (var ttInput in thuocTinhInputs.Where(t => !string.IsNullOrWhiteSpace(t.Ten)))
            {
                var tenTT = ttInput.Ten.Trim();
                var thuocTinh = existingThuocTinhs.FirstOrDefault(x => x.Ten.Equals(tenTT, StringComparison.OrdinalIgnoreCase));
                if (thuocTinh == null)
                {
                    thuocTinh = await _thuocTinhRepo.InsertAsync(new ThuocTinh { Ten = tenTT }, autoSave: true);
                    existingThuocTinhs.Add(thuocTinh);
                }
                else
                {
                    // THÊM: Cập nhật lại tên nếu user sửa cách viết hoa
                    if (thuocTinh.Ten != tenTT)
                    {
                        thuocTinh.Ten = tenTT;
                        await _thuocTinhRepo.UpdateAsync(thuocTinh, autoSave: true);
                    }
                }
                var giaTrisChoThuocTinhNay = new List<GiaTriThuocTinh>();
                foreach (var valRaw in ttInput.GiaTris.Where(v => !string.IsNullOrWhiteSpace(v)).Distinct())
                {
                    var val = valRaw.Trim();
                    var gt = existingGiaTris.FirstOrDefault(x => x.ThuocTinhId == thuocTinh.Id && x.GiaTri.Equals(val, StringComparison.OrdinalIgnoreCase));
                    if (gt == null)
                    {
                        gt = await _giaTriRepo.InsertAsync(new GiaTriThuocTinh
                        {
                            ThuocTinhId = thuocTinh.Id,
                            GiaTri = val
                        }, autoSave: true);
                        existingGiaTris.Add(gt);
                    }
                    giaTrisChoThuocTinhNay.Add(gt);
                }
                if (giaTrisChoThuocTinhNay.Any())
                    targetGiaTriByThuocTinh[thuocTinh.Ten] = giaTrisChoThuocTinhNay;
            }
            var targetCombinations = GenerateCombinations(targetGiaTriByThuocTinh.Values.ToList());
            var currentVariants = await _bienTheRepo.GetListAsync(x => x.SanPhamId == sanPhamId);
            var currentVariantIds = currentVariants.Select(v => v.Id).ToList();
            var currentMappings = new List<SanPhamBienTheThuocTinh>();
            if (currentVariantIds.Any())
            {
                currentMappings = await _bienTheThuocTinhRepo.GetListAsync(x => currentVariantIds.Contains(x.SanPhamBienTheId));
            }
            var processedVariantIds = new HashSet<Guid>();

            // Tính tỷ lệ giảm giá chung
            var tyLeGiam = 0m;
            if (phanTramKhuyenMai.HasValue && phanTramKhuyenMai > 0)
            {
                tyLeGiam = phanTramKhuyenMai.Value / 100m;
            }
            else if (giaMacDinh > 0 && giaKhuyenMaiMacDinh.HasValue && giaKhuyenMaiMacDinh > 0)
            {
                tyLeGiam = 1 - (giaKhuyenMaiMacDinh.Value / giaMacDinh);
            }

            foreach (var combo in targetCombinations)
            {
                var variantName = string.Join(" - ", combo.Select(g => g.GiaTri));
                var key = string.Join("|", combo.OrderBy(g => g.ThuocTinhId).Select(g => g.Id));
                var existingVariant = currentVariants
                    .FirstOrDefault(v => GetVariantKey(v, currentMappings, existingGiaTris) == key);
                if (existingVariant != null)
                {
                    existingVariant.Ten = variantName;
                    var inputVariant = inputBienThes?.FirstOrDefault(x => x.Ten == variantName);

                    if (inputVariant != null)
                    {
                        existingVariant.Gia = inputVariant.Gia;
                        existingVariant.GiaKhuyenMai = inputVariant.GiaKhuyenMai;
                    }
                    else
                    {
                        if (existingVariant.Gia <= 0)
                            existingVariant.Gia = giaMacDinh;

                        if (existingVariant.GiaKhuyenMai <= 0 && tyLeGiam > 0)
                            existingVariant.GiaKhuyenMai = Math.Round(existingVariant.Gia * (1 - tyLeGiam));
                    }

                    await _bienTheRepo.UpdateAsync(existingVariant, autoSave: true);
                    processedVariantIds.Add(existingVariant.Id);
                }
                else
                {
                    var giaKmMoi = Math.Round(giaMacDinh * (1 - tyLeGiam));
                    var newVariant = await _bienTheRepo.InsertAsync(new SanPhamBienThe
                    {
                        SanPhamId = sanPhamId,
                        Ten = variantName,
                        Gia = giaMacDinh,
                        GiaKhuyenMai = giaKmMoi
                    }, autoSave: true);
                    processedVariantIds.Add(newVariant.Id);
                    var newMappings = combo.Select(gt => new SanPhamBienTheThuocTinh
                    {
                        SanPhamBienTheId = newVariant.Id,
                        GiaTriThuocTinhId = gt.Id
                    }).ToList();
                    if (newMappings.Any())
                    {
                        await _bienTheThuocTinhRepo.InsertManyAsync(newMappings, autoSave: true);
                    }
                }
            }
            var variantsToDelete = currentVariants.Where(v => !processedVariantIds.Contains(v.Id)).ToList();
            if (variantsToDelete.Any())
            {
                var deleteIds = variantsToDelete.Select(x => x.Id).ToList();
                await _bienTheThuocTinhRepo.DeleteAsync(x => deleteIds.Contains(x.SanPhamBienTheId));
                await _bienTheRepo.DeleteManyAsync(variantsToDelete);
            }
        }

        private string GetVariantKey(
            SanPhamBienThe variant,
            List<SanPhamBienTheThuocTinh> allMappings,
            List<GiaTriThuocTinh> allGiaTris)
        {
            var gtIds = allMappings
                .Where(m => m.SanPhamBienTheId == variant.Id)
                .Select(m => m.GiaTriThuocTinhId)
                .OrderBy(id => id);
            return string.Join("|", gtIds);
        }

        private async Task SyncVariantsDirectlyAsync(
            Guid sanPhamId,
            List<CreateUpdateSanPhamBienTheDto> inputBienThes)
        {
            var existing = await _bienTheRepo.GetListAsync(x => x.SanPhamId == sanPhamId);
            var existingDict = existing.ToDictionary(x => x.Id);
            var inputIds = inputBienThes
                .Where(x => x.Id.HasValue)
                .Select(x => x.Id.Value)
                .ToHashSet();
            foreach (var input in inputBienThes)
            {
                if (input.Id.HasValue && existingDict.TryGetValue(input.Id.Value, out var exist))
                {
                    exist.Ten = input.Ten?.Trim() ?? exist.Ten;
                    exist.Gia = input.Gia;
                    exist.GiaKhuyenMai = input.GiaKhuyenMai ?? 0;
                    await _bienTheRepo.UpdateAsync(exist);
                    if (input.SanPhamBienTheThuocTinhDtos?.Any() == true)
                    {
                        await _bienTheThuocTinhRepo.DeleteAsync(x => x.SanPhamBienTheId == exist.Id);
                        foreach (var map in input.SanPhamBienTheThuocTinhDtos)
                        {
                            await _bienTheThuocTinhRepo.InsertAsync(new SanPhamBienTheThuocTinh
                            {
                                SanPhamBienTheId = exist.Id,
                                GiaTriThuocTinhId = map.GiaTriThuocTinhId
                            }, autoSave: true);
                        }
                    }
                }
                else
                {
                    var newVariant = await _bienTheRepo.InsertAsync(new SanPhamBienThe
                    {
                        SanPhamId = sanPhamId,
                        Ten = input.Ten?.Trim() ?? "Mới",
                        Gia = input.Gia,
                        GiaKhuyenMai = input.GiaKhuyenMai ?? 0
                    }, autoSave: true);
                    if (input.SanPhamBienTheThuocTinhDtos?.Any() == true)
                    {
                        foreach (var map in input.SanPhamBienTheThuocTinhDtos)
                        {
                            await _bienTheThuocTinhRepo.InsertAsync(new SanPhamBienTheThuocTinh
                            {
                                SanPhamBienTheId = newVariant.Id,
                                GiaTriThuocTinhId = map.GiaTriThuocTinhId
                            }, autoSave: true);
                        }
                    }
                }
            }
            var toDelete = existing.Where(x => !inputIds.Contains(x.Id)).ToList();
            if (toDelete.Any())
            {
                var deleteIds = toDelete.Select(x => x.Id).ToList();
                await _bienTheThuocTinhRepo.DeleteAsync(x => deleteIds.Contains(x.SanPhamBienTheId));
                await _bienTheRepo.DeleteManyAsync(toDelete);
            }
        }

        private List<List<GiaTriThuocTinh>> GenerateCombinations(List<List<GiaTriThuocTinh>> lists)
        {
            if (!lists.Any()) return new List<List<GiaTriThuocTinh>>();
            var result = new List<List<GiaTriThuocTinh>> { new() };
            foreach (var list in lists)
            {
                if (!list.Any()) continue;
                result = result.SelectMany(r => list, (r, i) => new List<GiaTriThuocTinh>(r) { i }).ToList();
            }
            return result;
        }

        private string GenerateUniqueSlug(string input)
        {
            var baseSlug = GenerateSlug(input);
            var slug = baseSlug;
            int counter = 1;
            while (Repository.AnyAsync(x => x.Slug == slug).GetAwaiter().GetResult())
            {
                slug = $"{baseSlug}-{counter++}";
            }
            return slug;
        }

        private string GenerateSlug(string input)
        {
            var slug = input.ToLowerInvariant()
                .Replace("áàảãạăắằẳẵặâấầẩẫậ", "a")
                .Replace("éèẻẽẹêếềểễệ", "e")
                .Replace("íìỉĩị", "i")
                .Replace("óòỏõọôốồổỗộơớờởỡợ", "o")
                .Replace("úùủũụưứừửữự", "u")
                .Replace("ýỳỷỹỵ", "y")
                .Replace("đ", "d");
            slug = Regex.Replace(slug, @"[^a-z0-9\s-]", "");
            slug = Regex.Replace(slug, @"\s+", "-").Trim('-');
            return slug.Length > 100 ? slug.Substring(0, 100) : slug;
        }

        [AllowAnonymous]
        public async Task<PagedResultDto<SanPhamInListDto>> GetListFilterAsync(BaseListFilterDto input)
        {
            var query = (await Repository.GetQueryableAsync())
                .Where(x => x.TrangThai);
            if (!string.IsNullOrWhiteSpace(input.DanhMucSlug))
            {
                var danhMuc = await _danhMucRepo.FirstOrDefaultAsync(x => x.Slug == input.DanhMucSlug);
                if (danhMuc != null)
                    query = query.Where(x => x.DanhMucId == danhMuc.Id);
            }
            if (!string.IsNullOrWhiteSpace(input.Keyword))
            {
                query = query.Where(x => x.Ten.Contains(input.Keyword) || x.Slug.Contains(input.Keyword));
            }
            query = input.Sort switch
            {
                "name_asc" => query.OrderBy(x => x.Ten),
                "name_desc" => query.OrderByDescending(x => x.Ten),
                "price_asc" => query.OrderBy(x => x.GiaKhuyenMai > 0 ? x.GiaKhuyenMai : x.Gia),
                "price_desc" => query.OrderByDescending(x => x.GiaKhuyenMai > 0 ? x.GiaKhuyenMai : x.Gia),
                "oldest" => query.OrderBy(x => x.CreationTime),
                _ => query.OrderByDescending(x => x.CreationTime)
            };
            var total = await AsyncExecuter.LongCountAsync(query);
            var items = await AsyncExecuter.ToListAsync(query.Skip(input.SkipCount).Take(input.MaxResultCount));
            var dtos = ObjectMapper.Map<List<SanPham>, List<SanPhamInListDto>>(items);
            foreach (var dto in dtos)
            {
                dto.PhanTramGiamGia = TinhPhanTramGiam(dto.Gia, dto.GiaKhuyenMai);
            }
            return new PagedResultDto<SanPhamInListDto>(total, dtos);
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

        private int? TinhPhanTramGiam(decimal gia, decimal giaKhuyenMai)
        {
            if (gia <= 0 || giaKhuyenMai <= 0 || giaKhuyenMai >= gia) return null;
            return (int)Math.Round((gia - giaKhuyenMai) * 100 / gia);
        }

        [AllowAnonymous]
        public async Task<List<SanPhamInListDto>> GetByDanhMucAsync(string slug)
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
                x => x.DanhMucId == danhMuc.Id && x.TrangThai
            );
            var result = ObjectMapper.Map<List<SanPham>, List<SanPhamInListDto>>(entities);
            foreach (var item in result)
            {
                item.PhanTramGiamGia = TinhPhanTramGiam(item.Gia, item.GiaKhuyenMai);
            }
            return result;
        }

        [AllowAnonymous]
        public async Task<List<SanPhamInListDto>> GetTopBanChayAsync(int top = 6)
        {
            if (top <= 0) top = 6;
            var chiTietQuery = await _chiTietDonHangRepository.GetQueryableAsync();
            chiTietQuery = chiTietQuery.Where(ct => ct.DonHang.TrangThai == 3);
            var banChayQuery = chiTietQuery
                .GroupBy(ct => ct.SanPhamId)
                .Select(g => new
                {
                    SanPhamId = g.Key,
                    TongSoLuong = g.Sum(ct => ct.SoLuong)
                })
                .OrderByDescending(x => x.TongSoLuong)
                .Take(top);
            var topSanPhamIds = await AsyncExecuter
                .ToListAsync(banChayQuery.Select(x => x.SanPhamId));
            if (!topSanPhamIds.Any())
            {
                return new List<SanPhamInListDto>();
            }
            var products = await Repository.GetListAsync(x =>
                topSanPhamIds.Contains(x.Id) && x.TrangThai);
            var dtos = ObjectMapper.Map<List<SanPham>, List<SanPhamInListDto>>(products);
            foreach (var dto in dtos)
            {
                dto.PhanTramGiamGia = TinhPhanTramGiam(dto.Gia, dto.GiaKhuyenMai);
            }
            var result = topSanPhamIds
                .Select(id => dtos.FirstOrDefault(p => p.Id == id))
                .Where(p => p != null)
                .ToList();
            return result;
        }

        [AllowAnonymous]
        public async Task<SanPhamDto> GetBySlugAsync(string slug)
        {
            if (string.IsNullOrWhiteSpace(slug))
            {
                throw new UserFriendlyException("Slug không hợp lệ");
            }
            var entity = await Repository.FirstOrDefaultAsync(x =>
                x.Slug == slug && x.TrangThai);
            if (entity == null)
            {
                throw new UserFriendlyException("Sản phẩm không tồn tại");
            }
            var dto = ObjectMapper.Map<SanPham, SanPhamDto>(entity);
            
            dto.PhanTramGiamGia = TinhPhanTramGiam(dto.Gia, dto.GiaKhuyenMai);
            var bienThes = await _bienTheRepo.GetListAsync(x => x.SanPhamId == entity.Id);
            dto.BienThes = bienThes.Select(b => new SanPhamBienTheDto
            {
                Id = b.Id,
                Ten = b.Ten,
                Gia = b.Gia,
                GiaKhuyenMai = b.GiaKhuyenMai
            }).ToList();
            var bienTheIds = bienThes.Select(x => x.Id).ToList();
            if (bienTheIds.Any())
            {
                var mappings = await _bienTheThuocTinhRepo
                    .GetListAsync(x => bienTheIds.Contains(x.SanPhamBienTheId));
                var giaTriIds = mappings
                    .Select(x => x.GiaTriThuocTinhId)
                    .Distinct()
                    .ToList();
                var giaTris = await _giaTriRepo
                    .GetListAsync(x => giaTriIds.Contains(x.Id));
                var thuocTinhIds = giaTris
                    .Select(x => x.ThuocTinhId)
                    .Distinct()
                    .ToList();
                var thuocTinhs = await _thuocTinhRepo
                    .GetListAsync(x => thuocTinhIds.Contains(x.Id));
                dto.ThuocTinhs = thuocTinhs.Select(tt => new ThuocTinhDto
                {
                    Ten = tt.Ten,
                    GiaTris = giaTris
                        .Where(gt => gt.ThuocTinhId == tt.Id)
                        .Select(gt => gt.GiaTri)
                        .Distinct()
                        .OrderBy(v => v)
                        .ToList()
                }).ToList();
            }
            return dto;
        }
    }
}