using System;
using System.Collections.Generic;
using System.Text;
using ERPPlatform.Localization;
using Volo.Abp.Application.Services;

namespace ERPPlatform;

/* Inherit your application services from this class.
 */
public abstract class ERPPlatformAppService : ApplicationService
{
    protected ERPPlatformAppService()
    {
        LocalizationResource = typeof(ERPPlatformResource);
    }
}
