using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Inovatiqa.Core;
using Inovatiqa.Database.Models;
using Inovatiqa.Services.Customers.Interfaces;
using Inovatiqa.Services.Helpers.Interfaces;
using Inovatiqa.Services.Security;
using Inovatiqa.Services.Security.Interfaces;
using Inovatiqa.Web.Areas.Admin.Models.Customers;
using Inovatiqa.Web.Areas.Admin.Factories.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Primitives;
using Inovatiqa.Services.Messages.Interfaces;
using Inovatiqa.Services.Common.Interfaces;
using Inovatiqa.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Inovatiqa.Web.Controllers;
using Inovatiqa.Core.Domain.Messages;
using Inovatiqa.Services.WorkContext.Interfaces;
using Inovatiqa.Web.Mvc;
using Inovatiqa.Services.Customers;
using Inovatiqa.Web.Mvc.Filters;

namespace Inovatiqa.Web.Areas.Admin.Controllers
{
    public partial class CustomerController : BaseAdminController
    {
        #region Fields

        private readonly IPermissionService _permissionService;
        private readonly IDateTimeHelperService _dateTimeHelperService;
        private readonly ICustomerService _customerService;
        private readonly ICustomerModelFactory _customerModelFactory;
        private readonly INotificationService _notificationService;
        private readonly ICustomerAttributeService _customerAttributeService;
        private readonly ICustomerAttributeParserService _customerAttributeParserService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly INewsLetterSubscriptionService _newsLetterSubscriptionService;
        private readonly IAddressAttributeParserService _addressAttributeParserService;
        private readonly IAddressService _addressService;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly IEmailAccountService _emailAccountService;
        private readonly IQueuedEmailService _queuedEmailService;
        private readonly IWorkContextService _workContextService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ICustomerRegistrationService _customerRegistrationService;

        #endregion

        #region Ctor

        public CustomerController(IPermissionService permissionService,
            IDateTimeHelperService dateTimeHelperService,
            ICustomerService customerService,
            INotificationService notificationService,
            ICustomerModelFactory customerModelFactory,
            ICustomerAttributeService customerAttributeService,
            ICustomerAttributeParserService customerAttributeParserService,
            IGenericAttributeService genericAttributeService,
            INewsLetterSubscriptionService newsLetterSubscriptionService,
            IAddressAttributeParserService addressAttributeParserService,
             IAddressService addressService,
             IEmailAccountService emailAccountService,
             IWorkflowMessageService workflowMessageService,
             IQueuedEmailService queuedEmailService,
             IWorkContextService workContextService,
             ICustomerActivityService customerActivityService,
             ICustomerRegistrationService customerRegistrationService,
             IRazorViewEngine viewEngine) : base(viewEngine)
        {
            _permissionService = permissionService;
            _dateTimeHelperService = dateTimeHelperService;
            _customerService = customerService;
            _notificationService = notificationService;
            _customerModelFactory = customerModelFactory;
            _customerAttributeService = customerAttributeService;
            _customerAttributeParserService = customerAttributeParserService;
            _genericAttributeService = genericAttributeService;
            _newsLetterSubscriptionService = newsLetterSubscriptionService;
            _addressAttributeParserService = addressAttributeParserService;
            _addressService = addressService;
            _workflowMessageService = workflowMessageService;
            _emailAccountService = emailAccountService;
            _queuedEmailService = queuedEmailService;
            _workContextService = workContextService;
            _customerActivityService = customerActivityService;
            _customerRegistrationService = customerRegistrationService;
        }

        #endregion

        #region Utilities

        private bool SecondAdminAccountExists(Customer customer)
        {
            var customers = _customerService.GetAllCustomers(customerRoleIds: new[] { _customerService.GetCustomerRoleBySystemName(InovatiqaDefaults.AdministratorsRoleName).Id });

            return customers.Any(c => c.Active && c.Id != customer.Id);
        }

        protected virtual string ParseCustomCustomerAttributes(IFormCollection form)
        {
            if (form == null)
                throw new ArgumentNullException(nameof(form));

            var attributesXml = string.Empty;
            var customerAttributes = _customerAttributeService.GetAllCustomerAttributes();
            foreach (var attribute in customerAttributes)
            {
                var controlId = $"{InovatiqaDefaults.CustomerAttributePrefix}{attribute.Id}";
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
                                attributesXml = _customerAttributeParserService.AddCustomerAttribute(attributesXml,
                                    attribute, selectedAttributeId.ToString());
                        }

                        break;
                    case (int)AttributeControlType.Checkboxes:
                        var cblAttributes = form[controlId];
                        if (!StringValues.IsNullOrEmpty(cblAttributes))
                        {
                            foreach (var item in cblAttributes.ToString()
                                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                            {
                                var selectedAttributeId = int.Parse(item);
                                if (selectedAttributeId > 0)
                                    attributesXml = _customerAttributeParserService.AddCustomerAttribute(attributesXml,
                                        attribute, selectedAttributeId.ToString());
                            }
                        }

                        break;
                    case (int)AttributeControlType.ReadonlyCheckboxes:
                        var attributeValues = _customerAttributeService.GetCustomerAttributeValues(attribute.Id);
                        foreach (var selectedAttributeId in attributeValues
                            .Where(v => v.IsPreSelected)
                            .Select(v => v.Id)
                            .ToList())
                        {
                            attributesXml = _customerAttributeParserService.AddCustomerAttribute(attributesXml,
                                attribute, selectedAttributeId.ToString());
                        }

                        break;
                    case (int)AttributeControlType.TextBox:
                    case (int)AttributeControlType.MultilineTextbox:
                        ctrlAttributes = form[controlId];
                        if (!StringValues.IsNullOrEmpty(ctrlAttributes))
                        {
                            var enteredText = ctrlAttributes.ToString().Trim();
                            attributesXml = _customerAttributeParserService.AddCustomerAttribute(attributesXml,
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

        protected virtual string ValidateCustomerRoles(IList<CustomerRole> customerRoles, IList<CustomerRole> existingCustomerRoles)
        {
            if (customerRoles == null)
                throw new ArgumentNullException(nameof(customerRoles));

            if (existingCustomerRoles == null)
                throw new ArgumentNullException(nameof(existingCustomerRoles));

            var rolesToAdd = customerRoles.Except(existingCustomerRoles);
            var rolesToDelete = existingCustomerRoles.Except(customerRoles);
            if (rolesToAdd.Any(role => role.SystemName != InovatiqaDefaults.RegisteredRoleName) || rolesToDelete.Any())
            {
                if (!_permissionService.Authorize(StandardPermissionProvider.ManageAcl))
                    return "Not enough rights to manage customer roles.";
            }

            var isInGuestsRole = customerRoles.FirstOrDefault(cr => cr.SystemName == InovatiqaDefaults.GuestsRoleName) != null;
            var isInRegisteredRole = customerRoles.FirstOrDefault(cr => cr.SystemName == InovatiqaDefaults.RegisteredRoleName) != null;
            if (isInGuestsRole && isInRegisteredRole)
                return "The customer cannot be in both 'Guests' and 'Registered' customer roles";
            if (!isInGuestsRole && !isInRegisteredRole)
                return "Add the customer to 'Guests' or 'Registered' customer role";

            return string.Empty;
        }

        #endregion

        #region Customers

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("send-welcome-message")]
        public virtual IActionResult SendWelcomeMessage(CustomerModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            var customer = _customerService.GetCustomerById(model.Id);
            if (customer == null)
                return RedirectToAction("List");

            _workflowMessageService.SendCustomerWelcomeMessage(customer, InovatiqaDefaults.LanguageId);

            _notificationService.SuccessNotification("Welcome email has been successfully sent.");

            return RedirectToAction("Edit", new { id = customer.Id });
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("resend-activation-message")]
        public virtual IActionResult ReSendActivationMessage(CustomerModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            var customer = _customerService.GetCustomerById(model.Id);
            if (customer == null)
                return RedirectToAction("List");

            _genericAttributeService.SaveAttribute(customer.GetType().Name, customer.Id, InovatiqaDefaults.AccountActivationTokenAttribute, Guid.NewGuid().ToString());
            _workflowMessageService.SendCustomerEmailValidationMessage(customer, InovatiqaDefaults.LanguageId);

            _notificationService.SuccessNotification("Activation email has been successfully sent.");

            return RedirectToAction("Edit", new { id = customer.Id });
        }

        public virtual IActionResult SendEmail(CustomerModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            var customer = _customerService.GetCustomerById(model.Id);
            if (customer == null)
                return RedirectToAction("List");

            try
            {
                if (string.IsNullOrWhiteSpace(customer.Email))
                    throw new InovatiqaException("Customer email is empty");
                if (!CommonHelper.IsValidEmail(customer.Email))
                    throw new InovatiqaException("Customer email is not valid");
                if (string.IsNullOrWhiteSpace(model.SendEmail.Subject))
                    throw new InovatiqaException("Email subject is empty");
                if (string.IsNullOrWhiteSpace(model.SendEmail.Body))
                    throw new InovatiqaException("Email body is empty");

                var emailAccount = _emailAccountService.GetEmailAccountById(InovatiqaDefaults.DefaultEmailAccountId);
                if (emailAccount == null)
                    emailAccount = _emailAccountService.GetAllEmailAccounts().FirstOrDefault();
                if (emailAccount == null)
                    throw new InovatiqaException("Email account can't be loaded");
                var email = new QueuedEmail
                {
                    PriorityId = (int)QueuedEmailPriority.High,
                    EmailAccountId = emailAccount.Id,
                    FromName = emailAccount.DisplayName,
                    From = emailAccount.Email,
                    ToName = _customerService.GetCustomerFullName(customer),
                    To = customer.Email,
                    Subject = model.SendEmail.Subject,
                    Body = model.SendEmail.Body,
                    CreatedOnUtc = DateTime.UtcNow,
                    DontSendBeforeDateUtc = model.SendEmail.SendImmediately || !model.SendEmail.DontSendBeforeDate.HasValue ?
                        null : (DateTime?)_dateTimeHelperService.ConvertToUtcTime(model.SendEmail.DontSendBeforeDate.Value)
                };
                _queuedEmailService.InsertQueuedEmail(email);

                _notificationService.SuccessNotification("The email has been queued successfully.");
            }
            catch (Exception exc)
            {
                _notificationService.ErrorNotification(exc.Message);
            }

            return RedirectToAction("Edit", new { id = customer.Id });
        }

        //public virtual IActionResult SendPm(CustomerModel model)
        //{
        //    if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
        //        return AccessDeniedView();

        //    var customer = _customerService.GetCustomerById(model.Id);
        //    if (customer == null)
        //        return RedirectToAction("List");

        //    try
        //    {
        //        if (_customerService.IsGuest(customer))
        //            throw new InovatiqaException("Customer should be registered");
        //        if (string.IsNullOrWhiteSpace(model.SendPm.Subject))
        //            throw new InovatiqaException("Subject cannot be empty");
        //        if (string.IsNullOrWhiteSpace(model.SendPm.Message))
        //            throw new InovatiqaException("Message cannot be empty");

        //        var privateMessage = new PrivateMessage
        //        {
        //            StoreId = InovatiqaDefaults.StoreId,
        //            ToCustomerId = customer.Id,
        //            FromCustomerId = _workContextService.CurrentCustomer.Id,
        //            Subject = model.SendPm.Subject,
        //            Text = model.SendPm.Message,
        //            IsDeletedByAuthor = false,
        //            IsDeletedByRecipient = false,
        //            IsRead = false,
        //            CreatedOnUtc = DateTime.UtcNow
        //        };

        //        _forumService.InsertPrivateMessage(privateMessage);

        //        _notificationService.SuccessNotification("The PM has been sent successfully.");
        //    }
        //    catch (Exception exc)
        //    {
        //        _notificationService.ErrorNotification(exc.Message);
        //    }

        //    return RedirectToAction("Edit", new { id = customer.Id });
        //}

        [HttpPost]
        public virtual IActionResult Delete(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            var customer = _customerService.GetCustomerById(id);
            if (customer == null)
                return RedirectToAction("List");

            try
            { 
                if (_customerService.IsAdmin(customer) && !SecondAdminAccountExists(customer))
                {
                    _notificationService.ErrorNotification("You can't delete the last administrator. At least one administrator account should exists.");
                    return RedirectToAction("Edit", new { id = customer.Id });
                }

                if (_customerService.IsAdmin(customer) && !_customerService.IsAdmin(_workContextService.CurrentCustomer))
                {
                    _notificationService.ErrorNotification("You're not allowed to delete administrators. Only administrators can do it.");
                    return RedirectToAction("Edit", new { id = customer.Id });
                }

                _customerService.DeleteCustomer(customer);

            var subscription = _newsLetterSubscriptionService.GetNewsLetterSubscriptionByEmailAndStoreId(customer.Email, InovatiqaDefaults.StoreId);
            if (subscription != null)
                _newsLetterSubscriptionService.DeleteNewsLetterSubscription(subscription);

            _customerActivityService.InsertActivity("DeleteCustomer",
                    string.Format("Deleted a customer (ID = {0})", customer.Id));

                _notificationService.SuccessNotification("The customer has been deleted successfully.");

                return RedirectToAction("List");
            }
            catch (Exception exc)
            {
                _notificationService.ErrorNotification(exc.Message);
                return RedirectToAction("Edit", new { id = customer.Id });
            }
        }

        public virtual IActionResult Edit(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            var customer = _customerService.GetCustomerById(id);
            if (customer == null || customer.Deleted)
                return RedirectToAction("List");

            var model = _customerModelFactory.PrepareCustomerModel(null, customer);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [FormValueRequired("save", "save-continue")]
        public virtual IActionResult Edit(CustomerModel model, bool continueEditing, IFormCollection form)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            var customer = _customerService.GetCustomerById(model.Id);
            if (customer == null || customer.Deleted)
                return RedirectToAction("List");

            var allCustomerRoles = _customerService.GetAllCustomerRoles(true);
            var newCustomerRoles = new List<CustomerRole>();
            foreach (var customerRole in allCustomerRoles)
                if (model.SelectedCustomerRoleIds.Contains(customerRole.Id))
                    newCustomerRoles.Add(customerRole);

            customer.PaymentModeId = model.PaymentModeId;
            customer.PaymentTermsId = model.PaymentTermsId;
            customer.CreditLimit = model.CreditLimit;

            if(customer.PaymentModeId == (int)PaymentModes.PaymentTerms)
            {
                var poRole = allCustomerRoles.Where(x => x.Name == InovatiqaDefaults.PORoleName).FirstOrDefault();
                if (model.SelectedCustomerRoleIds.Contains(poRole.Id) == false)
                    model.SelectedCustomerRoleIds.Add(poRole.Id);
            }
            else
            {
                var poRole = allCustomerRoles.Where(x => x.Name == InovatiqaDefaults.PORoleName).FirstOrDefault();
                if (poRole != null && model.SelectedCustomerRoleIds.Contains(poRole.Id) == true)
                    model.SelectedCustomerRoleIds.Remove(poRole.Id);
                customer.PaymentTermsId = null;
                customer.CreditLimit = 0.0m;
            }

            var customerRolesError = ValidateCustomerRoles(newCustomerRoles, _customerService.GetCustomerRoles(customer));

            if (!string.IsNullOrEmpty(customerRolesError))
            {
                ModelState.AddModelError(string.Empty, customerRolesError);
                _notificationService.ErrorNotification(customerRolesError);
            }

            if (newCustomerRoles.Any() && newCustomerRoles.FirstOrDefault(c => c.SystemName == InovatiqaDefaults.RegisteredRoleName) != null &&
                !CommonHelper.IsValidEmail(model.Email))
            {
                ModelState.AddModelError(string.Empty, "Valid Email is required for customer to be in 'Registered' role");
                _notificationService.ErrorNotification("Valid Email is required for customer to be in 'Registered' role");
            }

            var customerAttributesXml = ParseCustomCustomerAttributes(form);
            if (newCustomerRoles.Any() && newCustomerRoles.FirstOrDefault(c => c.SystemName == InovatiqaDefaults.RegisteredRoleName) != null)
            {
                var customerAttributeWarnings = _customerAttributeParserService.GetAttributeWarnings(customerAttributesXml);
                foreach (var error in customerAttributeWarnings)
                {
                    ModelState.AddModelError(string.Empty, error);
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    customer.AdminComment = model.AdminComment;
                    customer.IsTaxExempt = model.IsTaxExempt;

                    if (!_customerService.IsAdmin(customer) || model.Active || SecondAdminAccountExists(customer))
                        customer.Active = model.Active;
                    else
                        _notificationService.ErrorNotification("You can't deactivate the last administrator. At least one administrator account should exists.");

                    if (!string.IsNullOrWhiteSpace(model.Email))
                        _customerRegistrationService.SetEmail(customer, model.Email, false);
                    else
                        customer.Email = model.Email;

                    if (InovatiqaDefaults.UsernamesEnabled)
                    {
                        if (!string.IsNullOrWhiteSpace(model.Username))
                            _customerRegistrationService.SetUsername(customer, model.Username);
                        else
                            customer.Username = model.Username;
                    }
                    //vendor
                    customer.VendorId = model.VendorId;

                    if (InovatiqaDefaults.AllowCustomersToSetTimeZone)
                        _genericAttributeService.SaveAttribute(customer.GetType().Name, customer.Id, InovatiqaDefaults.TimeZoneIdAttribute, model.TimeZoneId);
                    if (InovatiqaDefaults.GenderEnabled)
                        _genericAttributeService.SaveAttribute(customer.GetType().Name, customer.Id, InovatiqaDefaults.GenderAttribute, model.Gender);
                    if (InovatiqaDefaults.FirstNameEnabled)
                        _genericAttributeService.SaveAttribute(customer.GetType().Name, customer.Id, InovatiqaDefaults.FirstNameAttribute, model.FirstName);
                    if (InovatiqaDefaults.LastNameEnabled)
                        _genericAttributeService.SaveAttribute(customer.GetType().Name, customer.Id, InovatiqaDefaults.LastNameAttribute, model.LastName);
                    if (InovatiqaDefaults.DateOfBirthEnabled)
                        _genericAttributeService.SaveAttribute(customer.GetType().Name, customer.Id, InovatiqaDefaults.DateOfBirthAttribute, model.DateOfBirth);
                    if (InovatiqaDefaults.CompanyEnabled)
                        _genericAttributeService.SaveAttribute(customer.GetType().Name, customer.Id, InovatiqaDefaults.CompanyAttribute, model.Company);
                    if (InovatiqaDefaults.StreetAddressEnabled)
                        _genericAttributeService.SaveAttribute(customer.GetType().Name, customer.Id, InovatiqaDefaults.StreetAddressAttribute, model.StreetAddress);
                    if (InovatiqaDefaults.StreetAddress2Enabled)
                        _genericAttributeService.SaveAttribute(customer.GetType().Name, customer.Id, InovatiqaDefaults.StreetAddress2Attribute, model.StreetAddress2);
                    if (InovatiqaDefaults.ZipPostalCodeEnabled)
                        _genericAttributeService.SaveAttribute(customer.GetType().Name, customer.Id, InovatiqaDefaults.ZipPostalCodeAttribute, model.ZipPostalCode);
                    if (InovatiqaDefaults.CityEnabled)
                        _genericAttributeService.SaveAttribute(customer.GetType().Name, customer.Id, InovatiqaDefaults.CityAttribute, model.City);
                    if (InovatiqaDefaults.CountyEnabled)
                        _genericAttributeService.SaveAttribute(customer.GetType().Name, customer.Id, InovatiqaDefaults.CountyAttribute, model.County);
                    if (InovatiqaDefaults.CountryEnabled)
                        _genericAttributeService.SaveAttribute(customer.GetType().Name, customer.Id, InovatiqaDefaults.CountryIdAttribute, model.CountryId);
                    if (InovatiqaDefaults.CountryEnabled && InovatiqaDefaults.StateProvinceEnabled)
                        _genericAttributeService.SaveAttribute(customer.GetType().Name, customer.Id, InovatiqaDefaults.StateProvinceIdAttribute, model.StateProvinceId);
                    if (InovatiqaDefaults.PhoneEnabled)
                        _genericAttributeService.SaveAttribute(customer.GetType().Name, customer.Id, InovatiqaDefaults.PhoneAttribute, model.Phone);
                    if (InovatiqaDefaults.FaxEnabled)
                        _genericAttributeService.SaveAttribute(customer.GetType().Name, customer.Id, InovatiqaDefaults.FaxAttribute, model.Fax);

                    _genericAttributeService.SaveAttribute(customer.GetType().Name, customer.Id, InovatiqaDefaults.CustomCustomerAttributes, customerAttributesXml);

                    //if (!string.IsNullOrEmpty(customer.Email))
                    //{
                    //    var allStores = _storeService.GetAllStores();
                    //    foreach (var store in allStores)
                    //    {
                    //        var newsletterSubscription = _newsLetterSubscriptionService
                    //            .GetNewsLetterSubscriptionByEmailAndStoreId(customer.Email, store.Id);
                    //        if (model.SelectedNewsletterSubscriptionStoreIds != null &&
                    //            model.SelectedNewsletterSubscriptionStoreIds.Contains(store.Id))
                    //        {
                    //            //subscribed
                    //            if (newsletterSubscription == null)
                    //            {
                    //                _newsLetterSubscriptionService.InsertNewsLetterSubscription(new NewsLetterSubscription
                    //                {
                    //                    NewsLetterSubscriptionGuid = Guid.NewGuid(),
                    //                    Email = customer.Email,
                    //                    Active = true,
                    //                    StoreId = store.Id,
                    //                    CreatedOnUtc = DateTime.UtcNow
                    //                });
                    //            }
                    //        }
                    //        else
                    //        {
                    //            //not subscribed
                    //            if (newsletterSubscription != null)
                    //            {
                    //                _newsLetterSubscriptionService.DeleteNewsLetterSubscription(newsletterSubscription);
                    //            }
                    //        }
                    //    }
                    //}

                    var currentCustomerRoleIds = _customerService.GetCustomerRoleIds(customer, true);

                    foreach (var customerRole in allCustomerRoles)
                    {
                        if (customerRole.SystemName == InovatiqaDefaults.AdministratorsRoleName &&
                            !_customerService.IsAdmin(_workContextService.CurrentCustomer))
                            continue;

                        if (model.SelectedCustomerRoleIds.Contains(customerRole.Id))
                        {
                            //new role
                            if (currentCustomerRoleIds.All(roleId => roleId != customerRole.Id) || newCustomerRoles.All(rol => rol.Id != customerRole.Id))
                                _customerService.AddCustomerRoleMapping(new CustomerCustomerRoleMapping { CustomerId = customer.Id, CustomerRoleId = customerRole.Id });
                        }
                        else
                        {
                            if (customerRole.SystemName == InovatiqaDefaults.AdministratorsRoleName && !SecondAdminAccountExists(customer))
                            {
                                _notificationService.ErrorNotification("You can't remove the Administrator role. At least one administrator account should exists.");
                                continue;
                            }

                            //remove role
                            if (currentCustomerRoleIds.Any(roleId => roleId == customerRole.Id) || newCustomerRoles.Any(rol => rol.Id == customerRole.Id))
                                _customerService.RemoveCustomerRoleMapping(customer, customerRole);
                        }
                    }

                    _customerService.UpdateCustomer(customer);

                    if (_customerService.IsAdmin(customer) && customer.VendorId > 0)
                    {
                        customer.VendorId = 0;
                        _customerService.UpdateCustomer(customer);
                        _notificationService.ErrorNotification("A customer with a vendor associated could not be in \"Administrators\" role.");
                    }

                    if (_customerService.IsVendor(customer) && customer.VendorId == 0)
                    {
                        var vendorRole = _customerService.GetCustomerRoleBySystemName(InovatiqaDefaults.VendorsRoleName);
                        _customerService.RemoveCustomerRoleMapping(customer, vendorRole);

                        _notificationService.ErrorNotification("A customer in the Vendors role should have a vendor account associated.");
                    }

                    //activity log
                    _customerActivityService.InsertActivity("EditCustomer",
                        string.Format("Edited a customer (ID = {0})", customer.Id));

                    _notificationService.SuccessNotification("The customer has been updated successfully.");

                    if (!continueEditing)
                        return RedirectToAction("List");

                    return RedirectToAction("Edit", new { id = customer.Id });
                }
                catch (Exception exc)
                {
                    _notificationService.ErrorNotification(exc.Message);
                }
            }

            //prepare model
            model = _customerModelFactory.PrepareCustomerModel(model, customer, true);

            //if we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("changepassword")]
        public virtual IActionResult ChangePassword(CustomerModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            var customer = _customerService.GetCustomerById(model.Id);
            if (customer == null)
                return RedirectToAction("List");

            if (_customerService.IsAdmin(customer) && !_customerService.IsAdmin(_workContextService.CurrentCustomer))
            {
                _notificationService.ErrorNotification("You're not allowed to change passwords of administrators. Only administrators can do it.");
                return RedirectToAction("Edit", new { id = customer.Id });
            }

            if (!ModelState.IsValid)
                return RedirectToAction("Edit", new { id = customer.Id });

            var changePassRequest = new ChangePasswordRequest(model.Email,
                false, InovatiqaDefaults.Hashed, model.Password);
            var changePassResult = _customerRegistrationService.ChangePassword(changePassRequest);
            if (changePassResult.Success)
                _notificationService.SuccessNotification("The password has been changed successfully.");
            else
                foreach (var error in changePassResult.Errors)
                    _notificationService.ErrorNotification(error);

            return RedirectToAction("Edit", new { id = customer.Id });
        }

        public virtual IActionResult Index()
        {
            return RedirectToAction("List");
        }

        public virtual IActionResult List()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            var model = _customerModelFactory.PrepareCustomerSearchModel(new CustomerSearchModel());

            return View(model);
        }

        [HttpPost]
        public virtual IActionResult CustomerList(CustomerSearchModel searchModel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedDataTablesJson();

            var model = _customerModelFactory.PrepareCustomerListModel(searchModel);

            return Json(model);
        }

        public virtual IActionResult LoadCustomerStatistics(string period)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return Content(string.Empty);

            var result = new List<object>();

            var nowDt = _dateTimeHelperService.ConvertToUserTime(DateTime.Now);
            var timeZone = _dateTimeHelperService.CurrentTimeZone;
            var searchCustomerRoleIds = new[] { _customerService.GetCustomerRoleBySystemName(InovatiqaDefaults.RegisteredRoleName).Id };

            var culture = new CultureInfo(InovatiqaDefaults.LanguageCulture);

            switch (period)
            {
                case "year":
                    var yearAgoDt = nowDt.AddYears(-1).AddMonths(1);
                    var searchYearDateUser = new DateTime(yearAgoDt.Year, yearAgoDt.Month, 1);
                    for (var i = 0; i <= 12; i++)
                    {
                        result.Add(new
                        {
                            date = searchYearDateUser.Date.ToString("Y", culture),
                            value = _customerService.GetAllCustomers(
                                createdFromUtc: _dateTimeHelperService.ConvertToUtcTime(searchYearDateUser, timeZone),
                                createdToUtc: _dateTimeHelperService.ConvertToUtcTime(searchYearDateUser.AddMonths(1), timeZone),
                                customerRoleIds: searchCustomerRoleIds,
                                pageIndex: 0,
                                pageSize: 1, getOnlyTotalCount: true).TotalCount.ToString()
                        });

                        searchYearDateUser = searchYearDateUser.AddMonths(1);
                    }

                    break;
                case "month":
                    var monthAgoDt = nowDt.AddDays(-30);
                    var searchMonthDateUser = new DateTime(monthAgoDt.Year, monthAgoDt.Month, monthAgoDt.Day);
                    for (var i = 0; i <= 30; i++)
                    {
                        result.Add(new
                        {
                            date = searchMonthDateUser.Date.ToString("M", culture),
                            value = _customerService.GetAllCustomers(
                                createdFromUtc: _dateTimeHelperService.ConvertToUtcTime(searchMonthDateUser, timeZone),
                                createdToUtc: _dateTimeHelperService.ConvertToUtcTime(searchMonthDateUser.AddDays(1), timeZone),
                                customerRoleIds: searchCustomerRoleIds,
                                pageIndex: 0,
                                pageSize: 1, getOnlyTotalCount: true).TotalCount.ToString()
                        });

                        searchMonthDateUser = searchMonthDateUser.AddDays(1);
                    }

                    break;
                case "week":
                default:
                    var weekAgoDt = nowDt.AddDays(-7);
                    var searchWeekDateUser = new DateTime(weekAgoDt.Year, weekAgoDt.Month, weekAgoDt.Day);
                    for (var i = 0; i <= 7; i++)
                    {
                        result.Add(new
                        {
                            date = searchWeekDateUser.Date.ToString("d dddd", culture),
                            value = _customerService.GetAllCustomers(
                                createdFromUtc: _dateTimeHelperService.ConvertToUtcTime(searchWeekDateUser, timeZone),
                                createdToUtc: _dateTimeHelperService.ConvertToUtcTime(searchWeekDateUser.AddDays(1), timeZone),
                                customerRoleIds: searchCustomerRoleIds,
                                pageIndex: 0,
                                pageSize: 1, getOnlyTotalCount: true).TotalCount.ToString()
                        });

                        searchWeekDateUser = searchWeekDateUser.AddDays(1);
                    }

                    break;
            }

            return Json(result);
        }

        public virtual IActionResult Create()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            var model = _customerModelFactory.PrepareCustomerModel(new CustomerModel(), null);

            return View(model);
        }

        ////////////////////////[HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        ////////////////////////[FormValueRequired("save", "save-continue")]
        ////////////////////////public virtual IActionResult Create(CustomerModel model, bool continueEditing, IFormCollection form)
        ////////////////////////{
        ////////////////////////    if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
        ////////////////////////        return AccessDeniedView();

        ////////////////////////    if (!string.IsNullOrWhiteSpace(model.Email) && _customerService.GetCustomerByEmail(model.Email) != null)
        ////////////////////////        ModelState.AddModelError(string.Empty, "Email is already registered");

        ////////////////////////    if (!string.IsNullOrWhiteSpace(model.Username) && InovatiqaDefaults.UsernamesEnabled &&
        ////////////////////////        _customerService.GetCustomerByUsername(model.Username) != null)
        ////////////////////////    {
        ////////////////////////        ModelState.AddModelError(string.Empty, "Username is already registered");
        ////////////////////////    }

        ////////////////////////    var allCustomerRoles = _customerService.GetAllCustomerRoles(true);
        ////////////////////////    var newCustomerRoles = new List<CustomerRole>();
        ////////////////////////    foreach (var customerRole in allCustomerRoles)
        ////////////////////////        if (model.SelectedCustomerRoleIds.Contains(customerRole.Id))
        ////////////////////////            newCustomerRoles.Add(customerRole);
        ////////////////////////    var customerRolesError = ValidateCustomerRoles(newCustomerRoles, new List<CustomerRole>());
        ////////////////////////    if (!string.IsNullOrEmpty(customerRolesError))
        ////////////////////////    {
        ////////////////////////        ModelState.AddModelError(string.Empty, customerRolesError);
        ////////////////////////        _notificationService.ErrorNotification(customerRolesError);
        ////////////////////////    }

        ////////////////////////    if (newCustomerRoles.Any() && newCustomerRoles.FirstOrDefault(c => c.SystemName == InovatiqaDefaults.RegisteredRoleName) != null &&
        ////////////////////////        !CommonHelper.IsValidEmail(model.Email))
        ////////////////////////    {
        ////////////////////////        ModelState.AddModelError(string.Empty, "Valid Email is required for customer to be in 'Registered' role");

        ////////////////////////        _notificationService.ErrorNotification("Valid Email is required for customer to be in 'Registered' role";
        ////////////////////////    }

        ////////////////////////    var customerAttributesXml = ParseCustomCustomerAttributes(form);
        ////////////////////////    if (newCustomerRoles.Any() && newCustomerRoles.FirstOrDefault(c => c.SystemName == InovatiqaDefaults.RegisteredRoleName) != null)
        ////////////////////////    {
        ////////////////////////        var customerAttributeWarnings = _customerAttributeParserService.GetAttributeWarnings(customerAttributesXml);
        ////////////////////////        foreach (var error in customerAttributeWarnings)
        ////////////////////////        {
        ////////////////////////            ModelState.AddModelError(string.Empty, error);
        ////////////////////////        }
        ////////////////////////    }

        ////////////////////////    if (ModelState.IsValid)
        ////////////////////////    {
        ////////////////////////        var customer = model.ToCustomerEntity<Customer>();

        ////////////////////////        customer.CustomerGuid = Guid.NewGuid();
        ////////////////////////        customer.CreatedOnUtc = DateTime.UtcNow;
        ////////////////////////        customer.LastActivityDateUtc = DateTime.UtcNow;
        ////////////////////////        customer.RegisteredInStoreId = InovatiqaDefaults.StoreId;

        ////////////////////////        _customerService.InsertCustomer(customer);

        ////////////////////////        if (InovatiqaDefaults.AllowCustomersToSetTimeZone)
        ////////////////////////            _genericAttributeService.SaveAttribute(customer.GetType().Name, customer.Id, InovatiqaDefaults.TimeZoneIdAttribute, model.TimeZoneId);
        ////////////////////////        if (InovatiqaDefaults.GenderEnabled)
        ////////////////////////            _genericAttributeService.SaveAttribute(customer.GetType().Name, customer.Id, InovatiqaDefaults.GenderAttribute, model.Gender);
        ////////////////////////        if (InovatiqaDefaults.FirstNameEnabled)
        ////////////////////////            _genericAttributeService.SaveAttribute(customer.GetType().Name, customer.Id, InovatiqaDefaults.FirstNameAttribute, model.FirstName);
        ////////////////////////        if (InovatiqaDefaults.LastNameEnabled)
        ////////////////////////            _genericAttributeService.SaveAttribute(customer.GetType().Name, customer.Id, InovatiqaDefaults.LastNameAttribute, model.LastName);
        ////////////////////////        if (InovatiqaDefaults.DateOfBirthEnabled)
        ////////////////////////            _genericAttributeService.SaveAttribute(customer.GetType().Name, customer.Id, InovatiqaDefaults.DateOfBirthAttribute, model.DateOfBirth);
        ////////////////////////        if (InovatiqaDefaults.CompanyEnabled)
        ////////////////////////            _genericAttributeService.SaveAttribute(customer.GetType().Name, customer.Id, InovatiqaDefaults.CompanyAttribute, model.Company);
        ////////////////////////        if (InovatiqaDefaults.StreetAddressEnabled)
        ////////////////////////            _genericAttributeService.SaveAttribute(customer.GetType().Name, customer.Id, InovatiqaDefaults.StreetAddressAttribute, model.StreetAddress);
        ////////////////////////        if (InovatiqaDefaults.StreetAddress2Enabled)
        ////////////////////////            _genericAttributeService.SaveAttribute(customer.GetType().Name, customer.Id, InovatiqaDefaults.StreetAddress2Attribute, model.StreetAddress2);
        ////////////////////////        if (InovatiqaDefaults.ZipPostalCodeEnabled)
        ////////////////////////            _genericAttributeService.SaveAttribute(customer.GetType().Name, customer.Id, InovatiqaDefaults.ZipPostalCodeAttribute, model.ZipPostalCode);
        ////////////////////////        if (InovatiqaDefaults.CityEnabled)
        ////////////////////////            _genericAttributeService.SaveAttribute(customer.GetType().Name, customer.Id, InovatiqaDefaults.CityAttribute, model.City);
        ////////////////////////        if (InovatiqaDefaults.CountyEnabled)
        ////////////////////////            _genericAttributeService.SaveAttribute(customer.GetType().Name, customer.Id, InovatiqaDefaults.CountyAttribute, model.County);
        ////////////////////////        if (InovatiqaDefaults.CountryEnabled)
        ////////////////////////            _genericAttributeService.SaveAttribute(customer.GetType().Name, customer.Id, InovatiqaDefaults.CountryIdAttribute, model.CountryId);
        ////////////////////////        if (InovatiqaDefaults.CountryEnabled && InovatiqaDefaults.StateProvinceEnabled)
        ////////////////////////            _genericAttributeService.SaveAttribute(customer.GetType().Name, customer.Id, InovatiqaDefaults.StateProvinceIdAttribute, model.StateProvinceId);
        ////////////////////////        if (InovatiqaDefaults.PhoneEnabled)
        ////////////////////////            _genericAttributeService.SaveAttribute(customer.GetType().Name, customer.Id, InovatiqaDefaults.PhoneAttribute, model.Phone);
        ////////////////////////        if (InovatiqaDefaults.FaxEnabled)
        ////////////////////////            _genericAttributeService.SaveAttribute(customer.GetType().Name, customer.Id, InovatiqaDefaults.FaxAttribute, model.Fax);

        ////////////////////////        _genericAttributeService.SaveAttribute(customer.GetType().Name, customer.Id, InovatiqaDefaults.CustomCustomerAttributes, customerAttributesXml);

        ////////////////////////        if (!string.IsNullOrEmpty(customer.Email))
        ////////////////////////        {
        ////////////////////////            var newsletterSubscription = _newsLetterSubscriptionService
        ////////////////////////                    .GetNewsLetterSubscriptionByEmailAndStoreId(customer.Email, InovatiqaDefaults.StoreId);
        ////////////////////////            if (model.SelectedNewsletterSubscriptionStoreIds != null &&
        ////////////////////////                model.SelectedNewsletterSubscriptionStoreIds.Contains(InovatiqaDefaults.StoreId))
        ////////////////////////            {
        ////////////////////////                if (newsletterSubscription == null)
        ////////////////////////                {
        ////////////////////////                    _newsLetterSubscriptionService.InsertNewsLetterSubscription(new NewsLetterSubscription
        ////////////////////////                    {
        ////////////////////////                        NewsLetterSubscriptionGuid = Guid.NewGuid(),
        ////////////////////////                        Email = customer.Email,
        ////////////////////////                        Active = true,
        ////////////////////////                        StoreId = InovatiqaDefaults.StoreId,
        ////////////////////////                        CreatedOnUtc = DateTime.UtcNow
        ////////////////////////                    });
        ////////////////////////                }
        ////////////////////////            }
        ////////////////////////            else
        ////////////////////////            {
        ////////////////////////                if (newsletterSubscription != null)
        ////////////////////////                {
        ////////////////////////                    _newsLetterSubscriptionService.DeleteNewsLetterSubscription(newsletterSubscription);
        ////////////////////////                }
        ////////////////////////            }
        ////////////////////////        }

        ////////////////////////        if (!string.IsNullOrWhiteSpace(model.Password))
        ////////////////////////        {
        ////////////////////////            var changePassRequest = new ChangePasswordRequest(model.Email, false, 0, model.Password);
        ////////////////////////            var changePassResult = _customerRegistrationService.ChangePassword(changePassRequest);
        ////////////////////////            if (!changePassResult.Success)
        ////////////////////////            {
        ////////////////////////                foreach (var changePassError in changePassResult.Errors)
        ////////////////////////                    _notificationService.ErrorNotification(changePassError);
        ////////////////////////            }
        ////////////////////////        }

        ////////////////////////        //customer roles
        ////////////////////////        foreach (var customerRole in newCustomerRoles)
        ////////////////////////        {
        ////////////////////////            //ensure that the current customer cannot add to "Administrators" system role if he's not an admin himself
        ////////////////////////            if (customerRole.SystemName == NopCustomerDefaults.AdministratorsRoleName && !_customerService.IsAdmin(_workContext.CurrentCustomer))
        ////////////////////////                continue;

        ////////////////////////            _customerService.AddCustomerRoleMapping(new CustomerCustomerRoleMapping { CustomerId = customer.Id, CustomerRoleId = customerRole.Id });
        ////////////////////////        }

        ////////////////////////        _customerService.UpdateCustomer(customer);

        ////////////////////////        //ensure that a customer with a vendor associated is not in "Administrators" role
        ////////////////////////        //otherwise, he won't have access to other functionality in admin area
        ////////////////////////        if (_customerService.IsAdmin(customer) && customer.VendorId > 0)
        ////////////////////////        {
        ////////////////////////            customer.VendorId = 0;
        ////////////////////////            _customerService.UpdateCustomer(customer);

        ////////////////////////            _notificationService.ErrorNotification(_localizationService.GetResource("Admin.Customers.Customers.AdminCouldNotbeVendor"));
        ////////////////////////        }

        ////////////////////////        //ensure that a customer in the Vendors role has a vendor account associated.
        ////////////////////////        //otherwise, he will have access to ALL products
        ////////////////////////        if (_customerService.IsVendor(customer) && customer.VendorId == 0)
        ////////////////////////        {
        ////////////////////////            var vendorRole = _customerService.GetCustomerRoleBySystemName(NopCustomerDefaults.VendorsRoleName);
        ////////////////////////            _customerService.RemoveCustomerRoleMapping(customer, vendorRole);

        ////////////////////////            _notificationService.ErrorNotification(_localizationService.GetResource("Admin.Customers.Customers.CannotBeInVendoRoleWithoutVendorAssociated"));
        ////////////////////////        }

        ////////////////////////        //activity log
        ////////////////////////        _customerActivityService.InsertActivity("AddNewCustomer",
        ////////////////////////            string.Format(_localizationService.GetResource("ActivityLog.AddNewCustomer"), customer.Id), customer);
        ////////////////////////        _notificationService.SuccessNotification(_localizationService.GetResource("Admin.Customers.Customers.Added"));

        ////////////////////////        if (!continueEditing)
        ////////////////////////            return RedirectToAction("List");

        ////////////////////////        return RedirectToAction("Edit", new { id = customer.Id });
        ////////////////////////    }

        ////////////////////////    //prepare model
        ////////////////////////    model = _customerModelFactory.PrepareCustomerModel(model, null, true);

        ////////////////////////    //if we got this far, something failed, redisplay form
        ////////////////////////    return View(model);
        ////////////////////////}

        #endregion

        #region Reward points history



        #endregion

        #region Addresses

        [HttpPost]
        public virtual IActionResult AddressDelete(int id, int customerId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            var customer = _customerService.GetCustomerById(customerId)
                ?? throw new ArgumentException("No customer found with the specified id", nameof(customerId));

            var address = _customerService.GetCustomerAddress(customer.Id, id);

            if (address == null)
                return Content("No address found with the specified id");

            _customerService.RemoveCustomerAddress(customer, address);
            _customerService.UpdateCustomer(customer);

            _addressService.DeleteAddress(address);

            return new NullJsonResult();
        }

        [HttpPost]
        public virtual IActionResult AddressesSelect(CustomerAddressSearchModel searchModel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedDataTablesJson();

            var customer = _customerService.GetCustomerById(searchModel.CustomerId)
                ?? throw new ArgumentException("No customer found with the specified id");

            var model = _customerModelFactory.PrepareCustomerAddressListModel(searchModel, customer);

            return Json(model);
        }

        ////////[HttpPost]
        ////////public virtual IActionResult AddressDelete(int id, int customerId)
        ////////{
        ////////    if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
        ////////        return AccessDeniedView();

        ////////    var customer = _customerService.GetCustomerById(customerId)
        ////////        ?? throw new ArgumentException("No customer found with the specified id", nameof(customerId));

        ////////    var address = _customerService.GetCustomerAddress(customer.Id, id);

        ////////    if (address == null)
        ////////        return Content("No address found with the specified id");

        ////////    _customerService.RemoveCustomerAddress(customer, address);
        ////////    _customerService.UpdateCustomer(customer);

        ////////    //now delete the address record
        ////////    _addressService.DeleteAddress(address);

        ////////    return new NullJsonResult();
        ////////}

        public virtual IActionResult AddressCreate(int customerId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            var customer = _customerService.GetCustomerById(customerId);
            if (customer == null)
                return RedirectToAction("List");

            var model = _customerModelFactory.PrepareCustomerAddressModel(new CustomerAddressModel(), customer, null);

            return View(model);
        }

        ////////[HttpPost]
        ////////public virtual IActionResult AddressCreate(CustomerAddressModel model, IFormCollection form)
        ////////{
        ////////    if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
        ////////        return AccessDeniedView();

        ////////    var customer = _customerService.GetCustomerById(model.CustomerId);
        ////////    if (customer == null)
        ////////        return RedirectToAction("List");

        ////////    var customAttributes = _addressAttributeParserService.ParseCustomAddressAttributes(form);
        ////////    var customAttributeWarnings = _addressAttributeParserService.GetAttributeWarnings(customAttributes);
        ////////    foreach (var error in customAttributeWarnings)
        ////////    {
        ////////        ModelState.AddModelError(string.Empty, error);
        ////////    }

        ////////    if (ModelState.IsValid)
        ////////    {
        ////////        var address = model.Address.ToEntity<Address>();
        ////////        address.CustomAttributes = customAttributes;
        ////////        address.CreatedOnUtc = DateTime.UtcNow;

        ////////        //some validation
        ////////        if (address.CountryId == 0)
        ////////            address.CountryId = null;
        ////////        if (address.StateProvinceId == 0)
        ////////            address.StateProvinceId = null;

        ////////        _addressService.InsertAddress(address);

        ////////        _customerService.InsertCustomerAddress(customer, address);

        ////////        _notificationService.SuccessNotification(_localizationService.GetResource("Admin.Customers.Customers.Addresses.Added"));

        ////////        return RedirectToAction("AddressEdit", new { addressId = address.Id, customerId = model.CustomerId });
        ////////    }

        ////////    //prepare model
        ////////    model = _customerModelFactory.PrepareCustomerAddressModel(model, customer, null, true);

        ////////    //if we got this far, something failed, redisplay form
        ////////    return View(model);
        ////////}

        public virtual IActionResult AddressEdit(int addressId, int customerId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            var customer = _customerService.GetCustomerById(customerId);
            if (customer == null)
                return RedirectToAction("List");

            var address = _addressService.GetAddressById(addressId);
            if (address == null)
                return RedirectToAction("Edit", new { id = customer.Id });

            var model = _customerModelFactory.PrepareCustomerAddressModel(null, customer, address);

            return View(model);
        }

        [HttpPost]
        public virtual IActionResult AddressEdit(CustomerAddressModel model, IFormCollection form)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            var customer = _customerService.GetCustomerById(model.CustomerId);
            if (customer == null)
                return RedirectToAction("List");

            var address = _addressService.GetAddressById(model.Address.Id);
            if (address == null)
                return RedirectToAction("Edit", new { id = customer.Id });


            var customAttributes = _addressAttributeParserService.ParseCustomAddressAttributes(form);
            var customAttributeWarnings = _addressAttributeParserService.GetAttributeWarnings(customAttributes);
            foreach (var error in customAttributeWarnings)
            {
                ModelState.AddModelError(string.Empty, error);
            }

            if (ModelState.IsValid)
            {
                address = model.Address.ToAddressEntity(address);
                address.CustomAttributes = customAttributes;
                _addressService.UpdateAddress(address);

                _notificationService.SuccessNotification("The address has been updated successfully.");

                return RedirectToAction("AddressEdit", new { addressId = model.Address.Id, customerId = model.CustomerId });
            }

            model = _customerModelFactory.PrepareCustomerAddressModel(model, customer, address, true);

            return View(model);
        }

        #endregion

        #region Orders

        [HttpPost]
        public virtual IActionResult OrderList(CustomerOrderSearchModel searchModel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedDataTablesJson();

            var customer = _customerService.GetCustomerById(searchModel.CustomerId)
                ?? throw new ArgumentException("No customer found with the specified id");

            var model = _customerModelFactory.PrepareCustomerOrderListModel(searchModel, customer);

            return Json(model);
        }

        #endregion


        #region Current shopping cart/ wishlist

        [HttpPost]
        public virtual IActionResult GetCartList(CustomerShoppingCartSearchModel searchModel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedDataTablesJson();

            var customer = _customerService.GetCustomerById(searchModel.CustomerId)
                ?? throw new ArgumentException("No customer found with the specified id");

            var model = _customerModelFactory.PrepareCustomerShoppingCartListModel(searchModel, customer);

            return Json(model);
        }

        #endregion

        #region Activity log

        [HttpPost]
        public virtual IActionResult ListActivityLog(CustomerActivityLogSearchModel searchModel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedDataTablesJson();

            var customer = _customerService.GetCustomerById(searchModel.CustomerId)
                ?? throw new ArgumentException("No customer found with the specified id");

            var model = _customerModelFactory.PrepareCustomerActivityLogListModel(searchModel, customer);

            return Json(model);
        }

        #endregion

        #region Back in stock subscriptions



        #endregion

        #region GDPR


        #endregion

        #region Export / Import



        #endregion
    }
}