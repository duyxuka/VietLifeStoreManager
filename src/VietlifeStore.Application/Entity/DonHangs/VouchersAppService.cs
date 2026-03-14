using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VietlifeStore.Entity.DonHangsList.Vouchers;
using VietlifeStore.Permissions;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;

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
        public VouchersAppService(
            IRepository<Voucher, Guid> repository,
            IRepository<VoucherDaSuDung, Guid> voucherUsedRepo)
            : base(repository)
        {
            _voucherUsedRepo = voucherUsedRepo;

            GetPolicyName = VietlifeStorePermissions.Voucher.View;
            GetListPolicyName = VietlifeStorePermissions.Voucher.View;
            CreatePolicyName = VietlifeStorePermissions.Voucher.Create;
            UpdatePolicyName = VietlifeStorePermissions.Voucher.Update;
            DeletePolicyName = VietlifeStorePermissions.Voucher.Delete;
        }

        // ================= CREATE =================
        [Authorize(VietlifeStorePermissions.Voucher.Create)]
        public override async Task<VoucherDto> CreateAsync(CreateUpdateVoucherDto input)
        {
            var entity = new Voucher
            {
                MaVoucher = input.MaVoucher,
                GiamGia = input.GiamGia,
                LaPhanTram = input.LaPhanTram,
                JobId = "0",
                DonHangToiThieu = input.DonHangToiThieu,
                ThoiHanBatDau = input.ThoiHanBatDau,
                ThoiHanKetThuc = input.ThoiHanKetThuc,
                SoLuong = input.SoLuong,
                LaDatLich = input.LaDatLich,
                TrangThai = input.TrangThai
            };

            await Repository.InsertAsync(entity, autoSave: true);
            return MapToGetOutputDto(entity);
        }

        // ================= UPDATE =================
        [Authorize(VietlifeStorePermissions.Voucher.Update)]
        public override async Task<VoucherDto> UpdateAsync(Guid id, CreateUpdateVoucherDto input)
        {
            var entity = await Repository.GetAsync(id);

            entity.MaVoucher = input.MaVoucher;
            entity.GiamGia = input.GiamGia;
            entity.LaPhanTram = input.LaPhanTram;
            entity.JobId = input.JobId;
            entity.DonHangToiThieu = input.DonHangToiThieu;
            entity.ThoiHanBatDau = input.ThoiHanBatDau;
            entity.ThoiHanKetThuc = input.ThoiHanKetThuc;
            entity.SoLuong = input.SoLuong;
            entity.LaDatLich = input.LaDatLich;
            entity.TrangThai = input.TrangThai;

            await Repository.UpdateAsync(entity, autoSave: true);
            return MapToGetOutputDto(entity);
        }

        // ================= DELETE MULTIPLE =================
        [Authorize(VietlifeStorePermissions.Voucher.Delete)]
        public async Task DeleteMultipleAsync(IEnumerable<Guid> ids)
        {
            await Repository.DeleteManyAsync(ids);
            await UnitOfWorkManager.Current.SaveChangesAsync();
        }

        // ================= GET ALL ACTIVE =================
        [Authorize(VietlifeStorePermissions.Voucher.View)]
        public async Task<List<VoucherInListDto>> GetListAllAsync()
        {
            var list = await AsyncExecuter.ToListAsync(
                (await Repository.GetQueryableAsync())
                    .Where(x => x.TrangThai)
                    .OrderByDescending(x => x.CreationTime)
            );

            return ObjectMapper.Map<List<Voucher>, List<VoucherInListDto>>(list);
        }

        // ================= FILTER + PAGING =================
        [Authorize(VietlifeStorePermissions.Voucher.View)]
        public async Task<PagedResultDto<VoucherInListDto>> GetListFilterAsync(BaseListFilterDto input)
        {
            var query = (await Repository.GetQueryableAsync())
                .WhereIf(!string.IsNullOrWhiteSpace(input.Keyword),
                    x => x.MaVoucher.Contains(input.Keyword));

            var totalCount = await AsyncExecuter.LongCountAsync(query);

            var items = await AsyncExecuter.ToListAsync(
                query
                    .OrderByDescending(x => x.CreationTime)
                    .Skip(input.SkipCount)
                    .Take(input.MaxResultCount)
            );

            return new PagedResultDto<VoucherInListDto>(
                totalCount,
                ObjectMapper.Map<List<Voucher>, List<VoucherInListDto>>(items)
            );
        }

        [Authorize]
        public async Task<List<VoucherDto>> GetAvailableVouchersAsync(decimal orderTotal)
        {
            var userId = CurrentUser.GetId();

            // lấy danh sách voucher user đã dùng
            var usedVoucherIds = await AsyncExecuter.ToListAsync(
                (await _voucherUsedRepo.GetQueryableAsync())
                .Where(x => x.UserId == userId)
                .Select(x => x.VoucherId)
            );

            var now = DateTime.Now;

            var vouchers = await AsyncExecuter.ToListAsync(
                (await Repository.GetQueryableAsync())
                .Where(x =>
                    x.TrangThai &&
                    x.SoLuong > 0 &&
                    x.DonHangToiThieu <= orderTotal &&
                    !usedVoucherIds.Contains(x.Id) &&
                    (x.ThoiHanBatDau == null || x.ThoiHanBatDau <= now) &&
                    (x.ThoiHanKetThuc == null || x.ThoiHanKetThuc >= now)
                )
                .OrderByDescending(x => x.CreationTime)
            );

            return ObjectMapper.Map<List<Voucher>, List<VoucherDto>>(vouchers);
        }

        [AllowAnonymous]
        public async Task<VoucherDto> ValidateVoucherAsync(string code, decimal orderTotal)
        {
            var voucher = await Repository.FirstOrDefaultAsync(x => x.MaVoucher == code);

            if (voucher == null)
                throw new UserFriendlyException("Voucher không tồn tại");

            if (!voucher.TrangThai)
                throw new UserFriendlyException("Voucher đã bị khóa");

            if (voucher.SoLuong <= 0)
                throw new UserFriendlyException("Voucher đã hết lượt sử dụng");

            if (voucher.ThoiHanBatDau.HasValue && DateTime.Now < voucher.ThoiHanBatDau)
                throw new UserFriendlyException("Voucher chưa đến thời gian sử dụng");

            if (voucher.ThoiHanKetThuc.HasValue && DateTime.Now > voucher.ThoiHanKetThuc)
                throw new UserFriendlyException("Voucher đã hết hạn");

            if (orderTotal < voucher.DonHangToiThieu)
                throw new UserFriendlyException($"Đơn tối thiểu {voucher.DonHangToiThieu}");

            return ObjectMapper.Map<Voucher, VoucherDto>(voucher);
        }
    }
}
