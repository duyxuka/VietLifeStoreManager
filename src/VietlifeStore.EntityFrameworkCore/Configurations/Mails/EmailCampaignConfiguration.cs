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
    public class EmailCampaignConfiguration : IEntityTypeConfiguration<EmailCampaign>
    {
        public void Configure(EntityTypeBuilder<EmailCampaign> builder)
        {
            builder.ToTable(VietlifeStoreConsts.DbTablePrefix + "EmailCampaign");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.TenCampaign)
                   .IsRequired()
                   .HasMaxLength(200);

            builder.Property(x => x.Subject)
                   .IsRequired()
                   .HasMaxLength(300);

            builder.Property(x => x.TongSoEmail)
                   .HasDefaultValue(0);

            builder.Property(x => x.SoDaGui)
                   .HasDefaultValue(0);

            builder.Property(x => x.SoMo)
                   .HasDefaultValue(0);

            builder.Property(x => x.SoClick)
                   .HasDefaultValue(0);

            builder.HasOne(x => x.Template)
                   .WithMany(x => x.Campaigns)
                   .HasForeignKey(x => x.TemplateId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.Queues)
                   .WithOne(x => x.Campaign)
                   .HasForeignKey(x => x.CampaignId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
