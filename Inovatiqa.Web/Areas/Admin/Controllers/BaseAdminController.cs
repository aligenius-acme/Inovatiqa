using Inovatiqa.Core;
using Inovatiqa.Web.Controllers;
using Inovatiqa.Web.Framework;
using Inovatiqa.Web.Framework.Mvc.Filters;
using Inovatiqa.Web.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Newtonsoft.Json;

namespace Inovatiqa.Web.Areas.Admin.Controllers
{
    [Area(AreaNames.Admin)]
    [RequireHttps]
    [AutoValidateAntiforgeryToken]
    [AuthorizeAdmin]
    [ValidateVendor]
    [SaveSelectedTab]
    public abstract partial class BaseAdminController : BaseController
    {
        public BaseAdminController(IRazorViewEngine viewEngine) : base(viewEngine)
        {
        }
        public override JsonResult Json(object data)
        {
            var useIsoDateFormat = InovatiqaDefaults.UseIsoDateFormatInJsonResult;
            var serializerSettings = new JsonSerializerSettings();

            if (!useIsoDateFormat)
                return base.Json(data, serializerSettings);

            serializerSettings.DateFormatHandling = DateFormatHandling.IsoDateFormat;
            serializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Unspecified;

            return base.Json(data, serializerSettings);
        }

    }
}