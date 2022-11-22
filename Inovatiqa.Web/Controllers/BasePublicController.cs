using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;

namespace Inovatiqa.Web.Controllers
{
    public abstract partial class BasePublicController : BaseController
    {
        public BasePublicController(IRazorViewEngine viewEngine):base(viewEngine)
        {
        }
        protected virtual IActionResult InvokeHttp404()
        {
            Response.StatusCode = 404;
            return new EmptyResult();
        }
    }
}