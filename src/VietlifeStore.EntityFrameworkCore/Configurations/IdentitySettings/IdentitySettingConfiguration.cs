using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VietlifeStore.IdentitySettings;

namespace VietlifeStore.Configurations.IdentitySettings
{
    public class IdentitySettingConfiguration : IEntityTypeConfiguration<IdentitySetting>
    {
        public void Configure(EntityTypeBuilder<IdentitySetting> builder)
        {
            builder.ToTable(VietlifeStoreConsts.DbTablePrefix + "IdentitySettings",
                    VietlifeStoreConsts.DbSchema);
            builder.HasKey(x => x.Id);

            builder.Property(e => e.Name).IsRequired().HasMaxLength(200);

        }
    }
}
