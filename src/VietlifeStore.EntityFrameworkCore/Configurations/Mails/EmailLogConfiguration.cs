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
    public class EmailLogConfiguration : IEntityTypeConfiguration<EmailLog>
    {
        public void Configure(EntityTypeBuilder<EmailLog> builder)
        {
            builder.ToTable(VietlifeStoreConsts.DbTablePrefix + "EmailLog");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Email)
                   .IsRequired()
                   .HasMaxLength(256);

            builder.Property(x => x.ThongBaoLoi)
                   .HasMaxLength(1000);

            builder.HasOne(x => x.Queue)
                   .WithOne(x => x.Log)
                   .HasForeignKey<EmailLog>(x => x.QueueId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(x => x.Opens)
                   .WithOne(x => x.EmailLog)
                   .HasForeignKey(x => x.EmailLogId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
