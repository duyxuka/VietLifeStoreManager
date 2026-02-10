using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VietlifeStore.Entity.ChinhSachs;

namespace VietlifeStore.Configurations.ChinhSachs
{
    public class DanhMucChinhSachConfiguration : IEntityTypeConfiguration<DanhMucChinhSach>
    {
        public void Configure(EntityTypeBuilder<DanhMucChinhSach> builder)
        {
            builder.ToTable(VietlifeStoreConsts.DbTablePrefix + "DanhMucChinhSach");
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

            builder.HasMany(x => x.ChinhSachs)
                   .WithOne(x => x.DanhMucChinhSach)
                   .HasForeignKey(x => x.DanhMucChinhSachId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
