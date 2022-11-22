using Inovatiqa.Core;
using Inovatiqa.Core.Interfaces;
using Inovatiqa.Services.Catalog.Interfaces;
using Inovatiqa.Services.Common.Interfaces;
using Inovatiqa.Services.Customers.Interfaces;
using Inovatiqa.Services.WorkContext.Interfaces;
using Inovatiqa.Web.Factories.Interfaces;
using Inovatiqa.Web.Models.Catalog;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Inovatiqa.Web.Controllers
{
    public class CatalogController : BasePublicController
    {
        #region Fields

        private readonly ICategoryService _categoryService;
        private readonly ICatalogModelFactory _catalogModelFactory;
        private readonly IProductService _productService;
        private readonly IProductModelFactory _productModelFactory;
        private readonly IWorkContextService _workContextService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IWebHelper _webHelper;
        private readonly IManufacturerService _manufacturerService;
        private readonly ICustomerActivityService _customerActivityService;
        #endregion

        #region Ctor

        public CatalogController(ICategoryService categoryService,
             ICatalogModelFactory catalogModelFactory,
             IProductService productService,
             IProductModelFactory productModelFactory,
             IWorkContextService workContextService,
             IGenericAttributeService genericAttributeService,
             IWebHelper webHelper,
             IManufacturerService manufacturerService,
             ICustomerActivityService customerActivityService,
             IRazorViewEngine viewEngine) : base(viewEngine)
        {
            _categoryService = categoryService;
            _catalogModelFactory = catalogModelFactory;
            _productService = productService;
            _productModelFactory = productModelFactory;
            _workContextService = workContextService;
            _genericAttributeService = genericAttributeService;
            _webHelper = webHelper;
            _manufacturerService = manufacturerService;
            _customerActivityService = customerActivityService;
        }

        #endregion

        #region Methods

        #region Categories
        public virtual IActionResult ProductCategorySearch(CatalogPagingFilteringModel command, string term = null, int categoryFilter = -1, List<string> catSearchFilter = null, List<string> attSearchFilter = null, List<string> manufectuererFilter = null, int pageSize = 6, int minPrice = 0, int maxPrice = 0)
        {
            categoryFilter = Convert.ToInt32(TempData["categoryFilter"]);
            var model = _catalogModelFactory.PrepareProductCategorySearchModel(command, term, categoryFilter, catSearchFilter, attSearchFilter, manufectuererFilter, pageSize, minPrice, maxPrice);

            return Json(model);
        }
        public virtual IActionResult ProductManufacturerSearch(CatalogPagingFilteringModel command, string term = null, int categoryFilter = -1, List<string> catSearchFilter = null, List<string> attSearchFilter = null, List<string> manufectuererFilter = null, int pageSize = 6, int minPrice = 0, int maxPrice = 0)
        {
            categoryFilter = Convert.ToInt32(TempData["categoryFilter"]);
            var model = _catalogModelFactory.PrepareProductManufacturerSearchModel(command, term, categoryFilter, catSearchFilter, attSearchFilter, manufectuererFilter, pageSize, minPrice, maxPrice);
            return Json(model);
        }
        public virtual IActionResult Category(int categoryId, CatalogPagingFilteringModel command, int minPrice = 0, int maxPrice = 0, int pageSize = 6, string term = null, List<string> manufectuererFilter = null, List<string> catSearchFilter = null, List<string> attSearchFilter = null)
        {
            var category = _categoryService.GetCategoryById(categoryId);
            if (category == null || category.Deleted)
                return InvokeHttp404();

            var currentCustomer = _workContextService.CurrentCustomer;

            _genericAttributeService.SaveAttribute(currentCustomer.GetType().Name, currentCustomer.Id,
                InovatiqaDefaults.LastContinueShoppingPageAttribute,
                _webHelper.GetThisPageUrl(false),
                InovatiqaDefaults.StoreId);

            TempData["categoryFilter"] = categoryId;

            ProductSearchModel model = null;
                model = _catalogModelFactory.PrepareProductSearchModel(command, "", categoryId, catSearchFilter, attSearchFilter, manufectuererFilter, pageSize, minPrice, maxPrice);
            //if (term == null)
            //{
            //}
            //else
            //{
            //model = _catalogModelFactory.PrepareCategorySearchModel(category, command, term);

            //}
            model.categoryId = categoryId;
            model.MinPrice = minPrice == 0 ? "" : Convert.ToString(minPrice);
            model.MaxPrice = maxPrice == 0 ? "" : Convert.ToString(maxPrice);
            model.Products = model.Products.OrderByDescending(l => l.DefaultPictureModel.ImageUrl != "/images/thumbs/default-image_415.png").ThenByDescending(i => i.ShortDescription != "").ToList();
            return View("CategoryTemplate.ProductsInGridOrLines", model);
        }
        public virtual IActionResult PopulateCategories(int currentCategoryId = 0, int currentProductId = 0)
        {
            var categories = _catalogModelFactory.PrepareCategoryNavigationModel(currentCategoryId, currentProductId);
            return Json(categories);
        }

        #endregion


        #region Searching

        public virtual IActionResult SearchTermAutoComplete(string term)
        {
            if (string.IsNullOrWhiteSpace(term) || term.Length < InovatiqaDefaults.ProductSearchTermMinimumLength)
                return Content("");

            var productNumber = InovatiqaDefaults.ProductSearchAutoCompleteNumberOfProducts > 0 ?
                InovatiqaDefaults.ProductSearchAutoCompleteNumberOfProducts : 10;

            var products = _productService.SearchProducts(
                storeId: InovatiqaDefaults.StoreId,
                keywords: term,
                languageId: InovatiqaDefaults.LanguageId,
                visibleIndividuallyOnly: true,
                pageSize: productNumber);

            var showLinkToResultSearch = InovatiqaDefaults.ShowLinkToAllResultInSearchAutoComplete && (products.TotalCount > productNumber);

            var models = _productModelFactory.PrepareProductOverviewModels(products, false, InovatiqaDefaults.ShowProductImagesInSearchAutoComplete, InovatiqaDefaults.AutoCompleteSearchThumbPictureSize).ToList();
            var result = (from p in models
                          select new
                          {
                              p.Id,
                              label = p.Name,
                              producturl = Url.RouteUrl("Product", new { SeName = p.SeName }),
                              productpictureurl = p.DefaultPictureModel.ImageUrl,
                              showlinktoresultsearch = showLinkToResultSearch
                          })
                .ToList();
            return Json(result);
        }

        #endregion


        #region Manufacturers

        public virtual IActionResult Manufacturer(int manufacturerId, CatalogPagingFilteringModel command, int minPrice = 0, int maxPrice = 0)
        {
            var manufacturer = _manufacturerService.GetManufacturerById(manufacturerId);
            if (manufacturer == null || manufacturer.Deleted)
                return InvokeHttp404();

            var notAvailable =
                !manufacturer.Published;

            if (notAvailable)
                return InvokeHttp404();

            var currentCustomer = _workContextService.CurrentCustomer;


            _genericAttributeService.SaveAttribute<string>(currentCustomer.GetType().Name, currentCustomer.Id, InovatiqaDefaults.LastContinueShoppingPageAttribute, _webHelper.GetThisPageUrl(false), InovatiqaDefaults.StoreId);


            _customerActivityService.InsertActivity("PublicStore.ViewManufacturer",
                string.Format("Public store. Viewed a manufacturer details page ('{0}')", manufacturer.Name), manufacturer.Id, manufacturer.GetType().Name);

        
            var model = _catalogModelFactory.PrepareManufacturerModel(manufacturer, command, minPrice, maxPrice);

            //var templateViewPath = _catalogModelFactory.PrepareManufacturerTemplateViewPath(manufacturer.ManufacturerTemplateId);
            //return View(templateViewPath, model);
            model.MinPrice = minPrice == 0 ? "" : Convert.ToString(minPrice);
            model.MaxPrice = maxPrice == 0 ? "" : Convert.ToString(maxPrice);
            return View("ManufacturerTemplate.ProductsInGridOrLines", model);
        }

        public virtual IActionResult ManufacturerAll()
        {
            var model = _catalogModelFactory.PrepareManufacturerAllModels();
            return View(model);
        }

        #endregion

        #endregion
    }
}
