using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VietlifeStore.ChucNang.DatLichs.DatLichVouchers;
using VietlifeStore.Entity.DonHangs;

namespace VietlifeStore.Configurations.DonHangs
{
    public class VoucherConfiguration : IEntityTypeConfiguration<Voucher>
    {
        public void Configure(EntityTypeBuilder<Voucher> builder)
        {
            builder.ToTable(VietlifeStoreConsts.DbTablePrefix + "Voucher");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.MaVoucher)
                   .IsRequired()
                   .HasMaxLength(50);

            builder.HasIndex(x => x.MaVoucher)
                   .IsUnique();

            builder.Property(x => x.TenVoucher)
                   .HasMaxLength(200);

            builder.Property(x => x.MoTa)
                   .HasMaxLength(500);

            builder.Property(x => x.LoaiVoucher)
                   .IsRequired()
                   .HasConversion<int>();

            builder.Property(x => x.PhamVi)
                   .IsRequired()
                   .HasConversion<int>();

            builder.Property(x => x.GiamGia)
                   .IsRequired()
                   .HasPrecision(18, 2);

            builder.Property(x => x.GiamToiDa)
                   .HasPrecision(18, 2);

            builder.Property(x => x.DonHangToiThieu)
                   .IsRequired()
                   .HasPrecision(18, 2)
                   .HasDefaultValue(0);

            builder.Property(x => x.TongSoLuong)
                   .IsRequired()
                   .HasDefaultValue(0);

            builder.Property(x => x.DaDung)
                   .IsRequired()
                   .HasDefaultValue(0)
                   .IsConcurrencyToken(); // chống race condition khi nhiều user dùng cùng lúc

            builder.Property(x => x.GioiHanMoiUser)
                   .IsRequired()
                   .HasDefaultValue(1);

            builder.Property(x => x.TrangThai)
                   .IsRequired()
                   .HasConversion<int>()
                   .HasDefaultValue(TrangThaiVoucher.ChuaKichHoat);

            builder.Property(x => x.HangfireActivateJobId)
                   .HasMaxLength(100)
                   .IsRequired(false);

            builder.Property(x => x.HangfireExpireJobId)
                   .HasMaxLength(100)
                   .IsRequired(false);

            builder.Property(x => x.HangfireWarnJobId)
                   .HasMaxLength(100)
                   .IsRequired(false);

            // Relations
            builder.HasMany(x => x.LichSuSuDung)
                   .WithOne(x => x.Voucher)
                   .HasForeignKey(x => x.VoucherId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.DoiTuongApDung)
                   .WithOne(x => x.Voucher)
                   .HasForeignKey(x => x.VoucherId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(x => x.DanhSachNguoiDung)
                   .WithOne(x => x.Voucher)
                   .HasForeignKey(x => x.VoucherId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(x => x.Schedules)
                   .WithOne(x => x.Voucher)
                   .HasForeignKey(x => x.VoucherId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
