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
    public class ChuongTrinhGiamGiaConfiguration : IEntityTypeConfiguration<ChuongTrinhGiamGia>
    {
        public void Configure(EntityTypeBuilder<ChuongTrinhGiamGia> builder)
        {
            builder.ToTable(VietlifeStoreConsts.DbTablePrefix + "ChuongTrinhGiamGia");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.TenChuongTrinh)
                   .IsRequired()
                   .HasMaxLength(255);

            builder.Property(x => x.MoTa)
                   .HasMaxLength(1000);

            builder.Property(x => x.TrangThai)
                   .IsRequired();

            // Index cực quan trọng cho job
            builder.HasIndex(x => new { x.TrangThai, x.ThoiGianBatDau, x.ThoiGianKetThuc });

            // Quan hệ 1 - nhiều
            builder.HasMany(x => x.Items)
                   .WithOne(x => x.ChuongTrinhGiamGia)
                   .HasForeignKey(x => x.ChuongTrinhId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
