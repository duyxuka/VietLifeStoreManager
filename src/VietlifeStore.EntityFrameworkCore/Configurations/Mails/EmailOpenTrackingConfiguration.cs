using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VietlifeStore.ChucNang.DatLichs.Emails;

namespace VietlifeStore.Configurations.Mails
{
    public class EmailOpenTrackingConfiguration : IEntityTypeConfiguration<EmailOpenTracking>
    {
        public void Configure(EntityTypeBuilder<EmailOpenTracking> builder)
        {
            builder.ToTable(VietlifeStoreConsts.DbTablePrefix + "EmailOpenTracking");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.DiaChiIP)
                   .HasMaxLength(100);

            builder.Property(x => x.UserAgent)
                   .HasMaxLength(500);

            builder.HasOne(x => x.EmailLog)
                   .WithMany(x => x.Opens)
                   .HasForeignKey(x => x.EmailLogId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
