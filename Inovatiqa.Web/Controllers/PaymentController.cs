using Inovatiqa.Services.Common.Interfaces;
using Inovatiqa.Services.Customers.Interfaces;
using Inovatiqa.Services.Orders.Interfaces;
using Inovatiqa.Services.Logging.Interfaces;
using Inovatiqa.Services.Payments.Interfaces;
using Inovatiqa.Services.Shipping.Interfaces;
using Inovatiqa.Services.WorkContext.Interfaces;
using Inovatiqa.Web.Factories.Interfaces;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc;
using Inovatiqa.Web.Models.Payment;
using Inovatiqa.Core;
using System;
using Inovatiqa.Database.Models;
using System.Collections.Generic;
using System.IO;
using Inovatiqa.Web.Models.Checkout;
using Microsoft.AspNetCore.Http;
using Inovatiqa.Web.Models.Common;
using Inovatiqa.Services.Messages.Interfaces;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using System.Threading.Tasks;
using System.IO.Compression;
using System.Net.Http;
using System.Text;
using System.Net;
using Newtonsoft.Json.Linq;
using Square;
using Square.Models;
using Square.Exceptions;

namespace Inovatiqa.Web.Controllers
{
    public class PaymentController : BasePublicController
    {
        #region Fields
        //private static ISquareClient _client;

        private readonly ICustomerService _customerService;
        private readonly IOrderModelFactory _orderModelFactory;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IPaymentMethodService _paymentMethodService;
        private readonly IOrderService _orderService;
        private readonly IPaymentService _paymentService;
        private readonly IPdfService _pdfService;
        private readonly IShipmentService _shipmentService;
        private readonly IWorkContextService _workContextService;
        private readonly IPaymentModelFactory _paymentModelFactory;
        private readonly ILoggerService _loggerService;
        private readonly IEmailAccountService _emailAccountService;
        private readonly IQueuedEmailService _queuedEmailService;
        private readonly INotificationService _notificationService;
        private readonly IWebHostEnvironment _hostingEnvironment;

        #endregion

        #region Ctor

        public PaymentController(ICustomerService customerService,
            IOrderModelFactory orderModelFactory,
            IOrderProcessingService orderProcessingService,
            IOrderService orderService,
            IPaymentService paymentService,
            IPdfService pdfService,
            IPaymentMethodService paymentMethodService,
            IShipmentService shipmentService,
            IWorkContextService workContextService,
            IPaymentModelFactory paymentModelFactory,
            ILoggerService loggerService,
            IRazorViewEngine viewEngine,
            IEmailAccountService emailAccountService,
            IQueuedEmailService queuedEmailService,
            INotificationService notificationService,
            IWebHostEnvironment environment) : base(viewEngine)
        {
            _customerService = customerService;
            _orderModelFactory = orderModelFactory;
            _orderProcessingService = orderProcessingService;
            _orderService = orderService;
            _paymentService = paymentService;
            _pdfService = pdfService;
            _paymentMethodService = paymentMethodService;
            _shipmentService = shipmentService;
            _workContextService = workContextService;
            _paymentModelFactory = paymentModelFactory;
            _loggerService = loggerService;
            _emailAccountService = emailAccountService;
            _queuedEmailService = queuedEmailService;
            _notificationService = notificationService;
            _hostingEnvironment = environment;
        }

        #endregion

        #region Methods

        public virtual IActionResult PaymentPortal()
        {
            if (!_customerService.IsRegistered(_workContextService.CurrentCustomer))
            {
                return Challenge();
            }
            var model = _paymentModelFactory.PreparePaymentPortalShipmentListModel();
            return View(model);
        }

        public virtual IActionResult OpenInvoices()
        {
            var model = _paymentModelFactory.PreparePaymentPortalShipmentListModel();
            return View(model);
        }

        public virtual IActionResult UnAppliedCash()
        {
            var model = _paymentModelFactory.PreparePaymentPortalShipmentListModel();
            return View(model);
        }

        public virtual IActionResult PaidInvoices()
        {
            var model = _paymentModelFactory.PreparePaymentPortalShipmentListModel();
            return View(model);
        }

        //public virtual IActionResult MakePayment([FromBody] RequestData requestData)
        //{
        //    const string ACCESS_TOKEN = "EAAAEDD2M6uGJmYkgShovGbvjPjzZZFEsvcYWn-X1B79v2Imx51cpfVrAX3DtLdB";
        //    _client = new SquareClient.Builder()
        //        .Environment(Square.Environment.Sandbox)
        //        .AccessToken(ACCESS_TOKEN)
        //        .Build();
        //    Money ammount = new Money(1000, "USD");
        //    string idempodencyKey = Guid.NewGuid().ToString();
        //    CreatePaymentRequest createPaymentRequest = new CreatePaymentRequest(requestData.sourceId, idempodencyKey, ammount);
        //    var result = _client.PaymentsApi.CreatePayment(createPaymentRequest);
        //    //var customer = _workContextService.CurrentCustomer;
        //    //if (!_customerService.IsRegistered(customer))
        //    //    return Challenge();

        //    //var model = _paymentModelFactory.PreparePaymentInfoModel(customer, totalPayment, amountToPay, invoiceIds, invoiceIdsAmounts);

        //    //var result = model.client.LocationsApi.RetrieveLocation(InovatiqaDefaults.LocationId);
        //    //model.Country = result.Location.Country;
        //    //model.Currency = result.Location.Currency;

        //    //return View(model);
        //    return View("Payment succeed");
        //}

        public virtual IActionResult MakePayment(decimal totalPayment, decimal amountToPay, string invoiceIds, string invoiceIdsAmounts)
        {
            var customer = _workContextService.CurrentCustomer;
            if (!_customerService.IsRegistered(customer))
                return Challenge();

            var model = _paymentModelFactory.PreparePaymentInfoModel(customer, totalPayment, amountToPay, invoiceIds, invoiceIdsAmounts);

            var result = model.client.LocationsApi.RetrieveLocation(InovatiqaDefaults.LocationId);
            model.Country = result.Location.Country;
            model.Currency = result.Location.Currency;

            return View(model);
        }

        public virtual IActionResult MakingPayment(PaymentInfoModel paymentInfoModel)
        {
            var confirmPaymentModel = new CheckoutConfirmModel();
            try
            {

                var InvoicesIds = paymentInfoModel.InvoiceIds.Split(",");
                var Invoiceamount = paymentInfoModel.invoiceIdsAmounts.Split(",");
                var customer = _workContextService.CurrentCustomer;
                for (int i = 0; i < InvoicesIds.Length; i++)
                {
                    var shipment = _shipmentService.GetShipmentById(Convert.ToInt32(InvoicesIds[i]));
                    Inovatiqa.Services.Payments.ProcessPaymentRequest processPaymentRequest = new Services.Payments.ProcessPaymentRequest();
                    //{
                    //    StoreId = InovatiqaDefaults.StoreId,
                    //    CardNonce =   paymentInfoModel.StoredCardId, 
                    //    //CustomerId = customer.Id,// old request was sending card nonce but in our case it was empty the selected card nonce was stored in StoredCardId      paymentInfoModel.CardNonce,
                    //    OrderTotal = decimal.Parse(shipment.AmountPaid.ToString())
                    //};

                    processPaymentRequest.StoreId = InovatiqaDefaults.StoreId;
                    processPaymentRequest.CardNonce = paymentInfoModel.StoredCardId;
                    processPaymentRequest.OrderTotal = Convert.ToDecimal(Invoiceamount[i]);
                    processPaymentRequest.CustomerId = customer.Id;


                    var processPaymentResult = _paymentMethodService.ProcessShipmentPayment(processPaymentRequest);

                    if (processPaymentResult == null)
                        throw new InovatiqaException("ProcessPaymentResult is not available");

                    if (processPaymentResult.Success)
                    {
                        var capturePaymentResult = _orderProcessingService.CaptureShipment(processPaymentResult.AuthorizationTransactionId, shipment);
                        if (capturePaymentResult.Success)
                        {
                            TempData["success"] = "Your Invoice number " + paymentInfoModel.InvoiceIds + " have been successfully paid.";
                            if (Convert.ToDecimal(Invoiceamount[i]) == shipment.TotalAmount - Convert.ToDecimal(shipment.AmountPaid))
                            {
                                shipment.AmountPaid = (shipment.AmountPaid ?? 0) + Convert.ToDecimal(Invoiceamount[i]);
                                shipment.PaymentStatusId = (int)PaymentStatus.Paid;
                                shipment.InvoicePayedDateUtc = DateTime.UtcNow;
                                _shipmentService.UpdateShipment(shipment);
                            }
                            else if (Convert.ToDecimal(Invoiceamount[i]) < shipment.TotalAmount)
                            {
                                shipment.AmountPaid = (shipment.AmountPaid ?? 0) + Convert.ToDecimal(Invoiceamount[i]);
                                // changes by hamza
                                if (shipment.TotalAmount == shipment.AmountPaid)
                                {
                                    shipment.PaymentStatusId = (int)PaymentStatus.Paid;
                                }
                                else
                                    shipment.PaymentStatusId = (int)PaymentStatus.PartiallyPaid;
                                _shipmentService.UpdateShipment(shipment);
                            }
                            var ammountpaid = shipment.AmountPaid;
                            var OrderTotalPaid = _shipmentService.GetShipmentsByOrderId(shipment.OrderId).Sum(s => s.AmountPaid);
                            var order = _orderService.GetOrderById(shipment.OrderId);
                            order.PaymentStatusId = order.OrderSubtotalInclTax == OrderTotalPaid ? (int)PaymentStatus.Paid : (int)PaymentStatus.PartiallyPaid;
                            _orderService.UpdateOrder(order);
                            foreach (var error in processPaymentResult.Errors)
                            {
                                confirmPaymentModel.Warnings.Add(error);
                                TempData["error"] = confirmPaymentModel.Warnings;
                            }
                        }
                    }
                }


                return RedirectToAction("OpenInvoices");


            }
            catch (Exception exc)
            {
                _loggerService.Warning(exc.Message, exc, _workContextService.CurrentCustomer);
                return Json(new { error = 1, message = exc.Message });
            }

            //return Json(new { error = 1, message = confirmPaymentModel.Warnings });
        }


        public virtual IActionResult ShippingPdfInvoice(int orderId=0, int shipmentId = 0)
        {
            var order = _orderService.GetOrderById(orderId);
            var orders = new List<Database.Models.Order>
            {
                order
            };

            byte[] bytes;
            using (var stream = new MemoryStream())
            {
                _pdfService.PrintOrdersToPdf(stream, orders, InovatiqaDefaults.LanguageId, 0, shipmentId);
                bytes = stream.ToArray();
            }

            return File(bytes, MimeTypes.ApplicationPdf, $"order_{order.Id}.pdf");
        }


        [HttpPost]
        public virtual IActionResult OrderList()
        {
            var model = _orderModelFactory.PrepareCustomerOrderListModel();
            return View(model);
        }

        [HttpPost]
        public virtual IActionResult SavePaymentInfo(IFormCollection form)
        {
            try
            {
                var customer = _workContextService.CurrentCustomer;
                var response = _paymentService.CreateCustomerCard(form["CardNonce"], customer);
                //   return response;
                // return RedirectToAction("MakePayment");
                var split = response.Split(",");
                if (split[0] == "error")
                {
                    TempData["error"] = split[1];
                }
                else
                {
                    TempData["success"] = split[1];
                }

                return Json(new
                {
                    html = RenderPartialViewToString("_AlertBox",response)
                });
            }
            catch (Exception exc)
            {
                return Json(new
                {
                    message = exc.Message
                });


            }
        }
        public async virtual Task<IActionResult> MakeAchPayment(string Token, string Amount, string InvoiceIds, string AmountsToPay)
        {
            var InvoiceAmounts = AmountsToPay.Split(",").ToList();
            var Invoices = InvoiceIds.Split(",").ToList();
            try
            {
                var response = await _paymentService.ProcessACHBankPayment(Token, Amount);
                if (response.Errors == null || response.Errors.Count == 0)
                {
                    TempData["success"] = "Your Invoice number " + InvoiceIds + " have been successfully paid.";
                    for (int i = 0; i < Invoices.Count; i++)
                    {
                        var shipment = _shipmentService.GetShipmentById(Convert.ToInt32(Invoices[i]));
                        shipment.AmountPaid = Convert.ToDecimal(shipment.AmountPaid) + Convert.ToDecimal(InvoiceAmounts[i]);
                        shipment.PaymentStatusId = shipment.AmountPaid == shipment.TotalAmount ? (int)PaymentStatus.Paid : (shipment.AmountPaid > 0 ? (int)PaymentStatus.PartiallyPaid : (int)PaymentStatus.Pending);
                        _shipmentService.UpdateShipment(shipment);
                        var OrderTotalPaid = _shipmentService.GetShipmentsByOrderId(shipment.OrderId).Sum(s => s.AmountPaid);
                        var order = _orderService.GetOrderById(shipment.OrderId);
                        order.PaymentStatusId = order.OrderSubtotalInclTax == OrderTotalPaid ? (int)PaymentStatus.Paid : (int)PaymentStatus.PartiallyPaid;
                        _orderService.UpdateOrder(order);
                    }
                }
                return new JsonResult(new { payment = response.Payment });
            }
            catch (ApiException e)
            {
                return new JsonResult(new { errors = e.Errors });
            }
        }
        [HttpPost]
        public virtual async Task<IActionResult> FooterSendEmails(EmailModel model, IFormFile File, string RequestType, string relatedTo, IFormCollection form)
        {
            try
            {
                var tempstring = "<p>" +
                    (model.CustomerFirstName != null ? "<p><b> First Name : </b> " + model.CustomerFirstName + " </p>" : "") +
                    (model.CustomerLastName != null ? "<p><b> Last Name : </b> " + model.CustomerLastName + " </p>" : "") +
                    (model.CustomerEmail != null ? "<p><b> Email : </b> " + model.CustomerEmail + " </p>" : "") +
                    (model.PhoneNumber != null ? "<p><b> Phone Number : </b> " + model.PhoneNumber + " </p>" : "") +
                    (model.ReleatedTo != null ? "<p><b> ReleatedTo  : </b> " + model.ReleatedTo + " </p>" : "") +
                    (model.OrderNumber != null ? "<p><b> Order Number  : </b> " + model.OrderNumber + " </p>" : "") +
                    (model.RequestType != null ? "<p><b> Request Type : </b> " + model.RequestType + " </p>" : "") +
                    (model.Company != null ? "<p><b> Company : </b> " + model.Company + " </p>" : "") +
                    (model.Address1 != null ? "<p><b> Address1 : </b> " + model.Address1 + " </p>" : "") +
                    (model.Address2 != null ? "<p><b> Address2 : </b> " + model.Address2 + " </p>" : "") +
                    (model.City != null ? "<p><b> City : </b> " + model.City + " </p>" : "") +
                    (model.State != null ? "<p><b> State : </b> " + model.State + " </p>" : "") +
                    (model.ZipCode != null ? "<p><b> ZipCode : </b> " + model.ZipCode + " </p>" : "") +
                    (model.Phone != null ? "<p><b> Phone : </b> " + model.Phone + " </p>" : "") +
                    "<p><b> PrivacyPolicy : </b> " + model.PrivacyPolicy + " </p>" +
                    "<p><b> MedicalEquipment : </b> " + model.MedicalEquipment + " </p>" +
                    "<p><b> PhysicalTherapy : </b> " + model.PhysicalTherapy + " </p>" +
                    (model.URL != null ? "<p><b> URL : </b> " + model.URL + " </p>" : "") +
                    (model.Fax != null ? "<p><b> Fax : </b> " + model.Fax + " </p>" : "") +
                    (model.Quantity != null ? "<p><b> Quantity : </b> " + model.Quantity + " </p>" : "") +
                    (model.ManufacturerProductNumber != null ? "<p><b> ManufacturerProductNumber : </b> " + model.ManufacturerProductNumber + " </p>" : "") +
                    (model.Description != null ? "<p><b> Description : </b> " + model.Description + " </p>" : "") +
                    (model.Title != null ? "<p><b> Title : </b> " + model.Title + " </p>" : "") +
                    (model.FacilityGPOId != null ? "<p><b> FacilityGPOId : </b> " + model.FacilityGPOId + " </p>" : "") +
                    (model.FederalTaxID != null ? "<p><b> FederalTaxID : </b> " + model.FederalTaxID + " </p>" : "") +
                    (model.GPOYourFacilityCanAccess != null ? "<p><b> GPOYourFacilityCanAccess : </b> " + model.GPOYourFacilityCanAccess + " </p>" : "") +
                    (model.ProductsInterested != null ? "<p><b> ProductsInterested : </b> " + model.ProductsInterested + " </p>" : "") +
                    (model.AdditionalInformation != null ? "<p><b> AdditionalInformation : </b> " + model.AdditionalInformation + " </p>" : "") +
                    (model.Message != null ? "<p><b> Message : </b> " + model.Message + " </p>" : "");


                model.EmailSubject = model.EmailSubject;
                model.EmailBody = tempstring;

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
                string uploads = Path.Combine(_hostingEnvironment.WebRootPath, "files\\footerfiles");
                Guid guid = Guid.NewGuid();
                string filePath = Path.Combine(uploads, File != null ? guid.ToString() + Path.GetExtension(File.FileName) : "");


                if (File != null)
                {
                    using (Stream fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await File.CopyToAsync(fileStream);
                    }
                }
                List<string> AdminEmail = new List<string> { 
                    "Contact Your Account Manager",
                    "Contact Us"
                };

                var email = new QueuedEmail
                {
                    //PriorityId = (int)QueuedEmailPriority.High,
                    EmailAccountId = emailAccount.Id,
                    FromName = emailAccount.DisplayName,
                    From = emailAccount.Email,
                    ToName = model.CustomerFirstName + " " + model.CustomerLastName,
                    To = AdminEmail.Contains(model.EmailSubject) ? InovatiqaDefaults.AdministratorEmail : model.CustomerEmail,
                    Subject = model.EmailSubject,
                    Body = model.EmailBody,
                    AttachmentFileName = File != null ? guid.ToString() + Path.GetExtension(File.FileName) : null,
                    AttachmentFilePath = File != null ? filePath : null
                    //CreatedOnUtc = DateTime.UtcNow,
                    //    DontSendBeforeDateUtc = model.SendEmail.SendImmediately || !model.SendEmail.DontSendBeforeDate.HasValue ?
                    //        null : (DateTime?)_dateTimeHelperService.ConvertToUtcTime(model.SendEmail.DontSendBeforeDate.Value)
                };
                _queuedEmailService.InsertQueuedEmail(email);

                _notificationService.SuccessNotification("The email has been queued successfully.");

                TempData["success"] = "Message sent successfully.";

            }
            catch (Exception exc)
            {
                TempData["error"] = "Something went wrong while sending your message. Please reload the page and try again";
                TempData["error1"] = exc.Message;
                _notificationService.ErrorNotification(exc.Message);

            }

            // Set rediredction to COntact us page and show message
            TempData["message"] = "Message sent successfully.";
            TempData["messageClass"] = "alert-success";
            return Redirect(model.Method);
            //return (model.Method != null && model.Method != "" && model.Controller != null && model.Controller != "") ? RedirectToAction(model.Method, model.Controller) : RedirectToAction("Index", "Home");
        }
        #endregion
    }
    public class RequestData
    {
        public string locationId { get; set; }
        public string sourceId { get; set; }
    }
}