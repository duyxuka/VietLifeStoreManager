using Volo.Abp.Ui.Branding;
using Volo.Abp.DependencyInjection;
using Microsoft.Extensions.Localization;
using VietlifeStore.Localization;

namespace VietlifeStore.Web;

[Dependency(ReplaceServices = true)]
public class VietlifeStoreBrandingProvider : DefaultBrandingProvider
{
    private IStringLocalizer<VietlifeStoreResource> _localizer;

    public VietlifeStoreBrandingProvider(IStringLocalizer<VietlifeStoreResource> localizer)
    {
        _localizer = localizer;
    }

    public override string AppName => _localizer["AppName"];
}
