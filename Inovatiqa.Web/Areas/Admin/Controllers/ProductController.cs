using System;
using Inovatiqa.Core;
using Inovatiqa.Database.Models;
using Inovatiqa.Services.Catalog.Interfaces;
using Inovatiqa.Services.Messages.Interfaces;
using Inovatiqa.Services.Security;
using Inovatiqa.Services.Security.Interfaces;
using Inovatiqa.Services.WorkContext.Interfaces;
using Inovatiqa.Web.Areas.Admin.Factories.Interfaces;
using Inovatiqa.Web.Areas.Admin.Models.Catalog;
using Inovatiqa.Web.Mvc;
using Inovatiqa.Web.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Inovatiqa.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Inovatiqa.Web.Controllers;
using Inovatiqa.Services.Media.Interfaces;
using System.Linq;
using Inovatiqa.Services.Customers.Interfaces;
using Microsoft.AspNetCore.Mvc.Rendering;
using Inovatiqa.Database.Interfaces;

namespace Inovatiqa.Web.Areas.Admin.Controllers
{
    public partial class ProductController : BaseAdminController
    {
        #region Fields

        private readonly IPermissionService _permissionService;
        private readonly IWorkContextService _workContextService;
        private readonly IProductService _productService;
        private readonly INotificationService _notificationService;
        private readonly IProductModelFactory _productModelFactory;
        private readonly IProductAttributeService _productAttributeService;
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly IPictureService _pictureService;
        private readonly ICategoryService _categoryService;
        private readonly IManufacturerService _manufacturerService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ICustomerService _customerService;
        private readonly IRepository<Customer> _customerRepository;

        #endregion

        #region Ctor

        public ProductController(IPermissionService permissionService,
            IWorkContextService workContextService,
            IProductService productService,
            IProductModelFactory productModelFactory,
            IProductAttributeService productAttributeService,
            INotificationService notificationService,
            ISpecificationAttributeService specificationAttributeService,
            IPictureService pictureService,
            ICategoryService categoryService,
            IManufacturerService manufacturerService,
            ICustomerActivityService customerActivityService,
            IRazorViewEngine viewEngine,
            ICustomerService customerService,
            IRepository<Customer> customerRepository) : base(viewEngine)
        {
            _permissionService = permissionService;
            _workContextService = workContextService;
            _productService = productService;
            _productModelFactory = productModelFactory;
            _productAttributeService = productAttributeService;
            _notificationService = notificationService;
            _specificationAttributeService = specificationAttributeService;
            _pictureService = pictureService;
            _categoryService = categoryService;
            _manufacturerService = manufacturerService;
            _customerActivityService = customerActivityService;
            _customerService = customerService;
            _customerRepository = customerRepository;
        }

        #endregion

        #region Utilities

        protected virtual void SaveCategoryMappings(Product product, ProductModel model)
        {
            var existingProductCategories = _categoryService.GetProductCategoriesByProductId(product.Id, true);

            foreach (var existingProductCategory in existingProductCategories)
                if (!model.SelectedCategoryIds.Contains(existingProductCategory.CategoryId))
                    _categoryService.DeleteProductCategory(existingProductCategory);

            foreach (var categoryId in model.SelectedCategoryIds)
            {
                if (_categoryService.FindProductCategory(existingProductCategories, product.Id, categoryId) == null)
                {
                    //find next display order
                    var displayOrder = 1;
                    var existingCategoryMapping = _categoryService.GetProductCategoriesByCategoryId(categoryId, showHidden: true);
                    if (existingCategoryMapping.Any())
                        displayOrder = existingCategoryMapping.Max(x => x.DisplayOrder) + 1;
                    _categoryService.InsertProductCategory(new ProductCategoryMapping
                    {
                        ProductId = product.Id,
                        CategoryId = categoryId,
                        DisplayOrder = displayOrder
                    });
                }
            }
        }

        protected virtual void SaveManufacturerMappings(Product product, ProductModel model)
        {
            var existingProductManufacturers = _manufacturerService.GetProductManufacturersByProductId(product.Id, true);

            foreach (var existingProductManufacturer in existingProductManufacturers)
                if (!model.SelectedManufacturerIds.Contains(existingProductManufacturer.ManufacturerId))
                    _manufacturerService.DeleteProductManufacturer(existingProductManufacturer);

            foreach (var manufacturerId in model.SelectedManufacturerIds)
            {
                if (_manufacturerService.FindProductManufacturer(existingProductManufacturers, product.Id, manufacturerId) == null)
                {
                    //find next display order
                    var displayOrder = 1;
                    var existingManufacturerMapping = _manufacturerService.GetProductManufacturersByManufacturerId(manufacturerId, showHidden: true);
                    if (existingManufacturerMapping.Any())
                        displayOrder = existingManufacturerMapping.Max(x => x.DisplayOrder) + 1;
                    _manufacturerService.InsertProductManufacturer(new ProductManufacturerMapping
                    {
                        ProductId = product.Id,
                        ManufacturerId = manufacturerId,
                        DisplayOrder = displayOrder
                    });
                }
            }
        }

        #endregion

        #region Methods

        #region Product list / create / edit / delete

        [HttpPost, ActionName("List")]
        [FormValueRequired("go-to-product-by-sku")]
        public virtual IActionResult GoToSku(ProductSearchModel searchModel)
        {
            var productId = _productService.GetProductBySku(searchModel.GoDirectlyToSku)?.Id
                ?? _productAttributeService.GetProductAttributeCombinationBySku(searchModel.GoDirectlyToSku)?.ProductId;

            if (productId != null)
                return RedirectToAction("Edit", "Product", new { id = productId });

            return List();
        }

        public virtual IActionResult SkuReservedWarning(int productId, string sku)
        {
            string message;

            var productBySku = _productService.GetProductBySku(sku);
            if (productBySku != null)
            {
                if (productBySku.Id == productId)
                    return Json(new { Result = string.Empty });

                message = string.Format("The entered SKU is already reserved for the product '{0}'", productBySku.Name);
                return Json(new { Result = message });
            }

            var combinationBySku = _productAttributeService.GetProductAttributeCombinationBySku(sku);
            if (combinationBySku == null)
                return Json(new { Result = string.Empty });

            message = string.Format("The entered SKU is already reserved for one of combinations of the product '{0}'",
                _productService.GetProductById(combinationBySku.ProductId)?.Name);

            return Json(new { Result = message });
        }

        public virtual IActionResult Index()
        {
            return RedirectToAction("List");
        }

        public virtual IActionResult List()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var model = _productModelFactory.PrepareProductSearchModel(new ProductSearchModel());

            return View(model);
        }

        [HttpPost]
        public virtual IActionResult ProductList(ProductSearchModel searchModel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedDataTablesJson();

            var model = _productModelFactory.PrepareProductListModel(searchModel);

            return Json(model);
        }

        public virtual IActionResult Edit(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var product = _productService.GetProductById(id);
            if (product == null || product.Deleted)
                return RedirectToAction("List");

            var vendor = _workContextService.CurrentVendor;

            if (vendor != null && product.VendorId != vendor.Id)
                return RedirectToAction("List");

            var model = _productModelFactory.PrepareProductModel(null, product);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual IActionResult Edit(ProductModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var product = _productService.GetProductById(model.Id);
            if (product == null || product.Deleted)
                return RedirectToAction("List");

            var vendor = _workContextService.CurrentVendor;
            if (vendor != null && product.VendorId != vendor.Id)
                return RedirectToAction("List");

            if (product.StockQuantity != model.LastStockQuantity)
            {
                _notificationService.ErrorNotification("Quantity has been changed while you were editing the product. Changes haven't been saved. Please ensure that everything is correct and click 'Save' button one more time.");
                return RedirectToAction("Edit", new { id = product.Id });
            }

            if (ModelState.IsValid)
            {
                if (vendor != null)
                    model.VendorId = vendor.Id;

                if (vendor != null && model.ShowOnHomepage != product.ShowOnHomepage)
                    model.ShowOnHomepage = product.ShowOnHomepage;

                //var prevTotalStockQuantity = _productService.GetTotalStockQuantity(product);
                //var prevDownloadId = product.DownloadId;
                //var prevSampleDownloadId = product.SampleDownloadId;
                //var previousStockQuantity = product.StockQuantity;
                //var previousWarehouseId = product.WarehouseId;
                var previousProductTypeId = product.ProductTypeId;

                product = model.ToProductEntity(product);

                product.UpdatedOnUtc = DateTime.UtcNow;
                _productService.UpdateProduct(product);

                if (previousProductTypeId == (int)ProductType.GroupedProduct && product.ProductTypeId == (int)ProductType.SimpleProduct)
                {
                    var storeId = InovatiqaDefaults.StoreId;
                    var vendorId = vendor?.Id ?? 0;

                    var associatedProducts = _productService.GetAssociatedProducts(product.Id, storeId, vendorId);
                    foreach (var associatedProduct in associatedProducts)
                    {
                        associatedProduct.ParentGroupedProductId = 0;
                        _productService.UpdateProduct(associatedProduct);
                    }
                }

                //categories
                SaveCategoryMappings(product, model);

                //manufacturers
                SaveManufacturerMappings(product, model);

                ////back in stock notifications
                //if (product.ManageInventoryMethodId == (int)ManageInventoryMethod.ManageStock &&
                //    product.BackorderModeId == (int)BackorderMode.NoBackorders &&
                //    product.AllowBackInStockSubscriptions &&
                //    _productService.GetTotalStockQuantity(product) > 0 &&
                //    prevTotalStockQuantity <= 0 &&
                //    product.Published &&
                //    !product.Deleted)
                //{
                //    _backInStockSubscriptionService.SendNotificationsToSubscribers(product);
                //}

                ////delete an old "download" file (if deleted or updated)
                //if (prevDownloadId > 0 && prevDownloadId != product.DownloadId)
                //{
                //    var prevDownload = _downloadService.GetDownloadById(prevDownloadId);
                //    if (prevDownload != null)
                //        _downloadService.DeleteDownload(prevDownload);
                //}

                ////delete an old "sample download" file (if deleted or updated)
                //if (prevSampleDownloadId > 0 && prevSampleDownloadId != product.SampleDownloadId)
                //{
                //    var prevSampleDownload = _downloadService.GetDownloadById(prevSampleDownloadId);
                //    if (prevSampleDownload != null)
                //        _downloadService.DeleteDownload(prevSampleDownload);
                //}

                ////quantity change history
                //if (previousWarehouseId != product.WarehouseId)
                //{
                //    //warehouse is changed 
                //    //compose a message
                //    var oldWarehouseMessage = string.Empty;
                //    if (previousWarehouseId > 0)
                //    {
                //        var oldWarehouse = _shippingService.GetWarehouseById(previousWarehouseId);
                //        if (oldWarehouse != null)
                //            oldWarehouseMessage = string.Format(_localizationService.GetResource("Admin.StockQuantityHistory.Messages.EditWarehouse.Old"), oldWarehouse.Name);
                //    }

                //    var newWarehouseMessage = string.Empty;
                //    if (product.WarehouseId > 0)
                //    {
                //        var newWarehouse = _shippingService.GetWarehouseById(product.WarehouseId);
                //        if (newWarehouse != null)
                //            newWarehouseMessage = string.Format(_localizationService.GetResource("Admin.StockQuantityHistory.Messages.EditWarehouse.New"), newWarehouse.Name);
                //    }

                //    var message = string.Format(_localizationService.GetResource("Admin.StockQuantityHistory.Messages.EditWarehouse"), oldWarehouseMessage, newWarehouseMessage);

                //    //record history
                //    _productService.AddStockQuantityHistoryEntry(product, -previousStockQuantity, 0, previousWarehouseId, message);
                //    _productService.AddStockQuantityHistoryEntry(product, product.StockQuantity, product.StockQuantity, product.WarehouseId, message);
                //}
                //else
                //{
                //    _productService.AddStockQuantityHistoryEntry(product, product.StockQuantity - previousStockQuantity, product.StockQuantity,
                //        product.WarehouseId, _localizationService.GetResource("Admin.StockQuantityHistory.Messages.Edit"));
                //}

                //activity log
                _customerActivityService.InsertActivity("EditProduct",
                    string.Format("Edited a product ('{0}')", product.Name));

                _notificationService.SuccessNotification("The product has been updated successfully.");

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = product.Id });
            }

            //prepare model
            model = _productModelFactory.PrepareProductModel(model, product, true);

            //if we got this far, something failed, redisplay form
            return View(model);
        }

        #endregion

        #region Required products



        #endregion

        #region Related products



        #endregion

        #region Cross-sell products



        #endregion

        #region Associated products



        #endregion

        #region Product pictures
        [HttpPost]
        public virtual IActionResult ProductPictureAdd(int pictureId, int displayOrder, string overrideAltAttribute, string overrideTitleAttribute, int productId)
        {
            var product = _productService.GetProductById(productId);
            var picture = _pictureService.GetPictureById(pictureId);
            if (product == null && picture == null)
            {
                return Json(new
                {
                    success = false
                });
            }
            var addPicture = new ProductPictureMapping
            {
                PictureId = pictureId,
                ProductId = productId,
                DisplayOrder = displayOrder
            };
            _productService.UpdateProductPicture(addPicture);
            _pictureService.UpdatePicture(pictureId,
                _pictureService.LoadPictureBinary(picture),
                picture.MimeType,
                picture.SeoFilename,
                overrideAltAttribute,
                overrideTitleAttribute);
            return Json(new
            {
                success = true
            });
        }

        [HttpPost]
        public virtual IActionResult ProductPictureList(ProductPictureSearchModel searchModel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedDataTablesJson();

            var product = _productService.GetProductById(searchModel.ProductId)
                ?? throw new ArgumentException("No product found with the specified id");

            var vendor = _workContextService.CurrentVendor;

            if (vendor != null && product.VendorId != vendor.Id)
                return Content("This is not your product");

            var model = _productModelFactory.PrepareProductPictureListModel(searchModel, product);

            return Json(model);
        }

        [HttpPost]
        public virtual IActionResult ProductPictureUpdate(ProductPictureModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var productPicture = _productService.GetProductPictureById(model.Id)
                ?? throw new ArgumentException("No product picture found with the specified id");

            var vendor = _workContextService.CurrentVendor;


            if (vendor != null)
            {
                var product = _productService.GetProductById(productPicture.ProductId);
                if (product != null && product.VendorId != vendor.Id)
                    return Content("This is not your product");
            }

            var picture = _pictureService.GetPictureById(productPicture.PictureId)
                ?? throw new ArgumentException("No picture found with the specified id");

            _pictureService.UpdatePicture(picture.Id,
                _pictureService.LoadPictureBinary(picture),
                picture.MimeType,
                picture.SeoFilename,
                model.OverrideAltAttribute,
                model.OverrideTitleAttribute);

            productPicture.DisplayOrder = model.DisplayOrder;
            _productService.UpdateProductPicture(productPicture);

            return new NullJsonResult();
        }

        [HttpPost]
        public virtual IActionResult ProductPictureDelete(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var productPicture = _productService.GetProductPictureById(id)
                ?? throw new ArgumentException("No product picture found with the specified id");

            var vendor = _workContextService.CurrentVendor;

            if (vendor != null)
            {
                var product = _productService.GetProductById(productPicture.ProductId);
                if (product != null && product.VendorId != vendor.Id)
                    return Content("This is not your product");
            }

            var pictureId = productPicture.PictureId;
            _productService.DeleteProductPicture(productPicture);

            var picture = _pictureService.GetPictureById(pictureId)
                ?? throw new ArgumentException("No picture found with the specified id");

            _pictureService.DeletePicture(picture);

            return new NullJsonResult();
        }

        #endregion

        #region Product specification attributes

        [HttpPost]
        public virtual IActionResult ProductSpecAttrDelete(AddSpecificationAttributeModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var psa = _specificationAttributeService.GetProductSpecificationAttributeById(model.SpecificationId);
            if (psa == null)
            {
                SaveSelectedPanelName("product-specification-attributes");
                _notificationService.ErrorNotification("No product specification attribute found with the specified id");
                return RedirectToAction("Edit", new { id = model.ProductId });
            }

            var vendor = _workContextService.CurrentVendor;
            if (vendor != null && _productService.GetProductById(psa.ProductId).VendorId != vendor.Id)
            {
                _notificationService.ErrorNotification("This is not your product");
                return RedirectToAction("List", new { id = model.ProductId });
            }

            _specificationAttributeService.DeleteProductSpecificationAttribute(psa);

            SaveSelectedPanelName("product-specification-attributes");

            return RedirectToAction("Edit", new { id = psa.ProductId });
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual IActionResult ProductSpecificationAttributeAdd(AddSpecificationAttributeModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var product = _productService.GetProductById(model.ProductId);
            if (product == null)
            {
                _notificationService.ErrorNotification("No product found with the specified id");
                return RedirectToAction("List");
            }

            var vendor = _workContextService.CurrentVendor;
            if (vendor != null && product.VendorId != vendor.Id)
            {
                return RedirectToAction("List");
            }

            if (model.AttributeTypeId != (int)SpecificationAttributeType.Option)
                model.AllowFiltering = false;

            if (model.AttributeTypeId == (int)SpecificationAttributeType.Option)
                model.ValueRaw = null;

            if (model.AttributeTypeId == (int)SpecificationAttributeType.CustomText
                || model.AttributeTypeId == (int)SpecificationAttributeType.Hyperlink)
                model.ValueRaw = model.Value;

            var psa = model.ToProductSpecificationAttributeMappingEntity<ProductSpecificationAttributeMapping>();
            psa.CustomValue = model.ValueRaw;
            _specificationAttributeService.InsertProductSpecificationAttribute(psa);

            switch (psa.AttributeTypeId)
            {
                case (int)SpecificationAttributeType.CustomText:
                    break;
                case (int)SpecificationAttributeType.CustomHtmlText:
                    break;
                case (int)SpecificationAttributeType.Option:
                    break;
                case (int)SpecificationAttributeType.Hyperlink:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (continueEditing)
                return RedirectToAction("ProductSpecAttributeAddOrEdit",
                    new { productId = psa.ProductId, specificationId = psa.Id });

            //select an appropriate panel
            SaveSelectedPanelName("product-specification-attributes");
            return RedirectToAction("Edit", new { id = model.ProductId });
        }

        [HttpPost]
        public virtual IActionResult ProductSpecAttrList(ProductSpecificationAttributeSearchModel searchModel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedDataTablesJson();

            var product = _productService.GetProductById(searchModel.ProductId)
                ?? throw new ArgumentException("No product found with the specified id");

            var vendor = _workContextService.CurrentVendor;

            if (vendor != null && product.VendorId != vendor.Id)
                return Content("This is not your product");

            var model = _productModelFactory.PrepareProductSpecificationAttributeListModel(searchModel, product);

            return Json(model);
        }

        #endregion

        #region Product tags



        #endregion

        #region Purchased with order

        [HttpPost]
        public virtual IActionResult PurchasedWithOrders(ProductOrderSearchModel searchModel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedDataTablesJson();

            var product = _productService.GetProductById(searchModel.ProductId)
                ?? throw new ArgumentException("No product found with the specified id");

            var vendor = _workContextService.CurrentVendor;

            if (vendor != null && product.VendorId != vendor.Id)
                return Content("This is not your product");

            var model = _productModelFactory.PrepareProductOrderListModel(searchModel, product);

            return Json(model);
        }

        #endregion

        #region Export / Import



        #endregion

        #region Tier prices

        [HttpPost]
        public virtual IActionResult TierPriceList(TierPriceSearchModel searchModel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedDataTablesJson();

            var vendor = _workContextService.CurrentVendor;
            var EntityId = 0;
            if(searchModel.EntityName == "Product")
            {
                var product = _productService.GetProductById(searchModel.EntityId)
                    ?? throw new ArgumentException("No product found with the specified id");
                if (vendor != null && product.VendorId != vendor.Id)
                    return Content("This is not your product");
                EntityId = product.Id;
            }
            else if (searchModel.EntityName == "Category")
            {
                var category = _categoryService.GetCategoryById(searchModel.EntityId)
                    ?? throw new ArgumentException("No Category found with the specified id");
                EntityId = category.Id;
            }
            else if(searchModel.EntityName == "ALL")
            {
                EntityId = 0;
            }
            else
            {
                return Json("");
            }


            var model = _productModelFactory.PrepareTierPriceListModel(searchModel, EntityId, searchModel.EntityName);

            return Json(model);
        }
        public virtual IActionResult SearchCustomer(string customerName)
        {
            var customer = _customerRepository.Query().Where(c => c.Username.Contains(customerName)).ToList();
            return Json(customer);
        }
        public virtual IActionResult TierPriceCreatePopup(int EntityId, string EntityName)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();
            var product = new Product();
            var category = new Category();
            if (EntityName == "Product")
            {
                product = _productService.GetProductById(EntityId)
                    ?? throw new ArgumentException("No product found with the specified id");
            }
            else if (EntityName == "Category")
            {
                category = _categoryService.GetCategoryById(EntityId)
                    ?? throw new ArgumentException("No product found with the specified id");
            }
            

            var model = _productModelFactory.PrepareTierPriceModel(new TierPriceModel(), EntityId, EntityName, null);

            if(EntityName == "Product")
            {
                var customerList = _customerService.GetAllCustomers().ToList();
                model.AvailabelCustomer =  customerList.Where(ac => ac.Email != null).Select(ac => new SelectListItem { Text = ac.Email.ToString(), Value = ac.Id.ToString() }).ToList();
            }


            return View(model);
        }

        [HttpPost]
        [FormValueRequired("save")]
        public virtual IActionResult TierPriceCreatePopup(TierPriceModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();
            var product = new Product();
            var category = new Category();
            if(model.EntityName == "Product")
            {
                product = _productService.GetProductById(model.EntityId)
                    ?? throw new ArgumentException("No product found with the specified id");
            }
            else if(model.EntityName == "Category")
            {
                category = _categoryService.GetCategoryById(model.EntityId)
                    ?? throw new ArgumentException("No product found with the specified id");
            }

            var vendor = _workContextService.CurrentVendor;

            if (vendor != null && product.VendorId != vendor.Id)
                return RedirectToAction("List", "Product");

            if (ModelState.IsValid)
            {
                var tierpriceold = model.ToTierPriceEntity<TierPrice>();
                var tierPrice = new EntityTierPrice ();
                tierPrice.EntityId = model.EntityId;
                tierPrice.EntityName = model.EntityName;
                tierPrice.CustomerId = model.CustomerId > 0 ? model.CustomerId : (int?)null;
                tierPrice.Rate = model.Rate;
                tierPrice.StartDateTimeUtc = model.StartDateTimeUtc;
                tierPrice.EndDateTimeUtc = model.EndDateTimeUtc;

                _productService.InsertTierPrice(tierPrice);
                if(tierPrice.EntityName == "Product")
                {
                    _productService.UpdateHasTierPricesProperty(product);
                }

                ViewBag.RefreshPage = true;

                return View(model);
            }

            //model = _productModelFactory.PrepareTierPriceModel(model, product, null, true);

            return View(model);
        }

        public virtual IActionResult TierPriceEditPopup(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var tierPrice = _productService.GetTierPriceById(id);
            if (tierPrice == null)
                return RedirectToAction("List", "Product");
            if(tierPrice.EntityName == "Product")
            {
                var product = _productService.GetProductById(Convert.ToInt32(tierPrice.EntityId))
                    ?? throw new ArgumentException("No product found with the specified id");
                var vendor = _workContextService.CurrentVendor;

                if (vendor != null && product.VendorId != vendor.Id)
                    return RedirectToAction("List", "Product");
            }
            else if(tierPrice.EntityName == "Category")
            {
                var category = _categoryService.GetCategoryById(Convert.ToInt32(tierPrice.EntityId))
                    ?? throw new ArgumentException("No product found with the specified id");
            }

            var model = _productModelFactory.PrepareTierPriceModel(null, Convert.ToInt32(tierPrice.EntityId), tierPrice.EntityName, tierPrice);

            var customerList = _customerService.GetAllCustomers().ToList();
            model.AvailabelCustomer = customerList.Where(ac => ac.Email != null).Select(ac => new SelectListItem { Text = ac.Email.ToString(), Value = ac.Id.ToString() }).ToList();


            return View(model);
        }
        //tier price change
        [HttpPost]
        public virtual IActionResult TierPriceEditPopup(TierPriceModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var tierPrice = _productService.GetTierPriceById(model.Id);
            if (tierPrice == null && model.EntityName == "Product")
                return RedirectToAction("List", "Product");
            else if (tierPrice == null && model.EntityName == "Category")
                return RedirectToAction("List", "Category");
            if (tierPrice.EntityName == "Product")
            {
                var product = _productService.GetProductById(Convert.ToInt32(tierPrice.EntityId))
                    ?? throw new ArgumentException("No product found with the specified id");

                var vendor = _workContextService.CurrentVendor;

                if (vendor != null && product.VendorId != vendor.Id)
                    return RedirectToAction("List", "Product");
            }
            else if(tierPrice.EntityName == "Category")
            {
                var category = _categoryService.GetCategoryById(Convert.ToInt32(tierPrice.EntityId))
                    ?? throw new ArgumentException("No product found with the specified id");
            }

            if (ModelState.IsValid)
            {
                //tierPrice = model.ToTierPriceEntity(tierPrice);
                //tierPrice = new EntityTierPrice();
                tierPrice.EntityId = model.EntityId;
                tierPrice.EntityName = model.EntityName;
                tierPrice.Rate = model.Rate;
                tierPrice.StartDateTimeUtc = model.StartDateTimeUtc;
                tierPrice.EndDateTimeUtc = model.EndDateTimeUtc;
                tierPrice.CustomerId = model.CustomerId > 0 ? model.CustomerId : (int?)null;
                _productService.UpdateTierPrice(tierPrice);

                ViewBag.RefreshPage = true;

                return View(model);
            }

            //model = _productModelFactory.PrepareTierPriceModel(model, product, tierPrice, true);

            return View(model);
        }
        //tier price change
        [HttpPost]
        public virtual IActionResult TierPriceDelete(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var tierPrice = _productService.GetTierPriceById(id)
                ?? throw new ArgumentException("No tier price found with the specified id");

            var product = new Product();
            if (tierPrice.EntityName == "Product")
            {
                product = _productService.GetProductById(Convert.ToInt32(tierPrice.EntityId))
                    ?? throw new ArgumentException("No product found with the specified id");

                var vendor = _workContextService.CurrentVendor;

                if (vendor != null && product.VendorId != vendor.Id)
                    return Content("This is not your product");
            }
            else if(tierPrice.EntityName == "Category")
            {
                var category = _categoryService.GetCategoryById(Convert.ToInt32(tierPrice.EntityId))
                    ?? throw new ArgumentException("No product found with the specified id");
            }

            _productService.DeleteTierPrice(tierPrice);
            if (tierPrice.EntityName == "Product")
            {
                _productService.UpdateHasTierPricesProperty(product);
            }

            return new NullJsonResult();
        }

        #endregion

        #region Product attributes

        public virtual IActionResult ProductAttributeMappingEdit(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var productAttributeMapping = _productAttributeService.GetProductAttributeMappingById(id)
                ?? throw new ArgumentException("No product attribute mapping found with the specified id");


            var product = _productService.GetProductById(productAttributeMapping.ProductId)
                ?? throw new ArgumentException("No product found with the specified id");

            var vendor = _workContextService.CurrentVendor;
            if (vendor != null && product.VendorId != vendor.Id)
            {
                _notificationService.ErrorNotification("This is not your product");
                return RedirectToAction("List");
            }

            var model = _productModelFactory.PrepareProductAttributeMappingModel(null, product, productAttributeMapping);

            return View(model);
        }


        #endregion

        #region Product attribute combinations

        [HttpPost]
        public virtual IActionResult ProductAttributeCombinationList(ProductAttributeCombinationSearchModel searchModel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedDataTablesJson();

            var product = _productService.GetProductById(searchModel.ProductId)
                ?? throw new ArgumentException("No product found with the specified id");

            var vendor = _workContextService.CurrentVendor;
            if (vendor != null && product.VendorId != vendor.Id)
                return Content("This is not your product");

            var model = _productModelFactory.PrepareProductAttributeCombinationListModel(searchModel, product);

            return Json(model);
        }

        [HttpPost]
        public virtual IActionResult ProductAttributeCombinationDelete(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var combination = _productAttributeService.GetProductAttributeCombinationById(id)
                ?? throw new ArgumentException("No product attribute combination found with the specified id");

            var product = _productService.GetProductById(combination.ProductId)
                ?? throw new ArgumentException("No product found with the specified id");

            var vendor = _workContextService.CurrentVendor;

            if (vendor != null && product.VendorId != vendor.Id)
                return Content("This is not your product");

            _productAttributeService.DeleteProductAttributeCombination(combination);

            return new NullJsonResult();
        }

        #endregion

        #region Product editor settings



        #endregion

        #region Stock quantity history



        #endregion

        #endregion
    }
}