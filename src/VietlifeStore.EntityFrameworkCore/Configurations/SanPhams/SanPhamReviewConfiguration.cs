using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VietlifeStore.Entity.SanPhams;

namespace VietlifeStore.Configurations.SanPhams
{
    public class SanPhamReviewConfiguration : IEntityTypeConfiguration<SanPhamReview>
    {
        public void Configure(EntityTypeBuilder<SanPhamReview> builder)
        {
            builder.ToTable(VietlifeStoreConsts.DbTablePrefix + "SanPhamReview");
            builder.HasKey(x => x.Id);

            builder.HasIndex(x => x.SanPhamId);
            builder.HasOne(x => x.SanPham)
                .WithMany(x => x.Reviews)
                .HasForeignKey(x => x.SanPhamId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
