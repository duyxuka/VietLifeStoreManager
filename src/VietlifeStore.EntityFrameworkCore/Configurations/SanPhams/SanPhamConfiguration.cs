using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VietlifeStore.Entity.SanPhams;

namespace VietlifeStore.Configurations.SanPhams
{
    public class SanPhamConfiguration : IEntityTypeConfiguration<SanPham>
    {
        public void Configure(EntityTypeBuilder<SanPham> builder)
        {
            builder.ToTable(VietlifeStoreConsts.DbTablePrefix + "SanPham");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Ten)
                   .IsRequired()
                   .HasMaxLength(256);

            builder.Property(x => x.Slug)
                   .IsRequired()
                   .HasMaxLength(256);

            builder.HasIndex(x => x.Slug).IsUnique();

            builder.Property(x => x.Gia)
                   .HasPrecision(18, 2);

            builder.Property(x => x.GiaKhuyenMai)
                   .HasPrecision(18, 2);

            builder.Property(x => x.TrangThai)
                   .HasDefaultValue(true);

            builder.HasOne(x => x.DanhMucSanPham)
                   .WithMany(x => x.SanPhams)
                   .HasForeignKey(x => x.DanhMucId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.QuaTang)
                   .WithMany(x => x.SanPhams)
                   .HasForeignKey(x => x.QuaTangId)
                   .IsRequired(false)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
