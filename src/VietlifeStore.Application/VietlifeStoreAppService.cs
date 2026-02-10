using VietlifeStore.Localization;
using Volo.Abp.Application.Services;

namespace VietlifeStore;

/* Inherit your application services from this class.
 */
public abstract class VietlifeStoreAppService : ApplicationService
{
    protected VietlifeStoreAppService()
    {
        LocalizationResource = typeof(VietlifeStoreResource);
    }
}
