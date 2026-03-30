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
    public class EmailUnsubscribeConfiguration : IEntityTypeConfiguration<EmailUnsubscribe>
    {
        public void Configure(EntityTypeBuilder<EmailUnsubscribe> builder)
        {
            builder.ToTable(VietlifeStoreConsts.DbTablePrefix + "EmailUnsubscribe");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Email)
                   .IsRequired()
                   .HasMaxLength(256);

            builder.Property(x => x.LyDo)
                   .HasMaxLength(500);

            builder.Property(x => x.DiaChiIP)
                   .HasMaxLength(100);
        }
    }
}
