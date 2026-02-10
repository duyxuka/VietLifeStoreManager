using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VietlifeStore.Entity.ChinhSachs;

namespace VietlifeStore.Configurations.ChinhSachs
{
    public class ChinhSachConfiguration : IEntityTypeConfiguration<ChinhSach>
    {
        public void Configure(EntityTypeBuilder<ChinhSach> builder)
        {
            builder.ToTable(VietlifeStoreConsts.DbTablePrefix + "ChinhSach");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.TieuDe)
                   .IsRequired()
                   .HasMaxLength(256);

            builder.Property(x => x.TrangThai)
                   .HasDefaultValue(true);

            builder.HasOne(x => x.DanhMucChinhSach)
                   .WithMany(x => x.ChinhSachs)
                   .HasForeignKey(x => x.DanhMucChinhSachId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
