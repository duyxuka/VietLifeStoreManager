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
    public class VoucherDoiTuongConfiguration : IEntityTypeConfiguration<VoucherDoiTuong>
    {
        public void Configure(EntityTypeBuilder<VoucherDoiTuong> builder)
        {
            builder.ToTable(VietlifeStoreConsts.DbTablePrefix + "VoucherDoiTuong");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.VoucherId)
                   .IsRequired();

            builder.Property(x => x.LoaiDoiTuong)
                   .IsRequired()
                   .HasConversion<int>();

            builder.Property(x => x.DoiTuongId)
                   .IsRequired();

            // Không cho trùng cùng 1 đối tượng trên cùng 1 voucher
            builder.HasIndex(x => new { x.VoucherId, x.LoaiDoiTuong, x.DoiTuongId })
                   .IsUnique();
        }
    }
}
