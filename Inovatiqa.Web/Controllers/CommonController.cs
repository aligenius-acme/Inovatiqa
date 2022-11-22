using Microsoft.AspNetCore.Mvc;
using Inovatiqa.Web.Models.Common;
using Inovatiqa.Web.Factories.Interfaces;
using Inovatiqa.Services.Common.Interfaces;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Http;
using Inovatiqa.Core;
using Inovatiqa.Services.Messages.Interfaces;
using System.Linq;
using Inovatiqa.Database.Models;
using System;
using Inovatiqa.Services.Catalog.Interfaces;
using System.Collections.Generic;
using Inovatiqa.Web.Models.Catalog;
using Inovatiqa.Services.Seo.Interfaces;

namespace Inovatiqa.Web.Controllers
{
    [AutoValidateAntiforgeryToken]
    public partial class CommonController : BasePublicController
    {
        #region Fields

        private readonly ICommonModelFactory _commonModelFactory;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IEmailAccountService _emailAccountService;
        private readonly ICustomerModelFactory _customerModelFactory;
        private readonly INotificationService _notificationService;
        private readonly INewsLetterSubscriptionService _newsLetterSubscriptionService;
        private readonly IAddressAttributeParserService _addressAttributeParserService;
        private readonly IAddressService _addressService;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly IQueuedEmailService _queuedEmailService;
        private readonly IManufacturerService _manufacturerService;
        private readonly ICategoryService _categoryService;
        private readonly IUrlRecordService _urlRecordService;


        #endregion

        #region Ctor

        public CommonController(ICommonModelFactory commonModelFactory,
            IGenericAttributeService genericAttributeService,
            IHttpContextAccessor httpContextAccessor,
            INotificationService notificationService,
            ICustomerModelFactory customerModelFactory,
            INewsLetterSubscriptionService newsLetterSubscriptionService,
            IAddressAttributeParserService addressAttributeParserService,
             IAddressService addressService,
             IEmailAccountService emailAccountService,
             IWorkflowMessageService workflowMessageService,
             IQueuedEmailService queuedEmailService,
             IRazorViewEngine viewEngine,
             IManufacturerService manufacturerService,
             ICategoryService categoryService,
             IUrlRecordService urlRecordService) : base(viewEngine)
        {
            _commonModelFactory = commonModelFactory;
            _genericAttributeService = genericAttributeService;
            _httpContextAccessor = httpContextAccessor;
            _notificationService = notificationService;
            _customerModelFactory = customerModelFactory;
            _genericAttributeService = genericAttributeService;
            _newsLetterSubscriptionService = newsLetterSubscriptionService;
            _addressAttributeParserService = addressAttributeParserService;
            _addressService = addressService;
            _workflowMessageService = workflowMessageService;
            _emailAccountService = emailAccountService;
            _queuedEmailService = queuedEmailService;
            _manufacturerService = manufacturerService;
            _categoryService = categoryService;
            _urlRecordService = urlRecordService;
        }

        #endregion

        #region Methods

        public virtual IActionResult PageNotFound()
        {
            Response.StatusCode = 404;
            Response.ContentType = "text/html";

            return View();
        }
        public virtual IActionResult NotAllowed()
        {
            Response.StatusCode = 405;
            Response.ContentType = "text/html";
            return View();
        }
        public virtual IActionResult ContactUs()
        {
            //var model = new ContactUsModel();
            //model = _commonModelFactory.PrepareContactUsModel(model, false);
            EmailModel model = new EmailModel();
            return View(model);
        }
        public virtual IActionResult OrderStatus()
        {
            return View();
        }
        public virtual IActionResult ReturnPolicy()
        {
            return View();
        }
        public virtual IActionResult ShippingPolicy()
        {
            return View();
        }
        public virtual IActionResult PurchaseOrders()
        {
            return View();
        }
        public virtual IActionResult PaymentOptions()
        {
            return View();
        }
        public virtual IActionResult ReOrderManagement()
        {
            return View();
        }
        public virtual IActionResult FAQHelpCenter()
        {
            return View();
        }

        // Footer Tab 2
        public virtual IActionResult RXPrescription()
        {
            return View();
        }
        public virtual IActionResult ECatalogs()
        {
            EmailModel model = new EmailModel();
            return View(model);
        }
        public virtual IActionResult ContractPricing()
        {
            return View();
        }
        public virtual IActionResult RequestQuote()
        {
            return View();
        }

        public virtual IActionResult PaymentTerms()
        {
            return View();
        }
        public virtual IActionResult GPOPurchasing()
        {
            return View();
        }
        public virtual IActionResult PriceMatch()
        {
            return View();
        }


        public virtual IActionResult AboutUs()
        {
            return View();
        }
        public virtual IActionResult CustomersWeServe()
        {
            return RedirectToActionPermanent("AboutUs");
        }
        public virtual IActionResult WhyInovatiqa()
        {
            return View();
        }


 

        public virtual IActionResult Testimonials()
        {
            return View();
        }

        public virtual IActionResult Brands()
        {
            //commented by hamza
            /*var model = new ManufacturerNavigationModel();
            var manufacturers = _manufacturerService.GetAllManufacturers(storeId: InovatiqaDefaults.StoreId).OrderBy(man => man.Name).ToList();
            foreach(var manufacturer in manufacturers)
            {
                var modelMan = new ManufacturerBriefInfoModel
                {
                    Id = manufacturer.Id,
                    Name = manufacturer.Name.ToString(),
                    SeName = _urlRecordService.GetActiveSlug(manufacturer.Id, InovatiqaDefaults.ManufacturerSlugName, InovatiqaDefaults.LanguageId)
                };
                model.Manufacturers.Add(modelMan);
            }
            return View(model);*/
            return View();
        }



        [HttpPost]
        public virtual IActionResult ContactUsEmail(EmailModel model, string CustomerFirstName, string CustomerLastName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(model.CustomerEmail))
                    throw new InovatiqaException("Customer email is empty");
                if (!CommonHelper.IsValidEmail(model.CustomerEmail))
                    throw new InovatiqaException("Customer email is not valid");
                if (string.IsNullOrWhiteSpace(model.EmailSubject))
                    throw new InovatiqaException("Email subject is empty");
                if (string.IsNullOrWhiteSpace(model.EmailBody))
                    throw new InovatiqaException("Email body is empty");

                var emailAccount = _emailAccountService.GetEmailAccountById(InovatiqaDefaults.DefaultEmailAccountId);
                if (emailAccount == null)
                    emailAccount = _emailAccountService.GetAllEmailAccounts().FirstOrDefault();
                if (emailAccount == null)
                    throw new InovatiqaException("Email account can't be loaded");
                var email = new QueuedEmail
                {
                    //PriorityId = (int)QueuedEmailPriority.High,
                    EmailAccountId = emailAccount.Id,
                    FromName = emailAccount.DisplayName,
                    From = emailAccount.Email,
                    ToName = model.CustomerFirstName + " " + model.CustomerLastName,
                    To = model.CustomerEmail,
                    Subject = model.EmailSubject,
                    Body = model.EmailBody
                    //CreatedOnUtc = DateTime.UtcNow,
                    //    DontSendBeforeDateUtc = model.SendEmail.SendImmediately || !model.SendEmail.DontSendBeforeDate.HasValue ?
                    //        null : (DateTime?)_dateTimeHelperService.ConvertToUtcTime(model.SendEmail.DontSendBeforeDate.Value)

                    // < nop - editor asp -for= "SendEmail.Body" asp - template = "RichEditor" />
                };
                _queuedEmailService.InsertQueuedEmail(email);

                _notificationService.SuccessNotification("The email has been queued successfully.");
            }
            catch (Exception exc)
            {
                _notificationService.ErrorNotification(exc.Message);
            }

            return View();
        }


        [HttpPost]
        [IgnoreAntiforgeryToken]
        public virtual IActionResult EuCookieLawAccept()
        {
            //if (_httpContextAccessor.HttpContext?.Response == null)
            //    return Json("");

            //var cookieName = $"{InovatiqaDefaults.Prefix}{InovatiqaDefaults.AcceptCookie}";
            //_httpContextAccessor.HttpContext.Response.Cookies.Delete(cookieName);

            //var cookieExpires = InovatiqaDefaults.AcceptCookieExpires;
            //var cookieExpiresDate = DateTime.Now.AddMonths(cookieExpires);

            //var options = new CookieOptions
            //{
            //    HttpOnly = true,
            //    Expires = cookieExpiresDate,
            //    Secure = InovatiqaDefaults.IsCurrentConnectionSecured
            //};
            //_httpContextAccessor.HttpContext.Response.Cookies.Append(cookieName, Guid.NewGuid().ToString(), options);
            return Json("");
        }

        public virtual IActionResult HealthAccounts()
        {
            return View();
        }

        public virtual IActionResult DistributionServices()
        {
            return View();
        }

        public virtual IActionResult InternationalShipping()
        {
            return View();
        }


        public virtual IActionResult GovernmentContracts()
        {
            return View();
        }
        #endregion
    }
}