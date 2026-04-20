using AutoMapper;
using System.Linq;
using VietlifeStore.ChucNang.DatLichs.DatLichGiamGiaSanPhams;
using VietlifeStore.ChucNang.DatLichs.DatLichGiamGiaSanPhams.ChuongTrinhGiamGiaItems;
using VietlifeStore.ChucNang.DatLichs.DatLichGiamGiaSanPhams.ChuongTrinhGiamGias;
using VietlifeStore.ChucNang.DatLichs.DatLichVouchers;
using VietlifeStore.ChucNang.DatLichs.Emails;
using VietlifeStore.ChucNang.DatLichs.Emails.EmailCampaigns;
using VietlifeStore.ChucNang.DatLichs.Emails.EmailQueues;
using VietlifeStore.ChucNang.DatLichs.Emails.EmailTemplates;
using VietlifeStore.Entity.Banners;
using VietlifeStore.Entity.CamNangs;
using VietlifeStore.Entity.CamNangsList.CamNangComments;
using VietlifeStore.Entity.CamNangsList.CamNangs;
using VietlifeStore.Entity.CamNangsList.DanhMucCamNangs;
using VietlifeStore.Entity.CamNangsList.TinTucs;
using VietlifeStore.Entity.ChinhSachs;
using VietlifeStore.Entity.ChinhSachsList.ChinhSachs;
using VietlifeStore.Entity.ChinhSachsList.DanhMucChinhSachs;
using VietlifeStore.Entity.DonHangs;
using VietlifeStore.Entity.DonHangsList.ChiTietDonHangs;
using VietlifeStore.Entity.DonHangsList.DonHangs;
using VietlifeStore.Entity.DonHangsList.Vouchers;
using VietlifeStore.Entity.LienHes;
using VietlifeStore.Entity.Payments;
using VietlifeStore.Entity.SanPhams;
using VietlifeStore.Entity.SanPhamsList.AnhSanPhams;
using VietlifeStore.Entity.SanPhamsList.DanhMucSanPhams;
using VietlifeStore.Entity.SanPhamsList.GiaTriThuocTinhs;
using VietlifeStore.Entity.SanPhamsList.QuaTangs;
using VietlifeStore.Entity.SanPhamsList.SanPhamBienThes;
using VietlifeStore.Entity.SanPhamsList.SanPhamBienTheThuocTinhs;
using VietlifeStore.Entity.SanPhamsList.SanPhamReviews;
using VietlifeStore.Entity.SanPhamsList.SanPhams;
using VietlifeStore.Entity.SanPhamsList.ThuocTinhs;
using VietlifeStore.Entity.SEOs;
using VietlifeStore.Entity.TaiKhoans;
using VietlifeStore.Entity.Videoplatform;
using VietlifeStore.Entity.VideoPlatform;
using VietlifeStore.Roles;
using VietlifeStore.System.Roles;
using VietlifeStore.System.Users;
using Volo.Abp.Identity;

namespace VietlifeStore;

public class VietlifeStoreApplicationAutoMapperProfile : Profile
{
    public VietlifeStoreApplicationAutoMapperProfile()
    {
        /* You can configure your AutoMapper mapping configuration here.
         * Alternatively, you can split your mapping configurations
         * into multiple profile classes for a better organization. */

        //Role
        CreateMap<IdentityRole, RoleDto>().ForMember(x => x.Description,
             map => map.MapFrom(x => x.ExtraProperties.ContainsKey(RoleConsts.DescriptionFieldName)
             ?
             x.ExtraProperties[RoleConsts.DescriptionFieldName]
             :
             null));
        CreateMap<IdentityRole, RoleInListDto>().ForMember(x => x.Description,
            map => map.MapFrom(x => x.ExtraProperties.ContainsKey(RoleConsts.DescriptionFieldName)
            ?
            x.ExtraProperties[RoleConsts.DescriptionFieldName]
            :
            null));
        CreateMap<CreateUpdateRoleDto, IdentityRole>();

        //User
        CreateMap<TaiKhoan, UserDto>();
        CreateMap<TaiKhoan, UserInListDto>();

        //Banner
        CreateMap<CreateUpdateBannerDto, Banner>();
        CreateMap<Banner, BannerDto>();
        CreateMap<Banner, BannerInListDto>();

        //DanhMucCamNang
        CreateMap<CreateUpdateDanhMucCamNangDto, DanhMucCamNang>();
        CreateMap<DanhMucCamNang, DanhMucCamNangDto>();
        CreateMap<DanhMucCamNang, DanhMucCamNangInListDto>();

        //CamNang
        CreateMap<CreateUpdateCamNangDto, CamNang>();
        CreateMap<CamNang, CamNangDto>();
        CreateMap<CamNang, CamNangInListDto>().ForMember(dest => dest.CreationTime, opt => opt.MapFrom(src => src.CreationTime));

        //DanhMucChinhSach
        CreateMap<CreateUpdateDanhMucChinhSachDto, DanhMucChinhSach>();
        CreateMap<DanhMucChinhSach, DanhMucChinhSachDto>();
        CreateMap<DanhMucChinhSach, DanhMucChinhSachInListDto>();

        //ChinhSach
        CreateMap<CreateUpdateChinhSachDto, ChinhSach>();
        CreateMap<ChinhSach, ChinhSachDto>();
        CreateMap<ChinhSach, ChinhSachInListDto>();

        //DonHang
        CreateMap<CreateUpdateDonHangDto, DonHang>().ForMember(x => x.ChiTietDonHangs, opt => opt.Ignore());
        CreateMap<DonHang, DonHangDto>().ForMember(
                dest => dest.ChiTietDonHangDtos,
                opt => opt.MapFrom(src => src.ChiTietDonHangs)
            );
        ;
        CreateMap<DonHang, DonHangInListDto>();

        //ChiTietDonHang
        CreateMap<CreateUpdateChiTietDonHangDto, ChiTietDonHang>();
        CreateMap<ChiTietDonHang, ChiTietDonHangDto>()
        .ForMember(dest => dest.TenSanPham,
            opt => opt.MapFrom(src => src.SanPham.Ten));
        CreateMap<ChiTietDonHang, ChiTietDonHangInListDto>();

        //Voucher
        CreateMap<CreateUpdateVoucherDto, Voucher>();
        CreateMap<Voucher, VoucherDto>()
            .ForMember(dest => dest.SanPhamIds, opt => opt.MapFrom(src =>
                src.DoiTuongApDung
                    .Where(x => x.LoaiDoiTuong == LoaiDoiTuong.SanPham)
                    .Select(x => x.DoiTuongId).ToList()))
            .ForMember(dest => dest.DanhMucIds, opt => opt.MapFrom(src =>
                src.DoiTuongApDung
                    .Where(x => x.LoaiDoiTuong == LoaiDoiTuong.DanhMuc)
                    .Select(x => x.DoiTuongId).ToList()));
        CreateMap<Voucher, VoucherInListDto>();

        //LienHe
        CreateMap<CreateUpdateLienHeDto, LienHe>();
        CreateMap<LienHe, LienHeDto>();
        CreateMap<LienHe, LienHeInListDto>();

        //SanPham
        CreateMap<CreateUpdateSanPhamDto, SanPham>();
        CreateMap<SanPham, SanPhamDto>();
        CreateMap<SanPham, SanPhamInListDto>();

        //AnhSanPham
        CreateMap<CreateUpdateAnhSanPhamDto, AnhSanPham>();
        CreateMap<AnhSanPham, AnhSanPhamDto>();
        CreateMap<AnhSanPham, AnhSanPhamInListDto>();

        //DanhMucSanPham
        CreateMap<CreateUpdateDanhMucSanPhamDto, DanhMucSanPham>();
        CreateMap<DanhMucSanPham, DanhMucSanPhamDto>();
        CreateMap<DanhMucSanPham, DanhMucSanPhamInListDto>();

        //GiaTriThuocTinh
        CreateMap<CreateUpdateGiaTriThuocTinhDto, GiaTriThuocTinh>();
        CreateMap<GiaTriThuocTinh, GiaTriThuocTinhDto>();
        CreateMap<GiaTriThuocTinh, GiaTriThuocTinhInListDto>();

        //QuaTang
        CreateMap<CreateUpdateQuaTangDto, QuaTang>();
        CreateMap<QuaTang, QuaTangDto>();
        CreateMap<QuaTang, QuaTangInListDto>();

        //SanPhamBienThe
        CreateMap<CreateUpdateSanPhamBienTheDto, SanPhamBienThe>();
        CreateMap<SanPhamBienThe, SanPhamBienTheDto>();
        CreateMap<SanPhamBienThe, SanPhamBienTheInListDto>();

        //SanPhamBienTheThuocTinh
        CreateMap<CreateUpdateSanPhamBienTheThuocTinhDto, SanPhamBienTheThuocTinh>();
        CreateMap<SanPhamBienTheThuocTinh, SanPhamBienTheThuocTinhDto>();
        CreateMap<SanPhamBienTheThuocTinh, SanPhamBienTheThuocTinhInListDto>();

        //ThuocTinh
        CreateMap<CreateUpdateThuocTinhDto, ThuocTinh>();
        CreateMap<ThuocTinh, ThuocTinhDto>();
        CreateMap<ThuocTinh, ThuocTinhInListDto>();

        //SocialVideo
        CreateMap<CreateUpdateSocialVideoDto, SocialVideo>();
        CreateMap<SocialVideo, SocialVideoDto>();
        CreateMap<SocialVideo, SocialVideoInListDto>();

        CreateMap<PaymentInformationModel, PaymentInformationModelDto>();
        CreateMap<CreateUpdatePaymentInformationModelDto, PaymentInformationModel>();
        CreateMap<PaymentInformationModel, PaymentInformationModelInListDto>();

        CreateMap<CamNangComment, CamNangCommentDto>();
        CreateMap<CreateUpdateCamNangCommentDto, CamNangComment>();
        CreateMap<CamNangComment, CamNangCommentInListDto>();

        CreateMap<SanPhamReview, SanPhamReviewDto>();
        CreateMap<CreateUpdateSanPhamReviewDto, SanPhamReview>();
        CreateMap<SanPhamReview, SanPhamReviewInListDto>();

        CreateMap<ChuongTrinhGiamGia, ChuongTrinhDto>();
        CreateMap<CreateUpdateChuongTrinhDto, ChuongTrinhGiamGia>()
            .ForMember(dest => dest.Items, opt => opt.Ignore());
        CreateMap<ChuongTrinhGiamGia, ChuongTrinhInListDto>()
            .ForMember(dest => dest.SoLuongSanPham,
                opt => opt.MapFrom(src => src.Items.Count));

        CreateMap<ChuongTrinhGiamGiaItem, ChuongTrinhItemDto>();
        CreateMap<CreateUpdateChuongTrinhItemDto, ChuongTrinhGiamGiaItem>();

        //Mail
        // EmailTemplate
        CreateMap<EmailTemplate, EmailTemplateDto>();
        CreateMap<CreateUpdateEmailTemplateDto, EmailTemplate>();
        CreateMap<EmailTemplate, EmailTemplateInListDto>();

        // EmailCampaign
        CreateMap<EmailCampaign, EmailCampaignDto>();
        CreateMap<CreateUpdateEmailCampaignDto, EmailCampaign>();
        CreateMap<EmailCampaign, EmailCampaignInListDto>();

        // EmailQueue
        CreateMap<EmailQueue, EmailQueueDto>();

        //SEO
        CreateMap<SeoConfig, SeoConfigDto>();
        CreateMap<CreateUpdateSeoConfigDto, SeoConfig>();
        CreateMap<SeoConfig, SeoConfigInListDto>();

        //TinTuc
        CreateMap<CreateUpdateTinTucDto, TinTuc>();
        CreateMap<TinTuc, TinTucDto>();
        CreateMap<TinTuc, TinTucInListDto>().ForMember(dest => dest.CreationTime, opt => opt.MapFrom(src => src.CreationTime));
    }
}
