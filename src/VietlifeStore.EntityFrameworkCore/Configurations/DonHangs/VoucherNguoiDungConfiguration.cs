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
    public class VoucherNguoiDungConfiguration : IEntityTypeConfiguration<VoucherNguoiDung>
    {
        public void Configure(EntityTypeBuilder<VoucherNguoiDung> builder)
        {
            builder.ToTable(VietlifeStoreConsts.DbTablePrefix + "VoucherNguoiDung");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.VoucherId)
                   .IsRequired();

            builder.Property(x => x.UserId)
                   .IsRequired();

            builder.Property(x => x.SoLuongNhan)
                   .IsRequired()
                   .HasDefaultValue(1);

            builder.Property(x => x.DaDung)
                   .IsRequired()
                   .HasDefaultValue(0);

            builder.Property(x => x.NgayNhan)
                   .IsRequired();

            builder.Property(x => x.DaHetHan)
                   .IsRequired()
                   .HasDefaultValue(false);

            // Mỗi user chỉ nhận 1 bản ghi / voucher
            builder.HasIndex(x => new { x.VoucherId, x.UserId })
                   .IsUnique();
        }
    }
}
