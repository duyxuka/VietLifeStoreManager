using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VietlifeStore.ChucNang.DatLichs.DatLichGiamGiaSanPhams;

namespace VietlifeStore.Configurations.DatLichGiamGias
{
    public class ChuongTrinhGiamGiaItemConfiguration : IEntityTypeConfiguration<ChuongTrinhGiamGiaItem>
    {
        public void Configure(EntityTypeBuilder<ChuongTrinhGiamGiaItem> builder)
        {
            builder.ToTable(VietlifeStoreConsts.DbTablePrefix + "ChuongTrinhGiamGiaItem");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.GiaSauGiam)
                   .HasColumnType("decimal(18,2)");

            builder.Property(x => x.GiaGocSnapshot)
                   .HasColumnType("decimal(18,2)");

            builder.Property(x => x.GiaGocBienTheSnapshot)
                   .HasColumnType("decimal(18,2)");

            // ❗ unique để tránh 1 sản phẩm trùng trong cùng CT
            builder.HasIndex(x => new { x.ChuongTrinhId, x.SanPhamId, x.BienTheId })
                   .IsUnique();

            // (optional) index để query nhanh
            builder.HasIndex(x => x.SanPhamId);
            builder.HasIndex(x => x.BienTheId);

            // Quan hệ
            builder.HasOne(x => x.ChuongTrinhGiamGia)
                   .WithMany(x => x.Items)
                   .HasForeignKey(x => x.ChuongTrinhId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
