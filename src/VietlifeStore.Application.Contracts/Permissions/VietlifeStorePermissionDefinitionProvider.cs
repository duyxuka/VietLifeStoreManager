using VietlifeStore.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Identity;
using Volo.Abp.Localization;
using Volo.Abp.MultiTenancy;

namespace VietlifeStore.Permissions;

public class VietlifeStorePermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var identityGroup = context.GetGroupOrNull("AbpIdentity")
        ?? context.AddGroup("AbpIdentity", L("Permission:IdentityManagement"));

        var userPermission = identityGroup.GetPermissionOrNull(IdentityPermissions.Users.Default)
            ?? identityGroup.AddPermission(IdentityPermissions.Users.Default, L("Permission:Identity.Users"));

        userPermission.AddChild(ExtendedIdentityPermissions.Users.View, L("Permission:Identity.Users.View"));

        var rolePermission = identityGroup.GetPermissionOrNull(IdentityPermissions.Roles.Default)
            ?? identityGroup.AddPermission(IdentityPermissions.Roles.Default, L("Permission:Identity.Roles"));

        rolePermission.AddChild(ExtendedIdentityPermissions.Roles.View, L("Permission:Identity.Roles.View"));


        var VietLifeStoreGroup = context.AddGroup(VietlifeStorePermissions.VietlifeStoreGroupName, L("Permission:VietlifeStore"));

        //Banner
        var bannerPermission = VietLifeStoreGroup.AddPermission(VietlifeStorePermissions.Banner.Default, L("Permission:Banner"));
        bannerPermission.AddChild(VietlifeStorePermissions.Banner.View, L("Permission:Banner.View"));
        bannerPermission.AddChild(VietlifeStorePermissions.Banner.Create, L("Permission:Banner.Create"));
        bannerPermission.AddChild(VietlifeStorePermissions.Banner.Update, L("Permission:Banner.Update"));
        bannerPermission.AddChild(VietlifeStorePermissions.Banner.Delete, L("Permission:Banner.Delete"));

        //DanhMucCamNang
        var danhMucCamNangPermission = VietLifeStoreGroup.AddPermission(VietlifeStorePermissions.DanhMucCamNang.Default, L("Permission:DanhMucCamNang"));
        danhMucCamNangPermission.AddChild(VietlifeStorePermissions.DanhMucCamNang.View, L("Permission:DanhMucCamNang.View"));
        danhMucCamNangPermission.AddChild(VietlifeStorePermissions.DanhMucCamNang.Create, L("Permission:DanhMucCamNang.Create"));
        danhMucCamNangPermission.AddChild(VietlifeStorePermissions.DanhMucCamNang.Update, L("Permission:DanhMucCamNang.Update"));
        danhMucCamNangPermission.AddChild(VietlifeStorePermissions.DanhMucCamNang.Delete, L("Permission:DanhMucCamNang.Delete"));

        //CamNang
        var camNangPermission = VietLifeStoreGroup.AddPermission(VietlifeStorePermissions.CamNang.Default, L("Permission:CamNang"));
        camNangPermission.AddChild(VietlifeStorePermissions.CamNang.View, L("Permission:CamNang.View"));
        camNangPermission.AddChild(VietlifeStorePermissions.CamNang.Create, L("Permission:CamNang.Create"));
        camNangPermission.AddChild(VietlifeStorePermissions.CamNang.Update, L("Permission:CamNang.Update"));
        camNangPermission.AddChild(VietlifeStorePermissions.CamNang.Delete, L("Permission:CamNang.Delete"));

        //DanhMucChinhSach
        var danhMucChinhSachPermission = VietLifeStoreGroup.AddPermission(VietlifeStorePermissions.DanhMucChinhSach.Default, L("Permission:DanhMucChinhSach"));
        danhMucChinhSachPermission.AddChild(VietlifeStorePermissions.DanhMucChinhSach.View, L("Permission:DanhMucChinhSach.View"));
        danhMucChinhSachPermission.AddChild(VietlifeStorePermissions.DanhMucChinhSach.Create, L("Permission:DanhMucChinhSach.Create"));
        danhMucChinhSachPermission.AddChild(VietlifeStorePermissions.DanhMucChinhSach.Update, L("Permission:DanhMucChinhSach.Update"));
        danhMucChinhSachPermission.AddChild(VietlifeStorePermissions.DanhMucChinhSach.Delete, L("Permission:DanhMucChinhSach.Delete"));

        //ChinhSach
        var chinhSachPermission = VietLifeStoreGroup.AddPermission(VietlifeStorePermissions.ChinhSach.Default, L("Permission:ChinhSach"));
        chinhSachPermission.AddChild(VietlifeStorePermissions.ChinhSach.View, L("Permission:ChinhSach.View"));
        chinhSachPermission.AddChild(VietlifeStorePermissions.ChinhSach.Create, L("Permission:ChinhSach.Create"));
        chinhSachPermission.AddChild(VietlifeStorePermissions.ChinhSach.Update, L("Permission:ChinhSach.Update"));
        chinhSachPermission.AddChild(VietlifeStorePermissions.ChinhSach.Delete, L("Permission:ChinhSach.Delete"));

        //DonHang
        var donHangPermission = VietLifeStoreGroup.AddPermission(VietlifeStorePermissions.DonHang.Default, L("Permission:DonHang"));
        donHangPermission.AddChild(VietlifeStorePermissions.DonHang.View, L("Permission:DonHang.View"));
        donHangPermission.AddChild(VietlifeStorePermissions.DonHang.Create, L("Permission:DonHang.Create"));
        donHangPermission.AddChild(VietlifeStorePermissions.DonHang.Update, L("Permission:DonHang.Update"));
        donHangPermission.AddChild(VietlifeStorePermissions.DonHang.Delete, L("Permission:DonHang.Delete"));

        var chiTietDonHangPermission = VietLifeStoreGroup.AddPermission(VietlifeStorePermissions.ChiTietDonHang.Default, L("Permission:ChiTietDonHang"));
        chiTietDonHangPermission.AddChild(VietlifeStorePermissions.ChiTietDonHang.View, L("Permission:ChiTietDonHang.View"));
        chiTietDonHangPermission.AddChild(VietlifeStorePermissions.ChiTietDonHang.Create, L("Permission:ChiTietDonHang.Create"));
        chiTietDonHangPermission.AddChild(VietlifeStorePermissions.ChiTietDonHang.Update, L("Permission:ChiTietDonHang.Update"));
        chiTietDonHangPermission.AddChild(VietlifeStorePermissions.ChiTietDonHang.Delete, L("Permission:ChiTietDonHang.Delete"));

        //Voucher
        var voucherPermission = VietLifeStoreGroup.AddPermission(VietlifeStorePermissions.Voucher.Default, L("Permission:Voucher"));
        voucherPermission.AddChild(VietlifeStorePermissions.Voucher.View, L("Permission:Voucher.View"));
        voucherPermission.AddChild(VietlifeStorePermissions.Voucher.Create, L("Permission:Voucher.Create"));
        voucherPermission.AddChild(VietlifeStorePermissions.Voucher.Update, L("Permission:Voucher.Update"));
        voucherPermission.AddChild(VietlifeStorePermissions.Voucher.Delete, L("Permission:Voucher.Delete"));

        //LienHe
        var lienHePermission = VietLifeStoreGroup.AddPermission(VietlifeStorePermissions.LienHe.Default, L("Permission:LienHe"));
        lienHePermission.AddChild(VietlifeStorePermissions.LienHe.View, L("Permission:LienHe.View"));
        lienHePermission.AddChild(VietlifeStorePermissions.LienHe.Create, L("Permission:LienHe.Create"));
        lienHePermission.AddChild(VietlifeStorePermissions.LienHe.Update, L("Permission:LienHe.Update"));
        lienHePermission.AddChild(VietlifeStorePermissions.LienHe.Delete, L("Permission:LienHe.Delete"));

        //SanPham
        var sanPhamPermission = VietLifeStoreGroup.AddPermission(VietlifeStorePermissions.SanPham.Default, L("Permission:SanPham"));
        sanPhamPermission.AddChild(VietlifeStorePermissions.SanPham.View, L("Permission:SanPham.View"));
        sanPhamPermission.AddChild(VietlifeStorePermissions.SanPham.Create, L("Permission:SanPham.Create"));
        sanPhamPermission.AddChild(VietlifeStorePermissions.SanPham.Update, L("Permission:SanPham.Update"));
        sanPhamPermission.AddChild(VietlifeStorePermissions.SanPham.Delete, L("Permission:SanPham.Delete"));

        //AnhSanPham
        var anhSanPhamPermission = VietLifeStoreGroup.AddPermission(VietlifeStorePermissions.AnhSanPham.Default, L("Permission:AnhSanPham"));
        anhSanPhamPermission.AddChild(VietlifeStorePermissions.AnhSanPham.View, L("Permission:AnhSanPham.View"));
        anhSanPhamPermission.AddChild(VietlifeStorePermissions.AnhSanPham.Create, L("Permission:AnhSanPham.Create"));
        anhSanPhamPermission.AddChild(VietlifeStorePermissions.AnhSanPham.Update, L("Permission:AnhSanPham.Update"));
        anhSanPhamPermission.AddChild(VietlifeStorePermissions.AnhSanPham.Delete, L("Permission:AnhSanPham.Delete"));

        //DanhMucSanPham
        var danhMucSanPhamPermission = VietLifeStoreGroup.AddPermission(VietlifeStorePermissions.DanhMucSanPham.Default, L("Permission:DanhMucSanPham"));
        danhMucSanPhamPermission.AddChild(VietlifeStorePermissions.DanhMucSanPham.View, L("Permission:DanhMucSanPham.View"));
        danhMucSanPhamPermission.AddChild(VietlifeStorePermissions.DanhMucSanPham.Create, L("Permission:DanhMucSanPham.Create"));
        danhMucSanPhamPermission.AddChild(VietlifeStorePermissions.DanhMucSanPham.Update, L("Permission:DanhMucSanPham.Update"));
        danhMucSanPhamPermission.AddChild(VietlifeStorePermissions.DanhMucSanPham.Delete, L("Permission:DanhMucSanPham.Delete"));

        //GiaTriThuocTinh
        var giaTriThuocTinhPermission = VietLifeStoreGroup.AddPermission(VietlifeStorePermissions.GiaTriThuocTinh.Default, L("Permission:GiaTriThuocTinh"));
        giaTriThuocTinhPermission.AddChild(VietlifeStorePermissions.GiaTriThuocTinh.View, L("Permission:GiaTriThuocTinh.View"));
        giaTriThuocTinhPermission.AddChild(VietlifeStorePermissions.GiaTriThuocTinh.Create, L("Permission:GiaTriThuocTinh.Create"));
        giaTriThuocTinhPermission.AddChild(VietlifeStorePermissions.GiaTriThuocTinh.Update, L("Permission:GiaTriThuocTinh.Update"));
        giaTriThuocTinhPermission.AddChild(VietlifeStorePermissions.GiaTriThuocTinh.Delete, L("Permission:GiaTriThuocTinh.Delete"));

        //QuaTang
        var quaTangPermission = VietLifeStoreGroup.AddPermission(VietlifeStorePermissions.QuaTang.Default, L("Permission:QuaTang"));
        quaTangPermission.AddChild(VietlifeStorePermissions.QuaTang.View, L("Permission:QuaTang.View"));
        quaTangPermission.AddChild(VietlifeStorePermissions.QuaTang.Create, L("Permission:QuaTang.Create"));
        quaTangPermission.AddChild(VietlifeStorePermissions.QuaTang.Update, L("Permission:QuaTang.Update"));
        quaTangPermission.AddChild(VietlifeStorePermissions.QuaTang.Delete, L("Permission:QuaTang.Delete"));

        //SanPhamBienThe
        var sanPhamBienThePermission = VietLifeStoreGroup.AddPermission(VietlifeStorePermissions.SanPhamBienThe.Default, L("Permission:SanPhamBienThe"));
        sanPhamBienThePermission.AddChild(VietlifeStorePermissions.SanPhamBienThe.View, L("Permission:SanPhamBienThe.View"));
        sanPhamBienThePermission.AddChild(VietlifeStorePermissions.SanPhamBienThe.Create, L("Permission:SanPhamBienThe.Create"));
        sanPhamBienThePermission.AddChild(VietlifeStorePermissions.SanPhamBienThe.Update, L("Permission:SanPhamBienThe.Update"));
        sanPhamBienThePermission.AddChild(VietlifeStorePermissions.SanPhamBienThe.Delete, L("Permission:SanPhamBienThe.Delete"));

        //SanPhamBienTheThuocTinh
        var sanPhamBienTheThuocTinhPermission = VietLifeStoreGroup.AddPermission(VietlifeStorePermissions.SanPhamBienTheThuocTinh.Default, L("Permission:SanPhamBienTheThuocTinh"));
        sanPhamBienTheThuocTinhPermission.AddChild(VietlifeStorePermissions.SanPhamBienTheThuocTinh.View, L("Permission:SanPhamBienTheThuocTinh.View"));
        sanPhamBienTheThuocTinhPermission.AddChild(VietlifeStorePermissions.SanPhamBienTheThuocTinh.Create, L("Permission:SanPhamBienTheThuocTinh.Create"));
        sanPhamBienTheThuocTinhPermission.AddChild(VietlifeStorePermissions.SanPhamBienTheThuocTinh.Update, L("Permission:SanPhamBienTheThuocTinh.Update"));
        sanPhamBienTheThuocTinhPermission.AddChild(VietlifeStorePermissions.SanPhamBienTheThuocTinh.Delete, L("Permission:SanPhamBienTheThuocTinh.Delete"));

        //ThuocTinh
        var thuocTinhPermission = VietLifeStoreGroup.AddPermission(VietlifeStorePermissions.ThuocTinh.Default, L("Permission:ThuocTinh"));
        thuocTinhPermission.AddChild(VietlifeStorePermissions.ThuocTinh.View, L("Permission:ThuocTinh.View"));
        thuocTinhPermission.AddChild(VietlifeStorePermissions.ThuocTinh.Create, L("Permission:ThuocTinh.Create"));
        thuocTinhPermission.AddChild(VietlifeStorePermissions.ThuocTinh.Update, L("Permission:ThuocTinh.Update"));
        thuocTinhPermission.AddChild(VietlifeStorePermissions.ThuocTinh.Delete, L("Permission:ThuocTinh.Delete"));

        //SocialVideo
        var socialVideoPermission = VietLifeStoreGroup.AddPermission(VietlifeStorePermissions.SocialVideo.Default, L("Permission:SocialVideo"));
        socialVideoPermission.AddChild(VietlifeStorePermissions.SocialVideo.View, L("Permission:SocialVideo.View"));
        socialVideoPermission.AddChild(VietlifeStorePermissions.SocialVideo.Create, L("Permission:SocialVideo.Create"));
        socialVideoPermission.AddChild(VietlifeStorePermissions.SocialVideo.Update, L("Permission:SocialVideo.Update"));
        socialVideoPermission.AddChild(VietlifeStorePermissions.SocialVideo.Delete, L("Permission:SocialVideo.Delete"));

    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<VietlifeStoreResource>(name);
    }
}
