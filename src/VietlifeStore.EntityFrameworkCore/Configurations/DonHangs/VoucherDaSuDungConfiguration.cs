using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VietlifeStore.Entity.DonHangs;

namespace VietlifeStore.Configurations.DonHangs
{
    public class VoucherDaSuDungConfiguration : IEntityTypeConfiguration<VoucherDaSuDung>
    {
        public void Configure(EntityTypeBuilder<VoucherDaSuDung> builder)
        {
            builder.ToTable(VietlifeStoreConsts.DbTablePrefix + "VoucherDaSuDung");
            builder.HasKey(x => x.Id);
        }
    }
}
