using System;
using System.Linq;
using Inovatiqa.Core;
using Inovatiqa.Database.Models;
using Inovatiqa.Services.Common.Interfaces;
using Inovatiqa.Services.Customers.Interfaces;
using Inovatiqa.Services.Media.Interfaces;
using Inovatiqa.Services.Messages.Interfaces;
using Inovatiqa.Services.Security;
using Inovatiqa.Services.Security.Interfaces;
using Inovatiqa.Services.Seo.Interfaces;
using Inovatiqa.Services.Vendors;
using Inovatiqa.Services.Vendors.Interfaces;
using Inovatiqa.Web.Areas.Admin.Factories.Interfaces;
using Inovatiqa.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Inovatiqa.Web.Areas.Admin.Models.Vendors;
using Inovatiqa.Web.Controllers;
using Inovatiqa.Web.Mvc.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Primitives;

namespace Inovatiqa.Web.Areas.Admin.Controllers
{
    public partial class VendorController : BaseAdminController
    {
        #region Fields

        private readonly IAddressService _addressService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ICustomerService _customerService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly IPictureService _pictureService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IVendorAttributeParserService _vendorAttributeParserService;
        private readonly IVendorAttributeService _vendorAttributeService;
        private readonly IVendorModelFactory _vendorModelFactory;
        private readonly IVendorService _vendorService;

        #endregion

        #region Ctor

        public VendorController(IAddressService addressService,
            ICustomerActivityService customerActivityService,
            ICustomerService customerService,
            IGenericAttributeService genericAttributeService,
            INotificationService notificationService,
            IPermissionService permissionService,
            IPictureService pictureService,
            IUrlRecordService urlRecordService,
            IVendorAttributeParserService vendorAttributeParserService,
            IVendorAttributeService vendorAttributeService,
            IVendorModelFactory vendorModelFactory,
            IVendorService vendorService,
             IRazorViewEngine viewEngine) : base(viewEngine)
        {
            _addressService = addressService;
            _customerActivityService = customerActivityService;
            _customerService = customerService;
            _genericAttributeService = genericAttributeService;
            _notificationService = notificationService;
            _permissionService = permissionService;
            _pictureService = pictureService;
            _urlRecordService = urlRecordService;
            _vendorAttributeParserService = vendorAttributeParserService;
            _vendorAttributeService = vendorAttributeService;
            _vendorModelFactory = vendorModelFactory;
            _vendorService = vendorService;
        }

        #endregion

        #region Utilities

        protected virtual string ParseVendorAttributes(IFormCollection form)
        {
            if (form == null)
                throw new ArgumentNullException(nameof(form));

            var attributesXml = string.Empty;
            var vendorAttributes = _vendorAttributeService.GetAllVendorAttributes();
            foreach (var attribute in vendorAttributes)
            {
                var controlId = $"{InovatiqaDefaults.VendorAttributePrefix}{attribute.Id}";
                StringValues ctrlAttributes;
                switch (attribute.AttributeControlTypeId)
                {
                    case (int)AttributeControlType.DropdownList:
                    case (int)AttributeControlType.RadioList:
                        ctrlAttributes = form[controlId];
                        if (!StringValues.IsNullOrEmpty(ctrlAttributes))
                        {
                            var selectedAttributeId = int.Parse(ctrlAttributes);
                            if (selectedAttributeId > 0)
                                attributesXml = _vendorAttributeParserService.AddVendorAttribute(attributesXml,
                                    attribute, selectedAttributeId.ToString());
                        }

                        break;
                    case (int)AttributeControlType.Checkboxes:
                        var cblAttributes = form[controlId];
                        if (!StringValues.IsNullOrEmpty(cblAttributes))
                        {
                            foreach (var item in cblAttributes.ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                            {
                                var selectedAttributeId = int.Parse(item);
                                if (selectedAttributeId > 0)
                                    attributesXml = _vendorAttributeParserService.AddVendorAttribute(attributesXml,
                                        attribute, selectedAttributeId.ToString());
                            }
                        }

                        break;
                    case (int)AttributeControlType.ReadonlyCheckboxes:
                        var attributeValues = _vendorAttributeService.GetVendorAttributeValues(attribute.Id);
                        foreach (var selectedAttributeId in attributeValues
                            .Where(v => v.IsPreSelected)
                            .Select(v => v.Id)
                            .ToList())
                        {
                            attributesXml = _vendorAttributeParserService.AddVendorAttribute(attributesXml,
                                attribute, selectedAttributeId.ToString());
                        }

                        break;
                    case (int)AttributeControlType.TextBox:
                    case (int)AttributeControlType.MultilineTextbox:
                        ctrlAttributes = form[controlId];
                        if (!StringValues.IsNullOrEmpty(ctrlAttributes))
                        {
                            var enteredText = ctrlAttributes.ToString().Trim();
                            attributesXml = _vendorAttributeParserService.AddVendorAttribute(attributesXml,
                                attribute, enteredText);
                        }

                        break;
                    case (int)AttributeControlType.Datepicker:
                    case (int)AttributeControlType.ColorSquares:
                    case (int)AttributeControlType.ImageSquares:
                    case (int)AttributeControlType.FileUpload:
                    default:
                        break;
                }
            }

            return attributesXml;
        }

        #endregion

        #region Vendors

        public virtual IActionResult Index()
        {
            return RedirectToAction("List");
        }

        public virtual IActionResult List()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageVendors))
                return AccessDeniedView();

            var model = _vendorModelFactory.PrepareVendorSearchModel(new VendorSearchModel());

            return View(model);
        }

        [HttpPost]
        public virtual IActionResult List(VendorSearchModel searchModel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageVendors))
                return AccessDeniedDataTablesJson();

            var model = _vendorModelFactory.PrepareVendorListModel(searchModel);

            return Json(model);
        }

        public virtual IActionResult Create()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageVendors))
                return AccessDeniedView();

            var model = _vendorModelFactory.PrepareVendorModel(new VendorModel(), null);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [FormValueRequired("save", "save-continue")]
        public virtual IActionResult Create(VendorModel model, bool continueEditing, IFormCollection form)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageVendors))
                return AccessDeniedView();

            var vendorAttributesXml = ParseVendorAttributes(form);
            _vendorAttributeParserService.GetAttributeWarnings(vendorAttributesXml).ToList()
                .ForEach(warning => ModelState.AddModelError(string.Empty, warning));

            if (ModelState.IsValid)
            {
                var vendor = model.ToVendorEntity<Vendor>();
                _vendorService.InsertVendor(vendor);

                _customerActivityService.InsertActivity("AddNewVendor",
                    string.Format("Added a new vendor (ID = {0})", vendor.Id), vendor.Id);

                model.SeName = model.SeName;
                _urlRecordService.SaveVendorSlug(vendor, model.SeName, 0);

                var address = model.Address.ToAddressEntity<Address>();
                address.CreatedOnUtc = DateTime.UtcNow;

                if (address.CountryId == 0)
                    address.CountryId = null;
                if (address.StateProvinceId == 0)
                    address.StateProvinceId = null;
                _addressService.InsertAddress(address);
                vendor.AddressId = address.Id;
                _vendorService.UpdateVendor(vendor);

                _genericAttributeService.SaveAttribute(vendor.GetType().Name, vendor.Id, InovatiqaDefaults.VendorAttributes, vendorAttributesXml);

                _notificationService.SuccessNotification("The new vendor has been added successfully.");

                if (!continueEditing)
                    return RedirectToAction("List");
                
                return RedirectToAction("Edit", new { id = vendor.Id });
            }

            model = _vendorModelFactory.PrepareVendorModel(model, null, true);

            return View(model);
        }

        public virtual IActionResult Edit(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageVendors))
                return AccessDeniedView();

            var vendor = _vendorService.GetVendorById(id);
            if (vendor == null || vendor.Deleted)
                return RedirectToAction("List");

            var model = _vendorModelFactory.PrepareVendorModel(null, vendor);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual IActionResult Edit(VendorModel model, bool continueEditing, IFormCollection form)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageVendors))
                return AccessDeniedView();

            var vendor = _vendorService.GetVendorById(model.Id);
            if (vendor == null || vendor.Deleted)
                return RedirectToAction("List");

            var vendorAttributesXml = ParseVendorAttributes(form);
            _vendorAttributeParserService.GetAttributeWarnings(vendorAttributesXml).ToList()
                .ForEach(warning => ModelState.AddModelError(string.Empty, warning));

            if (ModelState.IsValid)
            {
                var prevPictureId = vendor.PictureId;
                vendor = model.ToVendorEntity<Vendor>();
                _vendorService.UpdateVendor(vendor);

                _genericAttributeService.SaveAttribute(vendor.GetType().Name, vendor.Id, InovatiqaDefaults.VendorAttributes, vendorAttributesXml);

                _customerActivityService.InsertActivity("EditVendor",
                    string.Format("Edited a vendor (ID = {0})", vendor.Id), vendor.Id);

                _urlRecordService.SaveVendorSlug(vendor, model.SeName, 0);

                var address = _addressService.GetAddressById(vendor.AddressId);
                if (address == null)
                {
                    address = model.Address.ToAddressEntity<Address>();
                    address.CreatedOnUtc = DateTime.UtcNow;

                    if (address.CountryId == 0)
                        address.CountryId = null;
                    if (address.StateProvinceId == 0)
                        address.StateProvinceId = null;

                    _addressService.InsertAddress(address);
                    vendor.AddressId = address.Id;
                    _vendorService.UpdateVendor(vendor);
                }
                else
                {
                    address = model.Address.ToAddressEntity(address);

                    if (address.CountryId == 0)
                        address.CountryId = null;
                    if (address.StateProvinceId == 0)
                        address.StateProvinceId = null;

                    _addressService.UpdateAddress(address);
                }

                if (prevPictureId > 0 && prevPictureId != vendor.PictureId)
                {
                    var prevPicture = _pictureService.GetPictureById(prevPictureId);
                    if (prevPicture != null)
                        _pictureService.DeletePicture(prevPicture);
                }

                _notificationService.SuccessNotification("The vendor has been updated successfully.");

                if (!continueEditing)
                    return RedirectToAction("List");
                
                return RedirectToAction("Edit", new { id = vendor.Id });
            }

            model = _vendorModelFactory.PrepareVendorModel(model, vendor, true);

            return View(model);
        }

        [HttpPost]
        public virtual IActionResult Delete(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageVendors))
                return AccessDeniedView();

            var vendor = _vendorService.GetVendorById(id);
            if (vendor == null)
                return RedirectToAction("List");

            var associatedCustomers = _customerService.GetAllCustomers(vendorId: vendor.Id);
            foreach (var customer in associatedCustomers)
            {
                customer.VendorId = 0;
                _customerService.UpdateCustomer(customer);
            }

            _vendorService.DeleteVendor(vendor);

            _customerActivityService.InsertActivity("DeleteVendor",
                string.Format("Deleted a vendor (ID = {0})", vendor.Id), vendor.Id);

            _notificationService.SuccessNotification("The vendor has been deleted successfully.");

            return RedirectToAction("List");
        }

        #endregion
    }
}