using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
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

            builder.Property(x => x.GiamGia)
                   .HasPrecision(18, 2);

            builder.Property(x => x.DonHangToiThieu)
                   .HasPrecision(18, 2);

            builder.Property(x => x.SoLuong)
                   .IsRequired();

            builder.Property(x => x.TrangThai)
                   .HasDefaultValue(false);
        }
    }
}
