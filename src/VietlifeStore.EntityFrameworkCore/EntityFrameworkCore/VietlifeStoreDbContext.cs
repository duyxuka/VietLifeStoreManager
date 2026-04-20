using Microsoft.EntityFrameworkCore;
using Volo.Abp.AuditLogging.EntityFrameworkCore;
using Volo.Abp.BackgroundJobs.EntityFrameworkCore;
using Volo.Abp.BlobStoring.Database.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.Modeling;
using Volo.Abp.FeatureManagement.EntityFrameworkCore;
using Volo.Abp.Identity;
using Volo.Abp.Identity.EntityFrameworkCore;
using Volo.Abp.PermissionManagement.EntityFrameworkCore;
using Volo.Abp.SettingManagement.EntityFrameworkCore;
using Volo.Abp.OpenIddict.EntityFrameworkCore;
using Volo.Abp.TenantManagement;
using Volo.Abp.TenantManagement.EntityFrameworkCore;
using VietlifeStore.Entity.SanPhams;
using VietlifeStore.Entity.Banners;
using VietlifeStore.Entity.CamNangs;
using VietlifeStore.Entity.ChinhSachs;
using VietlifeStore.Entity.DonHangs;
using VietlifeStore.Entity.LienHes;
using VietlifeStore.Configurations.Banners;
using VietlifeStore.Configurations.CamNangs;
using VietlifeStore.Configurations.LienHes;
using VietlifeStore.Configurations.ChinhSachs;
using VietlifeStore.Configurations.DonHangs;
using VietlifeStore.Configurations.SanPhams;
using VietlifeStore.Configurations.TaiKhoans;
using VietlifeStore.IdentitySettings;
using VietlifeStore.Configurations.IdentitySettings;
using VietlifeStore.Entity.TaiKhoans;
using VietlifeStore.Entity.VideoPlatform;
using VietlifeStore.Configurations.VideoPlatform;
using VietlifeStore.Entity.Payments;
using VietlifeStore.Configurations.Payments;
using VietlifeStore.ChucNang.DatLichs.DatLichVouchers;
using VietlifeStore.ChucNang.DatLichs.DatLichGiamGiaSanPhams;
using VietlifeStore.Configurations.DatLichGiamGias;
using System.Reflection.Emit;
using VietlifeStore.Configurations.Mails;
using VietlifeStore.ChucNang.DatLichs.Emails;
using VietlifeStore.ChucNang.ChatAIs;
using VietlifeStore.Configurations.ChatAIs;
using VietlifeStore.Entity.SEOs;
using VietlifeStore.Configurations.SEOs;

namespace VietlifeStore.EntityFrameworkCore;

[ReplaceDbContext(typeof(IIdentityDbContext))]
[ReplaceDbContext(typeof(ITenantManagementDbContext))]
[ConnectionStringName("Default")]
public class VietlifeStoreDbContext :
    AbpDbContext<VietlifeStoreDbContext>,
    ITenantManagementDbContext,
    IIdentityDbContext
{
    /* Add DbSet properties for your Aggregate Roots / Entities here. */


    #region Entities from the modules

    /* Notice: We only implemented IIdentityProDbContext and ISaasDbContext
     * and replaced them for this DbContext. This allows you to perform JOIN
     * queries for the entities of these modules over the repositories easily. You
     * typically don't need that for other modules. But, if you need, you can
     * implement the DbContext interface of the needed module and use ReplaceDbContext
     * attribute just like IIdentityProDbContext and ISaasDbContext.
     *
     * More info: Replacing a DbContext of a module ensures that the related module
     * uses this DbContext on runtime. Otherwise, it will use its own DbContext class.
     */

    // Identity
    public DbSet<IdentityUser> Users { get; set; }
    public DbSet<IdentityRole> Roles { get; set; }
    public DbSet<IdentityClaimType> ClaimTypes { get; set; }
    public DbSet<OrganizationUnit> OrganizationUnits { get; set; }
    public DbSet<IdentitySecurityLog> SecurityLogs { get; set; }
    public DbSet<IdentityLinkUser> LinkUsers { get; set; }
    public DbSet<IdentityUserDelegation> UserDelegations { get; set; }
    public DbSet<IdentitySession> Sessions { get; set; }

    // Tenant Management
    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<TenantConnectionString> TenantConnectionStrings { get; set; }

    //VietlifeStore modules
    public DbSet<IdentitySetting> IdentitySettings { get; set; }
    public DbSet<DanhMucSanPham> DanhMucSanPhams { get; set; }
    public DbSet<SanPham> SanPhams { get; set; }
    public DbSet<AnhSanPham> AnhSanPhams { get; set; }
    public DbSet<SanPhamBienThe> SanPhamBienThes { get; set; }
    public DbSet<GiaTriThuocTinh> GiaTriThuocTinhs { get; set; }
    public DbSet<ThuocTinh> ThuocTinhs { get; set; }
    public DbSet<QuaTang> QuaTangs { get; set; }
    public DbSet<SanPhamBienTheThuocTinh> SanPhamBienTheThuocTinhs { get; set; }
    public DbSet<Banner> Banners { get; set; }
    public DbSet<CamNang> CamNangs { get; set; }
    public DbSet<DanhMucCamNang> DanhMucCamNangs { get; set; }
    public DbSet<DanhMucChinhSach> DanhMucChinhSachs { get; set; }
    public DbSet<ChinhSach> ChinhSachs { get; set; }
    public DbSet<DonHang> DonHangs { get; set; }
    public DbSet<ChiTietDonHang> ChiTietDonHangs { get; set; }
    public DbSet<Voucher> Vouchers { get; set; }
    public DbSet<LienHe> LienHes { get; set; }
    public DbSet<TaiKhoan> TaiKhoans { get; set; }
    public DbSet<SocialVideo> SocialVideos { get; set; }
    public DbSet<PaymentInformationModel> PaymentInformationModels { get; set; }
    public DbSet<VoucherDaSuDung> VoucherDaSuDungs { get; set; }
    public DbSet<CamNangComment> CamNangComments { get; set; }
    public DbSet<SanPhamReview> SanPhamReviews { get; set; }
    public DbSet<ChuongTrinhGiamGia> ChuongTrinhGiamGias { get; set; }
    public DbSet<ChuongTrinhGiamGiaItem> ChuongTrinhGiamGiaItems { get; set; }

    public DbSet<VoucherDoiTuong> VoucherDoiTuongs { get; set; }
    public DbSet<VoucherNguoiDung> VoucherNguoiDungs { get; set; }
    public DbSet<VoucherSchedule> VoucherSchedules { get; set; }

    //Mail
    public DbSet<EmailCampaign> EmailCampaigns { get; set; }
    public DbSet<EmailTemplate> EmailTemplates { get; set; }
    public DbSet<EmailQueue> EmailQueues { get; set; }
    public DbSet<EmailLog> EmailLogs { get; set; }
    public DbSet<EmailOpenTracking> EmailOpenTrackings { get; set; }
    public DbSet<EmailUnsubscribe> EmailUnsubscribes { get; set; }

    //ChatAI
    public DbSet<Conversation> Conversations { get; set; }
    public DbSet<ChatMessage> ChatMessages { get; set; }

    //SEO
    public DbSet<SeoConfig> SeoConfigs { get; set; }

    //Tin t?c
    public DbSet<TinTuc> TinTucs { get; set; }


    #endregion

    public VietlifeStoreDbContext(DbContextOptions<VietlifeStoreDbContext> options)
        : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        /* Include modules to your migration db context */

        builder.ConfigurePermissionManagement();
        builder.ConfigureSettingManagement();
        builder.ConfigureBackgroundJobs();
        builder.ConfigureAuditLogging();
        builder.ConfigureFeatureManagement();
        builder.ConfigureIdentity();
        builder.ConfigureOpenIddict();
        builder.ConfigureTenantManagement();
        builder.ConfigureBlobStoring();

        builder.ApplyConfiguration(new IdentitySettingConfiguration());
        builder.ApplyConfiguration(new BannerConfiguration());
        builder.ApplyConfiguration(new DanhMucCamNangConfiguration());
        builder.ApplyConfiguration(new CamNangConfiguration());
        builder.ApplyConfiguration(new LienHeConfiguration());
        builder.ApplyConfiguration(new DanhMucChinhSachConfiguration());
        builder.ApplyConfiguration(new ChinhSachConfiguration());
        builder.ApplyConfiguration(new DonHangConfiguration());
        builder.ApplyConfiguration(new ChiTietDonHangConfiguration());
        builder.ApplyConfiguration(new DanhMucSanPhamConfiguration());
        builder.ApplyConfiguration(new SanPhamConfiguration());
        builder.ApplyConfiguration(new AnhSanPhamConfiguration());

        builder.ApplyConfiguration(new ThuocTinhConfiguration());
        builder.ApplyConfiguration(new GiaTriThuocTinhConfiguration());

        builder.ApplyConfiguration(new SanPhamBienTheConfiguration());
        builder.ApplyConfiguration(new SanPhamBienTheThuocTinhConfiguration());

        builder.ApplyConfiguration(new QuaTangConfiguration());
        builder.ApplyConfiguration(new TaiKhoanConfiguration());
        builder.ApplyConfiguration(new SocialVideoConfigurator());
        builder.ApplyConfiguration(new PaymentInformationConfiguration());
        builder.ApplyConfiguration(new CamNangCommentConfiguration());
        builder.ApplyConfiguration(new SanPhamReviewConfiguration());

        builder.ApplyConfiguration(new ChuongTrinhGiamGiaConfiguration());
        builder.ApplyConfiguration(new ChuongTrinhGiamGiaItemConfiguration());

        builder.ApplyConfiguration(new VoucherConfiguration());
        builder.ApplyConfiguration(new VoucherDaSuDungConfiguration());
        builder.ApplyConfiguration(new VoucherNguoiDungConfiguration());
        builder.ApplyConfiguration(new VoucherDoiTuongConfiguration());
        builder.ApplyConfiguration(new VoucherScheduleConfiguration());

        //mail
        builder.ApplyConfiguration(new EmailCampaignConfiguration());
        builder.ApplyConfiguration(new EmailTemplateConfiguration());
        builder.ApplyConfiguration(new EmailQueueConfiguration());
        builder.ApplyConfiguration(new EmailLogConfiguration());
        builder.ApplyConfiguration(new EmailOpenTrackingConfiguration());
        builder.ApplyConfiguration(new EmailUnsubscribeConfiguration());

        //ChatAI
        builder.ApplyConfiguration(new ConversationConfiguration());
        builder.ApplyConfiguration(new ChatMessageConfiguration());

        //SEO
        builder.ApplyConfiguration(new SEOConfiguration());

        //Tin t?c
        builder.ApplyConfiguration(new TinTucConfiguration());
        /* Configure your own tables/entities inside here */

        //builder.Entity<YourEntity>(b =>
        //{
        //    b.ToTable(VietlifeStoreConsts.DbTablePrefix + "YourEntities", VietlifeStoreConsts.DbSchema);
        //    b.ConfigureByConvention(); //auto configure for the base class props
        //    //...
        //});
    }
}
