using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VietlifeStore.Entity.SanPhams;

namespace VietlifeStore.Configurations.SanPhams
{
    public class SanPhamBienTheConfiguration : IEntityTypeConfiguration<SanPhamBienThe>
    {
        public void Configure(EntityTypeBuilder<SanPhamBienThe> builder)
        {
            builder.ToTable(VietlifeStoreConsts.DbTablePrefix + "SanPhamBienThe");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Gia)
                   .HasPrecision(18, 2);

            builder.Property(x => x.GiaKhuyenMai)
                   .HasPrecision(18, 2);

            builder.HasOne(x => x.SanPham)
                   .WithMany(x => x.SanPhamBienThes)
                   .HasForeignKey(x => x.SanPhamId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
