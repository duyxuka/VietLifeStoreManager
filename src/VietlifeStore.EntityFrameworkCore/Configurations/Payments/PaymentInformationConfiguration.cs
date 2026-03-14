using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VietlifeStore.Entity.Payments;

namespace VietlifeStore.Configurations.Payments
{
    public class PaymentInformationConfiguration : IEntityTypeConfiguration<PaymentInformationModel>
    {
        public void Configure(EntityTypeBuilder<PaymentInformationModel> builder)
        {
            builder.ToTable(VietlifeStoreConsts.DbTablePrefix + "PaymentInformationModel");
            builder.HasKey(x => x.Id);
        }
    }
}
