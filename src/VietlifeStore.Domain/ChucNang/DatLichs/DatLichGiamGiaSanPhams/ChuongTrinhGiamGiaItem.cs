using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VietlifeStore.Entity.SanPhams;
using Volo.Abp.Domain.Entities;

namespace VietlifeStore.ChucNang.DatLichs.DatLichGiamGiaSanPhams
{
    public class ChuongTrinhGiamGiaItem : Entity<Guid>
    {
        public Guid ChuongTrinhId { get; set; }
        public virtual ChuongTrinhGiamGia ChuongTrinhGiamGia { get; set; }
        public Guid? SanPhamId { get; set; }
        public virtual SanPham? SanPham { get; set; }
        public Guid? BienTheId { get; set; }
        public virtual SanPhamBienThe? BienThe { get; set; }
        // 🔥 Giá giảm nhập tay
        public decimal GiaSauGiam { get; set; }

        // Snapshot rollback
        public decimal? GiaGocSnapshot { get; set; }
        public decimal? GiaGocBienTheSnapshot { get; set; }

        // 🎁 Quà tặng
        public Guid? QuaTangId { get; set; }
        public virtual QuaTang? QuaTang { get; set; }
    }
}
