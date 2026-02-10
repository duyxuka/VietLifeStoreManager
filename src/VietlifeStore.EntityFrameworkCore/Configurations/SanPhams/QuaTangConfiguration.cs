using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VietlifeStore.Entity.SanPhams;

namespace VietlifeStore.Configurations.SanPhams
{
    public class QuaTangConfiguration : IEntityTypeConfiguration<QuaTang>
    {
        public void Configure(EntityTypeBuilder<QuaTang> builder)
        {
            builder.ToTable(VietlifeStoreConsts.DbTablePrefix + "QuaTang");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Ten)
                   .IsRequired()
                   .HasMaxLength(256);

            builder.Property(x => x.Gia)
                   .HasPrecision(18, 2);

            builder.Property(x => x.TrangThai)
                   .HasDefaultValue(true);
        }
    }
}
