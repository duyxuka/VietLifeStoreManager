using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VietlifeStore.Entity.Banners;

namespace VietlifeStore.Configurations.Banners
{
    public class BannerConfiguration : IEntityTypeConfiguration<Banner>
    {
        public void Configure(EntityTypeBuilder<Banner> builder)
        {
            builder.ToTable(VietlifeStoreConsts.DbTablePrefix + "Banner");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.TrangThai)
                   .HasDefaultValue(true);

            builder.Property(x => x.TieuDe)
                   .IsRequired()
                   .HasMaxLength(256);
        }
    }
}
