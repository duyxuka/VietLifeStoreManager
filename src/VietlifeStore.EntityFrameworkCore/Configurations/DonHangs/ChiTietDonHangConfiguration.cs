using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VietlifeStore.Entity.DonHangs;
using VietlifeStore.Entity.SanPhams;

namespace VietlifeStore.Configurations.DonHangs
{
    public class ChiTietDonHangConfiguration : IEntityTypeConfiguration<ChiTietDonHang>
    {
        public void Configure(EntityTypeBuilder<ChiTietDonHang> builder)
        {
            builder.ToTable(VietlifeStoreConsts.DbTablePrefix + "ChiTietDonHang");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.SoLuong)
                   .IsRequired();

            builder.Property(x => x.Gia)
                   .HasPrecision(18, 2);

            builder.Property(x => x.SanPhamBienThe)
                   .HasMaxLength(256);

            builder.Property(x => x.QuaTang)
                   .HasMaxLength(256);

            builder.HasOne(x => x.DonHang)
                   .WithMany(x => x.ChiTietDonHangs)
                   .HasForeignKey(x => x.DonHangId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.SanPham)
                   .WithMany()
                   .HasForeignKey(x => x.SanPhamId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
