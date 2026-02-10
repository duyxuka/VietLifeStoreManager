using VietlifeStore.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace VietlifeStore.Controllers;

/* Inherit your controllers from this class.
 */
public abstract class VietlifeStoreController : AbpControllerBase
{
    protected VietlifeStoreController()
    {
        LocalizationResource = typeof(VietlifeStoreResource);
    }
}
