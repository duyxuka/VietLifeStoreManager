using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities.Auditing;

namespace VietlifeStore.ChucNang.DatLichs.DatLichGiamGiaSanPhams
{
    public class ChuongTrinhGiamGia : FullAuditedEntity<Guid>
    {
        public string TenChuongTrinh { get; set; } = default!; // VD: Valentine
        public DateTime ThoiGianBatDau { get; set; }
        public DateTime ThoiGianKetThuc { get; set; }

        public LichGiamGiaTrangThai TrangThai { get; set; } = LichGiamGiaTrangThai.Pending;

        public string? MoTa { get; set; }
        public string? ActivateJobId { get; set; }
        public string? ExpireJobId { get; set; }

        public virtual ICollection<ChuongTrinhGiamGiaItem> Items { get; set; } = new List<ChuongTrinhGiamGiaItem>();
    }
}
