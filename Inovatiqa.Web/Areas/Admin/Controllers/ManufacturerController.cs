using System;
using Inovatiqa.Services.Catalog.Interfaces;
using Inovatiqa.Services.Customers.Interfaces;
using Inovatiqa.Services.Discounts.Interfaces;
using Inovatiqa.Services.Media.Interfaces;
using Inovatiqa.Services.Security;
using Inovatiqa.Services.Security.Interfaces;
using Inovatiqa.Services.Seo.Interfaces;
using Inovatiqa.Services.WorkContext.Interfaces;
using Inovatiqa.Web.Areas.Admin.Models.Catalog;
using Microsoft.AspNetCore.Mvc;
using Inovatiqa.Web.Areas.Admin.Factories.Interfaces;
using Microsoft.AspNetCore.Mvc.Razor;
using Inovatiqa.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Inovatiqa.Web.Mvc.Filters;
using Inovatiqa.Services.Messages.Interfaces;
using Inovatiqa.Web.Mvc;

namespace Inovatiqa.Web.Areas.Admin.Controllers
{
    public partial class ManufacturerController : BaseAdminController
    {
        #region Fields

        private readonly IAclService _aclService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ICustomerService _customerService;
        private readonly IDiscountService _discountService;
        private readonly IManufacturerModelFactory _manufacturerModelFactory;
        private readonly IManufacturerService _manufacturerService;
        private readonly IPermissionService _permissionService;
        private readonly IPictureService _pictureService;
        private readonly IProductService _productService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IWorkContextService _workContextService;
        private readonly INotificationService _notificationService;

        #endregion

        #region Ctor

        public ManufacturerController(IAclService aclService,
            ICustomerActivityService customerActivityService,
            ICustomerService customerService,
            IDiscountService discountService,
            IManufacturerModelFactory manufacturerModelFactory,
            IManufacturerService manufacturerService,
            IPermissionService permissionService,
            IPictureService pictureService,
            IProductService productService,
            IUrlRecordService urlRecordService,
            IWorkContextService workContextService,
            INotificationService notificationService,
             IRazorViewEngine viewEngine) : base(viewEngine)
        {
            _aclService = aclService;
            _customerActivityService = customerActivityService;
            _customerService = customerService;
            _discountService = discountService;
            _manufacturerModelFactory = manufacturerModelFactory;
            _manufacturerService = manufacturerService;
            _permissionService = permissionService;
            _pictureService = pictureService;
            _productService = productService;
            _urlRecordService = urlRecordService;
            _workContextService = workContextService;
            _notificationService = notificationService;
        }

        #endregion

        #region Utilities


        #endregion

        #region List

        public virtual IActionResult Index()
        {
            return RedirectToAction("List");
        }

        public virtual IActionResult List()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageManufacturers))
                return AccessDeniedView();

            var model = _manufacturerModelFactory.PrepareManufacturerSearchModel(new ManufacturerSearchModel());

            return View(model);
        }

        [HttpPost]
        public virtual IActionResult List(ManufacturerSearchModel searchModel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageManufacturers))
                return AccessDeniedDataTablesJson();

            var model = _manufacturerModelFactory.PrepareManufacturerListModel(searchModel);

            return Json(model);
        }

        #endregion

        #region Create / Edit / Delete

        [HttpPost]
        public virtual IActionResult ProductUpdate(ManufacturerProductModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageManufacturers))
                return AccessDeniedView();

            var productManufacturer = _manufacturerService.GetProductManufacturerById(model.Id)
                ?? throw new ArgumentException("No product manufacturer mapping found with the specified id");

            productManufacturer = model.ToManufacturerEntity(productManufacturer);
            _manufacturerService.UpdateProductManufacturer(productManufacturer);

            return new NullJsonResult();
        }

        public virtual IActionResult Edit(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageManufacturers))
                return AccessDeniedView();

            var manufacturer = _manufacturerService.GetManufacturerById(id);
            if (manufacturer == null || manufacturer.Deleted)
                return RedirectToAction("List");

            var model = _manufacturerModelFactory.PrepareManufacturerModel(null, manufacturer);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual IActionResult Edit(ManufacturerModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageManufacturers))
                return AccessDeniedView();

            var manufacturer = _manufacturerService.GetManufacturerById(model.Id);
            if (manufacturer == null || manufacturer.Deleted)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                var prevPictureId = manufacturer.PictureId;
                manufacturer = model.ToManufacturerEntity(manufacturer);
                manufacturer.UpdatedOnUtc = DateTime.UtcNow;
                _manufacturerService.UpdateManufacturer(manufacturer);

                _urlRecordService.SaveManufacturerSlug(manufacturer, model.SeName, 0);

                _manufacturerService.UpdateManufacturer(manufacturer);

                if (prevPictureId > 0 && prevPictureId != manufacturer.PictureId)
                {
                    var prevPicture = _pictureService.GetPictureById(prevPictureId);
                    if (prevPicture != null)
                        _pictureService.DeletePicture(prevPicture);
                }

                _customerActivityService.InsertActivity("EditManufacturer",
                    string.Format("Edited a manufacturer ('{0}')", manufacturer.Name));

                _notificationService.SuccessNotification("The manufacturer has been updated successfully.");

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = manufacturer.Id });
            }

            model = _manufacturerModelFactory.PrepareManufacturerModel(model, manufacturer, true);

            return View(model);
        }

        #endregion


        #region Products

        [HttpPost]
        public virtual IActionResult ProductList(ManufacturerProductSearchModel searchModel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageManufacturers))
                return AccessDeniedDataTablesJson();

            var manufacturer = _manufacturerService.GetManufacturerById(searchModel.ManufacturerId)
                ?? throw new ArgumentException("No manufacturer found with the specified id");

            var model = _manufacturerModelFactory.PrepareManufacturerProductListModel(searchModel, manufacturer);

            return Json(model);
        }

        #endregion
    }
}