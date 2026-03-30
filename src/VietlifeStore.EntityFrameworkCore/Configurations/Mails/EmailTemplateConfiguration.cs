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
    public class EmailTemplateConfiguration : IEntityTypeConfiguration<EmailTemplate>
    {
        public void Configure(EntityTypeBuilder<EmailTemplate> builder)
        {
            builder.ToTable(VietlifeStoreConsts.DbTablePrefix + "EmailTemplate");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.TenTemplate)
                   .IsRequired()
                   .HasMaxLength(200);

            builder.Property(x => x.TieuDe)
                   .IsRequired()
                   .HasMaxLength(300);

            builder.Property(x => x.NoiDungHtml)
                   .IsRequired();

            builder.Property(x => x.MoTa)
                   .HasMaxLength(500);

            builder.Property(x => x.TrangThai)
                   .HasDefaultValue(true);

            builder.HasMany(x => x.Campaigns)
                   .WithOne(x => x.Template)
                   .HasForeignKey(x => x.TemplateId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
