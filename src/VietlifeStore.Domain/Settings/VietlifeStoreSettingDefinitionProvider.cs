using Volo.Abp.Settings;

namespace VietlifeStore.Settings;

public class VietlifeStoreSettingDefinitionProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        //Define your own settings here. Example:
        //context.Add(new SettingDefinition(VietlifeStoreSettings.MySetting1));
    }
}
