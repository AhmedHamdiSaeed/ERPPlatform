using Volo.Abp.Settings;

namespace ERPPlatform.Settings;

public class ERPPlatformSettingDefinitionProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        //Define your own settings here. Example:
        //context.Add(new SettingDefinition(ERPPlatformSettings.MySetting1));
    }
}
