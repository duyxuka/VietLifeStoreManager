using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VietlifeStore.Entity.CamNangs;

namespace VietlifeStore.Configurations.CamNangs
{
    public class DanhMucCamNangConfiguration : IEntityTypeConfiguration<DanhMucCamNang>
    {
        public void Configure(EntityTypeBuilder<DanhMucCamNang> builder)
        {
            builder.ToTable(VietlifeStoreConsts.DbTablePrefix + "DanhMucCamNang");
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

            builder.HasMany(x => x.CamNangs)
                   .WithOne(x => x.DanhMucCamNang)
                   .HasForeignKey(x => x.DanhMucCamNangId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
