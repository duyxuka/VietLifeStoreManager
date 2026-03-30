using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VietlifeStore.Entity.DonHangs;
using VietlifeStore.Entity.SanPhams;
using VietlifeStore.Permissions;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.Uow;

namespace VietlifeStore.Entity.DashboardStat
{
    [UnitOfWork]
    [Authorize(VietlifeStorePermissions.Dashboard.View)]
    public class DashboardAppService : ApplicationService, IDashboardAppService
    {
        private readonly IRepository<DonHang, Guid> _donHangRepo;
        private readonly IRepository<ChiTietDonHang, Guid> _chiTietRepo;
        private readonly IRepository<SanPham, Guid> _sanPhamRepo;
        private readonly IIdentityUserRepository _userRepo;

        public DashboardAppService(
            IRepository<DonHang, Guid> donHangRepo,
            IRepository<ChiTietDonHang, Guid> chiTietRepo,
            IRepository<SanPham, Guid> sanPhamRepo,
            IIdentityUserRepository userRepo)
        {
            _donHangRepo = donHangRepo;
            _chiTietRepo = chiTietRepo;
            _sanPhamRepo = sanPhamRepo;
            _userRepo = userRepo;
        }

        public async Task<DashboardStatsDto> GetStatsAsync()
        {
            var now = DateTime.Now;
            var startToday = now.Date;
            var startMonth = new DateTime(now.Year, now.Month, 1);
            var startYear = new DateTime(now.Year, 1, 1);
            var start6Thang = now.AddMonths(-6).Date;

            // ── Queryable ──────────────────────────────────────────────
            var donQ = await _donHangRepo.GetQueryableAsync();
            var ctQ = await _chiTietRepo.GetQueryableAsync();
            var spQ = await _sanPhamRepo.GetQueryableAsync();

            // ── TỔNG QUAN ──────────────────────────────────────────────
            var tongDon = await AsyncExecuter.LongCountAsync(donQ);
            var tongSP = await AsyncExecuter.LongCountAsync(spQ);
            var tongKH = await _userRepo.GetCountAsync();
            var tongDT = await AsyncExecuter.SumAsync(
                                donQ.Where(x => x.TrangThai == 3)
                                    .Select(x => x.TongTien));

            // ── ĐƠN HÀNG THEO TRẠNG THÁI ──────────────────────────────
            var donCho = await AsyncExecuter.LongCountAsync(donQ.Where(x => x.TrangThai == 0));
            var donXuLy = await AsyncExecuter.LongCountAsync(donQ.Where(x => x.TrangThai == 1));
            var donGiao = await AsyncExecuter.LongCountAsync(donQ.Where(x => x.TrangThai == 2));
            var donHoan = await AsyncExecuter.LongCountAsync(donQ.Where(x => x.TrangThai == 3));
            var donHuy = await AsyncExecuter.LongCountAsync(donQ.Where(x => x.TrangThai == 4));

            // ── DOANH THU ──────────────────────────────────────────────
            var qHoan = donQ.Where(x => x.TrangThai == 3);
            var dtHomNay = await AsyncExecuter.SumAsync(
                               qHoan.Where(x => x.NgayDat >= startToday)
                                    .Select(x => x.TongTien));
            var dtThang = await AsyncExecuter.SumAsync(
                               qHoan.Where(x => x.NgayDat >= startMonth)
                                    .Select(x => x.TongTien));
            var dtNam = await AsyncExecuter.SumAsync(
                               qHoan.Where(x => x.NgayDat >= startYear)
                                    .Select(x => x.TongTien));

            // ── THANH TOÁN ─────────────────────────────────────────────
            // PhuongThucThanhToan: 0 = COD, 1 = Ví điện tử
            var soCOD = await AsyncExecuter.LongCountAsync(
                              donQ.Where(x => x.PhuongThucThanhToan == "COD"));
            var soVi = await AsyncExecuter.LongCountAsync(
                              donQ.Where(x => x.PhuongThucThanhToan == "VNPAY"));
            var dtCOD = await AsyncExecuter.SumAsync(
                              qHoan.Where(x => x.PhuongThucThanhToan == "COD")
                                   .Select(x => x.TongTien));
            var dtVi = await AsyncExecuter.SumAsync(
                              qHoan.Where(x => x.PhuongThucThanhToan == "VNPAY")
                                   .Select(x => x.TongTien));

            // ── BIỂU ĐỒ THEO THÁNG ────────────────────────────────────
            var donNam = await AsyncExecuter.ToListAsync(
                donQ.Where(x => x.NgayDat >= startYear)
                    .Select(x => new { x.NgayDat.Month, x.TongTien, x.TrangThai }));

            var doanhThuTheoThang = Enumerable.Range(1, 12).Select(t => new DoanhThuTheoThangDto
            {
                Thang = t,
                Label = $"T{t}",
                DoanhThu = donNam.Where(x => x.Month == t && x.TrangThai == 3)
                                 .Sum(x => x.TongTien)
            }).ToList();

            var donHangTheoThang = Enumerable.Range(1, 12).Select(t => new DonHangTheoThangDto
            {
                Thang = t,
                Label = $"T{t}",
                SoDon = donNam.Count(x => x.Month == t && x.TrangThai != 4),
                SoHuy = donNam.Count(x => x.Month == t && x.TrangThai == 4)
            }).ToList();

            // ── TOP 5 SẢN PHẨM BÁN CHẠY (6 tháng) ───────────────────
            var topSP = await AsyncExecuter.ToListAsync(
                (from ct in ctQ
                 join sp in spQ on ct.SanPhamId equals sp.Id
                 where ct.DonHang.TrangThai == 3
                       && ct.DonHang.NgayDat >= start6Thang
                 group new { ct, sp } by new { sp.Id, sp.Ten, sp.Anh, sp.Slug } into g
                 orderby g.Sum(x => x.ct.SoLuong) descending
                 select new TopSanPhamDto
                 {
                     Ten = g.Key.Ten,
                     Anh = g.Key.Anh,
                     Slug = g.Key.Slug,
                     SoLuongBan = (int)g.Sum(x => x.ct.SoLuong),
                     DoanhThu = g.Sum(x => x.ct.Gia * x.ct.SoLuong)
                 })
                .Take(5));

            // ── TOP 5 KHÁCH HÀNG CHI TIÊU NHIỀU (6 tháng) ───────────
            var topKH = await AsyncExecuter.ToListAsync(
                (from d in donQ
                 where d.TrangThai == 3 && d.NgayDat >= start6Thang
                 group d by new { d.Ten, d.SoDienThoai, d.Email } into g
                 orderby g.Sum(x => x.TongTien) descending
                 select new TopKhachHangDto
                 {
                     HoTen = g.Key.Ten,
                     SoDienThoai = g.Key.SoDienThoai,
                     Email = g.Key.Email ?? "",
                     SoDon = g.Count(),
                     TongChiTieu = g.Sum(x => x.TongTien)
                 })
                .Take(5));

            // ── TOP SẢN PHẨM ĐƯỢC XEM NHIỀU ─────────────────────────
            var topXem = await AsyncExecuter.ToListAsync(
                spQ.Where(x => x.TrangThai)
                   .OrderByDescending(x => x.LuotXem)
                   .Take(5)
                   .Select(x => new TopSanPhamXemDto
                   {
                       Ten = x.Ten,
                       Slug = x.Slug,
                       LuotXem = x.LuotXem
                   }));

            // ── TRUY CẬP (placeholder — tích hợp tracking sau) ───────
            // Nếu bạn có bảng TrackingLog thì query ở đây
            // Tạm thời dùng LuotXem tổng hôm nay từ SanPham
            var luotTruyCapHomNay = await AsyncExecuter.SumAsync(
                spQ.Select(x => (long)x.LuotXem));

            return new DashboardStatsDto
            {
                // Tổng quan
                TongDonHang = tongDon,
                TongSanPham = tongSP,
                TongKhachHang = tongKH,
                TongDoanhThu = tongDT,

                // Đơn hàng
                DonChoXacNhan = donCho,
                DonDangXuLy = donXuLy,
                DonDangGiao = donGiao,
                DonHoanThanh = donHoan,
                DonDaHuy = donHuy,

                // Doanh thu
                DoanhThuHomNay = dtHomNay,
                DoanhThuThangNay = dtThang,
                DoanhThuNamNay = dtNam,

                // Thanh toán
                SoDonCOD = soCOD,
                SoDonViDienTu = soVi,
                DoanhThuCOD = dtCOD,
                DoanhThuViDienTu = dtVi,

                // Biểu đồ
                DoanhThuTheoThang = doanhThuTheoThang,
                DonHangTheoThang = donHangTheoThang,

                // Top 5
                TopSanPham = topSP,
                TopKhachHang = topKH,

                // Truy cập
                LuotTruyCapHomNay = luotTruyCapHomNay,
                NguoiDungOnline = TrackingHub.OnlineCount,
                TopSanPhamXemNhieu = topXem,
            };
        }
    }
}
