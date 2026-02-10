using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VietlifeStore.Entity.SanPhams;

namespace VietlifeStore.Configurations.SanPhams
{
    public class SanPhamBienTheThuocTinhConfiguration : IEntityTypeConfiguration<SanPhamBienTheThuocTinh>
    {
        public void Configure(EntityTypeBuilder<SanPhamBienTheThuocTinh> builder)
        {
            builder.ToTable(VietlifeStoreConsts.DbTablePrefix + "SanPhamBienTheThuocTinh");
            builder.HasKey(x => x.Id);

            builder.HasOne(x => x.SanPhamBienThe)
                   .WithMany(x => x.ThuocTinhs)
                   .HasForeignKey(x => x.SanPhamBienTheId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.GiaTriThuocTinh)
                   .WithMany()
                   .HasForeignKey(x => x.GiaTriThuocTinhId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new
            {
                x.SanPhamBienTheId,
                x.GiaTriThuocTinhId
            }).IsUnique();
        }
    }
}
