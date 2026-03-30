using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VietlifeStore.ChucNang.DatLichs.DatLichVouchers;
using VietlifeStore.Entity.SanPhams;
using Volo.Abp.Domain.Entities.Auditing;

namespace VietlifeStore.Entity.DonHangs
{
    public class Voucher : FullAuditedAggregateRoot<Guid>
    {
        protected Voucher() { }

        // Constructor dùng để tạo với Id xác định trước
        public Voucher(Guid id) : base(id) { }
        public string MaVoucher { get; set; }          // VD: "SALE50", "FREESHIP100K"
        public string TenVoucher { get; set; }          // "Giảm 50k đơn từ 200k"
        public string MoTa { get; set; }

        public LoaiVoucher LoaiVoucher { get; set; }    // Giảm đơn / ship / hoàn tiền
        public PhamViVoucher PhamVi { get; set; }        // Toàn shop / sản phẩm cụ thể...

        public decimal GiamGia { get; set; }
        public bool LaPhanTram { get; set; } = false;
        public decimal? GiamToiDa { get; set; }          // Nếu là %, giới hạn tối đa giảm bao nhiêu

        public decimal DonHangToiThieu { get; set; }

        // Số lượng
        public int TongSoLuong { get; set; }             // Tổng voucher phát hành
        public int DaDung { get; set; } = 0;             // Đã được dùng (dùng ConcurrencyCheck)
        public int GioiHanMoiUser { get; set; } = 1;     // Mỗi user dùng tối đa N lần

        public DateTime? ThoiHanBatDau { get; set; }
        public DateTime? ThoiHanKetThuc { get; set; }

        // Phát hành: null = tất cả user, có giá trị = chỉ user được cấp phát
        public bool ChiPhatHanhCuThe { get; set; } = false;

        public TrangThaiVoucher TrangThai { get; set; } = TrangThaiVoucher.ChuaKichHoat;

        // Hangfire job tracking
        public string HangfireActivateJobId { get; set; }
        public string HangfireExpireJobId { get; set; }
        public string? HangfireWarnJobId { get; set; }

        // Navigation
        public ICollection<VoucherDaSuDung> LichSuSuDung { get; set; }
        public ICollection<VoucherDoiTuong> DoiTuongApDung { get; set; }
        public ICollection<VoucherNguoiDung> DanhSachNguoiDung { get; set; }
        public ICollection<VoucherSchedule> Schedules { get; set; }
    }
}
