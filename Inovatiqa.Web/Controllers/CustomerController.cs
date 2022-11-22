using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Inovatiqa.Core;
using Inovatiqa.Database.Interfaces;
using Inovatiqa.Database.Models;
using Inovatiqa.Services.Authentication.Interfaces;
using Inovatiqa.Services.Catalog.Interfaces;
using Inovatiqa.Services.Common.Interfaces;
using Inovatiqa.Services.Customers;
using Inovatiqa.Services.Customers.Interfaces;
using Inovatiqa.Services.Directory.Interfaces;
using Inovatiqa.Services.Messages.Interfaces;
using Inovatiqa.Services.Orders.Interfaces;
using Inovatiqa.Services.WorkContext.Interfaces;
using Inovatiqa.Web.Extensions;
using Inovatiqa.Web.Factories.Interfaces;
using Inovatiqa.Web.Models.Customer;
using Inovatiqa.Web.Mvc.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;

namespace Inovatiqa.Web.Controllers
{
    [AutoValidateAntiforgeryToken]
    public partial class CustomerController : BasePublicController
    {
        #region Fields

        private readonly ICustomerModelFactory _customerModelFactory;
        private readonly IWorkContextService _workContextService;
        private readonly ICustomerService _customerService;
        private readonly IAuthenticationService _authenticationService;
        private readonly ICustomerRegistrationService _customerRegistrationService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly INewsLetterSubscriptionService _newsLetterSubscriptionService;
        private readonly IAddressService _addressService;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly ICustomerAttributeService _customerAttributeService;
        private readonly ICustomerAttributeParserService _customerAttributeParserService;
        private readonly IAddressModelFactory _addressModelFactory;
        private readonly IAddressAttributeParserService _addressAttributeParserService;
        private readonly ICountryService _countryService;
        private readonly IProductService _productService;
        private readonly IStateProvinceService _stateProvinceService;

        #endregion

        #region Ctor

        public CustomerController(ICustomerModelFactory customerModelFactory,
            IWorkContextService workContextService,
            ICustomerService customerService,
            IAuthenticationService authenticationService,
            ICustomerRegistrationService customerRegistrationService,
            IGenericAttributeService genericAttributeService,
            INewsLetterSubscriptionService newsLetterSubscriptionService,
            IAddressService addressService,
            IWorkflowMessageService workflowMessageService,
            ICustomerActivityService customerActivityService,
            IShoppingCartService shoppingCartService,
            ICustomerAttributeService customerAttributeService,
            ICustomerAttributeParserService customerAttributeParserService,
            IAddressModelFactory addressModelFactory,
            IAddressAttributeParserService addressAttributeParserService,
            ICountryService countryService,
             IRazorViewEngine viewEngine,
             IProductService productService,
             IStateProvinceService stateProvinceService) : base(viewEngine)
        {
            _customerModelFactory = customerModelFactory;
            _workContextService = workContextService;
            _customerService = customerService;
            _authenticationService = authenticationService;
            _customerRegistrationService = customerRegistrationService;
            _genericAttributeService = genericAttributeService;
            _newsLetterSubscriptionService = newsLetterSubscriptionService;
            _addressService = addressService;
            _workflowMessageService = workflowMessageService;
            _customerActivityService = customerActivityService;
            _shoppingCartService = shoppingCartService;
            _customerAttributeService = customerAttributeService;
            _customerAttributeParserService = customerAttributeParserService;
            _addressModelFactory = addressModelFactory;
            _addressAttributeParserService = addressAttributeParserService;
            _countryService = countryService;
            _productService = productService;
            _stateProvinceService = stateProvinceService;
        }

        #endregion

        #region Utilities

        protected virtual string ParseCustomCustomerAttributes(IFormCollection form)
        {
            if (form == null)
                throw new ArgumentNullException(nameof(form));

            var attributesXml = "";
            var attributes = _customerAttributeService.GetAllCustomerAttributes();
            foreach (var attribute in attributes)
            {
                var controlId = $"{InovatiqaDefaults.CustomerAttributePrefix}{attribute.Id}";
                switch (attribute.AttributeControlTypeId)
                {
                    case (int)AttributeControlType.DropdownList:
                    case (int)AttributeControlType.RadioList:
                        {
                            var ctrlAttributes = form[controlId];
                            if (!StringValues.IsNullOrEmpty(ctrlAttributes))
                            {
                                var selectedAttributeId = int.Parse(ctrlAttributes);
                                if (selectedAttributeId > 0)
                                    attributesXml = _customerAttributeParserService.AddCustomerAttribute(attributesXml,
                                        attribute, selectedAttributeId.ToString());
                            }
                        }
                        break;
                    case (int)AttributeControlType.Checkboxes:
                        {
                            var cblAttributes = form[controlId];
                            if (!StringValues.IsNullOrEmpty(cblAttributes))
                            {
                                foreach (var item in cblAttributes.ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                                {
                                    var selectedAttributeId = int.Parse(item);
                                    if (selectedAttributeId > 0)
                                        attributesXml = _customerAttributeParserService.AddCustomerAttribute(attributesXml,
                                            attribute, selectedAttributeId.ToString());
                                }
                            }
                        }
                        break;
                    case (int)AttributeControlType.ReadonlyCheckboxes:
                        {
                            var attributeValues = _customerAttributeService.GetCustomerAttributeValues(attribute.Id);
                            foreach (var selectedAttributeId in attributeValues
                                .Where(v => v.IsPreSelected)
                                .Select(v => v.Id)
                                .ToList())
                            {
                                attributesXml = _customerAttributeParserService.AddCustomerAttribute(attributesXml,
                                    attribute, selectedAttributeId.ToString());
                            }
                        }
                        break;
                    case (int)AttributeControlType.TextBox:
                    case (int)AttributeControlType.MultilineTextbox:
                        {
                            var ctrlAttributes = form[controlId];
                            if (!StringValues.IsNullOrEmpty(ctrlAttributes))
                            {
                                var enteredText = ctrlAttributes.ToString().Trim();
                                attributesXml = _customerAttributeParserService.AddCustomerAttribute(attributesXml,
                                    attribute, enteredText);
                            }
                        }
                        break;
                    case (int)AttributeControlType.Datepicker:
                    case (int)AttributeControlType.ColorSquares:
                    case (int)AttributeControlType.ImageSquares:
                    case (int)AttributeControlType.FileUpload:
                        {
                            if (form.Files != null && form.Files.Count > 0)
                            {
                                string extenssion = Path.GetExtension(form.Files[0].FileName).ToLower();
                                string fileName = Convert.ToString(Guid.NewGuid()) + extenssion;
                                var filepath = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", InovatiqaDefaults.StoredFilePath)).Root + $@"\{fileName}";
                                if (extenssion == ".pdf")
                                {
                                    using (var stream = System.IO.File.Create(filepath))
                                    {
                                        form.Files[0].CopyTo(stream);
                                        stream.Flush();
                                    }

                                }
                                /*string fileContents;
                                using (var stream = form.Files[0].OpenReadStream())
                                using (var reader = new StreamReader(stream))
                                {
                                    fileContents = reader.ReadToEndAsync().Result;
                                }
                                */
                                var storeFileUrl = Path.Combine(InovatiqaDefaults.StoredFilePath, fileName).Replace("\\", "/");
                                attributesXml = _customerAttributeParserService.AddCustomerAttribute(attributesXml,
                                        attribute, "/" + storeFileUrl);
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
            if (form.Files != null && form.Files.Count > 0)
            {
                string fileContents;
                using (var stream = form.Files[0].OpenReadStream())
                using (var reader = new StreamReader(stream))
                {
                    fileContents = reader.ReadToEndAsync().Result;
                }
                //attributesXml = _customerAttributeParserService.AddCustomerAttribute(attributesXml,
                //        attribute, fileContents.ToString());
            }
            return attributesXml;
        }

        public virtual IActionResult EditAccountInfo(string firstName = "", string lastName = "", string phone = "", string email = "")
        {
            //add by hamza
            var customer = _workContextService.CurrentCustomer;
            if (customer.ParentId != null && customer.ParentId != 0)
            {
                if (!_customerService.IsInCustomerRole(customer, "Subaccount_MAD"))
                {
                    return RedirectToAction("NotAllowed", "Common");
                }
            }
            var model = new CustomerInfoModel();
            if (firstName != "" || lastName != "" || email != "" || phone != "")
            {
                var user = _workContextService.CurrentCustomer;
                var address = _addressService.GetAddressById(Convert.ToInt32(user.BillingAddressId));
                address.FirstName = firstName;
                address.LastName = lastName;
                address.PhoneNumber = phone;
                user.Email = email;
                _customerService.UpdateCustomer(user);
                _addressService.UpdateAddress(address);
                TempData["Message"] = true;
            }
            //var customer = _workContextService.CurrentCustomer;
            customer.BillingAddress = _addressService.GetAddressById(Convert.ToInt32(customer.BillingAddressId));
            model.FirstName = customer.BillingAddress.FirstName;
            model.LastName = customer.BillingAddress.LastName;
            model.Phone = customer.BillingAddress.PhoneNumber;
            model.Email = customer.Email;
            return View(model);
        }


        #endregion

        #region Methods

        #region Login / logout

        [RequireHttps]
        public virtual IActionResult Login(bool? checkoutAsGuest)
        {
            if (_customerService.IsRegistered(_workContextService.CurrentCustomer))
            {
                return Redirect("~/");
            }
            var model = _customerModelFactory.PrepareLoginModel(checkoutAsGuest);
            return View(model);
        }

        [HttpPost]
        public virtual IActionResult Login(LoginModel model, string returnUrl, bool captchaValid)
        {
            if (InovatiqaDefaults.Enabled && InovatiqaDefaults.ShowOnLoginPage && !captchaValid)
            {
                ModelState.AddModelError("", "The reCAPTCHA response is invalid or malformed. Please try again.");
            }

            if (ModelState.IsValid)
            {
                if (InovatiqaDefaults.UsernamesEnabled && model.Username != null)
                {
                    model.Username = model.Username.Trim();
                }
                // checkout work

                var Customer = _workContextService.CurrentCustomer;
                var cart = _shoppingCartService.GetShoppingCart(Customer, Convert.ToInt32(ShoppingCartType.ShoppingCart));

                //end checkout work

                var loginResult = _customerRegistrationService.ValidateCustomer(InovatiqaDefaults.UsernamesEnabled ? model.Username : model.Email, model.Password);
                switch (loginResult)
                {
                    case CustomerLoginResults.Successful:
                        {
                            var customer = InovatiqaDefaults.UsernamesEnabled
                                ? _customerService.GetCustomerByUsername(model.Username)
                                : _customerService.GetCustomerByEmail(model.Email);

                            _shoppingCartService.MigrateShoppingCart(_workContextService.CurrentCustomer, customer, true);

                            _authenticationService.SignIn(customer, model.RememberMe);

                            //_eventPublisher.Publish(new CustomerLoggedinEvent(customer));

                            _customerActivityService.InsertActivity("PublicStore.Login",
                                "Login", customer.Id, customer.GetType().Name);

                            if (string.IsNullOrEmpty(returnUrl) || !Url.IsLocalUrl(returnUrl))
                                return RedirectToRoute("Homepage");
                            // merge anonymous cart with current cart
                            _shoppingCartService.MoveItemsToCurrentUser(cart.ToList(), customer);
                            // complete merging
                            return Redirect(returnUrl);
                        }
                    case CustomerLoginResults.CustomerNotExist:
                        ModelState.AddModelError("", "No customer account found");
                        break;
                    case CustomerLoginResults.Deleted:
                        ModelState.AddModelError("", "Customer is deleted");
                        break;
                    case CustomerLoginResults.NotActive:
                        ModelState.AddModelError("", "Account is not active");
                        break;
                    case CustomerLoginResults.NotRegistered:
                        ModelState.AddModelError("", "Account is not registered");
                        break;
                    case CustomerLoginResults.LockedOut:
                        ModelState.AddModelError("", "Customer is locked out");
                        break;
                    case CustomerLoginResults.WrongPassword:
                    default:
                        ModelState.AddModelError("", "The credentials provided are incorrect");
                        break;
                }
            }

            model = _customerModelFactory.PrepareLoginModel(model.CheckoutAsGuest);
            return View(model);
        }

        public virtual IActionResult Logout()
        {
            if (_workContextService.OriginalCustomerIfImpersonated != null)
            {
                _customerActivityService.InsertActivity(_workContextService.OriginalCustomerIfImpersonated, "Impersonation.Finished",
                    string.Format("Finished customer impersonation (Email: {0}, ID = {1})",
                    _workContextService.CurrentCustomer.Email, _workContextService.CurrentCustomer.Id),
                    _workContextService.CurrentCustomer.Id,
                    _workContextService.CurrentCustomer.GetType().Name);

                _customerActivityService.InsertActivity("Impersonation.Finished",
                    string.Format("Impersonation by store owner was finished (Email: {0}, ID = {1})",
                        _workContextService.OriginalCustomerIfImpersonated.Email, _workContextService.OriginalCustomerIfImpersonated.Id),
                    _workContextService.CurrentCustomer.Id,
                    _workContextService.OriginalCustomerIfImpersonated.GetType().Name);


                _genericAttributeService.SaveAttribute<int?>(_workContextService.OriginalCustomerIfImpersonated.GetType().Name, _workContextService.CurrentCustomer.Id, InovatiqaDefaults.ImpersonatedCustomerIdAttribute, null, InovatiqaDefaults.StoreId);

                return RedirectToAction("Edit", "Customer", new { id = _workContextService.CurrentCustomer.Id, area = InovatiqaDefaults.Admin });
            }

            _customerActivityService.InsertActivity(_workContextService.CurrentCustomer, "PublicStore.Logout",
                "Logout", _workContextService.CurrentCustomer.Id, _workContextService.CurrentCustomer.GetType().Name);


            _authenticationService.SignOut();

            //_eventPublisher.Publish(new CustomerLoggedOutEvent(_workContext.CurrentCustomer));

            return RedirectToRoute("Homepage");
        }

        #endregion

        #region Password recovery


        #endregion     

        #region Register

        [RequireHttps]
        public virtual IActionResult Register(string type)
        {
            if (_customerService.IsRegistered(_workContextService.CurrentCustomer))
            {
                return Redirect("~/");
            }
            var model = new RegisterModel();
            model = _customerModelFactory.PrepareRegisterModel(model, false, setDefaultValues: true);

            if (model != null)
            {
                if (type == "customer")
                {
                    model.IsCustomerRegistration = true;
                }
                else if (type == "vendor")
                {
                    model.IsCustomerRegistration = false;
                }
                else
                    model.IsCustomerRegistration = true;
            }

            return View(model);
        }

        [HttpPost]
        public virtual IActionResult Register(RegisterModel model, string returnUrl, bool captchaValid, IFormCollection form, IFormFile FileBanner)
        {
            if (_customerService.IsRegistered(_workContextService.CurrentCustomer))
            {
                _authenticationService.SignOut();

                _workContextService.CurrentCustomer = _customerService.InsertGuestCustomer();
            }
            
            var customer = _workContextService.CurrentCustomer;
            customer.RegisteredInStoreId = InovatiqaDefaults.StoreId;
            string customerAttributesXml = "";
            if (model.CustomerType == "vendorType")
            {
                customerAttributesXml = ParseCustomCustomerAttributes(form);
                var customerAttributeWarnings = _customerAttributeParserService.GetAttributeWarnings(customerAttributesXml);
                foreach (var error in customerAttributeWarnings)
                {
                    ModelState.AddModelError("", error);
                }
            }
            if (model.CustomerType == "customerType")
            {
                model.IsSameShipToAddress = true;
                //model.ShipToCountryId = ;
                //model.ShipToStateProvinceId = null;
            }
            //if(model.IsSameShipToAddress == true)
            //{
            //    foreach(var key in form)
            //    {
            //        if (key.Key.ToLower().Contains("shipto"))
            //            Console.Write(key.Value);
            //    }
            //}
            //if(model.IsSameShipToAddress == true)
            //{
            //    foreach(var key in ModelState.Keys)
            //    {
            //        var index = key.IndexOf(key);
            //        if (key.ToLower().Contains("shipto"))
            //            key.Remove(index);
            //    }
            //}
            if (ModelState.IsValid)
            {
                if (InovatiqaDefaults.UsernamesEnabled && model.Username != null)
                {
                    model.Username = model.Username.Trim();
                }

                var isApproved = InovatiqaDefaults.StandardRegistration;
                var registrationRequest = new CustomerRegistrationRequest(customer,
                    model.Email,
                    InovatiqaDefaults.UsernamesEnabled ? model.Username : model.Email,
                    model.Password,
                    InovatiqaDefaults.Hashed,
                    InovatiqaDefaults.StoreId,
                    isApproved);
                var registrationResult = _customerRegistrationService.RegisterCustomer(registrationRequest);
                if (registrationResult.Success)
                {
                    if (InovatiqaDefaults.GenderEnabled)
                        _genericAttributeService.SaveAttribute<string>(customer.GetType().Name, customer.Id, InovatiqaDefaults.GenderAttribute, model.Gender, InovatiqaDefaults.StoreId);

                    if (InovatiqaDefaults.FirstNameEnabled)
                        _genericAttributeService.SaveAttribute<string>(customer.GetType().Name, customer.Id, InovatiqaDefaults.FirstNameAttribute, model.FirstName, InovatiqaDefaults.StoreId);

                    if (InovatiqaDefaults.LastNameEnabled)
                        _genericAttributeService.SaveAttribute<string>(customer.GetType().Name, customer.Id, InovatiqaDefaults.LastNameAttribute, model.LastName, InovatiqaDefaults.StoreId);

                    if (InovatiqaDefaults.DateOfBirthEnabled)
                    {
                        var dateOfBirth = model.ParseDateOfBirth();
                        _genericAttributeService.SaveAttribute<DateTime?>(customer.GetType().Name, customer.Id, InovatiqaDefaults.DateOfBirthAttribute, dateOfBirth, InovatiqaDefaults.StoreId);
                    }
                    if (InovatiqaDefaults.CompanyEnabled && model.IsCustomerRegistration == false)
                        _genericAttributeService.SaveAttribute<string>(customer.GetType().Name, customer.Id, InovatiqaDefaults.CompanyAttribute, model.Company, InovatiqaDefaults.StoreId);
                    if (model.CustomerType == "customerType")
                    {
                        if (InovatiqaDefaults.StreetAddressEnabled)
                            _genericAttributeService.SaveAttribute<string>(customer.GetType().Name, customer.Id, InovatiqaDefaults.StreetAddressAttribute, model.BillToStreetAddress, InovatiqaDefaults.StoreId);

                        if (InovatiqaDefaults.StreetAddress2Enabled)
                            _genericAttributeService.SaveAttribute<string>(customer.GetType().Name, customer.Id, InovatiqaDefaults.StreetAddress2Attribute, model.StreetAddress2, InovatiqaDefaults.StoreId);

                        if (InovatiqaDefaults.ZipPostalCodeEnabled)
                            _genericAttributeService.SaveAttribute<string>(customer.GetType().Name, customer.Id, InovatiqaDefaults.ZipPostalCodeAttribute, model.BillToZipPostalCode, InovatiqaDefaults.StoreId);

                        if (InovatiqaDefaults.CityEnabled)
                            _genericAttributeService.SaveAttribute<string>(customer.GetType().Name, customer.Id, InovatiqaDefaults.CityAttribute, model.BillToCity, InovatiqaDefaults.StoreId);

                        if (InovatiqaDefaults.CountyEnabled)
                            _genericAttributeService.SaveAttribute<string>(customer.GetType().Name, customer.Id, InovatiqaDefaults.CountyAttribute, model.County, InovatiqaDefaults.StoreId);

                        if (InovatiqaDefaults.CountryEnabled)
                            _genericAttributeService.SaveAttribute<int>(customer.GetType().Name, customer.Id, InovatiqaDefaults.CountryIdAttribute, model.BillToCountryId, InovatiqaDefaults.StoreId);

                        if (InovatiqaDefaults.CountryEnabled && InovatiqaDefaults.StateProvinceEnabled)
                            _genericAttributeService.SaveAttribute<int>(customer.GetType().Name, customer.Id, InovatiqaDefaults.StateProvinceIdAttribute, model.BillToStateProvinceId, InovatiqaDefaults.StoreId);

                        if (InovatiqaDefaults.PhoneEnabled)
                            _genericAttributeService.SaveAttribute<string>(customer.GetType().Name, customer.Id, InovatiqaDefaults.PhoneAttribute, model.BillToPhone, InovatiqaDefaults.StoreId);

                        if (InovatiqaDefaults.FaxEnabled)
                            _genericAttributeService.SaveAttribute<string>(customer.GetType().Name, customer.Id, InovatiqaDefaults.FaxAttribute, model.Fax, InovatiqaDefaults.StoreId);

                        var defaultAddress = new Address
                        {
                            FirstName = _genericAttributeService.GetAttribute<string>(customer, InovatiqaDefaults.FirstNameAttribute, customer.Id),
                            Email = customer.Email,
                            Company = _genericAttributeService.GetAttribute<string>(customer, InovatiqaDefaults.CompanyAttribute, customer.Id),
                            CountryId = _genericAttributeService.GetAttribute<int>(customer, InovatiqaDefaults.CountryIdAttribute, customer.Id) > 0
                                    ? (int?)_genericAttributeService.GetAttribute<int>(customer, InovatiqaDefaults.CountryIdAttribute, customer.Id)
                                    : null,
                            StateProvinceId = _genericAttributeService.GetAttribute<int>(customer, InovatiqaDefaults.StateProvinceIdAttribute, customer.Id) > 0
                                    ? (int?)_genericAttributeService.GetAttribute<int>(customer, InovatiqaDefaults.StateProvinceIdAttribute, customer.Id)
                                    : null,
                            County = _genericAttributeService.GetAttribute<string>(customer, InovatiqaDefaults.CountyAttribute, customer.Id),
                            City = _genericAttributeService.GetAttribute<string>(customer, InovatiqaDefaults.CityAttribute, customer.Id),
                            Address1 = _genericAttributeService.GetAttribute<string>(customer, InovatiqaDefaults.StreetAddressAttribute, customer.Id),
                            Address2 = _genericAttributeService.GetAttribute<string>(customer, InovatiqaDefaults.StreetAddress2Attribute, customer.Id),
                            ZipPostalCode = _genericAttributeService.GetAttribute<string>(customer, InovatiqaDefaults.ZipPostalCodeAttribute, customer.Id),
                            PhoneNumber = _genericAttributeService.GetAttribute<string>(customer, InovatiqaDefaults.PhoneAttribute, customer.Id),
                            FaxNumber = _genericAttributeService.GetAttribute<string>(customer, InovatiqaDefaults.FaxAttribute, customer.Id),
                            CreatedOnUtc = customer.CreatedOnUtc
                        };
                        if (_addressService.IsAddressValid(defaultAddress))
                        {
                            if (defaultAddress.CountryId == 0)
                                defaultAddress.CountryId = null;
                            if (defaultAddress.StateProvinceId == 0)
                                defaultAddress.StateProvinceId = null;

                            _addressService.InsertAddress(defaultAddress);

                            _customerService.InsertCustomerAddress(customer, defaultAddress);
                            customer.BillingAddressId = defaultAddress.Id;
                            customer.ShippingAddressId = defaultAddress.Id;
                            _customerService.UpdateCustomer(customer);
                        }
                    }
                    else if (model.CustomerType == "vendorType")
                    {
                        var b2bRole = _customerService.GetCustomerRoleBySystemName(InovatiqaDefaults.B2BRoleName);
                        if (b2bRole != null)
                        {
                            _customerService.AddCustomerRoleMapping(new CustomerCustomerRoleMapping { CustomerId = registrationRequest.Customer.Id, CustomerRoleId = b2bRole.Id });
                        }

                        _genericAttributeService.SaveAttribute<string>(customer.GetType().Name, customer.Id, InovatiqaDefaults.BillToCityAttribute, model.BillToCity, InovatiqaDefaults.StoreId);
                        _genericAttributeService.SaveAttribute<int>(customer.GetType().Name, customer.Id, InovatiqaDefaults.BillToCountryIdAttribute, model.BillToCountryId, InovatiqaDefaults.StoreId);
                        _genericAttributeService.SaveAttribute<string>(customer.GetType().Name, customer.Id, InovatiqaDefaults.BillToPhoneAttribute, model.BillToPhone, InovatiqaDefaults.StoreId);
                        _genericAttributeService.SaveAttribute<int>(customer.GetType().Name, customer.Id, InovatiqaDefaults.BillToStateProvinceIdAttribute, model.BillToStateProvinceId, InovatiqaDefaults.StoreId);
                        _genericAttributeService.SaveAttribute<string>(customer.GetType().Name, customer.Id, InovatiqaDefaults.BillToStreetAddressAttribute, model.BillToStreetAddress, InovatiqaDefaults.StoreId);
                        _genericAttributeService.SaveAttribute<string>(customer.GetType().Name, customer.Id, InovatiqaDefaults.BillToZipPostalCodeAttribute, model.BillToZipPostalCode, InovatiqaDefaults.StoreId);
                        
                        

                        var BillingAddress = new Address
                        {
                            FirstName = _genericAttributeService.GetAttribute<string>(customer, InovatiqaDefaults.FirstNameAttribute, customer.Id),
                            LastName = _genericAttributeService.GetAttribute<string>(customer, InovatiqaDefaults.LastNameAttribute, customer.Id),
                            Company = _genericAttributeService.GetAttribute<string>(customer, InovatiqaDefaults.CompanyAttribute, customer.Id),
                            Email = customer.Email,
                            CountryId = _genericAttributeService.GetAttribute<int>(customer, InovatiqaDefaults.BillToCountryIdAttribute, customer.Id) > 0
                                ? (int?)_genericAttributeService.GetAttribute<int>(customer, InovatiqaDefaults.BillToCountryIdAttribute, customer.Id)
                                : null,
                            StateProvinceId = _genericAttributeService.GetAttribute<int>(customer, InovatiqaDefaults.BillToStateProvinceIdAttribute, customer.Id) > 0
                                ? (int?)_genericAttributeService.GetAttribute<int>(customer, InovatiqaDefaults.BillToStateProvinceIdAttribute, customer.Id)
                                : null,
                            //County = _genericAttributeService.GetAttribute<string>(customer, InovatiqaDefaults.CountyAttribute, customer.Id),
                            City = _genericAttributeService.GetAttribute<string>(customer, InovatiqaDefaults.BillToCityAttribute, customer.Id),
                            Address1 = _genericAttributeService.GetAttribute<string>(customer, InovatiqaDefaults.BillToStreetAddressAttribute, customer.Id),
                            ZipPostalCode = _genericAttributeService.GetAttribute<string>(customer, InovatiqaDefaults.BillToZipPostalCodeAttribute, customer.Id),
                            PhoneNumber = _genericAttributeService.GetAttribute<string>(customer, InovatiqaDefaults.BillToPhoneAttribute, customer.Id),
                            CreatedOnUtc = customer.CreatedOnUtc
                        };
                        if (_addressService.IsAddressValid(BillingAddress))
                        {
                            if (BillingAddress.CountryId == 0)
                                BillingAddress.CountryId = null;
                            if (BillingAddress.StateProvinceId == 0)
                                BillingAddress.StateProvinceId = null;

                            _addressService.InsertAddress(BillingAddress);

                            _customerService.InsertCustomerAddress(customer, BillingAddress);
                            customer.BillingAddressId = BillingAddress.Id;
                        }

                        if (model.IsSameShipToAddress == false)
                        {
                            _genericAttributeService.SaveAttribute<string>(customer.GetType().Name, customer.Id, InovatiqaDefaults.ShipToCityAttribute, model.ShipToCity, InovatiqaDefaults.StoreId);
                            _genericAttributeService.SaveAttribute<int>(customer.GetType().Name, customer.Id, InovatiqaDefaults.ShipToCountryIdAttribute, model.ShipToCountryId, InovatiqaDefaults.StoreId);
                            _genericAttributeService.SaveAttribute<string>(customer.GetType().Name, customer.Id, InovatiqaDefaults.ShipToPhoneAttribute, model.ShipToPhone, InovatiqaDefaults.StoreId);
                            _genericAttributeService.SaveAttribute<int>(customer.GetType().Name, customer.Id, InovatiqaDefaults.ShipToStateProvinceIdAttribute, model.ShipToStateProvinceId, InovatiqaDefaults.StoreId);
                            _genericAttributeService.SaveAttribute<string>(customer.GetType().Name, customer.Id, InovatiqaDefaults.ShipToStreetAddressAttribute, model.ShipToStreetAddress, InovatiqaDefaults.StoreId);
                            _genericAttributeService.SaveAttribute<string>(customer.GetType().Name, customer.Id, InovatiqaDefaults.ShipToZipPostalCodeAttribute, model.ShipToZipPostalCode, InovatiqaDefaults.StoreId);
                            _genericAttributeService.SaveAttribute<string>(customer.GetType().Name, customer.Id, InovatiqaDefaults.CustomCustomerAttributes, customerAttributesXml, InovatiqaDefaults.StoreId);
                            var ShippingAddress = new Address
                            {
                                FirstName = _genericAttributeService.GetAttribute<string>(customer, InovatiqaDefaults.FirstNameAttribute, customer.Id),
                                LastName = _genericAttributeService.GetAttribute<string>(customer, InovatiqaDefaults.LastNameAttribute, customer.Id),
                                Email = customer.Email,
                                Company = _genericAttributeService.GetAttribute<string>(customer, InovatiqaDefaults.CompanyAttribute, customer.Id),
                                CountryId = _genericAttributeService.GetAttribute<int>(customer, InovatiqaDefaults.ShipToCountryIdAttribute, customer.Id) > 0
                                    ? (int?)_genericAttributeService.GetAttribute<int>(customer, InovatiqaDefaults.ShipToCountryIdAttribute, customer.Id)
                                    : null,
                                StateProvinceId = _genericAttributeService.GetAttribute<int>(customer, InovatiqaDefaults.ShipToStateProvinceIdAttribute, customer.Id) > 0
                                    ? (int?)_genericAttributeService.GetAttribute<int>(customer, InovatiqaDefaults.ShipToStateProvinceIdAttribute, customer.Id)
                                    : null,
                                //County = _genericAttributeService.GetAttribute<string>(customer, InovatiqaDefaults.CountyAttribute, customer.Id),
                                City = _genericAttributeService.GetAttribute<string>(customer, InovatiqaDefaults.ShipToCityAttribute, customer.Id),
                                Address1 = _genericAttributeService.GetAttribute<string>(customer, InovatiqaDefaults.ShipToStreetAddressAttribute, customer.Id),
                                ZipPostalCode = _genericAttributeService.GetAttribute<string>(customer, InovatiqaDefaults.ShipToZipPostalCodeAttribute, customer.Id),
                                PhoneNumber = _genericAttributeService.GetAttribute<string>(customer, InovatiqaDefaults.ShipToPhoneAttribute, customer.Id),
                                CreatedOnUtc = customer.CreatedOnUtc
                            };
                            if (_addressService.IsAddressValid(ShippingAddress))
                            {
                                if (ShippingAddress.CountryId == 0)
                                    ShippingAddress.CountryId = null;
                                if (ShippingAddress.StateProvinceId == 0)
                                    ShippingAddress.StateProvinceId = null;

                                _addressService.InsertAddress(ShippingAddress);

                                _customerService.InsertCustomerAddress(customer, ShippingAddress);
                                customer.ShippingAddressId = ShippingAddress.Id;

                            }
                        }
                        else
                        {
                            customer.ShippingAddressId = BillingAddress.Id;
                        }
                        _customerService.UpdateCustomer(customer);

                        _genericAttributeService.SaveAttribute(customer.GetType().Name, customer.Id, InovatiqaDefaults.CustomCustomerAttributes, customerAttributesXml);

                    }

                    if (isApproved)
                        _authenticationService.SignIn(customer, true);
                    if (InovatiqaDefaults.NotifyNewCustomerRegistration)
                        _workflowMessageService.SendCustomerRegisteredNotificationMessage(customer,
                            InovatiqaDefaults.LanguageId);

                    //_eventPublisher.Publish(new CustomerRegisteredEvent(customer));

                    _genericAttributeService.SaveAttribute<string>(customer.GetType().Name, customer.Id, InovatiqaDefaults.AccountActivationTokenAttribute, Guid.NewGuid().ToString(), InovatiqaDefaults.StoreId);

                    _workflowMessageService.SendCustomerEmailValidationMessage(customer, InovatiqaDefaults.LanguageId);

                    return RedirectToRoute("RegisterResult",
                        new { resultId = (int)InovatiqaDefaults.EmailValidation });
                }

                foreach (var error in registrationResult.Errors)
                    ModelState.AddModelError("", error);
            }

            model = _customerModelFactory.PrepareRegisterModel(model, true);
            TempData["RegisteredSuccess"] = "1";
            return RedirectToAction("Login");
            
        }

        public virtual IActionResult UserNameAvailable(string username)
        {
            return Json(_customerService.GetCustomerByUsername(username) == null);
        }
        
        public virtual IActionResult RegisterResult(int resultId)
        {
            var model = _customerModelFactory.PrepareRegisterResultModel(resultId);
            return View(model);
        }

        #endregion

        #region My account / Info

        [RequireHttps]
        public virtual IActionResult Info()
        {
            var customer = _workContextService.CurrentCustomer;

            if (!_customerService.IsRegistered(customer))
                return Challenge();

            var model = new CustomerInfoModel();
            model = _customerModelFactory.PrepareCustomerInfoModel(model, customer, false);

            return View(model);
        }

        public virtual IActionResult IsUserLoggedIn()
        {
            var isRegistered = _customerService.IsRegistered(_workContextService.CurrentCustomer);
            return Json(isRegistered);
        }

        public virtual IActionResult GetUserDetails()
        {
            var customer = _customerService.GetCustomerById(_workContextService.CurrentCustomer.Id);
            return Json(customer);
        }
        public virtual IActionResult AccountInformation(IFormCollection form)
        {
            if (!_customerService.IsRegistered(_workContextService.CurrentCustomer))
            {
                return Challenge();
            }
            var customer = _workContextService.CurrentCustomer;
            // string email = "", string password = "", string confirmPassword = "", string FirstName = "", string LastName = ""
            // Implementation of Role - Can Modify Account Data
            /*if (customer.ParentId != null && customer.ParentId != 0)
            {
                if (!_customerService.IsInCustomerRole(customer, "Subaccount_MAD"))
                {
                    return RedirectToAction("NotAllowed", "Common");
                }
            }
            // Implementatiuon of Role Done*/
            var customerAccountInfoUpdateResultModel = new CustomerAccountInfoUpdateResultModel();
            var model = _customerModelFactory.UpdateCustomerAccountInformationResultModel(customerAccountInfoUpdateResultModel, form);

            model.BillingAddress = _addressService.GetAddressById(Convert.ToInt32(customer.BillingAddressId));
            model.ShippingAddress = _addressService.GetAddressById(Convert.ToInt32(customer.ShippingAddressId));

            model.BillingAddress.StateProvince = model.BillingAddress != null ? (_stateProvinceService.GetStateProvinceById(Convert.ToInt32(model.BillingAddress.StateProvinceId))) : null;
            model.ShippingAddress.StateProvince = model.ShippingAddress != null ? (_stateProvinceService.GetStateProvinceById(Convert.ToInt32(model.ShippingAddress.StateProvinceId))) : null;

            return View(model);
        }

        #endregion

        #region My account / Addresses

        [HttpsRequirement]
        public virtual IActionResult Addresses()
        {
            var customer = _workContextService.CurrentCustomer;
            // Implemetation of Role - Manage Address Book

            if (customer.ParentId != null)
            {
                if (!_customerService.IsInCustomerRole(customer, "Subaccount_MAB"))
                {
                    return RedirectToAction("NotAllowed", "Common");
                }
            }

            // Implementation of Role Ended
            if (!_customerService.IsRegistered(customer))
                return Challenge();

            var model = _customerModelFactory.PrepareCustomerAddressListModel();
            model.canEditMainAddresses = !_customerService.IsInCustomerRole(customer, "Subaccount_FUPA");
            return View(model);
        }

        [HttpPost]
        [HttpsRequirement]
        [IgnoreAntiforgeryToken]
        public virtual IActionResult AddressDelete(int addressId, int customerId)
        {
            var Currentcustomer = _workContextService.CurrentCustomer;
            var customer = _customerService.GetCustomerById(customerId);
            if (!_customerService.IsRegistered(Currentcustomer))
                return Challenge();

            var address = _customerService.GetCustomerAddress(customerId, addressId);
            if (address != null)
            {
                if(customer.ParentId != null && customer.ParentId != 0)
                {
                    customer.BillingAddressId = customer.BillingAddressId == addressId ? _customerService.GetCustomerById(Convert.ToInt32(customer.ParentId)).BillingAddressId : customer.BillingAddressId;
                    customer.ShippingAddressId = customer.ShippingAddressId == addressId ? _customerService.GetCustomerById(Convert.ToInt32(customer.ParentId)).ShippingAddressId : customer.ShippingAddressId;
                }
                _customerService.RemoveCustomerAddress(customer, address);
                _customerService.InsertCustomerAddress(customer, _addressService.GetAddressById(Convert.ToInt32(customer.BillingAddressId)));
                _customerService.UpdateCustomer(customer);
                _addressService.DeleteAddress(address);
            }

            return Json(new
            {
                redirect = Url.RouteUrl("CustomerAddresses"),
            });
        }

        [HttpsRequirement]
        public virtual IActionResult AddressAdd()
        {
            var customer = _workContextService.CurrentCustomer;

            if (!_customerService.IsRegistered(customer))
                return Challenge();

            var model = new CustomerAddressEditModel();
            _addressModelFactory.PrepareAddressModel(model.Address,
                address: null,
                loadCountries: () => _countryService.GetAllCountriesForBilling(InovatiqaDefaults.LanguageId),
                excludeProperties: false);
            TempData["IsAdding"] = true;
            var address = _addressService.GetAddressById(Convert.ToInt32(customer.BillingAddressId));
            TempData["FirstName"] = address != null ? address.FirstName : String.Empty;
            TempData["LastName"] = address != null ? address.LastName : String.Empty;
            return View(model);
        }

        [HttpPost]
        public virtual IActionResult AddressAdd(CustomerAddressEditModel model, IFormCollection form)
        {
            var customer = _workContextService.CurrentCustomer;

            if (!_customerService.IsRegistered(customer))
                return Challenge();

            var customAttributes = _addressAttributeParserService.ParseCustomAddressAttributes(form);
            var customAttributeWarnings = _addressAttributeParserService.GetAttributeWarnings(customAttributes);
            foreach (var error in customAttributeWarnings)
            {
                ModelState.AddModelError("", error);
            }

            if (ModelState.IsValid)
            {
                var address = model.Address.ToEntity();
                address.CustomAttributes = customAttributes;
                address.CreatedOnUtc = DateTime.UtcNow;
                if (address.CountryId == 0)
                    address.CountryId = null;
                if (address.StateProvinceId == 0)
                    address.StateProvinceId = null;


                _addressService.InsertAddress(address);

                _customerService.InsertCustomerAddress(customer, address);
                TempData["message"] = "Address Added Successfully.";
                TempData["messageClass"] = "success";
                return RedirectToAction("AddressAdd");
            }

            _addressModelFactory.PrepareAddressModel(model.Address,
                address: null,
                loadCountries: () => _countryService.GetAllCountriesForBilling(InovatiqaDefaults.LanguageId),
                excludeProperties: true,
                overrideAttributesXml: customAttributes);

            return View(model);
        }

        [HttpsRequirement]
        public virtual IActionResult AddressEdit(int addressId = 0, int customerId = 0)
        {
            var currentCustomer = _workContextService.CurrentCustomer;
            var customer = _customerService.GetCustomerById(customerId == 0 ? currentCustomer.Id : customerId);
            if (!_customerService.IsRegistered(currentCustomer))
                return Challenge();


            var address = _customerService.GetCustomerAddress(customer.Id, addressId);
            if (address == null)
                return RedirectToRoute("CustomerAddresses");

            var model = new CustomerAddressEditModel();
            model.Address.CustomerId = customerId;
            _addressModelFactory.PrepareAddressModel(model.Address,
                address: address,
                loadCountries: () => _countryService.GetAllCountriesForBilling(InovatiqaDefaults.LanguageId),
                excludeProperties: false);

            return View(model);
        }

        [HttpPost]
        public virtual IActionResult AddressEdit(CustomerAddressEditModel model, int addressId, IFormCollection form, int customerId = 0)
        {
            var currentCustomer = _workContextService.CurrentCustomer;
            var customer = _customerService.GetCustomerById(customerId == 0 ? currentCustomer.Id : customerId);
            if (!_customerService.IsRegistered(customer))
                return Challenge();

            var address = _customerService.GetCustomerAddress(customer.Id, addressId);
            if (address == null)
                return RedirectToRoute("BillingAddressList");

            var customAttributes = _addressAttributeParserService.ParseCustomAddressAttributes(form);
            var customAttributeWarnings = _addressAttributeParserService.GetAttributeWarnings(customAttributes);
            foreach (var error in customAttributeWarnings)
            {
                ModelState.AddModelError("", error);
            }

            if (ModelState.IsValid)
            {
                address = model.Address.ToEntity(address);
                address.CustomAttributes = customAttributes;
                _addressService.UpdateAddress(address);

                return RedirectToRoute("BillingAddressList");
            }

            _addressModelFactory.PrepareAddressModel(model.Address,
                address: address,
                loadCountries: () => _countryService.GetAllCountriesForBilling(InovatiqaDefaults.LanguageId),
                excludeProperties: true,
                overrideAttributesXml: customAttributes);
            return View(model);
        }
        public virtual IActionResult BulkDeleteAddresses(IFormCollection form, int customerId = 0)
        {
            var Ids = form["IDtoDelete"];
            var customer = _customerService.GetCustomerById(customerId) ?? _workContextService.CurrentCustomer;
            foreach(var id in Ids)
            {
                try
                {
                    var address = _addressService.GetAddressById(Convert.ToInt32(id));
                    if(address != customer.BillingAddress && address != customer.ShippingAddress)
                    {
                        _addressService.DeleteAddress(address);
                        TempData["Success"] = "true";
                    }
                    else
                    {
                        TempData["Success"] = "false";
                    }
                }
                catch(Exception ex)
                {
                    TempData["Success"] = "false";
                }
            }
            return RedirectToRoute("BillingAddressList");
        }

      
        #endregion

        #region My account / Downloadable products


        #endregion

        #region My account / Change password

        [HttpsRequirement]
        public virtual IActionResult ChangePassword()
        {
            var customer = _workContextService.CurrentCustomer;

            if (!_customerService.IsRegistered(customer))
                return Challenge();

            var model = _customerModelFactory.PrepareChangePasswordModel();

            if (_customerService.PasswordIsExpired(customer))
                ModelState.AddModelError(string.Empty, "Your password has expired, please create a new one");

            return View(model);
        }

        [HttpPost]
        public virtual IActionResult ChangePassword(ChangePasswordModel model)
        {
            var customer = _workContextService.CurrentCustomer;

            if (!_customerService.IsRegistered(customer))
                return Challenge();

            if (ModelState.IsValid)
            {
                if(model.ConfirmNewPassword == model.NewPassword && model.ConfirmNewPassword != null && model.NewPassword != null && model.OldPassword != null && model.NewPassword.Length >= 6)
                {
                    var changePasswordRequest = new ChangePasswordRequest(customer.Email,
                    true, InovatiqaDefaults.Hashed, model.NewPassword, model.OldPassword);
                    var changePasswordResult = _customerRegistrationService.ChangePassword(changePasswordRequest);
                    if (changePasswordResult.Success)
                    {
                        model.Result = "Success";
                        return View(model);
                    }
                    else
                    {
                        model.Result = "There was an error changing your password";
                    }
                    foreach (var error in changePasswordResult.Errors)
                        ModelState.AddModelError("", error);
                }
                else
                {
                    if (model.NewPassword == null || model.OldPassword == null || model.ConfirmNewPassword == null)
                        model.Result = "All fields are required. Please fill all fields.";
                    else if (model.NewPassword != model.ConfirmNewPassword)
                        model.Result = "New Password and Confirm Password are not same";
                    else if (model.NewPassword.Length < 6)
                        model.Result = "Password length should be at least 6 characters";
                }
                
            }

            return View(model);
        }

        [HttpsRequirement]
        public virtual IActionResult PasswordRecovery()
        {
            if (_customerService.IsRegistered(_workContextService.CurrentCustomer))
            {
                return Redirect("~/");
            }
            var model = new PasswordRecoveryModel();
            model = _customerModelFactory.PreparePasswordRecoveryModel(model);

            return View(model);
        }

        [HttpPost, ActionName("PasswordRecovery")]
        [FormValueRequired("send-email")]
        public virtual IActionResult PasswordRecoverySend(PasswordRecoveryModel model, bool captchaValid)
        {
            if (InovatiqaDefaults.Enabled && InovatiqaDefaults.ShowOnForgotPasswordPage && !captchaValid)
            {
                ModelState.AddModelError("", "The reCAPTCHA response is invalid or malformed. Please try again.");
            }

            if (ModelState.IsValid)
            {
                var customer = _customerService.GetCustomerByEmail(model.Email);
                if (customer != null && customer.Active && !customer.Deleted)
                {
                    var passwordRecoveryToken = Guid.NewGuid();
                    _genericAttributeService.SaveAttribute(customer.GetType().Name, customer.Id, InovatiqaDefaults.PasswordRecoveryTokenAttribute,
                        passwordRecoveryToken.ToString());
                    DateTime? generatedDateTime = DateTime.UtcNow;
                    _genericAttributeService.SaveAttribute(customer.GetType().Name, customer.Id,
                        InovatiqaDefaults.PasswordRecoveryTokenDateGeneratedAttribute, generatedDateTime);

                    _workflowMessageService.SendCustomerPasswordRecoveryMessage(customer,
                        InovatiqaDefaults.LanguageId, Convert.ToString(passwordRecoveryToken));

                    model.Result = "Email with instructions has been sent to you.";
                }
                else
                {
                    model.Result = "Email not found.";
                }
            }

            model = _customerModelFactory.PreparePasswordRecoveryModel(model);
            return View(model);
        }

        [HttpsRequirement]
        //available even when navigation is not allowed
        public virtual IActionResult PasswordRecoveryConfirm(string token, string customerGuid, string email = "")
        {
            //For backward compatibility with previous versions where email was used as a parameter in the URL
            //Guid guid;
            Guid guid = Guid.Empty;
            try
            {
                guid = Guid.Parse(customerGuid);
            }
            catch (Exception ex)
            {

            }
            var customer = _customerService.GetCustomerByEmail(email);
            if (customer == null)
                customer = _customerService.GetCustomerByGuid(guid);

            if (customer == null)
                return RedirectToRoute("Homepage");

            if (string.IsNullOrEmpty(_genericAttributeService.GetAttribute<string>(customer, InovatiqaDefaults.PasswordRecoveryTokenAttribute, customer.Id)))
            {
                return base.View(new PasswordRecoveryConfirmModel
                {
                    DisablePasswordChanging = true,
                    Result = "Your password already has been changed. For changing it once more, you need to again recover the password."
                });
            }

            var model = _customerModelFactory.PreparePasswordRecoveryConfirmModel();

            if (!_customerService.IsPasswordRecoveryTokenValid(customer, token))
            {
                model.DisablePasswordChanging = true;
                model.Result = "Wrong password recovery token";
            }

            if (_customerService.IsPasswordRecoveryLinkExpired(customer))
            {
                model.DisablePasswordChanging = true;
                model.Result = "Your password recovery link is expired";
            }
            model.Token = token;
            return View(model);
        }

        [HttpPost, ActionName("PasswordRecoveryConfirm")]
        [FormValueRequired("set-password")]
        public virtual IActionResult PasswordRecoveryConfirmPOST(string token, string customerGuid, PasswordRecoveryConfirmModel model, string email = "")
        {
            var customer = _customerService.GetCustomerByEmail(email);
            if (customer == null)
                customer = _customerService.GetCustomerByGuid(Guid.Parse(customerGuid));

            if (customer == null)
                return RedirectToRoute("Homepage");

            if (!_customerService.IsPasswordRecoveryTokenValid(customer, token))
            {
                model.DisablePasswordChanging = true;
                model.Result = "Wrong password recovery token";
                return View(model);
            }

            if (_customerService.IsPasswordRecoveryLinkExpired(customer))
            {
                model.DisablePasswordChanging = true;
                model.Result = "Your password recovery link is expired";
                return View(model);
            }

            if (ModelState.IsValid)
            {
                var response = _customerRegistrationService.ChangePassword(new ChangePasswordRequest(customer.Email,
                    false, InovatiqaDefaults.Hashed, model.NewPassword));
                if (response.Success)
                {
                    _genericAttributeService.SaveAttribute(customer.GetType().Name, customer.Id, InovatiqaDefaults.PasswordRecoveryTokenAttribute, "");

                    model.DisablePasswordChanging = true;
                    model.Result = "Your password has been changed";
                }
                else
                {
                    model.Result = response.Errors.FirstOrDefault();
                }

                return View(model);
            }

            //If we got this far, something failed, redisplay form
            return View(model);
        }


        public virtual IActionResult AccountActivate(Guid customerGuid)
        {
            var customer = _customerService.GetCustomerByGuid(customerGuid);
            if(customer == null)
            {
                return Challenge();
            }
            customer.Active = true;
            _customerService.UpdateCustomer(customer);
            var model = new LoginModel();
            TempData["Active"] = "Your Account Has been activated. Please Login Below";
            return View("Login", model);
        }
        public virtual IActionResult ManageSubAccounts()
        {
            var customer = _workContextService.CurrentCustomer;

            // customer role implemetation - Manage Sub Accounts
            if (customer.ParentId != null && customer.ParentId != 0)
            {
                if (!_customerService.IsInCustomerRole(customer, "Subaccount_CMS"))
                {
                    return RedirectToAction("NotAllowed", "Common");
                }
            }
            // Customer role implemetation
            var model = _customerModelFactory.GetAllChildAccounts();
            return View(model);
        }
        public virtual IActionResult AddSubAccount(IFormCollection collection = null, int ChildId = 0, bool isEditing = false, bool isSaving = false)
        {
            var customer = _workContextService.CurrentCustomer;

            if (customer.ParentId != null && customer.ParentId != 0)
            {
                if (!_customerService.IsInCustomerRole(customer, "Subaccount_CMS"))
                {
                    return RedirectToAction("NotAllowed", "Common");
                }
            } 
            ChildAccountModel model = new ChildAccountModel();
            if (!isEditing)
            {
                foreach (var c in _countryService.GetAllCountries(InovatiqaDefaults.LanguageId))
                {
                    model.AvailableCountries.Add(new SelectListItem
                    {
                        Text = c.Name,
                        Value = c.Id.ToString(),
                        Selected = c.Id == model.CountryId
                    });
                }
                var states = _stateProvinceService.GetStateProvincesByCountryId(model.CountryId, InovatiqaDefaults.LanguageId).ToList();
                if (states.Any())
                {
                    model.AvailableStates.Add(new SelectListItem { Text = "Select state", Value = "0" });

                    foreach (var s in states)
                    {
                        model.AvailableStates.Add(new SelectListItem { Text = s.Name, Value = s.Id.ToString(), Selected = (s.Id == model.StateProvinceId) });
                    }
                }
                else
                {
                    var anyCountrySelected = model.AvailableCountries.Any(x => x.Selected);

                    model.AvailableStates.Add(new SelectListItem
                    {
                        Text = anyCountrySelected ? "Other" : "Select state",
                        Value = "0"
                    });
                }
            }
            
            // if adding a new account - return simple view
            if (!isEditing && !isSaving && collection.Count == 0)
                return View(model);

            //if adding a new account - account saving

            if (!isEditing && !isSaving && collection.Count > 0)
            {
                if (collection["password1"] != collection["password2"])
                {
                    model.Messages.Add(new System.Collections.Generic.KeyValuePair<string, string>("danger", "Passwords Must Match"));
                    return View(model);
                }
                else if (collection["password1"].ToString().Length < 6)
                {
                    model.Messages.Add(new System.Collections.Generic.KeyValuePair<string, string>("info", "Password Must Contain At Least 6 Characters"));
                    return View(model);
                }
                var child = _customerModelFactory.PrepareChildCustomerModel(collection);
                var address = _customerModelFactory.PrepareChildAddressModel(collection);
                _customerService.InsertCustomer(child);
                _addressService.InsertAddress(address);
                _customerService.InsertCustomerAddress(child, address);
                // implement force use Parent Address

                if (!collection.ContainsKey("Subaccount_FUPA")) // if force use parent address is enabled, user addresses will be ignored and parent addresses will be used
                {
                    child.BillingAddressId = address.Id;
                    child.ShippingAddressId = address.Id;
                    _customerService.UpdateCustomer(child);
                }

                //implementation done
                foreach (var role in _customerService.GetAllCustomerRoles())
                    if (collection.ContainsKey(role.Name) || role.Name == "Registered" || role.Name == "B2B")
                        _customerService.AddCustomerRoleMapping(new CustomerCustomerRoleMapping { CustomerId = child.Id, CustomerRoleId = role.Id });
                var passwordModel = _customerRegistrationService.PrepareChildPasswordModel(collection, child.Id);
                _customerService.InsertCustomerPassword(passwordModel);
                return RedirectToAction("ManageSubAccounts", "Customer");
            }

            // if Editing an Account - Return View with populated values

            if (isEditing && !isSaving)
            {
                var child = _customerService.GetCustomerById(ChildId);
                var childDetails = _customerModelFactory.PrepareChildDetailsModel(child);
                childDetails.isEditing = true;
                foreach (var c in _countryService.GetAllCountries(InovatiqaDefaults.LanguageId))
                {
                    childDetails.AvailableCountries.Add(new SelectListItem
                    {
                        Text = c.Name,
                        Value = c.Id.ToString(),
                        Selected = c.Id == model.CountryId
                    });
                }
                var editStates = _stateProvinceService.GetStateProvincesByCountryId(childDetails.CountryId, InovatiqaDefaults.LanguageId).ToList();
                if (editStates.Any())
                {
                    childDetails.AvailableStates.Add(new SelectListItem { Text = "Select state", Value = "0" });

                    foreach (var s in editStates)
                    {
                        childDetails.AvailableStates.Add(new SelectListItem { Text = s.Name, Value = s.Id.ToString(), Selected = (s.Id == model.StateProvinceId) });
                    }
                }
                else
                {
                    childDetails.AvailableStates.Add(new SelectListItem
                    {
                        Text = childDetails.CountryId != 1 ? "Other" : "Select state",
                        Value = "0"
                    });
                }
                return View(childDetails);
            }

            //if Editing and saving edited Changes
            if (isEditing && isSaving)
            {
                if (collection["password1"] != "")
                    if (collection["password1"] != collection["password2"])
                    {
                        model.Messages.Add(new System.Collections.Generic.KeyValuePair<string, string>("danger", "Passwords Must Match"));
                        return View(model);
                    }
                    else if (collection["password1"].ToString().Length < 6)
                    {
                        model.Messages.Add(new System.Collections.Generic.KeyValuePair<string, string>("info", "Password Must Contain At Least 6 Characters"));
                        return View(model);
                    }
                var child = _customerService.GetCustomerById(ChildId);
                child.Email = collection["email"];
                child.Username = child.Email;
                child.Active = collection["isActive"] == "on";
                if (collection["Subaccount_FUPA"] == "on")
                {
                    child.BillingAddressId = customer.BillingAddressId;
                    child.ShippingAddressId = customer.ShippingAddressId;
                }
                else
                {
                    var childAddress = _customerService.GetAddressesByCustomerId(child.Id).FirstOrDefault();
                    child.BillingAddressId = childAddress.Id;
                    child.ShippingAddressId = childAddress.Id;
                }
                if (collection.ContainsKey("Subaccount_CAO"))
                {
                    child.MaxOrderApprovalValue = collection["OrderApprovalValue"] != "" ? Convert.ToDecimal(collection["OrderApprovalValue"]) : 0;
                }
                if (collection.ContainsKey("Subaccount_RABCO"))
                {
                    child.MaxOrderWithoutApproval = collection["OrderPlacementValue"] != "" ? Convert.ToDecimal(collection["OrderPlacementValue"]) : 0;
                }
                _customerService.UpdateCustomer(child);
                var address = _customerService.GetAddressesByCustomerId(ChildId).FirstOrDefault();
                address.City = collection["city"];
                address.CountryId = int.Parse(collection["countryId"]);
                address.StateProvinceId = Convert.ToInt32(collection["stateProvinceId"]);
                address.Address1 = collection["address1"];
                address.Address2 = collection["address2"];
                address.ZipPostalCode = collection["zip"];
                address.PhoneNumber = collection["phone"];
                address.FirstName = collection["firstName"];
                address.LastName = collection["lastName"];
                address.Email = collection["email"];
                if (collection["Subaccount_FUPCN"] == "on")
                {
                    address.Company = _addressService.GetAddressById(Convert.ToInt32(customer.BillingAddressId)).Company;
                }
                else
                {
                    address.Company = collection["company"];
                }
                _addressService.UpdateAddress(address);

                _customerService.RemoveAllCustomerRoleMappings(ChildId);
                foreach (var role in _customerService.GetAllCustomerRoles())
                    if (collection.ContainsKey(role.Name) || role.Name == "Registered" || role.Name == "B2B")
                        _customerService.AddCustomerRoleMapping(new CustomerCustomerRoleMapping { CustomerId = ChildId, CustomerRoleId = role.Id });

                if(collection["password1"] != "")
                {
                    var response = _customerRegistrationService.ChangePassword(new ChangePasswordRequest(collection["email"],
                                                                               false, InovatiqaDefaults.Hashed, collection["password1"]));
                }
                return RedirectToAction("ManageSubAccounts", "Customer");
            }
            return View(model);
        }
        public virtual JsonResult DeleteChildAccount(int Id)
        {
            try
            {
                var childAccount = _customerService.GetCustomerById(Id);
                _customerService.DeleteCustomer(childAccount);
                _customerService.RemoveAllCustomerRoleMappings(Id);
                return Json(true);
            }
            catch(Exception ex)
            {
                return Json(new { success = false, message = ex.Message});
            }
        }

        public virtual JsonResult GetProductForModal(int productID)
        {
            var product = _productService.GetProductById(productID);
            return Json(new { Name = product.Name});
        }

        public virtual IActionResult ChooseBillingAddress()
        {
            //edit by hamza
            var customer = _workContextService.CurrentCustomer;
            var model = new CustomerAddressListModel();
            if (customer.ParentId != null && customer.ParentId != 0)
            {
                //var sibingCustomer = _customerService.getAllChildAccounts(_customerService.GetCustomerById(Convert.ToInt32(customer.ParentId)));
                var sibingCustomer = _customerService.getAllChildAccounts(customer);
                if (_customerService.IsInCustomerRole(customer, "Subaccount_MAB"))
                {
                    foreach (var customerId in sibingCustomer)
                    {
                        foreach(var address in _customerService.GetAddressesByCustomerId(customerId).ToList())
                        {
                            address.StateProvince = _stateProvinceService.GetStateProvinceById(Convert.ToInt32(address.StateProvinceId));
                            model.Addresses.Add(new Models.Common.AddressModel
                            {
                                Id = address.Id,
                                Address1 = address.Address1,
                                Address2 = address.Address2,
                                City = address.City,
                                Company = address.Company,
                                CustomerId = customerId,
                                FirstName = address.FirstName,
                                LastName = address.LastName,
                                Email = address.Email,
                                ZipPostalCode = address.ZipPostalCode,
                                PhoneNumber = address.PhoneNumber,
                                CountryName = Convert.ToString(address.Country),
                                StateProvinceId = address.StateProvinceId,
                                StateProvinceName = Convert.ToString(address.StateProvince?.Name ?? "")
                            });
                        }
                    }
                    return View(model);
                }
            }
            var ChildCustomer = _customerService.getAllChildAccountsIds(customer);
            var subAccountAddressId = new List<int>(); 
            foreach (var sibling in ChildCustomer)
            {
                var subAccount = _customerService.GetCustomerById(sibling);
                subAccountAddressId.Add(Convert.ToInt32(subAccount.BillingAddressId));
                subAccountAddressId.Add(Convert.ToInt32(subAccount.ShippingAddressId));

            }
            subAccountAddressId = subAccountAddressId.Distinct().ToList();
            foreach(var address in _customerService.GetAddressesByCustomerId(_workContextService.CurrentCustomer.Id).ToList())
            {
               address.StateProvince = _stateProvinceService.GetStateProvinceById(Convert.ToInt32(address.StateProvinceId));
                model.Addresses.Add(new Models.Common.AddressModel
                {
                    Id = address.Id, 
                    Address1 = address.Address1,
                    Address2 = address.Address2,
                    City = address.City,
                    Company = address.Company,
                    FirstName = address.FirstName,
                    LastName = address.LastName,
                    Email = address.Email,
                    ZipPostalCode = address.ZipPostalCode,
                    PhoneNumber = address.PhoneNumber,
                    CountryName = Convert.ToString(address.Country),
                    StateProvinceId = address.StateProvinceId,
                    StateProvinceName = Convert.ToString(address.StateProvince?.Name ?? ""),
                    HasCustomerBillingAddress = address.CustomerBillingAddress.Count > 0 ? true : false,
                    HasCustomerShippingAddress = address.CustomerShippingAddress.Count > 0 ? true : false,
                    IsSiblingAddress = subAccountAddressId.Contains(address.Id) ? true : false
                });
                if(subAccountAddressId.Contains(address.Id))
                {
                    model.HasSubAccount = 1;
                }
                else
                {
                    model.HasSubAccount = 0;
                }
            }
            return View(model);
        }
        //public virtual IActionResult ChooseShippingAddress()
        //{
        //    var model = _customerService.GetAddressesByCustomerId(_workContextService.CurrentCustomer.Id);
        //    for (int i = 0; i < model.Count; i++)
        //    {
        //        model[i].StateProvince = _stateProvinceService.GetStateProvinceById(Convert.ToInt32(model[i].StateProvinceId));
        //    }
        //    return View(model);
        //}
        public virtual IActionResult ChooseShippingAddress()
        {
            //edit by hamza
            var customer = _workContextService.CurrentCustomer;
            var model = new CustomerAddressListModel();
            if (customer.ParentId != null && customer.ParentId != 0)
            {
                //var sibingCustomer = _customerService.getAllChildAccounts(_customerService.GetCustomerById(Convert.ToInt32(customer.ParentId)));
                var sibingCustomer = _customerService.getAllChildAccounts(customer);
                if (_customerService.IsInCustomerRole(customer, "Subaccount_MAB"))
                {
                    foreach (var customerId in sibingCustomer)
                    {
                        foreach (var address in _customerService.GetAddressesByCustomerId(customerId).ToList())
                        {
                            address.StateProvince = _stateProvinceService.GetStateProvinceById(Convert.ToInt32(address.StateProvinceId));
                            model.Addresses.Add(new Models.Common.AddressModel
                            {
                                Id = address.Id,
                                Address1 = address.Address1,
                                Address2 = address.Address2,
                                City = address.City,
                                Company = address.Company,
                                CustomerId = customerId,
                                FirstName = address.FirstName,
                                LastName = address.LastName,
                                Email = address.Email,
                                ZipPostalCode = address.ZipPostalCode,
                                PhoneNumber = address.PhoneNumber,
                                CountryName = Convert.ToString(address.Country),
                                StateProvinceId = address.StateProvinceId,
                                StateProvinceName = Convert.ToString(address.StateProvince?.Name ?? "")
                            });
                        }
                    }
                    return View(model);
                }
            }
            var ChildCustomer = _customerService.getAllChildAccountsIds(customer);
            var subAccountAddressId = new List<int>();
            foreach (var sibling in ChildCustomer)
            {
                var subAccount = _customerService.GetCustomerById(sibling);
                subAccountAddressId.Add(Convert.ToInt32(subAccount.BillingAddressId));
                subAccountAddressId.Add(Convert.ToInt32(subAccount.ShippingAddressId));

            }
            subAccountAddressId = subAccountAddressId.Distinct().ToList();
            foreach (var address in _customerService.GetAddressesByCustomerId(_workContextService.CurrentCustomer.Id).ToList())
            {
                address.StateProvince = _stateProvinceService.GetStateProvinceById(Convert.ToInt32(address.StateProvinceId));
                model.Addresses.Add(new Models.Common.AddressModel
                {
                    Id = address.Id,
                    Address1 = address.Address1,
                    Address2 = address.Address2,
                    City = address.City,
                    Company = address.Company,
                    FirstName = address.FirstName,
                    LastName = address.LastName,
                    Email = address.Email,
                    ZipPostalCode = address.ZipPostalCode,
                    PhoneNumber = address.PhoneNumber,
                    CountryName = Convert.ToString(address.Country),
                    StateProvinceId = address.StateProvinceId,
                    StateProvinceName = Convert.ToString(address.StateProvince?.Name ?? ""),
                    HasCustomerBillingAddress = address.CustomerBillingAddress.Count > 0 ? true : false,
                    HasCustomerShippingAddress = address.CustomerShippingAddress.Count > 0 ? true : false,
                    IsSiblingAddress = subAccountAddressId.Contains(address.Id) ? true : false
                });
                if (subAccountAddressId.Contains(address.Id))
                {
                    model.HasSubAccount = 1;
                }
                else
                {
                    model.HasSubAccount = 0;
                }
            }
            return View(model);
        }
        public virtual IActionResult BlueBandData()
        {
            var customer = _workContextService.CurrentCustomer;
            var BillIngAddress = _addressService.GetAddressById(Convert.ToInt32(customer.BillingAddressId));
            var ShippingAddress = _addressService.GetAddressById(Convert.ToInt32(customer.ShippingAddressId));
            return Json(new {
                CustomerId = customer.Id,
                FullName = BillIngAddress != null ? (BillIngAddress.FirstName + " " + BillIngAddress.LastName) : " ",
                BillingAddress = BillIngAddress != null ? 
                new {
                    Id = Convert.ToString(BillIngAddress.Id),
                    Company = BillIngAddress.Company,
                    Address = BillIngAddress.Address1,
                    City = BillIngAddress.City,
                    State = _stateProvinceService.GetStateProvinceById(Convert.ToInt32(BillIngAddress.StateProvinceId))?.Abbreviation,
                    Zip = BillIngAddress.ZipPostalCode,
                } : 
                new
                {
                    Id = "",
                    Company = "",
                    Address = "",
                    City = "",
                    State = "",
                    Zip = ""
                },
                ShippingAddress = ShippingAddress != null ? new
                {
                    Id = Convert.ToString(ShippingAddress.Id),
                    Company = ShippingAddress.Company,
                    Address = ShippingAddress.Address1,
                    City = ShippingAddress.City,
                    State = _stateProvinceService.GetStateProvinceById(Convert.ToInt32(ShippingAddress.StateProvinceId))?.Abbreviation,
                    Zip = ShippingAddress.ZipPostalCode,
                } : 
                new
                {
                    Id = "",
                    Company = "",
                    Address = "",
                    City = "",
                    State = "",
                    Zip = ""
                }
            });
        }
        public virtual IActionResult SwitchDefaultAddress(int type)
        {
            var model = new ChooseAddressModel();
            var customer = _workContextService.CurrentCustomer;
            model.AllAddresses = _customerService.GetAddressesByCustomerId(customer.Id);
            model.DefaultAddress = _addressService.GetAddressById(Convert.ToInt32(customer.BillingAddressId));
            for(int i = 0; i < model.AllAddresses.Count; i++)
            {
                model.AllAddresses[i].StateProvince = _stateProvinceService.GetStateProvinceById(Convert.ToInt32(model.AllAddresses[i].StateProvinceId));
            }
            model.Type = type;
            return View(model);
        }
        public virtual IActionResult SwitchAddress(int type, int id)
        {
            var customer = _workContextService.CurrentCustomer;
            try
            {
                _customerService.GetCustomerAddress(customer.Id, id);
                if (type == 1)
                    customer.BillingAddressId = id;
                else
                    customer.ShippingAddressId = id;
                _customerService.UpdateCustomer(customer);
            }
            catch(Exception ex)
            {
                throw ex;
            }
            return RedirectToAction("AccountInformation");
        }
        #endregion

        #region My account / Avatar


        #endregion

        #region GDPR tools


        #endregion

        #region Check gift card balance


        #endregion

        #endregion
    }
}