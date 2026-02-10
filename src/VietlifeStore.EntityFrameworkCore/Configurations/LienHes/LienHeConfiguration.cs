using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VietlifeStore.Entity.LienHes;

namespace VietlifeStore.Configurations.LienHes
{
    public class LienHeConfiguration : IEntityTypeConfiguration<LienHe>
    {
        public void Configure(EntityTypeBuilder<LienHe> builder)
        {
            builder.ToTable(VietlifeStoreConsts.DbTablePrefix + "LienHe");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.HoTen)
                   .IsRequired()
                   .HasMaxLength(256);

            builder.Property(x => x.Email)
                   .HasMaxLength(256);

            builder.Property(x => x.SoDienThoai)
                   .HasMaxLength(20);

            builder.Property(x => x.NoiDung)
                   .IsRequired()
                   .HasMaxLength(4000);

            builder.Property(x => x.DaXuLy)
                   .HasDefaultValue(false);
        }
    }
}
