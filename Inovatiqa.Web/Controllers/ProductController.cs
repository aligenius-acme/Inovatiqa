using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Inovatiqa.Core;
using Inovatiqa.Core.Interfaces;
using Inovatiqa.Core.Rss;
using Inovatiqa.Database.Models;
using Inovatiqa.Services.Catalog.Interfaces;
using Inovatiqa.Services.Customers.Interfaces;
using Inovatiqa.Services.Media.Interfaces;
using Inovatiqa.Services.Messages.Interfaces;
using Inovatiqa.Services.Orders.Interfaces;
using Inovatiqa.Services.Seo.Interfaces;
using Inovatiqa.Services.WorkContext.Interfaces;
using Inovatiqa.Web.Factories.Interfaces;
using Inovatiqa.Web.Models.Catalog;
using Inovatiqa.Web.Models.Media;
using Inovatiqa.Web.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Nest;

namespace Inovatiqa.Web.Controllers
{
    [AutoValidateAntiforgeryToken]
    public partial class ProductController : BasePublicController
    {
        #region Fields

        private readonly IProductService _productService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IWorkContextService _workContextService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IProductModelFactory _productModelFactory;
        private readonly ICompareProductsService _compareProductsService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly IRecentlyViewedProductsService _recentlyViewedProductsService;
        private readonly ICustomerService _customerService;
        private readonly IOrderService _orderService;
        private readonly IReviewTypeService _reviewTypeService;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly IWebHelper _webHelper;
        private readonly IPictureService _pictureService;
        private readonly ICatalogModelFactory _catalogModelFactory;
        private readonly IElasticClient _elasticClient;
        private readonly ICategoryService _categoryService;
        private readonly IManufacturerService _manufacturerService;

        #endregion

        #region Ctor

        public ProductController(IProductService productService,
            IShoppingCartService shoppingCartService,
            IWorkContextService workContextService,
            IUrlRecordService urlRecordService,
            IProductModelFactory productModelFactory,
            ICompareProductsService compareProductsService,
            ICustomerActivityService customerActivityService,
            IRecentlyViewedProductsService recentlyViewedProductsService,
            ICustomerService customerService,
            IOrderService orderService,
            IReviewTypeService reviewTypeService,
            IWorkflowMessageService workflowMessageService,
            IWebHelper webHelper,
            IPictureService pictureService,
            ICatalogModelFactory catalogModelFactory,
             IRazorViewEngine viewEngine,
             IElasticClient elasticClient,
             ICategoryService categoryService,
             IManufacturerService manufacturerService) : base(viewEngine)
        {
            _productService = productService;
            _shoppingCartService = shoppingCartService;
            _workContextService = workContextService;
            _urlRecordService = urlRecordService;
            _productModelFactory = productModelFactory;
            _compareProductsService = compareProductsService;
            _customerActivityService = customerActivityService;
            _recentlyViewedProductsService = recentlyViewedProductsService;
            _customerService = customerService;
            _orderService = orderService;
            _reviewTypeService = reviewTypeService;
            _workflowMessageService = workflowMessageService;
            _webHelper = webHelper;
            _pictureService = pictureService;
            _catalogModelFactory = catalogModelFactory;
            _elasticClient = elasticClient;
            _categoryService = categoryService;
            _manufacturerService = manufacturerService;
        }

        #endregion

        #region Product details page

        public virtual IActionResult ProductDetails(int productId, int updatecartitemid = 0)
        {
            var product = _productService.GetProductById(productId);
            if (product == null || product.Deleted)
                return InvokeHttp404();


            ShoppingCartItem updatecartitem = null;
            if (InovatiqaDefaults.AllowCartItemEditing && updatecartitemid > 0)
            {
                var cart = _shoppingCartService.GetShoppingCart(_workContextService.CurrentCustomer, storeId: InovatiqaDefaults.StoreId);
                updatecartitem = cart.FirstOrDefault(x => x.Id == updatecartitemid);
                if (updatecartitem == null)
                {
                    return RedirectToRoute("Product", new { SeName = _urlRecordService.GetActiveSlug(product.Id, InovatiqaDefaults.ProductSlugName, InovatiqaDefaults.LanguageId) });
                }
                if (product.Id != updatecartitem.ProductId)
                {
                    return RedirectToRoute("Product", new { SeName = _urlRecordService.GetActiveSlug(product.Id, InovatiqaDefaults.ProductSlugName, InovatiqaDefaults.LanguageId) });
                }
            }

            _recentlyViewedProductsService.AddProductToRecentlyViewedList(product.Id);

            _productService.UpdateProductViews(product);

            _customerActivityService.InsertActivity("PublicStore.ViewProduct",
                string.Format("Public store. Viewed a product details page ('{0}')", product.Name), product.Id, product.GetType().Name);

            var model = _productModelFactory.PrepareProductDetailsModel(product);
            var productTemplateViewPath = "ProductTemplate.Simple";

            return View(productTemplateViewPath, model);
        }
        public virtual IActionResult ProductCategorySearch(CatalogPagingFilteringModel command, string term = null, int categoryFilter = -1, List<string> catSearchFilter = null, List<string> attSearchFilter = null, List<string> manufectuererFilter = null, int pageSize = 6, int minPrice = 0, int maxPrice = 0)
        {
            var model = new CategorySearchModel();
            model = _catalogModelFactory.PrepareProductCategorySearchModel(command, term, categoryFilter, catSearchFilter, attSearchFilter, manufectuererFilter, pageSize, minPrice, maxPrice);

            //Categories Html into string
            if(model.NoFurtherChild == 1)
            {
                string categoriesHtml = "";
                if(model.CategoriesCount > 1)
                {
                    foreach(var cat in model.Categories)
                    {
                        if(!(model.searchedCategories.Contains(cat.Id)) && cat.Name != null)
                        {
                            string parentCategory = string.Format("<div class=\"panel-heading\" style=\"padding-bottom:0px; padding-top:0px;\"><p class=\"panel-title\"><a class=\"accordion-toggle\" style=\"color:#151515\" data-toggle=\"collapse\" data-parent=\"#accordion\" href=\"#collapse_{0}\">{1}</a></p></div><div id=\"collapse_{2}\" class=\"panel-collapse collapse\" style=\"padding-top:0px; padding-bottom:0px;\"><div class=\"panel-body\" id=\"appendChildCategory_{3}\">", cat.Id, cat.Name, cat.Id, cat.Id);
                            string childCategories = "";
                            foreach(var childCat in cat.ChildCategories)
                            {
                                if(childCat.Name != null)
                                {
                                    childCategories += string.Format("<div style=\"cursor: pointer;\" class=\"list\" onclick=\"category_filter('{0}')\">{1}<span>{2}</span></div>", childCat.Id, childCat.Name, childCat.ChildCategoriesCount);
                                }
                            }
                            categoriesHtml += parentCategory + childCategories + "</div></div>";
                            parentCategory = "";
                            childCategories = "";
                        }
                    }
                }
                else
                {
                    foreach (var cat in model.Categories)
                    {
                        foreach(var childCat in cat.ChildCategories)
                        {
                            if(childCat.Name != null)
                            {
                                categoriesHtml += String.Format(" <div style=\"cursor: pointer;\" class=\"list\" onclick=\"category_filter('{0}')\">{1}<span>{2}</span></div>", childCat.Id, childCat.Name, childCat.ChildCategoriesCount);
                            }
                        }
                    }
                }
                return Json(categoriesHtml);
            }
            else if (model.NoFurtherAttribut == 1)
            {
                string attributesHtml = "";
                foreach(var att in model.Attributes)
                {
                    if(att.Name != null)
                    {
                        string parentAttr = string.Format("<div class=\"panel-heading\" style=\"padding-bottom:0px; padding-top:0px;\"><p class=\"panel-title\"><a class=\"accordion-toggle\" style=\"color:#151515\" data-toggle=\"collapse\" data-parent=\"#accordion\" href=\"#collapse_{0}\">{1}</a></p></div><div id=\"collapse_{2}\" class=\"panel-collapse collapse\" style=\"padding-top:0px; padding-bottom:0px;\"><div class=\"panel-body\" id=\"appendAttributeValue_{3}\">", att.Id, att.Name, att.Id, att.Id);
                        string attrValues = "";
                        foreach(var attVal in att.AttributesValues)
                        {
                            if(attVal.Name != null)
                            {
                                attrValues += string.Format("<div style=\"cursor:pointer;\" class=\"list\" onclick=\"attribute_filter('{0}')\">{1}<span>{2}</span></div>", attVal.Ids, attVal.Name, attVal.AttributesValuesCount);
                            }
                        }
                        attributesHtml += parentAttr + attrValues + "</div></div>";
                    }
                }
                return Json(attributesHtml);
            }
            else
            {
                string noFurtherFilter = "<div class= \"alert alert-info\">No More Filter Available.</div>";
                return Json(noFurtherFilter);
            }
        }
        public virtual IActionResult ProductManufacturerSearch(CatalogPagingFilteringModel command, string term = null, int categoryFilter = -1, List<string> catSearchFilter = null, List<string> attSearchFilter = null, List<string> manufectuererFilter = null, int pageSize = 6, int minPrice = 0, int maxPrice = 0)
        {
            var model = new List<ManufacturerSearchModel>();
            model = _catalogModelFactory.PrepareProductManufacturerSearchModel(command, term, categoryFilter, catSearchFilter, attSearchFilter, manufectuererFilter, pageSize, minPrice, maxPrice);

            //Manufacturers Html into string
            string manufacturersHtml = "";
            if(model.Count() > 0)
            {
                model.ForEach(man =>  manufacturersHtml += string.Format("<div class=\"list\"><label><input type=\"checkbox\" {3} onclick=\"reloadWithManufacturer(this,'{0}')\" />{1}<span>{2}</span></label></div>", man.Id, man.Name, man.ManufacturersCount, man.IsSelected ? "checked" : ""));

                //foreach(var man in model)
                //{
                //    manufacturersHtml += string.Format("<div class=\"list\"><label><input type=\"checkbox\" {3} onclick=\"reloadWithManufacturer(this,'{0}')\" />{1}<span>{2}</span></label></div>", man.Id, man.Name, man.ManufacturersCount, man.IsSelected ? "checked" : "");
                //}
            }
            else
            {
                manufacturersHtml = "<div class= \"alert alert-info\">No Brands Available.</div>";
            }


            return Json(manufacturersHtml);
        }
        public virtual IActionResult ProductSearch(CatalogPagingFilteringModel command, string term = null, int categoryFilter = -1, List<string> catSearchFilter = null, List<string> attSearchFilter = null, List<string> manufectuererFilter = null, int pageSize = 6, int minPrice = 0, int maxPrice = 0)
        {
            var model = new ProductSearchModel();
            TempData["term"] = term;
            TempData["category"] = categoryFilter;
            if (string.IsNullOrWhiteSpace(term) || term.Length < InovatiqaDefaults.ProductSearchTermMinimumLength)
                return View(model);
            //if (categoryFilter == -1)
            //    categoryFilter.;
            //var currentCategory = catSearchFilter;
            //catSearchFilter.AddRange(catSearchFilter);
            model = _catalogModelFactory.PrepareProductSearchModel(command, term, categoryFilter, catSearchFilter, attSearchFilter, manufectuererFilter,  pageSize, minPrice, maxPrice);
            model.Products = model.Products.OrderByDescending(l => l.DefaultPictureModel.ImageUrl != "/images/thumbs/default-image_415.png").ThenByDescending(i => i.ShortDescription != "").ToList();
            model.Term = term;
            model.MinPrice = minPrice == 0 ? "" : Convert.ToString(minPrice);
            model.MaxPrice = maxPrice == 0 ? "" : Convert.ToString(maxPrice);
            return View(model);
        }
        #endregion

        #region Recently viewed products

        public virtual IActionResult RecentlyViewedProducts()
        {
            if (!InovatiqaDefaults.RecentlyViewedProductsEnabled)
                return Content("");

            var products = _recentlyViewedProductsService.GetRecentlyViewedProducts(InovatiqaDefaults.RecentlyViewedProductsNumber);

            var model = new List<ProductOverviewModel>();
            model.AddRange(_productModelFactory.PrepareProductOverviewModels(products));

            return View(model);
        }

        #endregion

        #region New (recently added) products page

        public virtual IActionResult NewProducts(CatalogPagingFilteringModel command)
        {
            if (!InovatiqaDefaults.NewProductsEnabled)
                return Content("");

            var model = _productModelFactory.PrepareNewProductsModel(command);

            return View(model);
        }
        public virtual IActionResult ShopProducts(CatalogPagingFilteringModel command, int pictureSize = 0)
        {
            List<CategoryModel> model = new List<CategoryModel>();
            var categories = _categoryService.GetParentCategories();
            if(categories != null && categories.Count > 0)
            {
                foreach(var category in categories)
                {
                    var catModel = new CategoryModel
                    {
                        Id = category.Id,
                        Name = category.Name,
                        Description = category.Description,
                        MetaKeywords = category.MetaKeywords,
                        MetaDescription = category.MetaDescription,
                        MetaTitle = category.MetaTitle,
                        SeName = _urlRecordService.GetActiveSlug(category.Id, InovatiqaDefaults.CategorySlugName, InovatiqaDefaults.LanguageId),
                        ChildCategories = _categoryService.GetChildCategories(category.Id).Take(4).ToList()
                    };
                   foreach (var cc in catModel.ChildCategories)
                    {
                        catModel.childCategoriesLinks.Add(new CategoryModel
                        {
                            SeName = _urlRecordService.GetActiveSlug(cc.Id, InovatiqaDefaults.CategorySlugName, InovatiqaDefaults.LanguageId),
                            Name = cc.Name
                        });
                    }
                    var picture = _pictureService.GetPictureById(category.PictureId);
                    var pictureModel = new PictureModel
                    {
                        FullSizeImageUrl = _pictureService.GetPictureUrl(ref picture),
                        ImageUrl = _pictureService.GetPictureUrl(ref picture, pictureSize),
                        Title = string.Format(
                            "Show products in category {0}",
                            catModel.Name),
                        AlternateText =
                            string.Format(
                                "Picture for category {0}",
                                catModel.Name)
                    };

                    catModel.PictureModel = pictureModel;
                    model.Add(catModel);
                }
            }
            return View(model);
        }

        public virtual IActionResult NewProductsRss()
        {
            var feed = new RssFeed(
                $"{InovatiqaDefaults.CurrentStoreName}: New products",
                "Information about products",
                new Uri(InovatiqaDefaults.StoreUrl),
                DateTime.UtcNow);

            if (!InovatiqaDefaults.NewProductsEnabled)
                return new RssActionResult(feed, _webHelper.GetThisPageUrl(false));

            var items = new List<RssItem>();

            var products = _productService.SearchProducts(
                storeId: InovatiqaDefaults.StoreId,
                visibleIndividuallyOnly: true,
                markedAsNewOnly: true,
                orderBy: ProductSortingEnum.CreatedOn,
                pageSize: InovatiqaDefaults.NewProductsNumber);
            foreach (var product in products)
            {
                var productUrl = Url.RouteUrl("Product", new { SeName = _urlRecordService.GetActiveSlug(product.Id, InovatiqaDefaults.ProductSlugName, InovatiqaDefaults.LanguageId) }, _webHelper.CurrentRequestProtocol);
                var productName = product.Name;
                var productDescription = product.ShortDescription;
                var item = new RssItem(productName, productDescription, new Uri(productUrl), $"urn:store:{InovatiqaDefaults.StoreId}:newProducts:product:{product.Id}", product.CreatedOnUtc);
                items.Add(item);

                var picture = _pictureService.GetPicturesByProductId(product.Id, 1).FirstOrDefault();
                if (picture != null)
                {
                    var imageUrl = _pictureService.GetPictureUrl(ref picture, InovatiqaDefaults.ProductDetailsPictureSize);
                    item.ElementExtensions.Add(new XElement("enclosure", new XAttribute("type", "image/jpeg"), new XAttribute("url", imageUrl), new XAttribute("length", picture.PictureBinary.Count)));
                }

            }
            feed.Items = items;
            return new RssActionResult(feed, _webHelper.GetThisPageUrl(false));
        }

        #endregion

        #region Product reviews

        public virtual IActionResult ProductReviews(int productId)
        {
            var product = _productService.GetProductById(productId);
            if (product == null || product.Deleted || !product.Published || !product.AllowCustomerReviews)
                return RedirectToRoute("Homepage");

            var model = new ProductReviewsModel();
            model = _productModelFactory.PrepareProductReviewsModel(model, product);

            if (_customerService.IsGuest(_workContextService.CurrentCustomer) && !InovatiqaDefaults.AllowAnonymousUsersToReviewProduct)
                ModelState.AddModelError("", "Only registered users can write reviews");

            if (InovatiqaDefaults.ProductReviewPossibleOnlyAfterPurchasing)
            {
                var hasCompletedOrders = _orderService.SearchOrders(customerId: _workContextService.CurrentCustomer.Id,
                    productId: productId,
                    osIds: new List<int> { (int)OrderStatus.Complete },
                    pageSize: 1).Any();
                if (!hasCompletedOrders)
                    ModelState.AddModelError(string.Empty, "Product can be reviewed only after purchasing it");
            }

            model.AddProductReview.Rating = InovatiqaDefaults.DefaultProductRatingValue;

            if (model.ReviewTypeList.Count > 0)
            {
                foreach (var additionalProductReview in model.AddAdditionalProductReviewList)
                {
                    additionalProductReview.Rating = additionalProductReview.IsRequired ? InovatiqaDefaults.DefaultProductRatingValue : 0;
                }
            }

            return View(model);
        }

        [HttpPost, ActionName("ProductReviews")]
        [FormValueRequired("add-review")]
        public virtual IActionResult ProductReviewsAdd(int productId, ProductReviewsModel model, bool captchaValid)
        {
            var product = _productService.GetProductById(productId);
            if (product == null || product.Deleted || !product.Published || !product.AllowCustomerReviews)
                return RedirectToRoute("Homepage");

            if (InovatiqaDefaults.Enabled && InovatiqaDefaults.ShowOnProductReviewPage && !captchaValid)
            {
                ModelState.AddModelError("", "The reCAPTCHA response is invalid or malformed. Please try again.");
            }

            if (_customerService.IsGuest(_workContextService.CurrentCustomer) && !InovatiqaDefaults.AllowAnonymousUsersToReviewProduct)
            {
                ModelState.AddModelError("", "Only registered users can write reviews");
            }

            if (InovatiqaDefaults.ProductReviewPossibleOnlyAfterPurchasing)
            {
                var hasCompletedOrders = _orderService.SearchOrders(customerId: _workContextService.CurrentCustomer.Id,
                    productId: productId,
                    osIds: new List<int> { (int)OrderStatus.Complete },
                    pageSize: 1).Any();
                if (!hasCompletedOrders)
                    ModelState.AddModelError(string.Empty, "Product can be reviewed only after purchasing it");
            }

            if (ModelState.IsValid)
            {
                var rating = model.AddProductReview.Rating;
                if (rating < 1 || rating > 5)
                    rating = InovatiqaDefaults.DefaultProductRatingValue;
                var isApproved = !InovatiqaDefaults.ProductReviewsMustBeApproved;

                var productReview = new ProductReview
                {
                    ProductId = product.Id,
                    CustomerId = _workContextService.CurrentCustomer.Id,
                    Title = model.AddProductReview.Title,
                    ReviewText = model.AddProductReview.ReviewText,
                    Rating = rating,
                    HelpfulYesTotal = 0,
                    HelpfulNoTotal = 0,
                    IsApproved = isApproved,
                    CreatedOnUtc = DateTime.UtcNow,
                    StoreId = 1,
                };

                _productService.InsertProductReview(productReview);

                foreach (var additionalReview in model.AddAdditionalProductReviewList)
                {
                    var additionalProductReview = new ProductReviewReviewTypeMapping
                    {
                        ProductReviewId = productReview.Id,
                        ReviewTypeId = additionalReview.ReviewTypeId,
                        Rating = additionalReview.Rating
                    };

                    _reviewTypeService.InsertProductReviewReviewTypeMappings(additionalProductReview);
                }

                _productService.UpdateProductReviewTotals(product);

                if (InovatiqaDefaults.NotifyStoreOwnerAboutNewProductReviews)
                    _workflowMessageService.SendProductReviewNotificationMessage(productReview, InovatiqaDefaults.LanguageId);

                _customerActivityService.InsertActivity("PublicStore.AddProductReview",
                    string.Format("Added a product review ('{0}')", product.Name), product.Id, product.GetType().Name);

                //if (productReview.IsApproved)
                //    _eventPublisher.Publish(new ProductReviewApprovedEvent(productReview));

                model = _productModelFactory.PrepareProductReviewsModel(model, product);
                model.AddProductReview.Title = null;
                model.AddProductReview.ReviewText = null;

                model.AddProductReview.SuccessfullyAdded = true;
                if (!isApproved)
                    model.AddProductReview.Result = "You will see the product review after approving by a store administrator.";
                else
                    model.AddProductReview.Result = "Product review is successfully added.";

                return View(model);
            }

            model = _productModelFactory.PrepareProductReviewsModel(model, product);
            return View(model);
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public virtual IActionResult SetProductReviewHelpfulness(int productReviewId, bool washelpful)
        {
            var productReview = _productService.GetProductReviewById(productReviewId);
            if (productReview == null)
                throw new ArgumentException("No product review found with the specified id");

            if (_customerService.IsGuest(_workContextService.CurrentCustomer) && !InovatiqaDefaults.AllowAnonymousUsersToReviewProduct)
            {
                return Json(new
                {
                    Result = "Only registered customers can set review helpfulness",
                    TotalYes = productReview.HelpfulYesTotal,
                    TotalNo = productReview.HelpfulNoTotal
                });
            }

            if (productReview.CustomerId == _workContextService.CurrentCustomer.Id)
            {
                return Json(new
                {
                    Result = "You cannot vote for your own review",
                    TotalYes = productReview.HelpfulYesTotal,
                    TotalNo = productReview.HelpfulNoTotal
                });
            }

            _productService.SetProductReviewHelpfulness(productReview, washelpful);

            _productService.UpdateProductReviewHelpfulnessTotals(productReview);

            return Json(new
            {
                Result = "Successfully voted",
                TotalYes = productReview.HelpfulYesTotal,
                TotalNo = productReview.HelpfulNoTotal
            });
        }

        public virtual IActionResult CustomerProductReviews(int? pageNumber)
        {
            if (_customerService.IsGuest(_workContextService.CurrentCustomer))
                return Challenge();

            if (!InovatiqaDefaults.ShowProductReviewsTabOnAccountPage)
            {
                return RedirectToRoute("CustomerInfo");
            }

            var model = _productModelFactory.PrepareCustomerProductReviewsModel(pageNumber);
            return View(model);
        }

        #endregion

        #region Email a friend



        #endregion

        #region Comparing products

        public virtual IActionResult CompareProducts()
        {
            if (!InovatiqaDefaults.CompareProductsEnabled)
                return RedirectToRoute("Homepage");

            var model = new CompareProductsModel
            {
                IncludeShortDescriptionInCompareProducts = InovatiqaDefaults.IncludeShortDescriptionInCompareProducts,
                IncludeFullDescriptionInCompareProducts = InovatiqaDefaults.IncludeFullDescriptionInCompareProducts,
            };

            var products = _compareProductsService.GetComparedProducts();
            

            _productModelFactory.PrepareProductOverviewModels(products, prepareSpecificationAttributes: true, prepareProductManufacturer: true, prepareProductAttributes: true)
                .ToList()
                .ForEach(model.Products.Add);
            return View(model);
        }

        public virtual IActionResult ClearCompareList()
        {
            if (!InovatiqaDefaults.CompareProductsEnabled)
                return RedirectToRoute("Homepage");

            _compareProductsService.ClearCompareProducts();

            return RedirectToRoute("CompareProducts");
        }
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public virtual IActionResult ClearCompareListWithoutRedirect()
        {
            _compareProductsService.ClearCompareProducts();
            return Json(new
            {
                success = true,
                message = "Compare products successfully cleared!"
            });
        }
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public virtual IActionResult AddProductToCompareList(int productId)
        {
            var product = _productService.GetProductById(productId);
            if (product == null || product.Deleted || !product.Published)
                return Json(new
                {
                    success = false,
                    message = "No product found with the specified ID"
                });

            if (!InovatiqaDefaults.CompareProductsEnabled)
                return Json(new
                {
                    success = false,
                    message = "Product comparison is disabled"
                });

            _compareProductsService.AddProductToCompareList(productId);

            _customerActivityService.InsertActivity("PublicStore.AddToCompareList",
                string.Format("Added a product to compare list ('{0}')", product.Name), product.Id, product.GetType().Name);


            return Json(new
            {
                success = true,
                message = string.Format("The product has been added to your <a href=\"{0}\">product comparison</a>", Url.RouteUrl("CompareProducts"))
            });
        }

        public virtual IActionResult RemoveProductFromCompareList(int productId)
        {
            var product = _productService.GetProductById(productId);
            if (product == null)
                return RedirectToRoute("Homepage");

            if (!InovatiqaDefaults.CompareProductsEnabled)
                return RedirectToRoute("Homepage");

            _compareProductsService.RemoveProductFromCompareList(productId);

            return RedirectToRoute("CompareProducts");
        }
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public virtual IActionResult RemoveProductFromCompareListAndStayonSamePage(int productId, bool redirectToCompareList = true)
        {
            var product = _productService.GetProductById(productId);
            if (product == null)
                return RedirectToRoute("Homepage");

            if (!InovatiqaDefaults.CompareProductsEnabled)
                return RedirectToRoute("Homepage");

            _compareProductsService.RemoveProductFromCompareList(productId);
            if (redirectToCompareList)
                return RedirectToRoute("CompareProducts");
            else
                return Json(new
                {
                    success = true,
                    message = string.Format("The product has been removed from <a href=\"{0}\">product comparison list</a>", Url.RouteUrl("CompareProducts"))
                });
        }

        public virtual IActionResult Products()
        {
            return View();
        }

        #endregion

        #region Indexing
        public virtual IActionResult IndexProducts(int start, int end)
        {
            //_productService.IndexProducts();
            _catalogModelFactory.IndexProducts(start, end);
            return Json(true);
        }
        //by hamza for product indexing bug fixing
        //public virtual IActionResult IndexProductsCheck(int start, int end)
        //{
        //    //_productService.IndexProducts();
        //    _catalogModelFactory.IndexProductsCheck(start, end);
        //    return Json(true);
        //}
        //by hamza for elasticSearch
        public virtual IActionResult IndexManufacturers(int start, int end)
        {
            _catalogModelFactory.IndexManufacturers(start, end);
            return Json(true);
        }
        //by hamza for elasticSearch
        public virtual IActionResult IndexCategories(int start, int end)
        {
            _catalogModelFactory.IndexCategories(start, end);
            return Json(true);
        }
        public virtual IActionResult IndexAttributes(int start, int end)
        {
            _catalogModelFactory.IndexAttributes(start, end);
            return Json(true);
        }

        #endregion
    }
}