using ERPPlatform.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace ERPPlatform.Controllers;

/* Inherit your controllers from this class.
 */
public abstract class ERPPlatformController : AbpControllerBase
{
    protected ERPPlatformController()
    {
        LocalizationResource = typeof(ERPPlatformResource);
    }
}
