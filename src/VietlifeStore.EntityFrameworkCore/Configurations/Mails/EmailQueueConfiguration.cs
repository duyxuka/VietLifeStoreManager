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
    public class EmailQueueConfiguration : IEntityTypeConfiguration<EmailQueue>
    {
        public void Configure(EntityTypeBuilder<EmailQueue> builder)
        {
            builder.ToTable(VietlifeStoreConsts.DbTablePrefix + "EmailQueue");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Email)
                   .IsRequired()
                   .HasMaxLength(256);

            builder.Property(x => x.TieuDe)
                   .IsRequired()
                   .HasMaxLength(300);

            builder.Property(x => x.NoiDung)
                   .IsRequired();

            builder.Property(x => x.SoLanThu)
                   .HasDefaultValue(0);

            builder.HasOne(x => x.Campaign)
                   .WithMany(x => x.Queues)
                   .HasForeignKey(x => x.CampaignId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Log)
                   .WithOne(x => x.Queue)
                   .HasForeignKey<EmailLog>(x => x.QueueId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
