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

            builder.Property(x => x.VoucherId)
                   .IsRequired();

            builder.Property(x => x.UserId)
                   .IsRequired();

            builder.Property(x => x.DonHangId)
                   .IsRequired();

            builder.Property(x => x.GiaTriGiam)
                   .IsRequired()
                   .HasPrecision(18, 2);

            builder.Property(x => x.NgaySuDung)
                   .IsRequired();

            // Index để query nhanh: user đã dùng voucher nào
            builder.HasIndex(x => new { x.VoucherId, x.UserId });

            // Index để tra cứu theo đơn hàng
            builder.HasIndex(x => x.DonHangId);
        }
    }
}
