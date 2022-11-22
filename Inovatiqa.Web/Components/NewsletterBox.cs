using Inovatiqa.Web.Factories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Inovatiqa.Web.Components
{
    public class NewsletterBoxViewComponent : ViewComponent
    {
        private readonly INewsletterModelFactory _newsletterModelFactory;

        public NewsletterBoxViewComponent(INewsletterModelFactory newsletterModelFactory)
        {
            _newsletterModelFactory = newsletterModelFactory;
        }

        public IViewComponentResult Invoke()
        {
            var model = _newsletterModelFactory.PrepareNewsletterBoxModel();
            return View(model);
        }
    }
}
