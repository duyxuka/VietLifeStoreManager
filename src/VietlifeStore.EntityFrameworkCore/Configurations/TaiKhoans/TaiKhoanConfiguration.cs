using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VietlifeStore.Entity.TaiKhoans;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace VietlifeStore.Configurations.TaiKhoans
{
    public class TaiKhoanConfiguration : IEntityTypeConfiguration<TaiKhoan>
    {
        public void Configure(EntityTypeBuilder<TaiKhoan> builder)
        {
            builder.ToTable("AbpUsers");

            builder.ConfigureByConvention();

            builder.Property(x => x.IsCustomer)
                .HasDefaultValue(true);

            builder.Property(x => x.Status)
                .HasDefaultValue(true);

            builder.HasMany(x => x.DonHangs)
                .WithOne(x => x.TaiKhoanKhachHang)
                .HasForeignKey(x => x.TaiKhoanKhachHangId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
