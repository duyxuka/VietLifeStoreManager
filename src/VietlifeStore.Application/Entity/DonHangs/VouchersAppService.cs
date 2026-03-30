using Hangfire;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VietlifeStore.ChucNang.DatLichs.DatLichVouchers;
using VietlifeStore.Entity.DonHangsList.Vouchers;
using VietlifeStore.Permissions;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;
using static VietlifeStore.Permissions.VietlifeStorePermissions;

namespace VietlifeStore.Entity.DonHangs
{
    public class VouchersAppService :
        CrudAppService<
            Voucher,
            VoucherDto,
            Guid,
            PagedResultRequestDto,
            CreateUpdateVoucherDto,
            CreateUpdateVoucherDto>,
        IVouchersAppService
    {
        private readonly IRepository<VoucherDaSuDung, Guid> _voucherUsedRepo;
        private readonly IRepository<VoucherNguoiDung, Guid> _voucherUserRepo;
        private readonly IRepository<VoucherSchedule, Guid> _scheduleRepo;
        private readonly IRepository<VoucherDoiTuong, Guid> _doiTuongRepo;
        private readonly IBackgroundJobClient _jobClient;

        public VouchersAppService(
            IRepository<Voucher, Guid> repository,
            IRepository<VoucherDaSuDung, Guid> voucherUsedRepo,
            IRepository<VoucherNguoiDung, Guid> voucherUserRepo,
            IRepository<VoucherSchedule, Guid> scheduleRepo,
            IRepository<VoucherDoiTuong, Guid> doiTuongRepo,
            IBackgroundJobClient jobClient)
            : base(repository)
        {
            _voucherUsedRepo = voucherUsedRepo;
            _voucherUserRepo = voucherUserRepo;
            _scheduleRepo = scheduleRepo;
            _doiTuongRepo = doiTuongRepo;
            _jobClient = jobClient;

            GetPolicyName = VietlifeStorePermissions.Voucher.View;
            GetListPolicyName = VietlifeStorePermissions.Voucher.View;
            CreatePolicyName = VietlifeStorePermissions.Voucher.Create;
            UpdatePolicyName = VietlifeStorePermissions.Voucher.Update;
            DeletePolicyName = VietlifeStorePermissions.Voucher.Delete;
        }

        // ================================================================
        //  CREATE
        // ================================================================
        [Authorize(VietlifeStorePermissions.Voucher.Create)]
        public override async Task<VoucherDto> CreateAsync(CreateUpdateVoucherDto input)
        {
            var existed = await Repository.AnyAsync(x => x.MaVoucher == input.MaVoucher.Trim().ToUpper());
            if (existed)
                throw new UserFriendlyException($"Mã voucher '{input.MaVoucher}' đã tồn tại.");

            var entity = new Voucher(Guid.NewGuid())
            {
                MaVoucher = input.MaVoucher.Trim().ToUpper(),
                TenVoucher = input.TenVoucher,
                MoTa = input.MoTa,
                LoaiVoucher = input.LoaiVoucher,
                PhamVi = input.PhamVi,
                GiamGia = input.GiamGia,
                LaPhanTram = input.LaPhanTram,
                GiamToiDa = input.GiamToiDa,
                DonHangToiThieu = input.DonHangToiThieu,
                TongSoLuong = input.TongSoLuong,
                DaDung = 0,
                GioiHanMoiUser = input.GioiHanMoiUser > 0 ? input.GioiHanMoiUser : 1,
                ThoiHanBatDau = input.ThoiHanBatDau,
                ThoiHanKetThuc = input.ThoiHanKetThuc,
                ChiPhatHanhCuThe = input.ChiPhatHanhCuThe,
                TrangThai = TrangThaiVoucher.ChuaKichHoat,
                HangfireActivateJobId = null,
                HangfireExpireJobId = null,
                HangfireWarnJobId = null,   // FIX: thêm property mới
            };

            // ----------------------------------------------------------------
            // FIX RACE CONDITION:
            // Bước 1 — Insert + commit entity TRƯỚC để Hangfire job không chạy
            //           vào record chưa tồn tại trong DB.
            // ----------------------------------------------------------------
            await Repository.InsertAsync(entity, autoSave: false);
            await LuuDoiTuongApDungAsync(entity.Id, input);
            await UnitOfWorkManager.Current.SaveChangesAsync(); // commit lần 1

            // Bước 2 — Lên lịch Hangfire (entity đã có trong DB)
            GanLichHangfire(entity);

            // Bước 3 — Ghi VoucherSchedule + cập nhật JobId vào entity
            await GhiScheduleBatchAsync(entity);
            await Repository.UpdateAsync(entity, autoSave: false);
            await UnitOfWorkManager.Current.SaveChangesAsync(); // commit lần 2

            return MapToGetOutputDto(entity);
        }

        // ================================================================
        //  GET SINGLE (with DoiTuongApDung)
        // ================================================================
        public override async Task<VoucherDto> GetAsync(Guid id)
        {
            var query = await Repository.WithDetailsAsync(x => x.DoiTuongApDung);
            var entity = await AsyncExecuter.FirstOrDefaultAsync(query, x => x.Id == id)
                ?? throw new EntityNotFoundException(typeof(Voucher), id);

            return ObjectMapper.Map<Voucher, VoucherDto>(entity);
        }

        // ================================================================
        //  UPDATE
        // ================================================================
        [Authorize(VietlifeStorePermissions.Voucher.Update)]
        public override async Task<VoucherDto> UpdateAsync(Guid id, CreateUpdateVoucherDto input)
        {
            var entity = await Repository.GetAsync(id);
            // ── GUARD: Voucher đã hết hạn / vô hiệu không cho sửa lịch ────────
            if (entity.TrangThai == TrangThaiVoucher.HetHan)
                throw new UserFriendlyException(
                    "Voucher đã hết hạn. Vui lòng tạo voucher mới thay vì chỉnh sửa.");

            if (entity.TrangThai == TrangThaiVoucher.VoHieu)
                throw new UserFriendlyException(
                    "Voucher đã bị vô hiệu hóa. Vui lòng tạo voucher mới.");

            // Cho phép sửa thông tin (tên, mô tả, số lượng...) nhưng không cho đổi lịch
            // nếu voucher đang hoạt động và đã có người dùng
            if (entity.TrangThai == TrangThaiVoucher.DangHoatDong)
            {
                var daDungRoi = await _voucherUsedRepo.AnyAsync(x => x.VoucherId == id);
                if (daDungRoi)
                {
                    // Chỉ cho sửa tên, mô tả, số lượng — không cho sửa lịch
                    if (input.ThoiHanBatDau != entity.ThoiHanBatDau ||
                        input.ThoiHanKetThuc != entity.ThoiHanKetThuc)
                        throw new UserFriendlyException(
                            "Không thể thay đổi thời gian của voucher đã có người sử dụng.");
                }
            }
            if (entity.MaVoucher != input.MaVoucher.Trim().ToUpper())
            {
                var daDung = await _voucherUsedRepo.AnyAsync(x => x.VoucherId == id);
                if (daDung)
                    throw new UserFriendlyException("Không thể đổi mã voucher đã có người sử dụng.");

                var trung = await Repository.AnyAsync(x => x.MaVoucher == input.MaVoucher.Trim().ToUpper() && x.Id != id);
                if (trung)
                    throw new UserFriendlyException($"Mã voucher '{input.MaVoucher}' đã tồn tại.");
            }

            // Hủy Hangfire job cũ
            HuyJobCu(entity);

            entity.MaVoucher = input.MaVoucher.Trim().ToUpper();
            entity.TenVoucher = input.TenVoucher;
            entity.MoTa = input.MoTa;
            entity.LoaiVoucher = input.LoaiVoucher;
            entity.PhamVi = input.PhamVi;
            entity.GiamGia = input.GiamGia;
            entity.LaPhanTram = input.LaPhanTram;
            entity.GiamToiDa = input.GiamToiDa;
            entity.DonHangToiThieu = input.DonHangToiThieu;
            entity.TongSoLuong = input.TongSoLuong;
            entity.GioiHanMoiUser = input.GioiHanMoiUser > 0 ? input.GioiHanMoiUser : 1;
            entity.ThoiHanBatDau = input.ThoiHanBatDau;
            entity.ThoiHanKetThuc = input.ThoiHanKetThuc;
            entity.ChiPhatHanhCuThe = input.ChiPhatHanhCuThe;

            // ── BƯỚC 1: Xóa dữ liệu cũ + commit TRƯỚC ──────────────────────
            await _scheduleRepo.DeleteAsync(x => x.VoucherId == entity.Id, autoSave: true);
            await _doiTuongRepo.DeleteAsync(x => x.VoucherId == entity.Id, autoSave: true);

            // ── BƯỚC 2: Lên lịch Hangfire + insert mới + update entity ─────────
            GanLichHangfire(entity);
            await GhiScheduleBatchAsync(entity);
            await LuuDoiTuongApDungAsync(entity.Id, input);
            await Repository.UpdateAsync(entity, autoSave: false);
            await UnitOfWorkManager.Current.SaveChangesAsync();

            return MapToGetOutputDto(entity);
        }

        // ================================================================
        //  DELETE MULTIPLE
        // ================================================================
        [Authorize(VietlifeStorePermissions.Voucher.Delete)]
        public async Task DeleteMultipleAsync(IEnumerable<Guid> ids)
        {
            var idList = ids.ToList();

            foreach (var id in idList)
            {
                var entity = await Repository.FindAsync(id);
                if (entity == null) continue;

                // Hủy Hangfire jobs
                HuyJobCu(entity);

                // FIX: Xóa các bảng liên quan để tránh orphan records
                var schedules = await _scheduleRepo.GetListAsync(x => x.VoucherId == id);
                await _scheduleRepo.DeleteManyAsync(schedules, autoSave: false);

                var doiTuong = await _doiTuongRepo.GetListAsync(x => x.VoucherId == id);
                await _doiTuongRepo.DeleteManyAsync(doiTuong, autoSave: false);

                var viVouchers = await _voucherUserRepo.GetListAsync(x => x.VoucherId == id);
                await _voucherUserRepo.DeleteManyAsync(viVouchers, autoSave: false);

                // Lưu ý: VoucherDaSuDung là lịch sử tài chính — cân nhắc soft-delete
                // thay vì hard-delete. Nếu muốn xóa hẳn, bỏ comment dòng dưới:
                // var used = await _voucherUsedRepo.GetListAsync(x => x.VoucherId == id);
                // await _voucherUsedRepo.DeleteManyAsync(used, autoSave: false);
            }

            await Repository.DeleteManyAsync(idList, autoSave: false);
            await UnitOfWorkManager.Current.SaveChangesAsync();
        }

        // ================================================================
        //  GET ALL ACTIVE
        // ================================================================
        [AllowAnonymous]
        public async Task<List<VoucherInListDto>> GetListAllAsync(
            int? phamVi = null,
            Guid? sanPhamId = null,
            Guid? danhMucId = null)
        {
            var query = (await Repository.GetQueryableAsync())
                .Where(x => x.TrangThai == TrangThaiVoucher.DangHoatDong);

            if (phamVi.HasValue)
            {
                var phamViEnum = (PhamViVoucher)phamVi.Value;

                query = query.Where(x =>
                    x.PhamVi == PhamViVoucher.ToanShop ||   // luôn hiển thị toàn shop
                    x.PhamVi == phamViEnum);
            }

            var list = await AsyncExecuter.ToListAsync(
                query.OrderByDescending(x => x.CreationTime)
            );

            // Filter theo sản phẩm/danh mục cụ thể (phải check bảng DoiTuong)
            if (sanPhamId.HasValue || danhMucId.HasValue)
            {
                var allDoiTuong = await AsyncExecuter.ToListAsync(
                    (await _doiTuongRepo.GetQueryableAsync())
                        .Where(x =>
                            (sanPhamId.HasValue && x.LoaiDoiTuong == LoaiDoiTuong.SanPham && x.DoiTuongId == sanPhamId.Value) ||
                            (danhMucId.HasValue && x.LoaiDoiTuong == LoaiDoiTuong.DanhMuc && x.DoiTuongId == danhMucId.Value))
                );

                var voucherIdsCoDoiTuong = new HashSet<Guid>(allDoiTuong.Select(x => x.VoucherId));

                list = list.Where(v =>
                    v.PhamVi == PhamViVoucher.ToanShop ||
                    (v.PhamVi == PhamViVoucher.SanPhamCuThe && sanPhamId.HasValue && voucherIdsCoDoiTuong.Contains(v.Id)) ||
                    (v.PhamVi == PhamViVoucher.DanhMucCuThe && danhMucId.HasValue && voucherIdsCoDoiTuong.Contains(v.Id))
                ).ToList();
            }
            // Map sang DTO
            var result = ObjectMapper.Map<List<Voucher>, List<VoucherInListDto>>(list);

            // Tính phần trăm đã dùng
            foreach (var v in result)
            {
                if (v.TongSoLuong > 0)
                {
                    v.PhanTramDaDung = (int)Math.Round(
                        (decimal)v.DaDung * 100 / v.TongSoLuong
                    );
                }
                else
                {
                    v.PhanTramDaDung = 0;
                }
            }

            return result;
        }

        // ================================================================
        //  FILTER + PAGING
        // ================================================================
        [Authorize(VietlifeStorePermissions.Voucher.View)]
        public async Task<PagedResultDto<VoucherInListDto>> GetListFilterAsync(BaseListFilterDto input)
        {
            var query = (await Repository.GetQueryableAsync())
                .WhereIf(!string.IsNullOrWhiteSpace(input.Keyword),
                    x => x.MaVoucher.Contains(input.Keyword) ||
                         x.TenVoucher.Contains(input.Keyword));

            var totalCount = await AsyncExecuter.LongCountAsync(query);
            var items = await AsyncExecuter.ToListAsync(
                query.OrderByDescending(x => x.CreationTime)
                     .Skip(input.SkipCount)
                     .Take(input.MaxResultCount)
            );

            return new PagedResultDto<VoucherInListDto>(
                totalCount,
                ObjectMapper.Map<List<Voucher>, List<VoucherInListDto>>(items)
            );
        }

        // ================================================================
        //  NHẬN VOUCHER VÀO VÍ
        // ================================================================
        [Authorize]
        public async Task NhanVoucherAsync(Guid voucherId)
        {
            var userId = CurrentUser.GetId();
            var voucher = await Repository.GetAsync(voucherId);

            if (voucher.TrangThai != TrangThaiVoucher.DangHoatDong)
                throw new UserFriendlyException("Voucher không còn hoạt động.");

            if (voucher.DaDung >= voucher.TongSoLuong)
                throw new UserFriendlyException("Voucher đã hết số lượng.");

            var daCoVoucher = await _voucherUserRepo
                .AnyAsync(x => x.VoucherId == voucherId && x.UserId == userId);
            if (daCoVoucher)
                throw new UserFriendlyException("Bạn đã lưu voucher này rồi.");

            await _voucherUserRepo.InsertAsync(new VoucherNguoiDung
            {
                VoucherId = voucherId,
                UserId = userId,
                SoLuongNhan = 1,
                DaDung = 0,
                NgayNhan = DateTime.Now,
            }, autoSave: true);
        }

        // ================================================================
        //  VÍ VOUCHER CỦA USER
        // ================================================================
        [Authorize]
        public async Task<List<VoucherDto>> GetMyVouchersAsync()
        {
            var userId = CurrentUser.GetId();
            var now = DateTime.Now;

            var voucherIds = await AsyncExecuter.ToListAsync(
                (await _voucherUserRepo.GetQueryableAsync())
                    .Where(x => x.UserId == userId && (x.SoLuongNhan - x.DaDung) > 0)
                    .Select(x => x.VoucherId)
            );

            var vouchers = await AsyncExecuter.ToListAsync(
                (await Repository.GetQueryableAsync())
                    .Where(x =>
                        voucherIds.Contains(x.Id) &&
                        x.TrangThai == TrangThaiVoucher.DangHoatDong &&
                        (x.ThoiHanKetThuc == null || x.ThoiHanKetThuc >= now))
                    .OrderByDescending(x => x.ThoiHanKetThuc)
            );

            return ObjectMapper.Map<List<Voucher>, List<VoucherDto>>(vouchers);
        }

        // ================================================================
        //  VALIDATE + TÍNH GIÁ TRỊ GIẢM
        // ================================================================
        [Authorize]
        public async Task<ApDungVoucherResultDto> ValidateVoucherAsync(string code, decimal orderTotal)
        {
            var userId = CurrentUser.GetId();
            var now = DateTime.Now;

            var voucher = await Repository.FirstOrDefaultAsync(x => x.MaVoucher == code.Trim().ToUpper())
                ?? throw new UserFriendlyException("Mã voucher không tồn tại.");

            if (voucher.TrangThai != TrangThaiVoucher.DangHoatDong)
                throw new UserFriendlyException("Voucher không còn hoạt động.");

            if (voucher.DaDung >= voucher.TongSoLuong)
                throw new UserFriendlyException("Voucher đã hết số lượng.");

            if (voucher.ThoiHanBatDau.HasValue && now < voucher.ThoiHanBatDau.Value)
                throw new UserFriendlyException("Voucher chưa đến thời gian sử dụng.");

            if (voucher.ThoiHanKetThuc.HasValue && now > voucher.ThoiHanKetThuc.Value)
                throw new UserFriendlyException("Voucher đã hết hạn.");

            if (orderTotal < voucher.DonHangToiThieu)
                throw new UserFriendlyException($"Đơn hàng tối thiểu {voucher.DonHangToiThieu:N0}đ mới được áp dụng.");

            var soLanDaDung = await _voucherUsedRepo
                .CountAsync(x => x.VoucherId == voucher.Id && x.UserId == userId);
            if (soLanDaDung >= voucher.GioiHanMoiUser)
                throw new UserFriendlyException("Bạn đã dùng hết lượt cho voucher này.");

            if (voucher.ChiPhatHanhCuThe)
            {
                var viVoucher = await _voucherUserRepo
                    .FirstOrDefaultAsync(x => x.VoucherId == voucher.Id && x.UserId == userId)
                    ?? throw new UserFriendlyException("Bạn không có voucher này trong ví.");

                if ((viVoucher.SoLuongNhan - viVoucher.DaDung) <= 0)
                    throw new UserFriendlyException("Bạn đã dùng hết voucher này.");
            }

            decimal giaTriGiam = voucher.LaPhanTram
                ? orderTotal * voucher.GiamGia / 100m
                : voucher.GiamGia;

            if (voucher.GiamToiDa.HasValue)
                giaTriGiam = Math.Min(giaTriGiam, voucher.GiamToiDa.Value);

            giaTriGiam = Math.Min(giaTriGiam, orderTotal);

            return new ApDungVoucherResultDto
            {
                VoucherId = voucher.Id,
                MaVoucher = voucher.MaVoucher,
                TenVoucher = voucher.TenVoucher,
                GiaTriGiam = Math.Round(giaTriGiam, 0),
                GiaSauGiam = Math.Round(orderTotal - giaTriGiam, 0),
                LoaiVoucher = voucher.LoaiVoucher,
            };
        }

        // ================================================================
        //  XÁC NHẬN SỬ DỤNG VOUCHER
        //  FIX: Thêm optimistic concurrency check để tránh oversell
        // ================================================================
        [Authorize]
        public async Task ConfirmSuDungVoucherAsync(Guid voucherId, Guid donHangId, decimal giaTriGiam)
        {
            var userId = CurrentUser.GetId();
            var voucher = await Repository.GetAsync(voucherId);

            // Kiểm tra lại lần cuối trước khi ghi nhận (guard against race condition)
            if (voucher.DaDung >= voucher.TongSoLuong)
                throw new UserFriendlyException("Voucher đã hết số lượng, không thể sử dụng.");

            if (voucher.TrangThai != TrangThaiVoucher.DangHoatDong)
                throw new UserFriendlyException("Voucher không còn hoạt động.");

            // Kiểm tra user chưa vượt giới hạn lượt
            var soLanDaDung = await _voucherUsedRepo
                .CountAsync(x => x.VoucherId == voucherId && x.UserId == userId);
            if (soLanDaDung >= voucher.GioiHanMoiUser)
                throw new UserFriendlyException("Bạn đã dùng hết lượt cho voucher này.");

            await _voucherUsedRepo.InsertAsync(new VoucherDaSuDung
            {
                VoucherId = voucherId,
                UserId = userId,
                DonHangId = donHangId,
                GiaTriGiam = giaTriGiam,
                NgaySuDung = DateTime.Now,
            }, autoSave: false);

            voucher.DaDung++;
            if (voucher.DaDung >= voucher.TongSoLuong)
                voucher.TrangThai = TrangThaiVoucher.HetSoLuong;

            await Repository.UpdateAsync(voucher, autoSave: false);

            if (voucher.ChiPhatHanhCuThe)
            {
                var viVoucher = await _voucherUserRepo
                    .FirstOrDefaultAsync(x => x.VoucherId == voucherId && x.UserId == userId);
                if (viVoucher != null)
                {
                    viVoucher.DaDung++;
                    await _voucherUserRepo.UpdateAsync(viVoucher, autoSave: false);
                }
            }

            await UnitOfWorkManager.Current.SaveChangesAsync();
        }

        // ================================================================
        //  VÔ HIỆU HÓA THỦ CÔNG
        // ================================================================
        [Authorize(VietlifeStorePermissions.Voucher.Update)]
        public async Task VoHieuHoaAsync(Guid id)
        {
            var entity = await Repository.GetAsync(id);
            HuyJobCu(entity);
            entity.TrangThai = TrangThaiVoucher.VoHieu;
            await Repository.UpdateAsync(entity, autoSave: true);
        }

        // ================================================================
        //  THỐNG KÊ
        // ================================================================
        [Authorize(VietlifeStorePermissions.Voucher.View)]
        public async Task<VoucherThongKeDto> GetThongKeAsync(Guid id)
        {
            var voucher = await Repository.GetAsync(id);

            var lichSu = await AsyncExecuter.ToListAsync(
                (await _voucherUsedRepo.GetQueryableAsync())
                    .Where(x => x.VoucherId == id)
            );

            return new VoucherThongKeDto
            {
                VoucherId = id,
                MaVoucher = voucher.MaVoucher,
                TongSoLuong = voucher.TongSoLuong,
                DaDung = voucher.DaDung,
                ConLai = voucher.TongSoLuong - voucher.DaDung,
                TongGiaTriGiam = lichSu.Sum(x => x.GiaTriGiam),
                SoNguoiDung = lichSu.Select(x => x.UserId).Distinct().Count(),
                TrangThai = voucher.TrangThai,
            };
        }

        // ================================================================
        //  PRIVATE — GÁN LỊCH HANGFIRE
        //  FIX: Lưu HangfireWarnJobId riêng để GhiScheduleBatchAsync dùng đúng jobId
        //  Gọi hàm này SAU KHI entity đã được commit vào DB.
        // ================================================================
        private void GanLichHangfire(Voucher entity)
        {
            var now = DateTime.Now;

            if (entity.ThoiHanBatDau.HasValue && entity.ThoiHanBatDau.Value > now)
            {
                var delay = entity.ThoiHanBatDau.Value - now;
                entity.HangfireActivateJobId = _jobClient.Schedule<VoucherJobHandler>(
                    x => x.KichHoatAsync(entity.Id), delay);
            }
            else
            {
                // Kích hoạt ngay
                entity.TrangThai = TrangThaiVoucher.DangHoatDong;
                entity.HangfireActivateJobId = null;
            }

            entity.HangfireWarnJobId = null;
            entity.HangfireExpireJobId = null;

            if (entity.ThoiHanKetThuc.HasValue && entity.ThoiHanKetThuc.Value > now)
            {
                var delayExpire = entity.ThoiHanKetThuc.Value - now;

                entity.HangfireExpireJobId = _jobClient.Schedule<VoucherJobHandler>(
                    x => x.HetHanAsync(entity.Id), delayExpire);

                if (delayExpire > TimeSpan.FromHours(1))
                {
                    // FIX: Lưu jobId cảnh báo vào property riêng
                    entity.HangfireWarnJobId = _jobClient.Schedule<VoucherJobHandler>(
                        x => x.CanhBaoHetHanAsync(entity.Id),
                        delayExpire - TimeSpan.FromHours(1));
                }
            }
        }

        // ================================================================
        //  PRIVATE — GHI VOUCHERSCHEDULE BATCH
        //  FIX: Dùng entity.HangfireWarnJobId thực thay vì "warn-" + expireJobId
        // ================================================================
        private async Task GhiScheduleBatchAsync(Voucher entity)
        {
            if (!string.IsNullOrEmpty(entity.HangfireActivateJobId))
            {
                await GhiScheduleAsync(
                    entity.Id,
                    entity.HangfireActivateJobId,
                    LoaiJobVoucher.KichHoat,
                    entity.ThoiHanBatDau!.Value);
            }

            if (!string.IsNullOrEmpty(entity.HangfireWarnJobId) &&
                entity.ThoiHanKetThuc.HasValue)
            {
                await GhiScheduleAsync(
                    entity.Id,
                    entity.HangfireWarnJobId,          // FIX: jobId thực
                    LoaiJobVoucher.CanhBaoHetHan,
                    entity.ThoiHanKetThuc.Value.AddHours(-1));
            }

            if (!string.IsNullOrEmpty(entity.HangfireExpireJobId))
            {
                await GhiScheduleAsync(
                    entity.Id,
                    entity.HangfireExpireJobId,
                    LoaiJobVoucher.VoHieuHoa,
                    entity.ThoiHanKetThuc!.Value);
            }
        }

        // ================================================================
        //  PRIVATE — HỦY JOB CŨ TRÊN HANGFIRE
        //  FIX: Thêm hủy HangfireWarnJobId
        // ================================================================
        private void HuyJobCu(Voucher entity)
        {
            if (!string.IsNullOrEmpty(entity.HangfireActivateJobId))
            {
                _jobClient.Delete(entity.HangfireActivateJobId);
                entity.HangfireActivateJobId = null;
            }
            if (!string.IsNullOrEmpty(entity.HangfireWarnJobId))
            {
                _jobClient.Delete(entity.HangfireWarnJobId);
                entity.HangfireWarnJobId = null;
            }
            if (!string.IsNullOrEmpty(entity.HangfireExpireJobId))
            {
                _jobClient.Delete(entity.HangfireExpireJobId);
                entity.HangfireExpireJobId = null;
            }
        }

        // ================================================================
        //  PRIVATE — GHI 1 VOUCHERSCHEDULE RECORD
        // ================================================================
        private async Task GhiScheduleAsync(
            Guid voucherId,
            string jobId,
            LoaiJobVoucher loai,
            DateTime thoiGian)
        {
            await _scheduleRepo.InsertAsync(new VoucherSchedule
            {
                VoucherId = voucherId,
                HangfireJobId = jobId,
                LoaiJob = loai,
                ThoiGianDuKien = thoiGian,
                TrangThai = TrangThaiJob.ChoXuLy,
                GhiChu = string.Empty,
            });
        }


        [Authorize]
        public async Task<List<VoucherDto>> GetMyVouchersWithStatusAsync(decimal orderTotal)
        {
            var userId = CurrentUser.GetId();
            var now = DateTime.Now;

            // Lấy voucherId trong ví còn lượt
            var voucherIds = await AsyncExecuter.ToListAsync(
                (await _voucherUserRepo.GetQueryableAsync())
                    .Where(x => x.UserId == userId && (x.SoLuongNhan - x.DaDung) > 0)
                    .Select(x => x.VoucherId)
            );

            if (!voucherIds.Any())
                return new List<VoucherDto>();

            // Lấy số lần đã dùng
            var daDungMap = (await AsyncExecuter.ToListAsync(
                (await _voucherUsedRepo.GetQueryableAsync())
                    .Where(x => x.UserId == userId)
                    .GroupBy(x => x.VoucherId)
                    .Select(g => new { VoucherId = g.Key, Count = g.Count() })
            )).ToDictionary(x => x.VoucherId, x => x.Count);

            var vouchers = await AsyncExecuter.ToListAsync(
                (await Repository.GetQueryableAsync())
                    .Where(x =>
                        voucherIds.Contains(x.Id) &&
                        x.TrangThai == TrangThaiVoucher.DangHoatDong &&
                        (x.ThoiHanKetThuc == null || x.ThoiHanKetThuc >= now))
                    .OrderByDescending(x => x.DonHangToiThieu)
            );

            return vouchers.Select(v =>
            {
                var soLanDaDung = daDungMap.TryGetValue(v.Id, out var c) ? c : 0;
                var duDieuKien = v.DonHangToiThieu <= orderTotal
                                 && soLanDaDung < v.GioiHanMoiUser;

                string lyDo = "";
                if (v.DonHangToiThieu > orderTotal)
                    lyDo = $"Cần thêm {(v.DonHangToiThieu - orderTotal):N0}đ";
                else if (soLanDaDung >= v.GioiHanMoiUser)
                    lyDo = "Đã dùng hết lượt";

                int phanTramDaDung = 0;
                if (v.TongSoLuong > 0)
                {
                    phanTramDaDung = (int)Math.Round((decimal)v.DaDung * 100 / v.TongSoLuong);
                }

                return new VoucherDto
                {
                    Id = v.Id,
                    MaVoucher = v.MaVoucher,
                    TenVoucher = v.TenVoucher,
                    GiamGia = v.GiamGia,
                    LaPhanTram = v.LaPhanTram,
                    GiamToiDa = v.GiamToiDa,
                    DonHangToiThieu = v.DonHangToiThieu,
                    ThoiHanKetThuc = v.ThoiHanKetThuc,
                    PhamVi = v.PhamVi,
                    DuDieuKien = duDieuKien,
                    LyDoKhongDuDieuKien = lyDo,
                    PhanTramDaDung = phanTramDaDung
                };
            })
            .OrderByDescending(x => x.DuDieuKien) // đủ điều kiện lên trước
            .ToList();
        }


        // ================================================================
        //  PRIVATE — LƯU ĐỐI TƯỢNG ÁP DỤNG
        // ================================================================
        private async Task LuuDoiTuongApDungAsync(Guid voucherId, CreateUpdateVoucherDto input)
        {
            if (input.PhamVi == PhamViVoucher.SanPhamCuThe && input.SanPhamIds?.Any() == true)
            {
                foreach (var id in input.SanPhamIds.Distinct())
                {
                    await _doiTuongRepo.InsertAsync(new VoucherDoiTuong
                    {
                        VoucherId = voucherId,
                        LoaiDoiTuong = LoaiDoiTuong.SanPham,
                        DoiTuongId = id,
                    });
                }
            }

            if (input.PhamVi == PhamViVoucher.DanhMucCuThe && input.DanhMucIds?.Any() == true)
            {
                foreach (var id in input.DanhMucIds.Distinct())
                {
                    await _doiTuongRepo.InsertAsync(new VoucherDoiTuong
                    {
                        VoucherId = voucherId,
                        LoaiDoiTuong = LoaiDoiTuong.DanhMuc,
                        DoiTuongId = id,
                    });
                }
            }
        }
    }
}