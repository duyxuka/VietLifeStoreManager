using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VietlifeStore.ChucNang.DatLichs.DatLichGiamGiaSanPhams.ChuongTrinhGiamGias;
using VietlifeStore.ChucNang.DatLichs.DatLichGiamGiaSanPhams;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp;
using VietlifeStore.Entity.SanPhams;
using VietlifeStore.ChucNang.DatLichs.DatLichGiamGiaSanPhams.ChuongTrinhGiamGiaItems;
using Microsoft.Extensions.Logging;
using VietlifeStore.Permissions;
using Microsoft.AspNetCore.Authorization;

namespace VietlifeStore.ChucNang.DatLichs
{
    public class ChuongTrinhGiamGiaAppService :
        CrudAppService<
            ChuongTrinhGiamGia,
            ChuongTrinhDto,
            Guid,
            PagedResultRequestDto,
            CreateUpdateChuongTrinhDto,
            CreateUpdateChuongTrinhDto>,
        IChuongTrinhGiamGiaAppService
    {
        private readonly IRepository<ChuongTrinhGiamGiaItem, Guid> _itemRepo;
        private readonly IRepository<SanPham, Guid> _sanPhamRepo;
        private readonly IRepository<SanPhamBienThe, Guid> _bienTheRepo;
        private readonly IGiamGiaScheduler _scheduler;
        private readonly ILogger<ChuongTrinhGiamGiaAppService> _logger;

        public ChuongTrinhGiamGiaAppService(
            IRepository<ChuongTrinhGiamGia, Guid> repository,
            IRepository<ChuongTrinhGiamGiaItem, Guid> itemRepo,
            IRepository<SanPham, Guid> sanPhamRepo,
            IRepository<SanPhamBienThe, Guid> bienTheRepo,
            IGiamGiaScheduler scheduler,
            ILogger<ChuongTrinhGiamGiaAppService> logger)
            : base(repository)
        {
            _itemRepo = itemRepo;
            _sanPhamRepo = sanPhamRepo;
            _bienTheRepo = bienTheRepo;
            _scheduler = scheduler;
            _logger = logger;

            GetPolicyName = VietlifeStorePermissions.ChuongTrinhGiamGia.Default;
            GetListPolicyName = VietlifeStorePermissions.ChuongTrinhGiamGia.View;
            CreatePolicyName = VietlifeStorePermissions.ChuongTrinhGiamGia.Create;
            UpdatePolicyName = VietlifeStorePermissions.ChuongTrinhGiamGia.Update;
            DeletePolicyName = VietlifeStorePermissions.ChuongTrinhGiamGia.Delete;
        }

        // ================= CREATE =================
        [Authorize(VietlifeStorePermissions.ChuongTrinhGiamGia.Create)]
        public override async Task<ChuongTrinhDto> CreateAsync(CreateUpdateChuongTrinhDto input)
        {
            ValidateThoiGian(input.ThoiGianBatDau, input.ThoiGianKetThuc);
            await ValidateGiaSauGiamAsync(input);
            await ValidateKhongXungDotAsync(input);

            var entity = ObjectMapper.Map<CreateUpdateChuongTrinhDto, ChuongTrinhGiamGia>(input);
            entity.TrangThai = LichGiamGiaTrangThai.Pending;

            await Repository.InsertAsync(entity, autoSave: true);

            foreach (var item in input.Items)
            {
                await _itemRepo.InsertAsync(new ChuongTrinhGiamGiaItem
                {
                    ChuongTrinhId = entity.Id,
                    SanPhamId = item.SanPhamId,
                    BienTheId = item.BienTheId,
                    GiaSauGiam = item.GiaSauGiam,
                    QuaTangId = item.QuaTangId
                }, autoSave: true);
            }

            _scheduler.Schedule(entity.Id, entity.ThoiGianBatDau, entity.ThoiGianKetThuc);

            return await GetAsync(entity.Id);
        }

        // ================= UPDATE =================
        [Authorize(VietlifeStorePermissions.ChuongTrinhGiamGia.Update)]
        public override async Task<ChuongTrinhDto> UpdateAsync(Guid id, CreateUpdateChuongTrinhDto input)
        {
            var ct = await Repository.GetAsync(id);

            if (ct.TrangThai == LichGiamGiaTrangThai.Expired)
                throw new UserFriendlyException("Chương trình đã kết thúc, không thể chỉnh sửa");

            if (ct.TrangThai == LichGiamGiaTrangThai.Cancelled)
                throw new UserFriendlyException("Chương trình đã bị hủy, không thể chỉnh sửa");

            if (ct.TrangThai == LichGiamGiaTrangThai.Active)
            {
                // Đang chạy: chỉ cho phép chỉnh ngày kết thúc (batDau giữ nguyên)
                ValidateThoiGianActive(ct.ThoiGianBatDau, input.ThoiGianKetThuc);
                await ValidateGiaSauGiamAsync(input, existingId: id);
                await ValidateKhongXungDotAsync(input, excludeId: id);
                await UpdateWhenActive(ct, input);
            }
            else
            {
                // Pending: cho phép chỉnh cả 2
                ValidateThoiGian(input.ThoiGianBatDau, input.ThoiGianKetThuc);
                await ValidateGiaSauGiamAsync(input, existingId: id);
                await ValidateKhongXungDotAsync(input, excludeId: id);
                await UpdateWhenPending(id, input);
            }

            return await GetAsync(id);
        }

        // ================= UPDATE KHI ĐANG CHẠY =================
        private async Task UpdateWhenActive(ChuongTrinhGiamGia ct, CreateUpdateChuongTrinhDto input)
        {
            // 1. Hủy job cũ
            _scheduler.Cancel(ct.Id);

            // 2. Rollback giá về 0 (GiaKhuyenMai)
            await Rollback(ct.Id);

            // 3. Cập nhật thông tin — giữ nguyên ThoiGianBatDau gốc
            ct.TenChuongTrinh = input.TenChuongTrinh;
            ct.ThoiGianKetThuc = input.ThoiGianKetThuc;
            // ThoiGianBatDau KHÔNG thay đổi vì chương trình đã chạy
            ct.TrangThai = LichGiamGiaTrangThai.Pending; // tạm về Pending để activate lại
            await Repository.UpdateAsync(ct, autoSave: true);

            // 4. Thay items
            await ReplaceItems(ct.Id, input.Items);

            // 5. Re-activate đồng bộ (không qua Hangfire để tránh race condition)
            await ActivateChuongTrinhAsync(ct);

            // 6. Chỉ schedule expire job (activate đã xong)
            _scheduler.ScheduleOnlyExpire(ct.Id, input.ThoiGianKetThuc);
        }

        // ================= UPDATE KHI CHỜ KÍCH HOẠT =================
        private async Task UpdateWhenPending(Guid id, CreateUpdateChuongTrinhDto input)
        {
            // 1. Hủy job cũ
            _scheduler.Cancel(id);

            // 2. Cập nhật entity
            await base.UpdateAsync(id, input);

            // 3. Thay items
            await ReplaceItems(id, input.Items);

            // 4. Schedule lại cả 2 job
            _scheduler.Schedule(id, input.ThoiGianBatDau, input.ThoiGianKetThuc);
        }

        // ================= HELPER: REPLACE ITEMS =================
        private async Task ReplaceItems(Guid ctId, IEnumerable<CreateUpdateChuongTrinhItemDto> newItems)
        {
            var oldItems = await _itemRepo.GetListAsync(x => x.ChuongTrinhId == ctId);
            await _itemRepo.DeleteManyAsync(oldItems, autoSave: true);

            foreach (var item in newItems)
            {
                await _itemRepo.InsertAsync(new ChuongTrinhGiamGiaItem
                {
                    ChuongTrinhId = ctId,
                    SanPhamId = item.SanPhamId,
                    BienTheId = item.BienTheId,
                    GiaSauGiam = item.GiaSauGiam,
                    QuaTangId = item.QuaTangId
                }, autoSave: true);
            }
        }

        // ================= GET =================
        [Authorize(VietlifeStorePermissions.ChuongTrinhGiamGia.View)]
        public override async Task<ChuongTrinhDto> GetAsync(Guid id)
        {
            var entity = await Repository.GetAsync(id);
            var dto = ObjectMapper.Map<ChuongTrinhGiamGia, ChuongTrinhDto>(entity);

            var items = await _itemRepo.GetListAsync(x => x.ChuongTrinhId == id);
            dto.Items = new List<ChuongTrinhItemDto>();

            foreach (var item in items)
            {
                var itemDto = new ChuongTrinhItemDto
                {
                    Id = item.Id,
                    SanPhamId = item.SanPhamId,
                    BienTheId = item.BienTheId,
                    GiaSauGiam = item.GiaSauGiam,
                    QuaTangId = item.QuaTangId,
                    // Ưu tiên snapshot (khi Active/Expired), fallback sang giá thực khi Pending
                    GiaBanDau = item.BienTheId.HasValue
                    ? (item.GiaGocBienTheSnapshot > 0
                        ? item.GiaGocBienTheSnapshot
                        : null)   // sẽ fallback bên dưới
                    : (item.GiaGocSnapshot > 0
                        ? item.GiaGocSnapshot
                        : null)   // sẽ fallback bên dưới
                };


                if (item.BienTheId.HasValue)
                {
                    var bt = await _bienTheRepo.FindAsync(item.BienTheId.Value);
                    if (bt != null)
                    {
                        itemDto.TenBienThe = bt.Ten;
                        // Fallback: nếu chưa có snapshot (Pending) thì dùng giá hiện tại
                        if (itemDto.GiaBanDau == null || itemDto.GiaBanDau == 0)
                            itemDto.GiaBanDau = bt.GiaKhuyenMai > 0 ? bt.GiaKhuyenMai : bt.Gia;
                    }
                    if (item.SanPhamId.HasValue)
                    {
                        var sp = await _sanPhamRepo.FindAsync(item.SanPhamId.Value);
                        itemDto.TenSanPham = sp?.Ten ?? "(Sản phẩm đã bị xóa)";
                    }
                }
                else if (item.SanPhamId.HasValue)
                {
                    var sp = await _sanPhamRepo.FindAsync(item.SanPhamId.Value);
                    if (sp != null)
                    {
                        itemDto.TenSanPham = sp.Ten;
                        // Fallback: nếu chưa có snapshot (Pending) thì dùng giá hiện tại
                        if (itemDto.GiaBanDau == null || itemDto.GiaBanDau == 0)
                            itemDto.GiaBanDau = sp.GiaKhuyenMai > 0 ? sp.GiaKhuyenMai : sp.Gia;
                    }
                    else
                    {
                        itemDto.TenSanPham = "(Sản phẩm đã bị xóa)";
                    }
                }

                dto.Items.Add(itemDto);
            }

            return dto;
        }

        // ================= GET ALL =================
        [Authorize(VietlifeStorePermissions.ChuongTrinhGiamGia.View)]
        public async Task<List<ChuongTrinhInListDto>> GetListAllAsync()
        {
            var list = await Repository.GetListAsync();
            var result = new List<ChuongTrinhInListDto>();

            foreach (var x in list)
            {
                result.Add(new ChuongTrinhInListDto
                {
                    Id = x.Id,
                    TenChuongTrinh = x.TenChuongTrinh,
                    ThoiGianBatDau = x.ThoiGianBatDau,
                    ThoiGianKetThuc = x.ThoiGianKetThuc,
                    TrangThai = x.TrangThai,
                    SoLuongSanPham = await _itemRepo.CountAsync(i => i.ChuongTrinhId == x.Id)
                });
            }
            return result;
        }

        // ================= FILTER =================
        [Authorize(VietlifeStorePermissions.ChuongTrinhGiamGia.View)]
        public async Task<PagedResultDto<ChuongTrinhInListDto>> GetListFilterAsync(BaseListFilterDto input)
        {
            var query = (await Repository.GetQueryableAsync())
                .WhereIf(!string.IsNullOrWhiteSpace(input.Keyword),
                    x => x.TenChuongTrinh.Contains(input.Keyword));

            var total = await AsyncExecuter.CountAsync(query);
            var items = await AsyncExecuter.ToListAsync(
                query.OrderByDescending(x => x.CreationTime)
                     .Skip(input.SkipCount)
                     .Take(input.MaxResultCount));

            var result = new List<ChuongTrinhInListDto>();
            foreach (var x in items)
            {
                result.Add(new ChuongTrinhInListDto
                {
                    Id = x.Id,
                    TenChuongTrinh = x.TenChuongTrinh,
                    TrangThai = x.TrangThai,
                    ThoiGianBatDau = x.ThoiGianBatDau,
                    ThoiGianKetThuc = x.ThoiGianKetThuc,
                    SoLuongSanPham = await _itemRepo.CountAsync(i => i.ChuongTrinhId == x.Id)
                });
            }

            return new PagedResultDto<ChuongTrinhInListDto>(total, result);
        }

        // ================= DELETE MULTIPLE =================
        [Authorize(VietlifeStorePermissions.ChuongTrinhGiamGia.Delete)]
        public async Task DeleteMultipleAsync(IEnumerable<Guid> ids)
        {
            var idList = ids.ToList();

            foreach (var id in idList)
            {
                var ct = await Repository.GetAsync(id);

                if (ct.TrangThai == LichGiamGiaTrangThai.Active)
                    await Rollback(id);

                var items = await _itemRepo.GetListAsync(x => x.ChuongTrinhId == id);
                await _itemRepo.DeleteManyAsync(items, autoSave: true);
                _scheduler.Cancel(id);
            }

            await Repository.DeleteManyAsync(idList, autoSave: true);
        }

        // ================= CANCEL =================
        [Authorize(VietlifeStorePermissions.ChuongTrinhGiamGia.Update)]
        public async Task CancelAsync(Guid id)
        {
            var ct = await Repository.GetAsync(id);

            if (ct.TrangThai == LichGiamGiaTrangThai.Expired)
                throw new UserFriendlyException("Chương trình đã kết thúc, không thể hủy");

            if (ct.TrangThai == LichGiamGiaTrangThai.Cancelled)
                throw new UserFriendlyException("Chương trình đã bị hủy trước đó");

            if (ct.TrangThai == LichGiamGiaTrangThai.Active)
                await Rollback(ct.Id);

            ct.TrangThai = LichGiamGiaTrangThai.Cancelled;
            _scheduler.Cancel(id);

            await Repository.UpdateAsync(ct, autoSave: true);
        }

        // ================= REMOVE ITEM =================
        [Authorize(VietlifeStorePermissions.ChuongTrinhGiamGia.Delete)]
        public async Task RemoveItemAsync(Guid chuongTrinhId, Guid itemId)
        {
            var ct = await Repository.GetAsync(chuongTrinhId);
            var item = await _itemRepo.GetAsync(itemId);

            if (ct.TrangThai == LichGiamGiaTrangThai.Expired)
                throw new UserFriendlyException("Chương trình đã kết thúc, không thể chỉnh sửa");

            if (ct.TrangThai == LichGiamGiaTrangThai.Cancelled)
                throw new UserFriendlyException("Chương trình đã bị hủy, không thể chỉnh sửa");

            if (ct.TrangThai == LichGiamGiaTrangThai.Active)
            {
                if (item.BienTheId.HasValue && item.GiaGocBienTheSnapshot.HasValue)
                {
                    var bt = await _bienTheRepo.FindAsync(item.BienTheId.Value);
                    if (bt != null) { bt.GiaKhuyenMai = 0; await _bienTheRepo.UpdateAsync(bt, autoSave: true); }
                }
                else if (item.SanPhamId.HasValue && item.GiaGocSnapshot.HasValue)
                {
                    var sp = await _sanPhamRepo.FindAsync(item.SanPhamId.Value);
                    if (sp != null) { sp.GiaKhuyenMai = 0; sp.QuaTangId = null; await _sanPhamRepo.UpdateAsync(sp, autoSave: true); }
                }
            }

            await _itemRepo.DeleteAsync(item, autoSave: true);
        }

        // ================= ACTIVATE BATCH =================
        public async Task ActivateAsync()
        {
            var now = DateTime.Now;
            var list = await Repository.GetListAsync(x =>
                x.TrangThai == LichGiamGiaTrangThai.Pending &&
                x.ThoiGianBatDau <= now);

            foreach (var ct in list)
                await ActivateChuongTrinhAsync(ct);
        }

        // ================= EXPIRE BATCH =================
        public async Task ExpireAsync()
        {
            var now = DateTime.Now;
            var list = await Repository.GetListAsync(x =>
                x.TrangThai == LichGiamGiaTrangThai.Active &&
                x.ThoiGianKetThuc <= now);

            foreach (var ct in list)
            {
                await Rollback(ct.Id);
                ct.TrangThai = LichGiamGiaTrangThai.Expired;
                await Repository.UpdateAsync(ct, autoSave: true);
            }
        }

        // ================= ACTIVATE SINGLE (Hangfire) =================
        public async Task ActivateSingleAsync(Guid id)
        {
            var ct = await Repository.GetAsync(id);
            if (ct.TrangThai != LichGiamGiaTrangThai.Pending) return;
            await ActivateChuongTrinhAsync(ct);
        }

        // ================= EXPIRE SINGLE (Hangfire) =================
        public async Task ExpireSingleAsync(Guid id)
        {
            var ct = await Repository.GetAsync(id);
            if (ct.TrangThai != LichGiamGiaTrangThai.Active) return;

            await Rollback(id);
            ct.TrangThai = LichGiamGiaTrangThai.Expired;
            await Repository.UpdateAsync(ct, autoSave: true);
        }

        // ================= PRIVATE: ACTIVATE =================
        private async Task ActivateChuongTrinhAsync(ChuongTrinhGiamGia ct)
        {
            var items = await _itemRepo.GetListAsync(x => x.ChuongTrinhId == ct.Id);

            foreach (var item in items)
            {
                if (item.BienTheId.HasValue)
                {
                    var bt = await _bienTheRepo.FindAsync(item.BienTheId.Value);
                    if (bt == null)
                    {
                        _logger.LogWarning("Biến thể {Id} không tồn tại, xóa khỏi chương trình {CtId}", item.BienTheId, ct.Id);
                        await _itemRepo.DeleteAsync(item, autoSave: true);
                        continue;
                    }
                    item.GiaGocBienTheSnapshot = bt.Gia;
                    bt.GiaKhuyenMai = item.GiaSauGiam;
                    await _bienTheRepo.UpdateAsync(bt, autoSave: true);
                    await _itemRepo.UpdateAsync(item, autoSave: true);
                }
                else if (item.SanPhamId.HasValue)
                {
                    var sp = await _sanPhamRepo.FindAsync(item.SanPhamId.Value);
                    if (sp == null)
                    {
                        _logger.LogWarning("Sản phẩm {Id} không tồn tại, xóa khỏi chương trình {CtId}", item.SanPhamId, ct.Id);
                        await _itemRepo.DeleteAsync(item, autoSave: true);
                        continue;
                    }
                    item.GiaGocSnapshot = sp.Gia;
                    sp.GiaKhuyenMai = item.GiaSauGiam;
                    sp.QuaTangId = item.QuaTangId;
                    await _sanPhamRepo.UpdateAsync(sp, autoSave: true);
                    await _itemRepo.UpdateAsync(item, autoSave: true);
                }
            }

            ct.TrangThai = LichGiamGiaTrangThai.Active;
            await Repository.UpdateAsync(ct, autoSave: true);
        }

        // ================= PRIVATE: ROLLBACK =================
        private async Task Rollback(Guid ctId)
        {
            var items = await _itemRepo.GetListAsync(x => x.ChuongTrinhId == ctId);

            foreach (var item in items)
            {
                if (item.BienTheId.HasValue && item.GiaGocBienTheSnapshot.HasValue)
                {
                    var bt = await _bienTheRepo.FindAsync(item.BienTheId.Value);
                    if (bt != null) { bt.GiaKhuyenMai = 0; await _bienTheRepo.UpdateAsync(bt, autoSave: true); }
                }
                else if (item.SanPhamId.HasValue && item.GiaGocSnapshot.HasValue)
                {
                    var sp = await _sanPhamRepo.FindAsync(item.SanPhamId.Value);
                    if (sp != null) { sp.GiaKhuyenMai = 0; sp.QuaTangId = null; await _sanPhamRepo.UpdateAsync(sp, autoSave: true); }
                }
            }
        }

        // ================= PRIVATE: VALIDATE THOI GIAN (Pending) =================
        private static void ValidateThoiGian(DateTime batDau, DateTime ketThuc)
        {
            if (batDau >= ketThuc)
                throw new UserFriendlyException("Thời gian bắt đầu phải nhỏ hơn thời gian kết thúc");

            if (ketThuc <= DateTime.Now)
                throw new UserFriendlyException("Thời gian kết thúc phải lớn hơn thời gian hiện tại");
        }

        // ================= PRIVATE: VALIDATE THOI GIAN (Active) =================
        // Khi đang chạy: chỉ validate ngày kết thúc, không đụng ngày bắt đầu
        private static void ValidateThoiGianActive(DateTime batDauGoc, DateTime ketThucMoi)
        {
            if (ketThucMoi <= DateTime.Now)
                throw new UserFriendlyException("Thời gian kết thúc phải lớn hơn thời gian hiện tại");

            if (ketThucMoi <= batDauGoc)
                throw new UserFriendlyException("Thời gian kết thúc phải lớn hơn thời gian bắt đầu");
        }

        // ================= PRIVATE: VALIDATE GIA SAU GIAM =================
        private async Task ValidateGiaSauGiamAsync(CreateUpdateChuongTrinhDto input, Guid? existingId = null)
        {
            if (input.Items == null || !input.Items.Any())
                throw new UserFriendlyException("Chương trình phải có ít nhất một sản phẩm");

            List<ChuongTrinhGiamGiaItem> existingItems = new();
            if (existingId.HasValue)
                existingItems = await _itemRepo.GetListAsync(x => x.ChuongTrinhId == existingId.Value);

            foreach (var item in input.Items)
            {
                if (item.GiaSauGiam <= 0)
                    throw new UserFriendlyException("Giá sau giảm phải lớn hơn 0");

                decimal giaGoc = 0;
                string tenSanPham = "";

                var existingItem = existingItems.FirstOrDefault(e =>
                    e.SanPhamId == item.SanPhamId && e.BienTheId == item.BienTheId);

                if (item.BienTheId.HasValue)
                {
                    if (existingItem?.GiaGocBienTheSnapshot > 0)
                        giaGoc = existingItem.GiaGocBienTheSnapshot!.Value;
                    else if (existingItem?.GiaGocSnapshot > 0)
                        giaGoc = existingItem.GiaGocSnapshot!.Value;
                    else
                    {
                        var bt = await _bienTheRepo.FindAsync(item.BienTheId.Value);
                        if (bt == null) throw new UserFriendlyException("Biến thể sản phẩm không tồn tại");
                        giaGoc = (bt.GiaKhuyenMai > 0 ? bt.GiaKhuyenMai : bt.Gia) ?? 0;
                    }

                    var btTen = await _bienTheRepo.FindAsync(item.BienTheId.Value);
                    tenSanPham = btTen?.Ten ?? "";
                }
                else if (item.SanPhamId.HasValue)
                {
                    if (existingItem?.GiaGocSnapshot > 0)
                        giaGoc = existingItem.GiaGocSnapshot!.Value;
                    else
                    {
                        var sp = await _sanPhamRepo.FindAsync(item.SanPhamId.Value);
                        if (sp == null) throw new UserFriendlyException("Sản phẩm không tồn tại");
                        giaGoc = sp.GiaKhuyenMai > 0 ? sp.GiaKhuyenMai : sp.Gia;
                    }

                    var spTen = await _sanPhamRepo.FindAsync(item.SanPhamId.Value);
                    tenSanPham = spTen?.Ten ?? "";
                }

                if (giaGoc > 0 && item.GiaSauGiam >= giaGoc)
                    throw new UserFriendlyException(
                        $"Giá sau giảm của \"{tenSanPham}\" ({item.GiaSauGiam:N0}đ) " +
                        $"phải nhỏ hơn giá gốc ({giaGoc:N0}đ)");
            }
        }

        // ================= PRIVATE: VALIDATE XUNG DOT =================
        private async Task ValidateKhongXungDotAsync(CreateUpdateChuongTrinhDto input, Guid? excludeId = null)
        {
            var chuongTrinhKhac = await Repository.GetListAsync(x =>
                x.TrangThai != LichGiamGiaTrangThai.Cancelled &&
                x.TrangThai != LichGiamGiaTrangThai.Expired &&
                (excludeId == null || x.Id != excludeId) &&
                x.ThoiGianBatDau < input.ThoiGianKetThuc &&
                x.ThoiGianKetThuc > input.ThoiGianBatDau);

            foreach (var ct in chuongTrinhKhac)
            {
                foreach (var item in input.Items)
                {
                    var trung = await _itemRepo.AnyAsync(i =>
                        i.ChuongTrinhId == ct.Id &&
                        i.SanPhamId == item.SanPhamId &&
                        i.BienTheId == item.BienTheId);

                    if (!trung) continue;

                    string tenSanPham = "";
                    if (item.BienTheId.HasValue)
                    {
                        var bt = await _bienTheRepo.FindAsync(item.BienTheId.Value);
                        tenSanPham = bt?.Ten ?? "";
                    }
                    else if (item.SanPhamId.HasValue)
                    {
                        var sp = await _sanPhamRepo.FindAsync(item.SanPhamId.Value);
                        tenSanPham = sp?.Ten ?? "";
                    }

                    throw new UserFriendlyException(
                        $"Sản phẩm \"{tenSanPham}\" đã được áp dụng trong chương trình " +
                        $"\"{ct.TenChuongTrinh}\" trong cùng khoảng thời gian " +
                        $"({ct.ThoiGianBatDau:dd/MM/yyyy HH:mm} - {ct.ThoiGianKetThuc:dd/MM/yyyy HH:mm})");
                }
            }
        }
    }
}