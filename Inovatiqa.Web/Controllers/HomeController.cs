using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;

namespace Inovatiqa.Web.Controllers
{
    public partial class HomeController : BasePublicController
    {
        public HomeController(
             IRazorViewEngine viewEngine) : base(viewEngine)
        {

        }
        public virtual IActionResult Index()
        {
            return View();
        }
        public virtual IActionResult EssentialTasks()
        {
            return View();
        }
    }
}