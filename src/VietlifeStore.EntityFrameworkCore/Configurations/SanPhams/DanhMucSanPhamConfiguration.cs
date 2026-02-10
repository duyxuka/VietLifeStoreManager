using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VietlifeStore.Entity.SanPhams;

namespace VietlifeStore.Configurations.SanPhams
{
    public class DanhMucSanPhamConfiguration : IEntityTypeConfiguration<DanhMucSanPham>
    {
        public void Configure(EntityTypeBuilder<DanhMucSanPham> builder)
        {
            builder.ToTable(VietlifeStoreConsts.DbTablePrefix + "DanhMucSanPham");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Ten)
                   .IsRequired()
                   .HasMaxLength(256);

            builder.Property(x => x.Slug)
                   .IsRequired()
                   .HasMaxLength(256);

            builder.HasIndex(x => x.Slug)
                   .IsUnique();

            builder.Property(x => x.TrangThai)
                   .HasDefaultValue(true);

            builder.HasMany(x => x.SanPhams)
                   .WithOne(x => x.DanhMucSanPham)
                   .HasForeignKey(x => x.DanhMucId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
