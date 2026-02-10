using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VietlifeStore.Entity.DonHangsList.Vouchers;
using VietlifeStore.Permissions;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

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
        public VouchersAppService(IRepository<Voucher, Guid> repository)
            : base(repository)
        {
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
    }
}
