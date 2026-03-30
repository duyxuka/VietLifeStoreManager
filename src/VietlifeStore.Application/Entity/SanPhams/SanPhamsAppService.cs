using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VietlifeStore.ChucNang.DatLichs.DatLichGiamGiaSanPhams;
using VietlifeStore.Entity.DonHangs;
using VietlifeStore.Entity.LienHes;
using VietlifeStore.Entity.MediaContainers;
using VietlifeStore.Entity.SanPhamsList.AnhSanPhams;
using VietlifeStore.Entity.SanPhamsList.SanPhamBienThes;
using VietlifeStore.Entity.SanPhamsList.SanPhams;
using VietlifeStore.Entity.SanPhamsList.ThuocTinhs;
using VietlifeStore.Entity.UploadFile;
using VietlifeStore.Permissions;
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
        private readonly IRepository<QuaTang, Guid> _quaTangRepo;

        public SanPhamsAppService(
        IRepository<SanPham, Guid> repository,
        IRepository<AnhSanPham, Guid> anhRepo,
        IRepository<SanPhamBienThe, Guid> bienTheRepo,
        IRepository<SanPhamBienTheThuocTinh, Guid> bienTheThuocTinhRepo,
        IRepository<ThuocTinh, Guid> thuocTinhRepo,
        IRepository<GiaTriThuocTinh, Guid> giaTriRepo,
        IRepository<ChiTietDonHang, Guid> chiTietDonHangRepository,
        IRepository<DanhMucSanPham, Guid> danhMucRepo,
        IRepository<QuaTang, Guid> quaTangRepo,
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
            _quaTangRepo = quaTangRepo;

            GetPolicyName = VietlifeStorePermissions.SanPham.View;
            GetListPolicyName = VietlifeStorePermissions.SanPham.View;
            CreatePolicyName = VietlifeStorePermissions.SanPham.Create;
            UpdatePolicyName = VietlifeStorePermissions.SanPham.Update;
            DeletePolicyName = VietlifeStorePermissions.SanPham.Delete;
        }


        public override async Task<SanPhamDto> GetAsync(Guid id)
        {
            var sanPhamQueryable = await Repository.GetQueryableAsync();
            var danhMucQueryable = await _danhMucRepo.GetQueryableAsync();
            var quaTangQueryable = await _quaTangRepo.GetQueryableAsync();

            var query =
                from sp in sanPhamQueryable
                join dm in danhMucQueryable
                    on sp.DanhMucId equals dm.Id
                join qt in quaTangQueryable
                    on sp.QuaTangId equals qt.Id into giftGroup
                from gift in giftGroup.DefaultIfEmpty()
                where sp.Id == id
                select new
                {
                    SanPham = sp,
                    DanhMuc = dm,
                    QuaTang = gift
                };

            var item = await AsyncExecuter.FirstOrDefaultAsync(query);

            if (item == null)
                throw new UserFriendlyException("Không tìm thấy sản phẩm");

            var dto = ObjectMapper.Map<SanPham, SanPhamDto>(item.SanPham);

            dto.DanhMucSlug = item.DanhMuc.Slug;
            dto.QuaTangTen = item.QuaTang?.Ten;
            dto.QuaTangGia = item.QuaTang?.Gia;
            dto.PhanTramGiamGia = TinhPhanTramGiam(dto.Gia, dto.GiaKhuyenMai);

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

        [Authorize(VietlifeStorePermissions.SanPham.Create)]
        public override async Task<SanPhamDto> CreateAsync(CreateUpdateSanPhamDto input)
        {
            if (string.IsNullOrWhiteSpace(input.Ten))
                throw new UserFriendlyException("Tên sản phẩm không được để trống");
            if (string.IsNullOrWhiteSpace(input.Anh))
                throw new UserFriendlyException("Không thấy file ảnh");
            if (string.IsNullOrWhiteSpace(input.Slug))
                input.Slug = await GenerateUniqueSlugAsync(input.Ten);

            var sanPham = ObjectMapper.Map<CreateUpdateSanPhamDto, SanPham>(input);
            sanPham.Anh = input.Anh;

            await Repository.InsertAsync(sanPham, autoSave: true);

            if (input.AnhPhu?.Any() == true)
            {
                var images = input.AnhPhu
                    .Select(x => new AnhSanPham { SanPhamId = sanPham.Id, Anh = x })
                    .ToList();

                await _anhRepo.InsertManyAsync(images, autoSave: true);
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

        [Authorize(VietlifeStorePermissions.SanPham.Update)]
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
                var newImages = input.AnhPhu
                    .Select(fileName => new AnhSanPham
                    {
                        SanPhamId = id,
                        Anh = fileName
                    })
                    .ToList();

                await _anhRepo.InsertManyAsync(newImages, autoSave: true);
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

        [Authorize(VietlifeStorePermissions.SanPham.Delete)]
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
            if (!thuocTinhInputs.Any())
                return;

            var inputThuocTinhNames = thuocTinhInputs
                .Where(t => !string.IsNullOrWhiteSpace(t.Ten))
                .Select(t => t.Ten.Trim())
                .Distinct()
                .ToList();

            var existingThuocTinhs = await _thuocTinhRepo.GetListAsync(x => inputThuocTinhNames.Contains(x.Ten));

            var thuocTinhDict = existingThuocTinhs
                .ToDictionary(x => x.Ten, StringComparer.OrdinalIgnoreCase);

            var existingGiaTris = new List<GiaTriThuocTinh>();

            if (existingThuocTinhs.Any())
            {
                var ids = existingThuocTinhs.Select(x => x.Id).ToList();
                existingGiaTris = await _giaTriRepo.GetListAsync(x => ids.Contains(x.ThuocTinhId));
            }

            var giaTriLookup = existingGiaTris
                .GroupBy(x => x.ThuocTinhId)
                .ToDictionary(g => g.Key, g => g.ToList());

            var targetGiaTriByThuocTinh = new Dictionary<string, List<GiaTriThuocTinh>>();

            foreach (var ttInput in thuocTinhInputs.Where(t => !string.IsNullOrWhiteSpace(t.Ten)))
            {
                var tenTT = ttInput.Ten.Trim();

                if (!thuocTinhDict.TryGetValue(tenTT, out var thuocTinh))
                {
                    thuocTinh = await _thuocTinhRepo.InsertAsync(
                        new ThuocTinh { Ten = tenTT },
                        autoSave: false);

                    thuocTinhDict[tenTT] = thuocTinh;
                }

                if (!giaTriLookup.TryGetValue(thuocTinh.Id, out var giaTriList))
                {
                    giaTriList = new List<GiaTriThuocTinh>();
                    giaTriLookup[thuocTinh.Id] = giaTriList;
                }

                var giaTrisChoThuocTinhNay = new List<GiaTriThuocTinh>();

                foreach (var valRaw in ttInput.GiaTris.Where(v => !string.IsNullOrWhiteSpace(v)).Distinct())
                {
                    var val = valRaw.Trim();

                    var gt = giaTriList
                        .FirstOrDefault(x => x.GiaTri.Equals(val, StringComparison.OrdinalIgnoreCase));

                    if (gt == null)
                    {
                        gt = await _giaTriRepo.InsertAsync(new GiaTriThuocTinh
                        {
                            ThuocTinhId = thuocTinh.Id,
                            GiaTri = val
                        }, autoSave: false);

                        giaTriList.Add(gt);
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

            var currentMappings = currentVariantIds.Any()
                ? await _bienTheThuocTinhRepo.GetListAsync(x => currentVariantIds.Contains(x.SanPhamBienTheId))
                : new List<SanPhamBienTheThuocTinh>();

            var mappingLookup = currentMappings
                .GroupBy(x => x.SanPhamBienTheId)
                .ToDictionary(g => g.Key, g => g.Select(x => x.GiaTriThuocTinhId).OrderBy(x => x).ToList());

            var variantKeyLookup = currentVariants.ToDictionary(
                v => v.Id,
                v => mappingLookup.ContainsKey(v.Id)
                    ? string.Join("|", mappingLookup[v.Id])
                    : ""
            );

            var processedVariantIds = new HashSet<Guid>();

            decimal tyLeGiam = 0m;

            if (phanTramKhuyenMai.HasValue && phanTramKhuyenMai > 0)
                tyLeGiam = phanTramKhuyenMai.Value / 100m;
            else if (giaMacDinh > 0 && giaKhuyenMaiMacDinh.HasValue && giaKhuyenMaiMacDinh > 0)
                tyLeGiam = 1 - (giaKhuyenMaiMacDinh.Value / giaMacDinh);

            foreach (var combo in targetCombinations)
            {
                var variantName = string.Join(" - ", combo.Select(g => g.GiaTri));

                var key = string.Join("|", combo
                    .OrderBy(g => g.ThuocTinhId)
                    .Select(g => g.Id));

                var existingVariant = currentVariants
                    .FirstOrDefault(v => variantKeyLookup[v.Id] == key);

                if (existingVariant != null)
                {
                    existingVariant.Ten = variantName;

                    var inputVariant = inputBienThes?
                        .FirstOrDefault(x => x.Ten == variantName);

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
                    }, autoSave: false);

                    processedVariantIds.Add(newVariant.Id);

                    var newMappings = combo.Select(gt => new SanPhamBienTheThuocTinh
                    {
                        SanPhamBienTheId = newVariant.Id,
                        GiaTriThuocTinhId = gt.Id
                    }).ToList();

                    if (newMappings.Any())
                        await _bienTheThuocTinhRepo.InsertManyAsync(newMappings, autoSave: false);
                }
            }

            var variantsToDelete = currentVariants
                .Where(v => !processedVariantIds.Contains(v.Id))
                .ToList();

            if (variantsToDelete.Any())
            {
                var deleteIds = variantsToDelete.Select(x => x.Id).ToList();

                await _bienTheThuocTinhRepo.DeleteAsync(x => deleteIds.Contains(x.SanPhamBienTheId));
                await _bienTheRepo.DeleteManyAsync(variantsToDelete);
            }

            await CurrentUnitOfWork.SaveChangesAsync();
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

        //private async Task SyncVariantsDirectlyAsync(
        //    Guid sanPhamId,
        //    List<CreateUpdateSanPhamBienTheDto> inputBienThes)
        //{
        //    var existing = await _bienTheRepo.GetListAsync(x => x.SanPhamId == sanPhamId);
        //    var existingDict = existing.ToDictionary(x => x.Id);
        //    var inputIds = inputBienThes
        //        .Where(x => x.Id.HasValue)
        //        .Select(x => x.Id.Value)
        //        .ToHashSet();
        //    foreach (var input in inputBienThes)
        //    {
        //        if (input.Id.HasValue && existingDict.TryGetValue(input.Id.Value, out var exist))
        //        {
        //            exist.Ten = input.Ten?.Trim() ?? exist.Ten;
        //            exist.Gia = input.Gia;
        //            exist.GiaKhuyenMai = input.GiaKhuyenMai ?? 0;
        //            await _bienTheRepo.UpdateAsync(exist);
        //            if (input.SanPhamBienTheThuocTinhDtos?.Any() == true)
        //            {
        //                await _bienTheThuocTinhRepo.DeleteAsync(x => x.SanPhamBienTheId == exist.Id);
        //                foreach (var map in input.SanPhamBienTheThuocTinhDtos)
        //                {
        //                    await _bienTheThuocTinhRepo.InsertAsync(new SanPhamBienTheThuocTinh
        //                    {
        //                        SanPhamBienTheId = exist.Id,
        //                        GiaTriThuocTinhId = map.GiaTriThuocTinhId
        //                    }, autoSave: true);
        //                }
        //            }
        //        }
        //        else
        //        {
        //            var newVariant = await _bienTheRepo.InsertAsync(new SanPhamBienThe
        //            {
        //                SanPhamId = sanPhamId,
        //                Ten = input.Ten?.Trim() ?? "Mới",
        //                Gia = input.Gia,
        //                GiaKhuyenMai = input.GiaKhuyenMai ?? 0
        //            }, autoSave: true);
        //            if (input.SanPhamBienTheThuocTinhDtos?.Any() == true)
        //            {
        //                foreach (var map in input.SanPhamBienTheThuocTinhDtos)
        //                {
        //                    await _bienTheThuocTinhRepo.InsertAsync(new SanPhamBienTheThuocTinh
        //                    {
        //                        SanPhamBienTheId = newVariant.Id,
        //                        GiaTriThuocTinhId = map.GiaTriThuocTinhId
        //                    }, autoSave: true);
        //                }
        //            }
        //        }
        //    }
        //    var toDelete = existing.Where(x => !inputIds.Contains(x.Id)).ToList();
        //    if (toDelete.Any())
        //    {
        //        var deleteIds = toDelete.Select(x => x.Id).ToList();
        //        await _bienTheThuocTinhRepo.DeleteAsync(x => deleteIds.Contains(x.SanPhamBienTheId));
        //        await _bienTheRepo.DeleteManyAsync(toDelete);
        //    }
        //}

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

        private async Task<string> GenerateUniqueSlugAsync(string input)
        {
            var baseSlug = RemoveVietnamese(input);
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

        [AllowAnonymous]
        public async Task<PagedResultDto<SanPhamInListDto>> GetListFilterAsync(BaseListFilterDto input)
        {
            var sanPhamQueryable = await Repository.GetQueryableAsync();
            var danhMucQueryable = await _danhMucRepo.GetQueryableAsync();
            var quaTangQueryable = await _quaTangRepo.GetQueryableAsync();
            var bienTheQueryable = await _bienTheRepo.GetQueryableAsync();

            var query =
                from sp in sanPhamQueryable
                join dm in danhMucQueryable on sp.DanhMucId equals dm.Id
                join qt in quaTangQueryable on sp.QuaTangId equals qt.Id into giftGroup
                from gift in giftGroup.DefaultIfEmpty()
                select new
                {
                    sp,
                    dm,
                    gift
                };

            if (!string.IsNullOrWhiteSpace(input.DanhMucSlug))
                query = query.Where(x => x.dm.Slug == input.DanhMucSlug);

            if (!string.IsNullOrWhiteSpace(input.Keyword))
                query = query.Where(x =>
                    x.sp.Ten.Contains(input.Keyword) ||
                    x.sp.Slug.Contains(input.Keyword));

            query = input.Sort switch
            {
                "name_asc" => query.OrderBy(x => x.sp.Ten),
                "name_desc" => query.OrderByDescending(x => x.sp.Ten),

                "price_asc" => query.OrderBy(x =>
                    x.sp.GiaKhuyenMai > 0 ? x.sp.GiaKhuyenMai : x.sp.Gia),

                "price_desc" => query.OrderByDescending(x =>
                    x.sp.GiaKhuyenMai > 0 ? x.sp.GiaKhuyenMai : x.sp.Gia),

                "oldest" => query.OrderBy(x => x.sp.CreationTime),

                _ => query
                    .OrderBy(x => x.sp.ThuTu ?? int.MaxValue)
                    .ThenByDescending(x => x.sp.CreationTime)
            };

            var total = await AsyncExecuter.LongCountAsync(query);

            var result = await AsyncExecuter.ToListAsync(
                query
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount)
                .Select(x => new SanPhamInListDto
                {
                    Id = x.sp.Id,
                    Ten = x.sp.Ten,
                    Slug = x.sp.Slug,

                    Gia = x.sp.Gia,
                    GiaKhuyenMai = x.sp.GiaKhuyenMai,
                    Anh = x.sp.Anh,

                    MoTaNgan = x.sp.MoTaNgan,

                    DanhMucSlug = x.dm.Slug,

                    QuaTangTen = x.gift != null ? x.gift.Ten : null,
                    QuaTangGia = x.gift != null ? x.gift.Gia : null,

                    ThuTu = x.sp.ThuTu,
                    LuotXem = x.sp.LuotXem,
                    LuotMua = x.sp.LuotMua,

                    // CHECK CÓ BIẾN THỂ
                    HasVariants = bienTheQueryable.Any(bt => bt.SanPhamId == x.sp.Id),

                    // GIÁ BIẾN THỂ MIN
                    GiaBienTheMin = bienTheQueryable
                        .Where(bt => bt.SanPhamId == x.sp.Id)
                        .Select(bt => (decimal?)bt.Gia)
                        .Min(),

                        // GIÁ BIẾN THỂ MAX
                    GiaBienTheMax = bienTheQueryable
                        .Where(bt => bt.SanPhamId == x.sp.Id)
                        .Select(bt => (decimal?)bt.Gia)
                        .Max(),

                        // GIÁ KHUYẾN MÃI BIẾN THỂ MIN
                    GiaKhuyenMaiBienTheMin = bienTheQueryable
                        .Where(bt => bt.SanPhamId == x.sp.Id && bt.GiaKhuyenMai > 0)
                        .Select(bt => (decimal?)bt.GiaKhuyenMai)
                        .Min(),

                         // GIÁ KHUYẾN MÃI BIẾN THỂ MAX
                    GiaKhuyenMaiBienTheMax = bienTheQueryable
                        .Where(bt => bt.SanPhamId == x.sp.Id && bt.GiaKhuyenMai > 0)
                        .Select(bt => (decimal?)bt.GiaKhuyenMai)
                        .Max(),

                        // GIẢM GIÁ SẢN PHẨM KHÔNG BIẾN THỂ
                    PhanTramGiamGia = TinhPhanTramGiam(
                            x.sp.Gia,
                            x.sp.GiaKhuyenMai
                    ),
                    PhanTramGiamGiaBienThe = bienTheQueryable
                        .Where(bt => bt.SanPhamId == x.sp.Id && bt.GiaKhuyenMai > 0 && bt.Gia > 0)
                        .Select(bt => (int?)Math.Round((decimal)((1 - bt.GiaKhuyenMai / bt.Gia) * 100)))
                        .Max()
                })
            );

            return new PagedResultDto<SanPhamInListDto>(total, result);
        }

        [Authorize(VietlifeStorePermissions.SanPham.View)]
        public async Task<List<SanPhamInListDto>> GetListAllAsync()
        {
            var list = await Repository.GetListAsync();
            return ObjectMapper.Map<List<SanPham>, List<SanPhamInListDto>>(list);
        }

        [Authorize(VietlifeStorePermissions.SanPham.View)]
        public async Task<List<SanPhamSelectDto>> GetListSelectAsync()
        {
            var query = await Repository.GetQueryableAsync();

            var result = await AsyncExecuter.ToListAsync(
                query
                .Where(x => x.TrangThai)
                .OrderBy(x => x.Ten)
                .Select(x => new SanPhamSelectDto
                {
                    Id = x.Id,
                    Ten = x.Ten
                })
            );

            return result;
        }

        public async Task DeleteMultipleAsync(IEnumerable<Guid> ids)
        {
            foreach (var id in ids)
            {
                await DeleteAsync(id);
            }
        }

        private static int? TinhPhanTramGiam(decimal gia, decimal giaKhuyenMai)
        {
            if (gia <= 0 || giaKhuyenMai <= 0 || giaKhuyenMai >= gia) return null;
            return (int)Math.Round((gia - giaKhuyenMai) * 100 / gia);
        }

        [AllowAnonymous]
        public async Task<List<SanPhamInListDto>> GetByDanhMucAsync(string slug)
        {
            if (string.IsNullOrWhiteSpace(slug))
                throw new UserFriendlyException("Slug danh mục không hợp lệ");

            var sanPhamQueryable = await Repository.GetQueryableAsync();
            var danhMucQueryable = await _danhMucRepo.GetQueryableAsync();
            var quaTangQueryable = await _quaTangRepo.GetQueryableAsync();
            var bienTheQueryable = await _bienTheRepo.GetQueryableAsync();

            var query =
            from sp in sanPhamQueryable
            join dm in danhMucQueryable
                on sp.DanhMucId equals dm.Id
            join qt in quaTangQueryable
                on sp.QuaTangId equals qt.Id into giftGroup
            from gift in giftGroup.DefaultIfEmpty()
            where dm.Slug == slug && sp.TrangThai
            orderby sp.ThuTu ?? int.MaxValue, sp.CreationTime descending
            select new SanPhamInListDto
            {
                Id = sp.Id,
                Ten = sp.Ten,
                Slug = sp.Slug,
                Gia = sp.Gia,
                GiaKhuyenMai = sp.GiaKhuyenMai,
                Anh = sp.Anh,
                LuotXem = sp.LuotXem,
                LuotMua = sp.LuotMua,
                MoTaNgan = sp.MoTaNgan,
                DanhMucSlug = dm.Slug,
                QuaTangTen = gift != null ? gift.Ten : null,
                QuaTangGia = gift != null ? gift.Gia : null,

                HasVariants = bienTheQueryable.Any(bt => bt.SanPhamId == sp.Id),


                // GIÁ BIẾN THỂ MIN
                GiaBienTheMin = bienTheQueryable
                        .Where(bt => bt.SanPhamId == sp.Id)
                        .Select(bt => (decimal?)bt.Gia)
                        .Min(),

                // GIÁ BIẾN THỂ MAX
                GiaBienTheMax = bienTheQueryable
                        .Where(bt => bt.SanPhamId == sp.Id)
                        .Select(bt => (decimal?)bt.Gia)
                        .Max(),

                // GIÁ KHUYẾN MÃI BIẾN THỂ MIN
                GiaKhuyenMaiBienTheMin = bienTheQueryable
                        .Where(bt => bt.SanPhamId == sp.Id && bt.GiaKhuyenMai > 0)
                        .Select(bt => (decimal?)bt.GiaKhuyenMai)
                        .Min(),

                // GIÁ KHUYẾN MÃI BIẾN THỂ MAX
                GiaKhuyenMaiBienTheMax = bienTheQueryable
                        .Where(bt => bt.SanPhamId == sp.Id && bt.GiaKhuyenMai > 0)
                        .Select(bt => (decimal?)bt.GiaKhuyenMai)
                        .Max(),

                // GIẢM GIÁ SẢN PHẨM KHÔNG BIẾN THỂ
                PhanTramGiamGia = TinhPhanTramGiam(
                            sp.Gia,
                            sp.GiaKhuyenMai
                    ),
                PhanTramGiamGiaBienThe = bienTheQueryable
                        .Where(bt => bt.SanPhamId == sp.Id && bt.GiaKhuyenMai > 0 && bt.Gia > 0)
                        .Select(bt => (int?)Math.Round((decimal)((1 - bt.GiaKhuyenMai / bt.Gia) * 100)))
                        .Max()
            };

            return await AsyncExecuter.ToListAsync(query);
        }

        [AllowAnonymous]
        public async Task<List<SanPhamInListDto>> GetTopBanChayAsync(int top = 6)
        {
            if (top <= 0) top = 6;

            var chiTietQuery = await _chiTietDonHangRepository.GetQueryableAsync();
            var sanPhamQuery = await Repository.GetQueryableAsync();
            var quaTangQuery = await _quaTangRepo.GetQueryableAsync();
            var bienTheQuery = await _bienTheRepo.GetQueryableAsync();

            var query =
                from ct in chiTietQuery
                where ct.DonHang.TrangThai == 3
                group ct by ct.SanPhamId into g
                orderby g.Sum(x => x.SoLuong) descending
                select new
                {
                    SanPhamId = g.Key,
                    TongSoLuong = g.Sum(x => x.SoLuong)
                };

            var resultQuery =
                from topSp in query.Take(top)

                join sp in sanPhamQuery
                    on topSp.SanPhamId equals sp.Id

                join qt in quaTangQuery
                    on sp.QuaTangId equals qt.Id into giftGroup
                from gift in giftGroup.DefaultIfEmpty()

                join bt in bienTheQuery
                    on sp.Id equals bt.SanPhamId into btGroup

                select new SanPhamInListDto
                {
                    Id = sp.Id,
                    Ten = sp.Ten,
                    Slug = sp.Slug,
                    Gia = sp.Gia,
                    GiaKhuyenMai = sp.GiaKhuyenMai,
                    Anh = sp.Anh,
                    LuotXem = sp.LuotXem,
                    LuotMua = sp.LuotMua,

                    QuaTangTen = gift != null ? gift.Ten : null,
                    QuaTangGia = gift != null ? gift.Gia : (decimal?)null,

                    HasVariants = btGroup.Any(),

                    // GIÁ BIẾN THỂ MIN
                    GiaBienTheMin = bienTheQuery
                        .Where(bt => bt.SanPhamId == sp.Id)
                        .Select(bt => (decimal?)bt.Gia)
                        .Min(),

                    // GIÁ BIẾN THỂ MAX
                    GiaBienTheMax = bienTheQuery
                        .Where(bt => bt.SanPhamId == sp.Id)
                        .Select(bt => (decimal?)bt.Gia)
                        .Max(),

                    // GIÁ KHUYẾN MÃI BIẾN THỂ MIN
                    GiaKhuyenMaiBienTheMin = bienTheQuery
                        .Where(bt => bt.SanPhamId == sp.Id && bt.GiaKhuyenMai > 0)
                        .Select(bt => (decimal?)bt.GiaKhuyenMai)
                        .Min(),

                    // GIÁ KHUYẾN MÃI BIẾN THỂ MAX
                    GiaKhuyenMaiBienTheMax = bienTheQuery
                        .Where(bt => bt.SanPhamId == sp.Id && bt.GiaKhuyenMai > 0)
                        .Select(bt => (decimal?)bt.GiaKhuyenMai)
                        .Max(),

                    // GIẢM GIÁ SẢN PHẨM KHÔNG BIẾN THỂ
                    PhanTramGiamGia = TinhPhanTramGiam(
                            sp.Gia,
                            sp.GiaKhuyenMai
                    ),
                    PhanTramGiamGiaBienThe = bienTheQuery
                        .Where(bt => bt.SanPhamId == sp.Id && bt.GiaKhuyenMai > 0 && bt.Gia > 0)
                        .Select(bt => (int?)Math.Round((decimal)((1 - bt.GiaKhuyenMai / bt.Gia) * 100)))
                        .Max()
                };

            return await AsyncExecuter.ToListAsync(resultQuery);
        }

        [Authorize(VietlifeStorePermissions.SanPham.Update)]
        public async Task UpdateThuTu(Guid id, int? thuTu)
        {
            var entity = await Repository.GetAsync(id);

            entity.ThuTu = thuTu;

            await Repository.UpdateAsync(entity);
        }

        [AllowAnonymous]
        public async Task<SanPhamDto> GetBySlugAsync(string slug)
        {
            if (string.IsNullOrWhiteSpace(slug))
                throw new UserFriendlyException("Slug không hợp lệ");

            var sanPhamQueryable = await Repository.GetQueryableAsync();
            var danhMucQueryable = await _danhMucRepo.GetQueryableAsync();
            var quaTangQueryable = await _quaTangRepo.GetQueryableAsync();

            // QUERY 1: sản phẩm
            var item = await AsyncExecuter.FirstOrDefaultAsync(
                from sp in sanPhamQueryable
                join dm in danhMucQueryable on sp.DanhMucId equals dm.Id
                join qt in quaTangQueryable on sp.QuaTangId equals qt.Id into giftGroup
                from gift in giftGroup.DefaultIfEmpty()
                where sp.Slug == slug
                select new
                {
                    sp,
                    DanhMucSlug = dm.Slug,
                    QuaTangTen = gift != null ? gift.Ten : null,
                    QuaTangGia = gift != null ? (decimal?)gift.Gia : null
                });

            if (item == null)
                throw new UserFriendlyException("Sản phẩm không tồn tại");

            var dto = ObjectMapper.Map<SanPham, SanPhamDto>(item.sp);

            dto.DanhMucSlug = item.DanhMucSlug;
            dto.QuaTangTen = item.QuaTangTen;
            dto.QuaTangGia = item.QuaTangGia;
            dto.PhanTramGiamGia = TinhPhanTramGiam(dto.Gia, dto.GiaKhuyenMai);

            // QUERY 2: ảnh phụ
            dto.AnhPhu = (await _anhRepo
                .GetListAsync(x => x.SanPhamId == dto.Id))
                .Select(x => x.Anh)
                .ToList();

            // QUERY 3: biến thể
            var bienThes = await _bienTheRepo
                .GetListAsync(x => x.SanPhamId == dto.Id);

            dto.BienThes = bienThes.Select(b => new SanPhamBienTheDto
            {
                Id = b.Id,
                Ten = b.Ten,
                Gia = b.Gia,
                GiaKhuyenMai = b.GiaKhuyenMai,
                SanPhamId = b.SanPhamId
            }).ToList();

            if (bienThes.Any())
            {
                dto.HasVariants = true;
                dto.GiaBienTheMin = bienThes.Min(b => (decimal?)b.Gia);
                dto.GiaBienTheMax = bienThes.Max(b => (decimal?)b.Gia);
                dto.GiaKhuyenMaiBienTheMin = bienThes
                    .Where(b => b.GiaKhuyenMai > 0)
                    .Select(b => (decimal?)b.GiaKhuyenMai)
                    .Min();
                dto.GiaKhuyenMaiBienTheMax = bienThes
                    .Where(b => b.GiaKhuyenMai > 0)
                    .Select(b => (decimal?)b.GiaKhuyenMai)
                    .Max();
                dto.PhanTramGiamGiaBienThe = bienThes
                        .Where(b => b.GiaKhuyenMai > 0 && b.Gia > 0)
                        .Select(bt => (int?)Math.Round((decimal)((1 - bt.GiaKhuyenMai / bt.Gia) * 100)))
                        .Max();
            }

            var bienTheIds = bienThes.Select(x => x.Id).ToList();

            if (!bienTheIds.Any())
                return dto;

            // QUERY 4: thuộc tính + giá trị
            var mappingQueryable = await _bienTheThuocTinhRepo.GetQueryableAsync();
            var giaTriQueryable = await _giaTriRepo.GetQueryableAsync();
            var thuocTinhQueryable = await _thuocTinhRepo.GetQueryableAsync();

            var attributes = await AsyncExecuter.ToListAsync(
                from map in mappingQueryable
                join gt in giaTriQueryable on map.GiaTriThuocTinhId equals gt.Id
                join tt in thuocTinhQueryable on gt.ThuocTinhId equals tt.Id
                where bienTheIds.Contains(map.SanPhamBienTheId)
                select new
                {
                    tt.Ten,
                    GiaTri = gt.GiaTri
                });

            dto.ThuocTinhs = attributes
                .GroupBy(x => x.Ten)
                .Select(g => new ThuocTinhDto
                {
                    Ten = g.Key,
                    GiaTris = g.Select(x => x.GiaTri)
                        .Distinct()
                        .OrderByDescending(x => x)
                        .ToList()
                })
                .ToList();
            return dto;
        }

        [Authorize(VietlifeStorePermissions.SanPham.View)]
        public async Task<List<SanPhamBienTheDto>> GetBienThesBySanPhamIdAsync(Guid sanPhamId)
        {
            var bienThes = await _bienTheRepo.GetListAsync(x => x.SanPhamId == sanPhamId);

            return bienThes.Select(b => new SanPhamBienTheDto
            {
                Id = b.Id,
                Ten = b.Ten,
                Gia = b.Gia,
                GiaKhuyenMai = b.GiaKhuyenMai,
            }).ToList();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task TangLuotXemAsync(Guid sanPhamId)
        {
            var sanPham = await Repository.GetAsync(sanPhamId);
            sanPham.LuotXem++;
            await Repository.UpdateAsync(sanPham, autoSave: true);
        }
    }
}