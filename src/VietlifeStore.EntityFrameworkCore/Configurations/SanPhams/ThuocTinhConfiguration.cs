using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VietlifeStore.Entity.SanPhams;

namespace VietlifeStore.Configurations.SanPhams
{
    public class ThuocTinhConfiguration : IEntityTypeConfiguration<ThuocTinh>
    {
        public void Configure(EntityTypeBuilder<ThuocTinh> builder)
        {
            builder.ToTable(VietlifeStoreConsts.DbTablePrefix + "ThuocTinh");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Ten)
                   .IsRequired()
                   .HasMaxLength(100);
        }
    }
}
