using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VietlifeStore.Entity.DonHangsList.ChiTietDonHangs;
using VietlifeStore.Entity.DonHangsList.DonHangs;
using VietlifeStore.Permissions;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Uow;

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

        public DonHangsAppService(
            IRepository<DonHang, Guid> repository,
            IRepository<ChiTietDonHang, Guid> chiTietRepo)
            : base(repository)
        {
            _chiTietRepo = chiTietRepo;

            GetPolicyName = VietlifeStorePermissions.DonHang.View;
            GetListPolicyName = VietlifeStorePermissions.DonHang.View;
            CreatePolicyName = VietlifeStorePermissions.DonHang.Create;
            UpdatePolicyName = VietlifeStorePermissions.DonHang.Update;
            DeletePolicyName = VietlifeStorePermissions.DonHang.Delete;
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
        public override async Task<DonHangDto> GetAsync(Guid id)
        {
            var query = await Repository.WithDetailsAsync(x => x.ChiTietDonHangs);
            var entity = await AsyncExecuter.FirstOrDefaultAsync(query, x => x.Id == id);

            if (entity == null)
                throw new EntityNotFoundException(typeof(DonHang), id);

            return ObjectMapper.Map<DonHang, DonHangDto>(entity);
        }

        // ================= CREATE =================
        public override async Task<DonHangDto> CreateAsync(CreateUpdateDonHangDto input)
        {
            var donHang = ObjectMapper.Map<CreateUpdateDonHangDto, DonHang>(input);
            donHang.NgayDat = DateTime.Now;

            await Repository.InsertAsync(donHang, autoSave: true);

            await SaveChiTietAsync(donHang, input.ChiTietDonHangs);

            await UnitOfWorkManager.Current.SaveChangesAsync();

            return ObjectMapper.Map<DonHang, DonHangDto>(donHang);
        }

        // ================= UPDATE =================
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
                        GiamGiaVoucher = ct.GiamGiaVoucher,
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
                tongTien += (ct.Gia - ct.GiamGiaVoucher) * ct.SoLuong;

                await _chiTietRepo.InsertAsync(new ChiTietDonHang
                {
                    DonHangId = donHang.Id,
                    SanPhamId = ct.SanPhamId,
                    SanPhamBienThe = ct.SanPhamBienThe,
                    QuaTang = ct.QuaTang,
                    SoLuong = ct.SoLuong,
                    Gia = ct.Gia,
                    GiamGiaVoucher = ct.GiamGiaVoucher,
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
            donHang.TongTien = details.Sum(x =>
                (x.Gia - x.GiamGiaVoucher) * x.SoLuong);
        }
    }
}
