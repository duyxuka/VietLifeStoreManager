using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VietlifeStore.Entity.DonHangs;
using Volo.Abp.Identity;

namespace VietlifeStore.Configurations.DonHangs
{
    public class DonHangConfiguration : IEntityTypeConfiguration<DonHang>
    {
        public void Configure(EntityTypeBuilder<DonHang> builder)
        {
            builder.ToTable(VietlifeStoreConsts.DbTablePrefix + "DonHang");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Ma)
              .IsRequired()
              .HasMaxLength(50);
            builder.Property(x => x.Ten)
                   .IsRequired()
                   .HasMaxLength(256);

            builder.Property(x => x.DiaChi)
                   .IsRequired()
                   .HasMaxLength(500);

            builder.Property(x => x.Email)
                   .HasMaxLength(256);

            builder.Property(x => x.SoDienThoai)
                   .HasMaxLength(20);

            builder.Property(x => x.GhiChu)
                   .HasMaxLength(1000);

            builder.Property(x => x.PhuongThucThanhToan)
                   .HasMaxLength(100);

            builder.Property(x => x.TrangThai)
                   .IsRequired();

            builder.Property(x => x.TongTien)
                   .HasPrecision(18, 2);

            builder.Property(x => x.TongSoLuong)
                   .HasPrecision(18, 2);

            builder.HasOne<IdentityUser>()
                   .WithMany()
                   .HasForeignKey(x => x.TaiKhoanKhachHangId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.ChiTietDonHangs)
                   .WithOne(x => x.DonHang)
                   .HasForeignKey(x => x.DonHangId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
