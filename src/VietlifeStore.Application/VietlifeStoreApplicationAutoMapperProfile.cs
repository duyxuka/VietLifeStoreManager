using AutoMapper;
using VietlifeStore.Entity.Banners;
using VietlifeStore.Entity.CamNangs;
using VietlifeStore.Entity.CamNangsList.CamNangs;
using VietlifeStore.Entity.CamNangsList.DanhMucCamNangs;
using VietlifeStore.Entity.ChinhSachs;
using VietlifeStore.Entity.ChinhSachsList.ChinhSachs;
using VietlifeStore.Entity.ChinhSachsList.DanhMucChinhSachs;
using VietlifeStore.Entity.DonHangs;
using VietlifeStore.Entity.DonHangsList.ChiTietDonHangs;
using VietlifeStore.Entity.DonHangsList.DonHangs;
using VietlifeStore.Entity.DonHangsList.Vouchers;
using VietlifeStore.Entity.LienHes;
using VietlifeStore.Entity.SanPhams;
using VietlifeStore.Entity.SanPhamsList.AnhSanPhams;
using VietlifeStore.Entity.SanPhamsList.DanhMucSanPhams;
using VietlifeStore.Entity.SanPhamsList.GiaTriThuocTinhs;
using VietlifeStore.Entity.SanPhamsList.QuaTangs;
using VietlifeStore.Entity.SanPhamsList.SanPhamBienThes;
using VietlifeStore.Entity.SanPhamsList.SanPhamBienTheThuocTinhs;
using VietlifeStore.Entity.SanPhamsList.SanPhams;
using VietlifeStore.Entity.SanPhamsList.ThuocTinhs;
using VietlifeStore.Entity.TaiKhoans;
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
        CreateMap<CamNang, CamNangInListDto>();

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
        CreateMap<ChiTietDonHang, ChiTietDonHangDto>();
        CreateMap<ChiTietDonHang, ChiTietDonHangInListDto>();

        //Voucher
        CreateMap<CreateUpdateVoucherDto, Voucher>();
        CreateMap<Voucher, VoucherDto>();
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
    }
}
