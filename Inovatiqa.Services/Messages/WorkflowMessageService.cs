using Inovatiqa.Core;
using Inovatiqa.Database.Models;
using Inovatiqa.Services.Common.Interfaces;
using Inovatiqa.Services.Customers.Interfaces;
using Inovatiqa.Services.Messages.Interfaces;
using Inovatiqa.Services.Orders.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Inovatiqa.Services.Messages
{
    public partial class WorkflowMessageService : IWorkflowMessageService
    {
        #region Fields

        private readonly IMessageTemplateService _messageTemplateService;
        private readonly IMessageTokenProviderService _messageTokenProviderService;
        private readonly IEmailAccountService _emailAccountService;
        private readonly ICustomerService _customerService;
        private readonly ITokenizerService _tokenizerService;
        private readonly IQueuedEmailService _queuedEmailService;
        private readonly IAddressService _addressService;
        private readonly IOrderService _orderService;

        #endregion

        #region Ctor

        public WorkflowMessageService(IMessageTemplateService messageTemplateService,
            IMessageTokenProviderService messageTokenProviderService,
            IEmailAccountService emailAccountService,
            ICustomerService customerService,
            IQueuedEmailService queuedEmailService,
            ITokenizerService tokenizerService,
            IAddressService addressService,
            IOrderService orderService)
        {
            _messageTemplateService = messageTemplateService;
            _messageTokenProviderService = messageTokenProviderService;
            _emailAccountService = emailAccountService;
            _customerService = customerService;
            _queuedEmailService = queuedEmailService;
            _tokenizerService = tokenizerService;
            _addressService = addressService;
            _orderService = orderService;
        }

        #endregion

        #region Utilities

        protected virtual IList<MessageTemplate> GetActiveMessageTemplates(string messageTemplateName, int storeId)
        {
            var messageTemplates = _messageTemplateService.GetMessageTemplatesByName(messageTemplateName, storeId);

            if (!messageTemplates?.Any() ?? true)
                return new List<MessageTemplate>();

            messageTemplates = messageTemplates.Where(messageTemplate => messageTemplate.IsActive).ToList();

            return messageTemplates;
        }

        protected virtual EmailAccount GetEmailAccountOfMessageTemplate(MessageTemplate messageTemplate, int languageId)
        {
            var emailAccountId = messageTemplate.EmailAccountId;

            var emailAccount = (_emailAccountService.GetEmailAccountById(emailAccountId) ?? _emailAccountService.GetEmailAccountById(InovatiqaDefaults.DefaultEmailAccountId)) ??
                               _emailAccountService.GetAllEmailAccounts().FirstOrDefault();
            return emailAccount;
        }

        #endregion

        #region Methods

        #region Customer workflow

        public virtual IList<int> SendCustomerPasswordRecoveryMessage(Customer customer, int languageId, string PasswordRecoveryToken)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            var messageTemplates = GetActiveMessageTemplates(MessageTemplateSystemNames.CustomerPasswordRecoveryMessage, InovatiqaDefaults.StoreId);
            if (!messageTemplates.Any())
                return new List<int>();

            var commonTokens = new List<Token>();
            _messageTokenProviderService.AddCustomerTokens(commonTokens, customer);

            return messageTemplates.Select(messageTemplate =>
            {
                var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

                var tokens = new List<Token>(commonTokens);
                _messageTokenProviderService.AddStoreTokens(tokens, emailAccount);

                //event notification
                //_eventPublisherService.MessageTokensAdded(messageTemplate, tokens);

                var toEmail = customer.Email;
                var toName = _customerService.GetCustomerFullName(customer);

                return SendNotification(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
            }).ToList();
        }

        public virtual IList<int> SendCustomerWelcomeMessage(Customer customer, int languageId)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            languageId = InovatiqaDefaults.LanguageId;

            var messageTemplates = GetActiveMessageTemplates(MessageTemplateSystemNames.CustomerWelcomeMessage, InovatiqaDefaults.StoreId);
            if (!messageTemplates.Any())
                return new List<int>();

            var commonTokens = new List<Token>();
            _messageTokenProviderService.AddCustomerTokens(commonTokens, customer);

            return messageTemplates.Select(messageTemplate =>
            {
                var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

                var tokens = new List<Token>(commonTokens);
                _messageTokenProviderService.AddStoreTokens(tokens, emailAccount);

                //event notification
                //_eventPublisher.MessageTokensAdded(messageTemplate, tokens);

                var toEmail = customer.Email;
                var toName = _customerService.GetCustomerFullName(customer);

                return SendNotification(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
            }).ToList();
        }

        public virtual IList<int> SendCustomerEmailRevalidationMessage(Customer customer, int languageId)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));


            var messageTemplates = GetActiveMessageTemplates(InovatiqaDefaults.CustomerEmailRevalidationMessage, InovatiqaDefaults.StoreId);
            if (!messageTemplates.Any())
                return new List<int>();

            var commonTokens = new List<Token>();
            _messageTokenProviderService.AddCustomerTokens(commonTokens, customer);

            return messageTemplates.Select(messageTemplate =>
            {
                var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

                var tokens = new List<Token>(commonTokens);
                _messageTokenProviderService.AddStoreTokens(tokens, emailAccount);

                //_eventPublisher.MessageTokensAdded(messageTemplate, tokens);

                var toEmail = customer.EmailToRevalidate;
                var toName = _customerService.GetCustomerFullName(customer);

                return SendNotification(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
            }).ToList();
        }

        public virtual IList<int> SendCustomerRegisteredNotificationMessage(Customer customer, int languageId)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));


            var messageTemplates = GetActiveMessageTemplates(InovatiqaDefaults.CustomerRegisteredNotification, InovatiqaDefaults.StoreId);
            if (!messageTemplates.Any())
                return new List<int>();

            var commonTokens = new List<Token>();
            _messageTokenProviderService.AddCustomerTokens(commonTokens, customer);

            return messageTemplates.Select(messageTemplate =>
            {
                var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

                var tokens = new List<Token>(commonTokens);
                _messageTokenProviderService.AddStoreTokens(tokens, emailAccount);

                //_eventPublisher.MessageTokensAdded(messageTemplate, tokens);

                var toEmail = emailAccount.Email;
                var toName = emailAccount.DisplayName;

                return SendNotification(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
            }).ToList();
        }

        public virtual IList<int> SendCustomerEmailValidationMessage(Customer customer, int languageId)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            var messageTemplates = GetActiveMessageTemplates(InovatiqaDefaults.CustomerEmailValidationMessage, InovatiqaDefaults.StoreId);
            if (!messageTemplates.Any())
                return new List<int>();

            var commonTokens = new List<Token>();
            _messageTokenProviderService.AddCustomerTokens(commonTokens, customer);

            return messageTemplates.Select(messageTemplate =>
            {
                var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

                var tokens = new List<Token>(commonTokens);
                _messageTokenProviderService.AddStoreTokens(tokens, emailAccount);

                //_eventPublisher.MessageTokensAdded(messageTemplate, tokens);

                var toEmail = customer.Email;
                var toName = _customerService.GetCustomerFullName(customer);

                return SendNotification(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
            }).ToList();
        }

        #endregion

        #region Order workflow

        public virtual IList<int> SendShipmentDeliveredCustomerNotification(Shipment shipment, int languageId)
        {
            if (shipment == null)
                throw new ArgumentNullException(nameof(shipment));

            var order = _orderService.GetOrderById(shipment.OrderId);

            if (order == null)
                throw new Exception("Order cannot be loaded");

            languageId = InovatiqaDefaults.LanguageId;

            var messageTemplates = GetActiveMessageTemplates(MessageTemplateSystemNames.ShipmentDeliveredCustomerNotification, InovatiqaDefaults.StoreId);
            if (!messageTemplates.Any())
                return new List<int>();

            var commonTokens = new List<Token>();
            _messageTokenProviderService.AddShipmentTokens(commonTokens, shipment, languageId);
            _messageTokenProviderService.AddOrderTokens(commonTokens, order, languageId);
            _messageTokenProviderService.AddCustomerTokens(commonTokens, order.CustomerId);

            return messageTemplates.Select(messageTemplate =>
            {
                //email account
                var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

                var tokens = new List<Token>(commonTokens);
                _messageTokenProviderService.AddStoreTokens(tokens, emailAccount);

                //event notification
                //_eventPublisher.MessageTokensAdded(messageTemplate, tokens);

                var billingAddress = _addressService.GetAddressById(order.BillingAddressId);

                var toEmail = billingAddress.Email;
                var toName = $"{billingAddress.FirstName} {billingAddress.LastName}";

                return SendNotification(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
            }).ToList();
        }

        public virtual IList<int> SendShipmentSentCustomerNotification(Shipment shipment, int languageId)
        {
            if (shipment == null)
                throw new ArgumentNullException(nameof(shipment));

            var order = _orderService.GetOrderById(shipment.OrderId);
            if (order == null)
                throw new Exception("Order cannot be loaded");

            languageId = InovatiqaDefaults.LanguageId;

            var messageTemplates = GetActiveMessageTemplates(MessageTemplateSystemNames.ShipmentSentCustomerNotification, InovatiqaDefaults.StoreId);
            if (!messageTemplates.Any())
                return new List<int>();

            var commonTokens = new List<Token>();
            _messageTokenProviderService.AddShipmentTokens(commonTokens, shipment, languageId);
            _messageTokenProviderService.AddOrderTokens(commonTokens, order, languageId);
            _messageTokenProviderService.AddCustomerTokens(commonTokens, order.CustomerId);

            return messageTemplates.Select(messageTemplate =>
            {
                var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

                var tokens = new List<Token>(commonTokens);
                _messageTokenProviderService.AddStoreTokens(tokens, emailAccount);

                //event notification
                //_eventPublisher.MessageTokensAdded(messageTemplate, tokens);

                var billingAddress = _addressService.GetAddressById(order.BillingAddressId);

                var toEmail = billingAddress.Email;
                var toName = $"{billingAddress.FirstName} {billingAddress.LastName}";

                return SendNotification(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
            }).ToList();
        }

        public virtual IList<int> SendOrderRefundedCustomerNotification(Order order, decimal refundedAmount, int languageId)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            languageId = InovatiqaDefaults.LanguageId;

            var messageTemplates = GetActiveMessageTemplates(MessageTemplateSystemNames.OrderRefundedCustomerNotification, InovatiqaDefaults.StoreId);
            if (!messageTemplates.Any())
                return new List<int>();

            var commonTokens = new List<Token>();
            _messageTokenProviderService.AddOrderTokens(commonTokens, order, languageId);
            _messageTokenProviderService.AddOrderRefundedTokens(commonTokens, order, refundedAmount);
            _messageTokenProviderService.AddCustomerTokens(commonTokens, order.CustomerId);

            return messageTemplates.Select(messageTemplate =>
            {
                var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

                var tokens = new List<Token>(commonTokens);
                _messageTokenProviderService.AddStoreTokens(tokens, emailAccount);

                //event notification
                //_eventPublisher.MessageTokensAdded(messageTemplate, tokens);

                var billingAddress = _addressService.GetAddressById(order.BillingAddressId);

                var toEmail = billingAddress.Email;
                var toName = $"{billingAddress.FirstName} {billingAddress.LastName}";

                return SendNotification(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
            }).ToList();
        }

        public virtual IList<int> SendOrderRefundedStoreOwnerNotification(Order order, decimal refundedAmount, int languageId)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            languageId = InovatiqaDefaults.LanguageId;

            var messageTemplates = GetActiveMessageTemplates(MessageTemplateSystemNames.OrderRefundedStoreOwnerNotification, InovatiqaDefaults.StoreId);
            if (!messageTemplates.Any())
                return new List<int>();

            var commonTokens = new List<Token>();
            _messageTokenProviderService.AddOrderTokens(commonTokens, order, languageId);
            _messageTokenProviderService.AddOrderRefundedTokens(commonTokens, order, refundedAmount);
            _messageTokenProviderService.AddCustomerTokens(commonTokens, order.CustomerId);

            return messageTemplates.Select(messageTemplate =>
            {
                var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

                var tokens = new List<Token>(commonTokens);
                _messageTokenProviderService.AddStoreTokens(tokens, emailAccount);

                //event notification
                //_eventPublisher.MessageTokensAdded(messageTemplate, tokens);

                var toEmail = emailAccount.Email;
                var toName = emailAccount.DisplayName;

                return SendNotification(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
            }).ToList();
        }

        public virtual IList<int> SendNewOrderNoteAddedCustomerNotification(OrderNote orderNote, int languageId)
        {
            if (orderNote == null)
                throw new ArgumentNullException(nameof(orderNote));

            var order = _orderService.GetOrderById(orderNote.OrderId);

            if (order == null)
                throw new Exception("Order cannot be loaded");

            languageId = InovatiqaDefaults.LanguageId;

            var messageTemplates = GetActiveMessageTemplates(MessageTemplateSystemNames.NewOrderNoteAddedCustomerNotification, InovatiqaDefaults.StoreId);
            if (!messageTemplates.Any())
                return new List<int>();

            var commonTokens = new List<Token>();
            _messageTokenProviderService.AddOrderNoteTokens(commonTokens, orderNote);
            _messageTokenProviderService.AddOrderTokens(commonTokens, order, languageId);
            _messageTokenProviderService.AddCustomerTokens(commonTokens, order.CustomerId);

            return messageTemplates.Select(messageTemplate =>
            {
                var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

                var tokens = new List<Token>(commonTokens);
                _messageTokenProviderService.AddStoreTokens(tokens, emailAccount);

                //event notification
                //_eventPublisher.MessageTokensAdded(messageTemplate, tokens);

                var billingAddress = _addressService.GetAddressById(order.BillingAddressId);

                var toEmail = billingAddress.Email;
                var toName = $"{billingAddress.FirstName} {billingAddress.LastName}";

                return SendNotification(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
            }).ToList();
        }

        public virtual IList<int> SendOrderPaidVendorNotification(Order order, Vendor vendor, int languageId)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            if (vendor == null)
                throw new ArgumentNullException(nameof(vendor));

            languageId = InovatiqaDefaults.LanguageId;

            var messageTemplates = GetActiveMessageTemplates(MessageTemplateSystemNames.OrderPaidVendorNotification, InovatiqaDefaults.StoreId);
            if (!messageTemplates.Any())
                return new List<int>();

            var commonTokens = new List<Token>();
            _messageTokenProviderService.AddOrderTokens(commonTokens, order, languageId, vendor.Id);
            _messageTokenProviderService.AddCustomerTokens(commonTokens, order.CustomerId);

            return messageTemplates.Select(messageTemplate =>
            {
                var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

                var tokens = new List<Token>(commonTokens);
                _messageTokenProviderService.AddStoreTokens(tokens, emailAccount);

                ////event notification
                //_eventPublisher.MessageTokensAdded(messageTemplate, tokens);

                var toEmail = vendor.Email;
                var toName = vendor.Name;

                return SendNotification(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
            }).ToList();
        }

        public virtual IList<int> SendOrderPaidStoreOwnerNotification(Order order, int languageId)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            languageId = InovatiqaDefaults.LanguageId;

            var messageTemplates = GetActiveMessageTemplates(MessageTemplateSystemNames.OrderPaidStoreOwnerNotification, InovatiqaDefaults.StoreId);
            if (!messageTemplates.Any())
                return new List<int>();

            var commonTokens = new List<Token>();
            _messageTokenProviderService.AddOrderTokens(commonTokens, order, languageId);
            _messageTokenProviderService.AddCustomerTokens(commonTokens, order.CustomerId);

            return messageTemplates.Select(messageTemplate =>
            {
                var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

                var tokens = new List<Token>(commonTokens);
                _messageTokenProviderService.AddStoreTokens(tokens, emailAccount);

                ////event notification
                //_eventPublisher.MessageTokensAdded(messageTemplate, tokens);

                var toEmail = emailAccount.Email;
                var toName = emailAccount.DisplayName;

                return SendNotification(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
            }).ToList();
        }

        public virtual IList<int> SendOrderPaidCustomerNotification(Order order, int languageId,
            string attachmentFilePath = null, string attachmentFileName = null)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            languageId = InovatiqaDefaults.LanguageId;

            var messageTemplates = GetActiveMessageTemplates(MessageTemplateSystemNames.OrderPaidCustomerNotification, InovatiqaDefaults.StoreId);
            if (!messageTemplates.Any())
                return new List<int>();

            var commonTokens = new List<Token>();
            _messageTokenProviderService.AddOrderTokens(commonTokens, order, languageId);
            _messageTokenProviderService.AddCustomerTokens(commonTokens, order.CustomerId);

            return messageTemplates.Select(messageTemplate =>
            {
                var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

                var tokens = new List<Token>(commonTokens);
                _messageTokenProviderService.AddStoreTokens(tokens, emailAccount);

                ////event notification
                //_eventPublisher.MessageTokensAdded(messageTemplate, tokens);

                var billingAddress = _addressService.GetAddressById(order.BillingAddressId);

                var toEmail = billingAddress.Email;
                var toName = $"{billingAddress.FirstName} {billingAddress.LastName}";

                return SendNotification(messageTemplate, emailAccount, languageId, tokens, toEmail, toName,
                    attachmentFilePath, attachmentFileName);
            }).ToList();
        }

        public virtual IList<int> SendOrderCancelledCustomerNotification(Order order, int languageId)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            languageId = InovatiqaDefaults.LanguageId;

            var messageTemplates = GetActiveMessageTemplates(MessageTemplateSystemNames.OrderCancelledCustomerNotification, InovatiqaDefaults.StoreId);
            if (!messageTemplates.Any())
                return new List<int>();

            var commonTokens = new List<Token>();
            _messageTokenProviderService.AddOrderTokens(commonTokens, order, languageId);
            _messageTokenProviderService.AddCustomerTokens(commonTokens, order.CustomerId);

            return messageTemplates.Select(messageTemplate =>
            {
                var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

                var tokens = new List<Token>(commonTokens);
                _messageTokenProviderService.AddStoreTokens(tokens, emailAccount);

                ////event notification
                //_eventPublisher.MessageTokensAdded(messageTemplate, tokens);

                var billingAddress = _addressService.GetAddressById(order.BillingAddressId);

                var toEmail = billingAddress.Email;
                var toName = $"{billingAddress.FirstName} {billingAddress.LastName}";

                return SendNotification(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
            }).ToList();
        }

        public virtual IList<int> SendOrderCompletedCustomerNotification(Order order, int languageId,
            string attachmentFilePath = null, string attachmentFileName = null)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            languageId = InovatiqaDefaults.LanguageId;

            var messageTemplates = GetActiveMessageTemplates(MessageTemplateSystemNames.OrderCompletedCustomerNotification, InovatiqaDefaults.StoreId);
            if (!messageTemplates.Any())
                return new List<int>();

            var commonTokens = new List<Token>();
            _messageTokenProviderService.AddOrderTokens(commonTokens, order, languageId);
            _messageTokenProviderService.AddCustomerTokens(commonTokens, order.CustomerId);

            return messageTemplates.Select(messageTemplate =>
            {
                var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

                var tokens = new List<Token>(commonTokens);
                _messageTokenProviderService.AddStoreTokens(tokens, emailAccount);

                ////event notification
                //_eventPublisher.MessageTokensAdded(messageTemplate, tokens);

                var billingAddress = _addressService.GetAddressById(order.BillingAddressId);

                var toEmail = billingAddress.Email;
                var toName = $"{billingAddress.FirstName} {billingAddress.LastName}";

                return SendNotification(messageTemplate, emailAccount, languageId, tokens, toEmail, toName,
                    attachmentFilePath, attachmentFileName);
            }).ToList();
        }

        public virtual IList<int> SendOrderPlacedVendorNotification(Order order, Vendor vendor, int languageId)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            if (vendor == null)
                throw new ArgumentNullException(nameof(vendor));

            languageId = InovatiqaDefaults.LanguageId;

            var messageTemplates = GetActiveMessageTemplates(MessageTemplateSystemNames.OrderPlacedVendorNotification, InovatiqaDefaults.StoreId);
            if (!messageTemplates.Any())
                return new List<int>();

            var commonTokens = new List<Token>();
            _messageTokenProviderService.AddOrderTokens(commonTokens, order, languageId, vendor.Id);
            _messageTokenProviderService.AddCustomerTokens(commonTokens, order.CustomerId);

            return messageTemplates.Select(messageTemplate =>
            {
                var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

                var tokens = new List<Token>(commonTokens);
                _messageTokenProviderService.AddStoreTokens(tokens, emailAccount);

                ////event notification
                //_eventPublisher.MessageTokensAdded(messageTemplate, tokens);

                var toEmail = vendor.Email;
                var toName = vendor.Name;

                return SendNotification(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
            }).ToList();
        }

        public virtual IList<int> SendOrderPlacedStoreOwnerNotification(Order order, int languageId)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            languageId = InovatiqaDefaults.LanguageId;

            var messageTemplates = GetActiveMessageTemplates(MessageTemplateSystemNames.OrderPlacedStoreOwnerNotification, InovatiqaDefaults.StoreId);
            if (!messageTemplates.Any())
                return new List<int>();

            var commonTokens = new List<Token>();
            _messageTokenProviderService.AddOrderTokens(commonTokens, order, languageId);
            _messageTokenProviderService.AddCustomerTokens(commonTokens, order.CustomerId);

            return messageTemplates.Select(messageTemplate =>
            {
                var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

                var tokens = new List<Token>(commonTokens);
                _messageTokenProviderService.AddStoreTokens(tokens, emailAccount);

                ////event notification
                //_eventPublisher.MessageTokensAdded(messageTemplate, tokens);

                var toEmail = emailAccount.Email;
                var toName = emailAccount.DisplayName;

                return SendNotification(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
            }).ToList();
        }

        public virtual IList<int> SendOrderPlacedCustomerNotification(Order order, int languageId,
            string attachmentFilePath = null, string attachmentFileName = null)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            languageId = InovatiqaDefaults.LanguageId;

            var messageTemplates = GetActiveMessageTemplates(MessageTemplateSystemNames.OrderPlacedCustomerNotification, InovatiqaDefaults.StoreId);
            if (!messageTemplates.Any())
                return new List<int>();

            var commonTokens = new List<Token>();
            _messageTokenProviderService.AddOrderTokens(commonTokens, order, languageId);
            _messageTokenProviderService.AddCustomerTokens(commonTokens, order.CustomerId);

            return messageTemplates.Select(messageTemplate =>
            {
                var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

                var tokens = new List<Token>(commonTokens);
                _messageTokenProviderService.AddStoreTokens(tokens, emailAccount);

                ////event notification
                //_eventPublisher.MessageTokensAdded(messageTemplate, tokens);

                var billingAddress = _addressService.GetAddressById(order.BillingAddressId);

                var toEmail = billingAddress.Email;
                var toName = $"{billingAddress.FirstName} {billingAddress.LastName}";

                return SendNotification(messageTemplate, emailAccount, languageId, tokens, toEmail, toName,
                    attachmentFilePath, attachmentFileName);
            }).ToList();
        }

        #endregion

        #region Newsletter workflow



        #endregion

        #region Send a message to a friend


        #endregion

        #region Return requests

        public virtual IList<int> SendReturnRequestStatusChangedCustomerNotification(ReturnRequest returnRequest, OrderItem orderItem, Order order)
        {
            if (returnRequest == null)
                throw new ArgumentNullException(nameof(returnRequest));

            if (orderItem == null)
                throw new ArgumentNullException(nameof(orderItem));

            if (order == null)
                throw new ArgumentNullException(nameof(order));

            var languageId = InovatiqaDefaults.LanguageId;

            var messageTemplates = GetActiveMessageTemplates(MessageTemplateSystemNames.ReturnRequestStatusChangedCustomerNotification, InovatiqaDefaults.StoreId);
            if (!messageTemplates.Any())
                return new List<int>();

            var customer = _customerService.GetCustomerById(returnRequest.CustomerId);

            var commonTokens = new List<Token>();
            _messageTokenProviderService.AddOrderTokens(commonTokens, order, languageId);
            _messageTokenProviderService.AddCustomerTokens(commonTokens, customer);
            _messageTokenProviderService.AddReturnRequestTokens(commonTokens, returnRequest, orderItem);

            return messageTemplates.Select(messageTemplate =>
            {
                var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

                var tokens = new List<Token>(commonTokens);
                _messageTokenProviderService.AddStoreTokens(tokens, emailAccount);

                //event notification
                //_eventPublisher.MessageTokensAdded(messageTemplate, tokens);

                var billingAddress = _addressService.GetAddressById(order.BillingAddressId);

                var toEmail = _customerService.IsGuest(customer) ?
                    billingAddress.Email :
                    customer.Email;
                var toName = _customerService.IsGuest(customer) ?
                    billingAddress.FirstName :
                    _customerService.GetCustomerFullName(customer);

                return SendNotification(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
            }).ToList();
        }

        public virtual IList<int> SendNewReturnRequestStoreOwnerNotification(ReturnRequest returnRequest, OrderItem orderItem, Order order, int languageId)
        {
            if (returnRequest == null)
                throw new ArgumentNullException(nameof(returnRequest));

            if (orderItem == null)
                throw new ArgumentNullException(nameof(orderItem));

            if (order == null)
                throw new ArgumentNullException(nameof(order));

            languageId = InovatiqaDefaults.LanguageId;

            var messageTemplates = GetActiveMessageTemplates(MessageTemplateSystemNames.NewReturnRequestStoreOwnerNotification, InovatiqaDefaults.StoreId);
            if (!messageTemplates.Any())
                return new List<int>();

            var commonTokens = new List<Token>();
            _messageTokenProviderService.AddOrderTokens(commonTokens, order, languageId);
            _messageTokenProviderService.AddCustomerTokens(commonTokens, returnRequest.CustomerId);
            _messageTokenProviderService.AddReturnRequestTokens(commonTokens, returnRequest, orderItem);

            return messageTemplates.Select(messageTemplate =>
            {
                var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

                var tokens = new List<Token>(commonTokens);
                _messageTokenProviderService.AddStoreTokens(tokens, emailAccount);

                //event notification
                //_eventPublisher.MessageTokensAdded(messageTemplate, tokens);

                var toEmail = emailAccount.Email;
                var toName = emailAccount.DisplayName;

                return SendNotification(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
            }).ToList();
        }
        public virtual IList<int> SendNewReturnRequestCustomerNotification(ReturnRequest returnRequest, OrderItem orderItem, Order order)
        {
            if (returnRequest == null)
                throw new ArgumentNullException(nameof(returnRequest));

            if (orderItem == null)
                throw new ArgumentNullException(nameof(orderItem));

            if (order == null)
                throw new ArgumentNullException(nameof(order));

            var languageId = InovatiqaDefaults.LanguageId;

            var messageTemplates = GetActiveMessageTemplates(MessageTemplateSystemNames.NewReturnRequestCustomerNotification, InovatiqaDefaults.StoreId);
            if (!messageTemplates.Any())
                return new List<int>();

            var customer = _customerService.GetCustomerById(returnRequest.CustomerId);

            var commonTokens = new List<Token>();
            _messageTokenProviderService.AddOrderTokens(commonTokens, order, languageId);
            _messageTokenProviderService.AddCustomerTokens(commonTokens, customer);
            _messageTokenProviderService.AddReturnRequestTokens(commonTokens, returnRequest, orderItem);

            return messageTemplates.Select(messageTemplate =>
            {
                var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

                var tokens = new List<Token>(commonTokens);
                _messageTokenProviderService.AddStoreTokens(tokens, emailAccount);

                //event notification
                //_eventPublisher.MessageTokensAdded(messageTemplate, tokens);

                var billingAddress = _addressService.GetAddressById(order.BillingAddressId);

                var toEmail = _customerService.IsGuest(customer) ?
                    billingAddress.Email :
                    customer.Email;
                var toName = _customerService.IsGuest(customer) ?
                    billingAddress.FirstName :
                    _customerService.GetCustomerFullName(customer);

                return SendNotification(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
            }).ToList();
        }


        #endregion

        #region Forum Notifications


        #endregion

        #region Misc

        public virtual int SendNotification(MessageTemplate messageTemplate,
            EmailAccount emailAccount, int languageId, IEnumerable<Token> tokens,
            string toEmailAddress, string toName,
            string attachmentFilePath = null, string attachmentFileName = null,
            string replyToEmailAddress = null, string replyToName = null,
            string fromEmail = null, string fromName = null, string subject = null)
        {
            if (messageTemplate == null)
                throw new ArgumentNullException(nameof(messageTemplate));

            if (emailAccount == null)
                throw new ArgumentNullException(nameof(emailAccount));

            var bcc = messageTemplate.BccEmailAddresses;
            if (string.IsNullOrEmpty(subject))
                subject = messageTemplate.Subject;
            var body = messageTemplate.Body;

            var subjectReplaced = _tokenizerService.Replace(subject, tokens, false);
            var bodyReplaced = _tokenizerService.Replace(body, tokens, true);

            toName = CommonHelper.EnsureMaximumLength(toName, 300);

            var email = new QueuedEmail
            {
                PriorityId = InovatiqaDefaults.QueuedEmailPrioritHigh,
                From = !string.IsNullOrEmpty(fromEmail) ? fromEmail : emailAccount.Email,
                FromName = !string.IsNullOrEmpty(fromName) ? fromName : emailAccount.DisplayName,
                To = toEmailAddress,
                ToName = toName,
                ReplyTo = replyToEmailAddress,
                ReplyToName = replyToName,
                Cc = string.Empty,
                Bcc = bcc,
                Subject = subjectReplaced,
                Body = bodyReplaced,
                AttachmentFilePath = attachmentFilePath,
                AttachmentFileName = attachmentFileName,
                AttachedDownloadId = messageTemplate.AttachedDownloadId,
                CreatedOnUtc = DateTime.UtcNow,
                EmailAccountId = emailAccount.Id,
                DontSendBeforeDateUtc = !messageTemplate.DelayBeforeSend.HasValue ? null
                    : (DateTime?)(DateTime.UtcNow + TimeSpan.FromHours(InovatiqaDefaults.DelayPeriod))
            };

            _queuedEmailService.InsertQueuedEmail(email);
            return email.Id;
        }

        public virtual IList<int> SendProductReviewNotificationMessage(ProductReview productReview, int languageId)
        {
            if (productReview == null)
                throw new ArgumentNullException(nameof(productReview));

            languageId = InovatiqaDefaults.LanguageId;

            var messageTemplates = GetActiveMessageTemplates(InovatiqaDefaults.ProductReviewStoreOwnerNotification, InovatiqaDefaults.StoreId);
            if (!messageTemplates.Any())
                return new List<int>();

            var commonTokens = new List<Token>();
            _messageTokenProviderService.AddProductReviewTokens(commonTokens, productReview);
            _messageTokenProviderService.AddCustomerTokens(commonTokens, productReview.CustomerId);

            return messageTemplates.Select(messageTemplate =>
            {
                var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

                var tokens = new List<Token>(commonTokens);
                _messageTokenProviderService.AddStoreTokens(tokens, emailAccount);

                //_eventPublisher.MessageTokensAdded(messageTemplate, tokens);

                var toEmail = emailAccount.Email;
                var toName = emailAccount.DisplayName;

                return SendNotification(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
            }).ToList();
        }

        #endregion

        #endregion
    }
}