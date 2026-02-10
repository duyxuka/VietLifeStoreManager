using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VietlifeStore.Entity.SanPhams;

namespace VietlifeStore.Configurations.SanPhams
{
    public class GiaTriThuocTinhConfiguration : IEntityTypeConfiguration<GiaTriThuocTinh>
    {
        public void Configure(EntityTypeBuilder<GiaTriThuocTinh> builder)
        {
            builder.ToTable(VietlifeStoreConsts.DbTablePrefix + "GiaTriThuocTinh");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.GiaTri)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.HasOne(x => x.ThuocTinh)
                   .WithMany()
                   .HasForeignKey(x => x.ThuocTinhId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
