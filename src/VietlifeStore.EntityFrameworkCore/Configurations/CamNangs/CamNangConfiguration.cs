using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VietlifeStore.Entity.CamNangs;

namespace VietlifeStore.Configurations.CamNangs
{
    public class CamNangConfiguration : IEntityTypeConfiguration<CamNang>
    {
        public void Configure(EntityTypeBuilder<CamNang> builder)
        {
            builder.ToTable(VietlifeStoreConsts.DbTablePrefix + "CamNang");
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

            builder.HasOne(x => x.DanhMucCamNang)
                   .WithMany(x => x.CamNangs)
                   .HasForeignKey(x => x.DanhMucCamNangId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
