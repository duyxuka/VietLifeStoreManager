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
    public class CamNangCommentConfiguration : IEntityTypeConfiguration<CamNangComment>
    {
        public void Configure(EntityTypeBuilder<CamNangComment> builder)
        {
            builder.ToTable(VietlifeStoreConsts.DbTablePrefix + "CamNangComment");

            builder.HasKey(x => x.Id);

            builder.HasOne(x => x.CamNang)
                   .WithMany(x => x.CamNangComments)
                   .HasForeignKey(x => x.CamNangId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Parent)
                .WithMany(x => x.Replies)
                .HasForeignKey(x => x.ParentId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
