using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VietlifeStore.Entity.CamNangs;

namespace VietlifeStore.Configurations.CamNangs
{
    public class TinTucConfiguration : IEntityTypeConfiguration<TinTuc>
    {
        public void Configure(EntityTypeBuilder<TinTuc> builder)
        {
            builder.ToTable(VietlifeStoreConsts.DbTablePrefix + "TinTuc");
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
        }
    }
}
