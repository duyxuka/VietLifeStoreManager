using VietlifeStore.Localization;
using Volo.Abp.AspNetCore.Mvc.UI.RazorPages;

namespace VietlifeStore.Web.Pages;

public abstract class VietlifeStorePageModel : AbpPageModel
{
    protected VietlifeStorePageModel()
    {
        LocalizationResourceType = typeof(VietlifeStoreResource);
    }
}
