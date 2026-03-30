using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Threading.Tasks;
using VietlifeStore.Entity.DonHangsList.ChiTietDonHangs;
using VietlifeStore.Entity.DonHangsList.DonHangs;
using VietlifeStore.Permissions;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Uow;
using Volo.Abp.Users;
using VietlifeStore.Entity.SanPhams;
using Hangfire;
using VietlifeStore.ChucNang.DatLichs.DatLichVouchers;

namespace VietlifeStore.Entity.DonHangs
{
    public class DonHangsAppService :
        CrudAppService<
            DonHang,
            DonHangDto,
            Guid,
            PagedResultRequestDto,
            CreateUpdateDonHangDto,
            CreateUpdateDonHangDto>,
        IDonHangsAppService
    {
        private readonly IRepository<ChiTietDonHang, Guid> _chiTietRepo;
        private readonly IRepository<Voucher, Guid> _voucherRepo;
        private readonly IRepository<VoucherDaSuDung, Guid> _voucherUsedRepo;
        private readonly IRepository<VoucherNguoiDung, Guid> _voucherUserRepo;
        private readonly IConfiguration _configuration;
        private readonly IRepository<SanPham, Guid> _sanPhamRepo;
        private readonly IBackgroundJobClient _backgroundJobClient;

        public DonHangsAppService(
            IRepository<DonHang, Guid> repository,
            IRepository<ChiTietDonHang, Guid> chiTietRepo,
            IRepository<Voucher, Guid> voucherRepo,
            IRepository<VoucherDaSuDung, Guid> voucherUsedRepo,
            IRepository<VoucherNguoiDung, Guid> voucherUserRepo,
            IConfiguration configuration,
            IRepository<SanPham, Guid> sanPhamRepo,
            IBackgroundJobClient backgroundJobClient)
            : base(repository)
        {
            _chiTietRepo = chiTietRepo;
            _voucherRepo = voucherRepo;
            _voucherUsedRepo = voucherUsedRepo;
            _voucherUserRepo = voucherUserRepo;
            _sanPhamRepo = sanPhamRepo;
            _backgroundJobClient = backgroundJobClient;
            _configuration = configuration;

            GetPolicyName = VietlifeStorePermissions.DonHang.View;
            GetListPolicyName = VietlifeStorePermissions.DonHang.View;
            CreatePolicyName = VietlifeStorePermissions.DonHang.Create;
            UpdatePolicyName = VietlifeStorePermissions.DonHang.Update;
            DeletePolicyName = VietlifeStorePermissions.DonHang.Delete;
            _configuration = configuration;
        }

        // ================= GET ALL =================
        [Authorize(VietlifeStorePermissions.DonHang.View)]
        public async Task<List<DonHangInListDto>> GetListAllAsync()
        {
            var list = await AsyncExecuter.ToListAsync(
                (await Repository.GetQueryableAsync())
                    .OrderByDescending(x => x.CreationTime)
            );

            return ObjectMapper.Map<List<DonHang>, List<DonHangInListDto>>(list);
        }

        // ================= FILTER + PAGING =================
        [Authorize(VietlifeStorePermissions.DonHang.View)]
        public async Task<PagedResultDto<DonHangInListDto>> GetListFilterAsync(BaseListFilterDto input)
        {
            var query = (await Repository.GetQueryableAsync())
                .WhereIf(
                    !string.IsNullOrWhiteSpace(input.Keyword),
                    x =>x.SoDienThoai.Contains(input.Keyword) ||
                        x.Email.Contains(input.Keyword)
                );

            var totalCount = await AsyncExecuter.LongCountAsync(query);

            var items = await AsyncExecuter.ToListAsync(
                query
                    .OrderByDescending(x => x.CreationTime)
                    .Skip(input.SkipCount)
                    .Take(input.MaxResultCount)
            );

            return new PagedResultDto<DonHangInListDto>(
                totalCount,
                ObjectMapper.Map<List<DonHang>, List<DonHangInListDto>>(items)
            );
        }

        // ================= GET DETAIL =================
        [Authorize(VietlifeStorePermissions.DonHang.View)]
        public override async Task<DonHangDto> GetAsync(Guid id)
        {
            var query = await Repository.WithDetailsAsync(x => x.ChiTietDonHangs);
            var entity = await AsyncExecuter.FirstOrDefaultAsync(query, x => x.Id == id);

            if (entity == null)
                throw new EntityNotFoundException(typeof(DonHang), id);

            return ObjectMapper.Map<DonHang, DonHangDto>(entity);
        }

        // ================= CREATE =================
        [Authorize]
        public override async Task<DonHangDto> CreateAsync(CreateUpdateDonHangDto input)
        {
            var donHang = ObjectMapper.Map<CreateUpdateDonHangDto, DonHang>(input);

            donHang.NgayDat = DateTime.Now;
            donHang.Ma = await GenerateOrderCodeAsync();
            donHang.GiamGiaVoucher = 0;

            await Repository.InsertAsync(donHang, autoSave: true);

            // Lưu chi tiết + tính TongTien trước (cần TongTien gốc để tính giảm giá)
            await SaveChiTietAsync(donHang, input.ChiTietDonHangs);

            // Xử lý voucher nếu có — dùng TongTien vừa tính từ chi tiết
            if (input.VoucherId.HasValue)
            {
                var tongTienGoc = donHang.TongTien; // decimal, không nullable
                var giaTriGiam = await XuLyVoucherAsync(
                    input.VoucherId.Value,
                    donHang.Id,
                    tongTienGoc
                );

                donHang.GiamGiaVoucher = giaTriGiam;
                donHang.TongTien = Math.Max(tongTienGoc - giaTriGiam, 0);
            }


            await UnitOfWorkManager.Current.SaveChangesAsync();
            _backgroundJobClient.Enqueue<DonHangEmailJob>(
                job => job.SendOrderSuccessAsync(donHang.Id));

            return ObjectMapper.Map<DonHang, DonHangDto>(donHang);
        }

        [Authorize(VietlifeStorePermissions.DonHang.Update)]
        public async Task UpdateStatusAsync(Guid id, byte status)
        {
            var order = await Repository.GetAsync(id);

            order.TrangThai = status;

            await Repository.UpdateAsync(order);

            await UnitOfWorkManager.Current.SaveChangesAsync();
        }

        // ================= UPDATE =================
        [Authorize(VietlifeStorePermissions.DonHang.Update)]
        public override async Task<DonHangDto> UpdateAsync(Guid id, CreateUpdateDonHangDto input)
        {
            var donHang = await Repository.GetAsync(id);

            ObjectMapper.Map(input, donHang);

            var oldDetails = await _chiTietRepo.GetListAsync(x => x.DonHangId == id);

            var requestIds = input.ChiTietDonHangs
                .Where(x => x.Id != Guid.Empty)
                .Select(x => x.Id)
                .ToList();

            // Xóa chi tiết bị bỏ
            foreach (var old in oldDetails)
            {
                if (!requestIds.Contains(old.Id))
                    await _chiTietRepo.DeleteAsync(old);
            }

            // Thêm / cập nhật
            foreach (var ct in input.ChiTietDonHangs)
            {
                if (ct.SoLuong <= 0) ct.SoLuong = 1;
                if (ct.Gia < 0) ct.Gia = 0;

                if (ct.Id == Guid.Empty)
                {
                    await _chiTietRepo.InsertAsync(new ChiTietDonHang
                    {
                        DonHangId = id,
                        SanPhamId = ct.SanPhamId,
                        SanPhamBienThe = ct.SanPhamBienThe,
                        QuaTang = ct.QuaTang,
                        SoLuong = ct.SoLuong,
                        Gia = ct.Gia,
                        TrangThai = ct.TrangThai
                    });
                }
                else
                {
                    var existing = oldDetails.First(x => x.Id == ct.Id);
                    ObjectMapper.Map(ct, existing);
                    await _chiTietRepo.UpdateAsync(existing);
                }
            }

            await RecalculateAsync(donHang);

            await UnitOfWorkManager.Current.SaveChangesAsync();

            return ObjectMapper.Map<DonHang, DonHangDto>(donHang);
        }

        // ================= DELETE MULTIPLE =================
        [Authorize(VietlifeStorePermissions.DonHang.Delete)]
        public async Task DeleteMultipleAsync(IEnumerable<Guid> ids)
        {
            await Repository.DeleteManyAsync(ids);
            await UnitOfWorkManager.Current.SaveChangesAsync();
        }

        // ================= PRIVATE =================
        private async Task SaveChiTietAsync(
            DonHang donHang,
            List<CreateUpdateChiTietDonHangDto> details)
        {
            decimal tongTien = 0;
            decimal tongSoLuong = 0;

            foreach (var ct in details)
            {
                if (ct.SoLuong <= 0) ct.SoLuong = 1;
                if (ct.Gia < 0) ct.Gia = 0;

                tongSoLuong += ct.SoLuong;
                tongTien += ct.Gia * ct.SoLuong;

                await _chiTietRepo.InsertAsync(new ChiTietDonHang
                {
                    DonHangId = donHang.Id,
                    SanPhamId = ct.SanPhamId,
                    SanPhamBienThe = ct.SanPhamBienThe,
                    QuaTang = ct.QuaTang,
                    SoLuong = ct.SoLuong,
                    Gia = ct.Gia,
                    TrangThai = ct.TrangThai
                });
            }

            donHang.TongSoLuong = tongSoLuong;
            donHang.TongTien = tongTien;
        }

        private async Task RecalculateAsync(DonHang donHang)
        {
            var details = await _chiTietRepo.GetListAsync(x => x.DonHangId == donHang.Id);

            donHang.TongSoLuong = details.Sum(x => x.SoLuong);

            var tongTien = details.Sum(x => x.Gia * x.SoLuong);

            if (donHang.GiamGiaVoucher.HasValue)
            {
                tongTien -= donHang.GiamGiaVoucher.Value;
            }

            if (tongTien < 0)
                tongTien = 0;

            donHang.TongTien = tongTien;
        }

        [Authorize]
        public async Task<PagedResultDto<DonHangDto>> GetMyOrdersPagedAsync(
            int skipCount,
            int maxResultCount,
            int? trangThai
)
        {
            var userId = CurrentUser.GetId();

            var query = (await Repository.GetQueryableAsync())
                .AsNoTracking()
                .Where(x => x.TaiKhoanKhachHangId == userId);

            // Filter trạng thái
            if (trangThai.HasValue)
            {
                query = query.Where(x => x.TrangThai == trangThai.Value);
            }

            var totalCount = await AsyncExecuter.LongCountAsync(query);

            var orders = await AsyncExecuter.ToListAsync(
                query
                .OrderByDescending(x => x.NgayDat)
                .Skip(skipCount)
                .Take(maxResultCount)
                .Select(x => new DonHangDto
                {
                    Id = x.Id,
                    NgayDat = x.NgayDat,
                    TongTien = x.TongTien,
                    TrangThai = x.TrangThai,
                    DiaChi = x.DiaChi,
                    SoDienThoai = x.SoDienThoai,
                    PhuongThucThanhToan = x.PhuongThucThanhToan,
                    GiamGiaVoucher = x.GiamGiaVoucher,

                    ChiTietDonHangDtos = x.ChiTietDonHangs.Select(ct => new ChiTietDonHangDto
                    {
                        SanPhamId = ct.SanPhamId,
                        TenSanPham = ct.SanPham.Ten,
                        SoLuong = ct.SoLuong,
                        Gia = ct.Gia,
                        SanPhamBienThe = ct.SanPhamBienThe,
                        QuaTang = ct.QuaTang
                    }).ToList()
                })
            );

            return new PagedResultDto<DonHangDto>(
                totalCount,
                orders
            );
        }

        private async Task<string> GenerateOrderCodeAsync()
        {
            var code = "DH" + DateTime.Now.ToString("yyyyMMddHHmmssfff");

            var exists = await Repository.AnyAsync(x => x.Ma == code);

            if (exists)
            {
                code = "DH" + DateTime.Now.ToString("yyyyMMddHHmmssfff")
                       + new Random().Next(10, 99);
            }

            return code;
        }

        [Authorize]
        public async Task CancelOrderAsync(Guid id)
        {
            var order = await Repository.GetAsync(id);

            if (order.TrangThai != 0 && order.TrangThai != 1)
            {
                throw new UserFriendlyException("Chỉ có thể hủy đơn hàng khi đang chờ xác nhận hoặc đang xử lý");
            }

            order.TrangThai = 4;

            await Repository.UpdateAsync(order);

            await UnitOfWorkManager.Current.SaveChangesAsync();

            _backgroundJobClient.Enqueue<DonHangEmailJob>(
                job => job.SendCancelOrderAsync(order.Id));
        }

        private async Task<decimal> XuLyVoucherAsync(
            Guid voucherId,
            Guid donHangId,
            decimal tongTienTruocGiam)
        {
            var userId = CurrentUser.GetId();
            var now = DateTime.UtcNow;

            var voucher = await _voucherRepo.GetAsync(voucherId);

            // --- Validate trạng thái ---
            if (voucher.TrangThai != TrangThaiVoucher.DangHoatDong)
                throw new UserFriendlyException("Voucher không còn hoạt động.");

            if (voucher.DaDung >= voucher.TongSoLuong)
                throw new UserFriendlyException("Voucher đã hết số lượng.");

            if (voucher.ThoiHanBatDau.HasValue && now < voucher.ThoiHanBatDau.Value)
                throw new UserFriendlyException("Voucher chưa đến thời gian sử dụng.");

            if (voucher.ThoiHanKetThuc.HasValue && now > voucher.ThoiHanKetThuc.Value)
                throw new UserFriendlyException("Voucher đã hết hạn.");

            if (tongTienTruocGiam < voucher.DonHangToiThieu)
                throw new UserFriendlyException(
                    $"Đơn hàng tối thiểu {voucher.DonHangToiThieu:N0}đ mới được áp dụng voucher.");

            // --- Kiểm tra giới hạn / user ---
            var soLanDaDung = await _voucherUsedRepo
                .CountAsync(x => x.VoucherId == voucherId && x.UserId == userId);

            if (soLanDaDung >= voucher.GioiHanMoiUser)
                throw new UserFriendlyException("Bạn đã dùng hết lượt cho voucher này.");

            // --- Kiểm tra ví nếu voucher phát hành riêng ---
            if (voucher.ChiPhatHanhCuThe)
            {
                var viVoucher = await _voucherUserRepo
                    .FirstOrDefaultAsync(x => x.VoucherId == voucherId && x.UserId == userId)
                    ?? throw new UserFriendlyException("Bạn không có voucher này trong ví.");

                if ((viVoucher.SoLuongNhan - viVoucher.DaDung) <= 0)
                    throw new UserFriendlyException("Bạn đã dùng hết voucher này.");

                // Trừ lượt trong ví
                viVoucher.DaDung++;
                await _voucherUserRepo.UpdateAsync(viVoucher);
            }

            // --- Tính giá trị giảm ---
            decimal giaTriGiam = voucher.LaPhanTram
                ? tongTienTruocGiam * voucher.GiamGia / 100m
                : voucher.GiamGia;

            if (voucher.GiamToiDa.HasValue)
                giaTriGiam = Math.Min(giaTriGiam, voucher.GiamToiDa.Value);

            giaTriGiam = Math.Min(giaTriGiam, tongTienTruocGiam);
            giaTriGiam = Math.Round(giaTriGiam, 0);

            // --- Ghi lịch sử ---
            await _voucherUsedRepo.InsertAsync(new VoucherDaSuDung
            {
                VoucherId = voucherId,
                UserId = userId,
                DonHangId = donHangId,
                GiaTriGiam = giaTriGiam,
                NgaySuDung = now,
            });

            // --- Tăng DaDung, kiểm tra hết số lượng ---
            voucher.DaDung++;
            if (voucher.DaDung >= voucher.TongSoLuong)
                voucher.TrangThai = TrangThaiVoucher.HetSoLuong;

            await _voucherRepo.UpdateAsync(voucher);

            return giaTriGiam;
        }
    }
}
