using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VietlifeStore.ChucNang.DatLichs.DatLichVouchers;
using VietlifeStore.Entity.DonHangs;

namespace VietlifeStore.Configurations.DonHangs
{
    public class VoucherScheduleConfiguration : IEntityTypeConfiguration<VoucherSchedule>
    {
        public void Configure(EntityTypeBuilder<VoucherSchedule> builder)
        {
            builder.ToTable(VietlifeStoreConsts.DbTablePrefix + "VoucherSchedule");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.VoucherId)
                   .IsRequired();

            builder.Property(x => x.HangfireJobId)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(x => x.LoaiJob)
                   .IsRequired()
                   .HasConversion<int>();

            builder.Property(x => x.ThoiGianDuKien)
                   .IsRequired();

            builder.Property(x => x.TrangThai)
                   .IsRequired()
                   .HasConversion<int>()
                   .HasDefaultValue(TrangThaiJob.ChoXuLy);

            builder.Property(x => x.GhiChu)
                   .HasMaxLength(1000); // chứa stack trace nếu lỗi

            // Index để VoucherJobHandler tìm schedule cần cập nhật nhanh
            builder.HasIndex(x => new { x.VoucherId, x.LoaiJob, x.TrangThai });

            builder.HasIndex(x => x.HangfireJobId);
        }
    }
}
