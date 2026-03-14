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
        private readonly IConfiguration _configuration;
        private readonly IRepository<SanPham, Guid> _sanPhamRepo;

        public DonHangsAppService(
            IRepository<DonHang, Guid> repository,
            IRepository<ChiTietDonHang, Guid> chiTietRepo,
            IRepository<Voucher, Guid> voucherRepo,
            IRepository<VoucherDaSuDung, Guid> voucherUsedRepo,
            IConfiguration configuration,
            IRepository<SanPham, Guid> sanPhamRepo)
            : base(repository)
        {
            _chiTietRepo = chiTietRepo;
            _voucherRepo = voucherRepo;
            _voucherUsedRepo = voucherUsedRepo;
            _sanPhamRepo = sanPhamRepo;

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
        public override async Task<DonHangDto> CreateAsync(CreateUpdateDonHangDto input)
        {
            var donHang = ObjectMapper.Map<CreateUpdateDonHangDto, DonHang>(input);

            donHang.NgayDat = DateTime.Now;
            donHang.Ma = await GenerateOrderCodeAsync();
            donHang.GiamGiaVoucher = input.GiamGiaVoucher;

            await Repository.InsertAsync(donHang, autoSave: true);

            if (input.VoucherId.HasValue)
            {
                var voucher = await _voucherRepo.GetAsync(input.VoucherId.Value);

                if (!voucher.TrangThai)
                    throw new UserFriendlyException("Voucher không hợp lệ");

                if (voucher.SoLuong <= 0)
                    throw new UserFriendlyException("Voucher đã hết");

                if (voucher.ThoiHanBatDau.HasValue && DateTime.Now < voucher.ThoiHanBatDau)
                    throw new UserFriendlyException("Voucher chưa bắt đầu");

                if (voucher.ThoiHanKetThuc.HasValue && DateTime.Now > voucher.ThoiHanKetThuc)
                    throw new UserFriendlyException("Voucher đã hết hạn");

                var userId = CurrentUser.GetId();

                var used = await _voucherUsedRepo.AnyAsync(x =>
                    x.UserId == userId &&
                    x.VoucherId == voucher.Id);

                if (used)
                    throw new UserFriendlyException("Bạn đã sử dụng voucher này");

                voucher.SoLuong--;
                await _voucherRepo.UpdateAsync(voucher);

                await _voucherUsedRepo.InsertAsync(new VoucherDaSuDung
                {
                    VoucherId = voucher.Id,
                    UserId = userId,
                    DonHangId = donHang.Id
                });
            }

            await SaveChiTietAsync(donHang, input.ChiTietDonHangs);
            if (donHang.GiamGiaVoucher.HasValue)
            {
                donHang.TongTien -= donHang.GiamGiaVoucher.Value;

                if (donHang.TongTien < 0)
                    donHang.TongTien = 0;
            }

            await UnitOfWorkManager.Current.SaveChangesAsync();
            _ = Task.Run(() => SendEmailOrderSuccess(donHang));

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

        private async Task SendEmailOrderSuccess(DonHang order)
        {
            var date = DateTime.Now;

            var smtpSection = _configuration.GetSection("Abp:Mailing:Smtp");

            var host = smtpSection["Host"];
            var port = int.Parse(smtpSection["Port"]);
            var username = smtpSection["UserName"];
            var password = smtpSection["Password"];
            var enableSsl = bool.Parse(smtpSection["EnableSsl"]);

            var senderEmail = new MailAddress(username, "Vietlife Store");
            var receiverEmail = new MailAddress(order.Email);

            var details = await _chiTietRepo.GetListAsync(x => x.DonHangId == order.Id);

            var productIds = details.Select(x => x.SanPhamId).ToList();

            var products = await AsyncExecuter.ToListAsync(
                (await _sanPhamRepo.GetQueryableAsync())
                .Where(x => productIds.Contains(x.Id))
            );

            string productDetails = "<table style='width:100%;border-collapse:collapse;'>"
            + "<thead>"
            + "<tr style='background:#f2f2f2;'>"
            + "<th style='padding:10px;border:1px solid #ddd;'>Tên sản phẩm</th>"
            + "<th style='padding:10px;border:1px solid #ddd;'>Ảnh</th>"
            + "<th style='padding:10px;border:1px solid #ddd;'>Biến thể</th>"
            + "<th style='padding:10px;border:1px solid #ddd;'>Quà tặng</th>"
            + "<th style='padding:10px;border:1px solid #ddd;'>Số lượng</th>"
            + "<th style='padding:10px;border:1px solid #ddd;'>Giá</th>"
            + "<th style='padding:10px;border:1px solid #ddd;'>Tổng</th>"
            + "</tr>"
            + "</thead><tbody>";

            foreach (var item in details)
            {
                var product = products.FirstOrDefault(x => x.Id == item.SanPhamId);

                var image = product?.Anh ?? "";

                productDetails += "<tr>"
                + $"<td style='padding:10px;border:1px solid #ddd'>{product?.Ten}</td>"
                + $"<td style='padding:10px;border:1px solid #ddd'><img src='http://42.96.61.186:8090/files/{image}' width='80'/></td>"
                + $"<td style='padding:10px;border:1px solid #ddd'>{item.SanPhamBienThe}</td>"
                + $"<td style='padding:10px;border:1px solid #ddd'>{item.QuaTang}</td>"
                + $"<td style='padding:10px;border:1px solid #ddd'>{item.SoLuong}</td>"
                + $"<td style='padding:10px;border:1px solid #ddd'>{item.Gia:#,##0} VNĐ</td>"
                + $"<td style='padding:10px;border:1px solid #ddd'>{(item.Gia * item.SoLuong):#,##0} VNĐ</td>"
                + "</tr>";
            }

            productDetails += $"<tr style='text-align:right'>"
            + $"<td colspan='6' style='padding:15px;border:1px solid #ddd'><b>Tổng tiền:</b></td>"
            + $"<td style='padding:15px;border:1px solid #ddd'><b>{order.TongTien:#,##0} VNĐ</b></td>"
            + "</tr>";

            productDetails += "</tbody></table>";

            string accountorder =
            "<table style='width:100%;border-collapse:collapse;'>"
            + "<tr style='background:#f2f2f2'>"
            + "<th style='padding:10px;border:1px solid #ddd'>Khách hàng</th>"
            + "<th style='padding:10px;border:1px solid #ddd'>SĐT</th>"
            + "<th style='padding:10px;border:1px solid #ddd'>Địa chỉ</th>"
            + "<th style='padding:10px;border:1px solid #ddd'>Thanh toán</th>"
            + "</tr>"
            + "<tr>"
            + $"<td style='padding:10px;border:1px solid #ddd'>{order.Ten}</td>"
            + $"<td style='padding:10px;border:1px solid #ddd'>{order.SoDienThoai}</td>"
            + $"<td style='padding:10px;border:1px solid #ddd'>{order.DiaChi}</td>"
            + $"<td style='padding:10px;border:1px solid #ddd'>{order.PhuongThucThanhToan}</td>"
            + "</tr></table>";

            string body =
            "<div style='font-family:Arial;font-size:14px'>"
            + "<h2 style='color:#10cb04'>Bạn đã đặt hàng thành công tại Vietlife Store</h2>"
            + $"<p>Mã đơn hàng: <b>{order.Ma}</b></p>"
            + $"<p>Ngày đặt: {date:dd/MM/yyyy HH:mm}</p>"
            + "<h3>Thông tin khách hàng</h3>"
            + accountorder
            + "<h3>Chi tiết đơn hàng</h3>"
            + productDetails
            + "<p>Chúng tôi sẽ liên hệ xác nhận đơn hàng sớm.</p>"
            + "</div>";

            using (var smtp = new SmtpClient(host, port))
            {
                smtp.Credentials = new NetworkCredential(username, password);
                smtp.EnableSsl = enableSsl;

                using (var message = new MailMessage(senderEmail, receiverEmail)
                {
                    Subject = $"Xác nhận đơn hàng #{order.Ma}",
                    Body = body,
                    IsBodyHtml = true
                })
                {
                    await smtp.SendMailAsync(message);
                }
            }
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

            _ = Task.Run(() => SendEmailCancelOrder(order));
        }

        private async Task SendEmailCancelOrder(DonHang order)
        {
            var smtpSection = _configuration.GetSection("Abp:Mailing:Smtp");

            var host = smtpSection["Host"];
            var port = int.Parse(smtpSection["Port"]);
            var username = smtpSection["UserName"];
            var password = smtpSection["Password"];
            var enableSsl = bool.Parse(smtpSection["EnableSsl"]);

            var senderEmail = new MailAddress(username, "Vietlife Store");
            var receiverEmail = new MailAddress(order.Email);

            var details = await _chiTietRepo.GetListAsync(x => x.DonHangId == order.Id);

            var productIds = details.Select(x => x.SanPhamId).ToList();

            var products = await AsyncExecuter.ToListAsync(
                (await _sanPhamRepo.GetQueryableAsync())
                .Where(x => productIds.Contains(x.Id))
            );

            string productList = "";

            foreach (var item in details)
            {
                var product = products.FirstOrDefault(x => x.Id == item.SanPhamId);

                productList +=
                "<tr>"
                + $"<td style='padding:8px;border:1px solid #ddd'>{product?.Ten}-{item.SanPhamBienThe}</td>"
                + $"<td style='padding:8px;border:1px solid #ddd'>{item.SoLuong}</td>"
                + $"<td style='padding:8px;border:1px solid #ddd'>{item.Gia:#,##0} VNĐ</td>"
                + "</tr>";
            }

            string body =
            "<div style='font-family:Arial;font-size:14px'>"
            + "<h2 style='color:#ff4d4f'>Đơn hàng đã được hủy</h2>"
            + $"<p>Mã đơn hàng: <b>{order.Ma}</b></p>"
            + $"<p>Khách hàng: <b>{order.Ten}</b></p>"
            + $"<p>Số điện thoại: {order.SoDienThoai}</p>"
            + $"<p>Địa chỉ: {order.DiaChi}</p>"

            + "<h3>Danh sách sản phẩm</h3>"

            + "<table style='width:100%;border-collapse:collapse'>"
            + "<tr style='background:#f5f5f5'>"
            + "<th style='padding:8px;border:1px solid #ddd'>Sản phẩm</th>"
            + "<th style='padding:8px;border:1px solid #ddd'>Số lượng</th>"
            + "<th style='padding:8px;border:1px solid #ddd'>Giá</th>"
            + "</tr>"
            + productList
            + "</table>"

            + $"<p style='margin-top:20px'>Tổng tiền đơn hàng: <b>{order.TongTien:#,##0} VNĐ</b></p>"

            + "<p style='color:#999'>Nếu đây là nhầm lẫn vui lòng đặt lại đơn hàng.</p>"

            + "<p>Cảm ơn bạn đã quan tâm đến Vietlife Store.</p>"
            + "</div>";

            using (var smtp = new SmtpClient(host, port))
            {
                smtp.Credentials = new NetworkCredential(username, password);
                smtp.EnableSsl = enableSsl;

                using (var message = new MailMessage(senderEmail, receiverEmail)
                {
                    Subject = $"Đơn hàng #{order.Ma} đã bị hủy",
                    Body = body,
                    IsBodyHtml = true
                })
                {
                    await smtp.SendMailAsync(message);
                }
            }
        }
    }
}
