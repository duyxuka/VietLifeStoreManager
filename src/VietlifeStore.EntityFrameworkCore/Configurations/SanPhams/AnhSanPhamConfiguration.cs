using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VietlifeStore.Entity.SanPhams;

namespace VietlifeStore.Configurations.SanPhams
{
    public class AnhSanPhamConfiguration : IEntityTypeConfiguration<AnhSanPham>
    {
        public void Configure(EntityTypeBuilder<AnhSanPham> builder)
        {
            builder.ToTable(VietlifeStoreConsts.DbTablePrefix + "AnhSanPham");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Anh)
                   .IsRequired();

            builder.Property(x => x.Status)
                   .HasDefaultValue(true);

            builder.HasOne(x => x.SanPham)
                   .WithMany(x => x.AnhSanPham)
                   .HasForeignKey(x => x.SanPhamId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
