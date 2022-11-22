using System.Linq;
using Inovatiqa.Core;
using Inovatiqa.Services.Catalog.Interfaces;
using Inovatiqa.Services.Orders.Interfaces;
using Inovatiqa.Web.Factories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Inovatiqa.Web.Components
{
    public class HomepageBestSellersViewComponent : ViewComponent
    {
        private readonly IProductModelFactory _productModelFactory;
        private readonly IProductService _productService;
        private readonly IOrderReportService _orderReportService;

        public HomepageBestSellersViewComponent(IProductModelFactory productModelFactory,
            IProductService productService,
            IOrderReportService orderReportService)
        {
            _productModelFactory = productModelFactory;
            _productService = productService;
            _orderReportService = orderReportService;
        }

        public IViewComponentResult Invoke(int? productThumbPictureSize)
        {
            if (!InovatiqaDefaults.ShowBestsellersOnHomepage)
                return Content("");

            var report = _orderReportService.BestSellersReport(
                        storeId: InovatiqaDefaults.StoreId,
                        pageSize: InovatiqaDefaults.NumberOfBestsellersOnHomepage)
                    .ToList();

            var products = _productService.GetProductsByIds(report.Select(x => x.ProductId).ToArray());

            if (!products.Any())
                return Content("");

            var model = _productModelFactory.PrepareProductOverviewModels(products, true, true, productThumbPictureSize).ToList();
            return View(model);
        }
    }
}