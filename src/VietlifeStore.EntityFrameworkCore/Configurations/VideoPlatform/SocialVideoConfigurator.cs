using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VietlifeStore.Entity.LienHes;
using VietlifeStore.Entity.VideoPlatform;

namespace VietlifeStore.Configurations.VideoPlatform
{
    public class SocialVideoConfigurator : IEntityTypeConfiguration<SocialVideo>
    {
        public void Configure(EntityTypeBuilder<SocialVideo> builder)
        {
            builder.ToTable(VietlifeStoreConsts.DbTablePrefix + "SocialVideo");
            builder.HasKey(x => x.Id);
        }
    }
}
