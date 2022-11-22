using Inovatiqa.Core;
using Inovatiqa.Database.Models;
using Inovatiqa.Services.Catalog.Interfaces;
using Inovatiqa.Services.Customers.Interfaces;
using Inovatiqa.Services.Messages.Interfaces;
using Inovatiqa.Services.Security;
using Inovatiqa.Services.Security.Interfaces;
using Inovatiqa.Services.WorkContext.Interfaces;
using Inovatiqa.Web.Areas.Admin.Factories.Interfaces;
using Inovatiqa.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Inovatiqa.Web.Areas.Admin.Models.Customers;
using Inovatiqa.Web.Controllers;
using Inovatiqa.Web.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using System;

namespace Inovatiqa.Web.Areas.Admin.Controllers
{
    public partial class CustomerRoleController : BaseAdminController
    {
        #region Fields

        private readonly ICustomerActivityService _customerActivityService;
        private readonly ICustomerRoleModelFactory _customerRoleModelFactory;
        private readonly ICustomerService _customerService;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly IProductService _productService;
        private readonly IWorkContextService _workContextService;

        #endregion

        #region Ctor

        public CustomerRoleController(ICustomerActivityService customerActivityService,
            ICustomerRoleModelFactory customerRoleModelFactory,
            ICustomerService customerService,
            INotificationService notificationService,
            IPermissionService permissionService,
            IProductService productService,
            IWorkContextService workContextService,
             IRazorViewEngine viewEngine) : base(viewEngine)
        {
            _customerActivityService = customerActivityService;
            _customerRoleModelFactory = customerRoleModelFactory;
            _customerService = customerService;
            _notificationService = notificationService;
            _permissionService = permissionService;
            _productService = productService;
            _workContextService = workContextService;
        }

        #endregion

        #region Methods

        public virtual IActionResult Index()
        {
            return RedirectToAction("List");
        }

        public virtual IActionResult List()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            var model = _customerRoleModelFactory.PrepareCustomerRoleSearchModel(new CustomerRoleSearchModel());

            return View(model);
        }

        [HttpPost]
        public virtual IActionResult List(CustomerRoleSearchModel searchModel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedDataTablesJson();

            var model = _customerRoleModelFactory.PrepareCustomerRoleListModel(searchModel);

            return Json(model);
        }

        public virtual IActionResult Create()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers) || !_permissionService.Authorize(StandardPermissionProvider.ManageAcl))
                return AccessDeniedView();

            //prepare model
            var model = _customerRoleModelFactory.PrepareCustomerRoleModel(new CustomerRoleModel(), null);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual IActionResult Create(CustomerRoleModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers) || !_permissionService.Authorize(StandardPermissionProvider.ManageAcl))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var customerRole = model.ToCustomerRoleEntity<CustomerRole>();
                _customerService.InsertCustomerRole(customerRole);

                _customerActivityService.InsertActivity("AddNewCustomerRole",
                    string.Format("Added a new customer role ('{0}')", customerRole.Name));

                _notificationService.SuccessNotification("The new customer role has been added successfully.");

                return continueEditing ? RedirectToAction("Edit", new { id = customerRole.Id }) : RedirectToAction("List");
            }

            model = _customerRoleModelFactory.PrepareCustomerRoleModel(model, null, true);

            return View(model);
        }

        public virtual IActionResult Edit(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers) || !_permissionService.Authorize(StandardPermissionProvider.ManageAcl))
                return AccessDeniedView();

            var customerRole = _customerService.GetCustomerRoleById(id);
            if (customerRole == null)
                return RedirectToAction("List");

            var model = _customerRoleModelFactory.PrepareCustomerRoleModel(null, customerRole);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual IActionResult Edit(CustomerRoleModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers) || !_permissionService.Authorize(StandardPermissionProvider.ManageAcl))
                return AccessDeniedView();

            var customerRole = _customerService.GetCustomerRoleById(model.Id);
            if (customerRole == null)
                return RedirectToAction("List");

            try
            {
                if (ModelState.IsValid)
                {
                    if (customerRole.IsSystemRole && !model.Active)
                        throw new InovatiqaException("System customer roles can't be disabled.");

                    if (customerRole.IsSystemRole && !customerRole.SystemName.Equals(model.SystemName, StringComparison.InvariantCultureIgnoreCase))
                        throw new InovatiqaException("The system name of system customer roles can't be edited.");

                    if (InovatiqaDefaults.RegisteredRoleName.Equals(customerRole.SystemName, StringComparison.InvariantCultureIgnoreCase) &&
                        model.PurchasedWithProductId > 0)
                        throw new InovatiqaException("You cannot specify 'Purchased with product' value for 'Registered' customer role");

                    customerRole = model.ToCustomerRoleEntity<CustomerRole>();
                    _customerService.UpdateCustomerRole(customerRole);

                    _customerActivityService.InsertActivity("EditCustomerRole",
                        string.Format("Edited a customer role ('{0}')", customerRole.Name));

                    _notificationService.SuccessNotification("The customer role has been updated successfully.");

                    return continueEditing ? RedirectToAction("Edit", new { id = customerRole.Id }) : RedirectToAction("List");
                }

                model = _customerRoleModelFactory.PrepareCustomerRoleModel(model, customerRole, true);

                return View(model);
            }
            catch (Exception exc)
            {
                _notificationService.ErrorNotification(exc);
                return RedirectToAction("Edit", new { id = customerRole.Id });
            }
        }

        [HttpPost]
        public virtual IActionResult Delete(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers) || !_permissionService.Authorize(StandardPermissionProvider.ManageAcl))
                return AccessDeniedView();

            var customerRole = _customerService.GetCustomerRoleById(id);
            if (customerRole == null)
                return RedirectToAction("List");

            try
            {
                _customerService.DeleteCustomerRole(customerRole);

                _customerActivityService.InsertActivity("DeleteCustomerRole",
                    string.Format("Deleted a customer role ('{0}')", customerRole.Name));

                _notificationService.SuccessNotification("The customer role has been deleted successfully.");

                return RedirectToAction("List");
            }
            catch (Exception exc)
            {
                _notificationService.ErrorNotification(exc.Message);
                return RedirectToAction("Edit", new { id = customerRole.Id });
            }
        }

        public virtual IActionResult AssociateProductToCustomerRolePopup()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers) || !_permissionService.Authorize(StandardPermissionProvider.ManageAcl))
                return AccessDeniedView();

            var model = _customerRoleModelFactory.PrepareCustomerRoleProductSearchModel(new CustomerRoleProductSearchModel());

            return View(model);
        }

        [HttpPost]
        public virtual IActionResult AssociateProductToCustomerRolePopupList(CustomerRoleProductSearchModel searchModel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers) || !_permissionService.Authorize(StandardPermissionProvider.ManageAcl))
                return AccessDeniedDataTablesJson();

            var model = _customerRoleModelFactory.PrepareCustomerRoleProductListModel(searchModel);

            return Json(model);
        }

        [HttpPost]
        [FormValueRequired("save")]
        public virtual IActionResult AssociateProductToCustomerRolePopup([Bind(Prefix = nameof(AddProductToCustomerRoleModel))] AddProductToCustomerRoleModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers) || !_permissionService.Authorize(StandardPermissionProvider.ManageAcl))
                return AccessDeniedView();

            var associatedProduct = _productService.GetProductById(model.AssociatedToProductId);
            if (associatedProduct == null)
                return Content("Cannot load a product");

            var vendor = _workContextService.CurrentVendor;


            if (vendor != null && associatedProduct.VendorId != vendor.Id)
                return Content("This is not your product");

            ViewBag.RefreshPage = true;
            ViewBag.productId = associatedProduct.Id;
            ViewBag.productName = associatedProduct.Name;

            return View(new CustomerRoleProductSearchModel());
        }

        #endregion
    }
}