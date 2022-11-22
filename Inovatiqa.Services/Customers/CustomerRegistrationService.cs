using Inovatiqa.Core;
using Inovatiqa.Database.Models;
using Inovatiqa.Services.Common.Interfaces;
using Inovatiqa.Services.Customers.Interfaces;
using Inovatiqa.Services.Messages.Interfaces;
using Inovatiqa.Services.Security.Interfaces;
using Inovatiqa.Services.WorkContext.Interfaces;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;

namespace Inovatiqa.Services.Customers
{
    public partial class CustomerRegistrationService : ICustomerRegistrationService
    {
        #region Fields

        private readonly ICustomerService _customerService;
        private readonly IEncryptionService _encryptionService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly INewsLetterSubscriptionService _newsLetterSubscriptionService;
        private readonly IWorkContextService _workContextService;
        private readonly IWorkflowMessageService _workflowMessageService;

        #endregion

        #region Ctor

        public CustomerRegistrationService(ICustomerService customerService,
            IEncryptionService encryptionService,
            IGenericAttributeService genericAttributeService,
            INewsLetterSubscriptionService newsLetterSubscriptionService,
            IWorkContextService workContextService,
            IWorkflowMessageService workflowMessageService)
        {
            _customerService = customerService;
            _encryptionService = encryptionService;
            _genericAttributeService = genericAttributeService;
            _newsLetterSubscriptionService = newsLetterSubscriptionService;
            _workContextService = workContextService;
            _workflowMessageService = workflowMessageService;
        }

        #endregion

        #region Utilities

        protected bool PasswordsMatch(CustomerPassword customerPassword, string enteredPassword)
        {
            if (customerPassword == null || string.IsNullOrEmpty(enteredPassword))
                return false;

            var savedPassword = string.Empty;
            switch (customerPassword.PasswordFormatId)
            {
                case InovatiqaDefaults.Clear:
                    savedPassword = enteredPassword;
                    break;
                case InovatiqaDefaults.Encrypted:
                    savedPassword = _encryptionService.EncryptText(enteredPassword);
                    break;
                case InovatiqaDefaults.Hashed:
                    savedPassword = _encryptionService.CreatePasswordHash(enteredPassword, customerPassword.PasswordSalt, InovatiqaDefaults.HashedPasswordFormat);
                    break;
            }

            if (customerPassword.Password == null)
                return false;

            return customerPassword.Password.Equals(savedPassword);
        }

        #endregion

        #region Methods

        public virtual CustomerLoginResults ValidateCustomer(string usernameOrEmail, string password)
        {
            var customer = InovatiqaDefaults.UsernamesEnabled ?
                _customerService.GetCustomerByUsername(usernameOrEmail) :
                _customerService.GetCustomerByEmail(usernameOrEmail);

            if (customer == null)
                return CustomerLoginResults.CustomerNotExist;
            if (customer.Deleted)
                return CustomerLoginResults.Deleted;
            if (!customer.Active)
                return CustomerLoginResults.NotActive;
            if (!_customerService.IsRegistered(customer))
                return CustomerLoginResults.NotRegistered;
            if (customer.CannotLoginUntilDateUtc.HasValue && customer.CannotLoginUntilDateUtc.Value > DateTime.UtcNow)
                return CustomerLoginResults.LockedOut;

            if (!PasswordsMatch(_customerService.GetCurrentPassword(customer.Id), password))
            {
                customer.FailedLoginAttempts++;
                if (InovatiqaDefaults.FailedPasswordAllowedAttempts > 0 &&
                    customer.FailedLoginAttempts >= InovatiqaDefaults.FailedPasswordAllowedAttempts)
                {
                    customer.CannotLoginUntilDateUtc = DateTime.UtcNow.AddMinutes(InovatiqaDefaults.FailedPasswordLockoutMinutes);
                    customer.FailedLoginAttempts = 0;
                }

                _customerService.UpdateCustomer(customer);

                return CustomerLoginResults.WrongPassword;
            }

            customer.FailedLoginAttempts = 0;
            customer.CannotLoginUntilDateUtc = null;
            customer.RequireReLogin = false;
            customer.LastLoginDateUtc = DateTime.UtcNow;
            _customerService.UpdateCustomer(customer);

            return CustomerLoginResults.Successful;
        }

        public virtual CustomerRegistrationResult RegisterCustomer(CustomerRegistrationRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (request.Customer == null)
                throw new ArgumentException("Can't load current customer");

            var result = new CustomerRegistrationResult();

            if (_customerService.IsRegistered(request.Customer))
            {
                result.AddError("Current customer is already registered");
                return result;
            }

            if (string.IsNullOrEmpty(request.Email))
            {
                result.AddError("Email is required.");
                return result;
            }

            if (!CommonHelper.IsValidEmail(request.Email))
            {
                result.AddError("Wrong email");
                return result;
            }

            if (string.IsNullOrWhiteSpace(request.Password))
            {
                result.AddError("Password is not provided");
                return result;
            }

            if (InovatiqaDefaults.UsernamesEnabled && string.IsNullOrEmpty(request.Username))
            {
                result.AddError("Username is required.");
                return result;
            }

            if (_customerService.GetCustomerByEmail(request.Email) != null)
            {
                result.AddError("The specified email already exists");
                return result;
            }

            if (InovatiqaDefaults.UsernamesEnabled && _customerService.GetCustomerByUsername(request.Username) != null)
            {
                result.AddError("The specified username already exists");
                return result;
            }

            request.Customer.Username = request.Username;
            request.Customer.Email = request.Email;

            var customerPassword = new CustomerPassword
            {
                CustomerId = request.Customer.Id,
                PasswordFormatId = request.PasswordFormatId,
                CreatedOnUtc = DateTime.UtcNow
            };
            switch (request.PasswordFormatId)
            {
                case InovatiqaDefaults.Clear:
                    customerPassword.Password = request.Password;
                    break;
                case InovatiqaDefaults.Encrypted:
                    customerPassword.Password = _encryptionService.EncryptText(request.Password);
                    break;
                case InovatiqaDefaults.Hashed:
                    var saltKey = _encryptionService.CreateSaltKey(InovatiqaDefaults.PasswordSaltKeySize);
                    customerPassword.PasswordSalt = saltKey;
                    customerPassword.Password = _encryptionService.CreatePasswordHash(request.Password, saltKey, InovatiqaDefaults.HashedPasswordFormat);
                    break;
            }

            _customerService.InsertCustomerPassword(customerPassword);

            request.Customer.Active = request.IsApproved;

            var registeredRole = _customerService.GetCustomerRoleBySystemName(InovatiqaDefaults.RegisteredRoleName);
            if (registeredRole == null)
                throw new InovatiqaException("'Registered' role could not be loaded");

            _customerService.AddCustomerRoleMapping(new CustomerCustomerRoleMapping { CustomerId = request.Customer.Id, CustomerRoleId = registeredRole.Id });

            if (_customerService.IsGuest(request.Customer))
            {
                var guestRole = _customerService.GetCustomerRoleBySystemName(InovatiqaDefaults.GuestsRoleName);
                _customerService.RemoveCustomerRoleMapping(request.Customer, guestRole);
            }

            _customerService.UpdateCustomer(request.Customer);

            return result;
        }

        public virtual ChangePasswordResult ChangePassword(ChangePasswordRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var result = new ChangePasswordResult();
            if (string.IsNullOrWhiteSpace(request.Email))
            {
                result.AddError("Email is not entered");
                return result;
            }

            if (string.IsNullOrWhiteSpace(request.NewPassword))
            {
                result.AddError("Password is not entered");
                return result;
            }

            var customer = _customerService.GetCustomerByEmail(request.Email);
            if (customer == null)
            {
                result.AddError("The specified email could not be found");
                return result;
            }

            if (request.ValidateRequest && !PasswordsMatch(_customerService.GetCurrentPassword(customer.Id), request.OldPassword))
            {
                result.AddError("Old password doesn't match");
                return result;
            }

            if (InovatiqaDefaults.UnduplicatedPasswordsNumber > 0)
            {
                var previousPasswords = _customerService.GetCustomerPasswords(customer.Id, passwordsToReturn: InovatiqaDefaults.UnduplicatedPasswordsNumber);

                var newPasswordMatchesWithPrevious = previousPasswords.Any(password => PasswordsMatch(password, request.NewPassword));
                if (newPasswordMatchesWithPrevious)
                {
                    result.AddError("You entered the password that is the same as one of the last passwords you used. Please create a new password.");
                    return result;
                }
            }

            var customerPassword = new CustomerPassword
            {
                CustomerId = customer.Id,
                PasswordFormatId = request.NewPasswordFormatId,
                CreatedOnUtc = DateTime.UtcNow
            };
            switch (request.NewPasswordFormatId)
            {
                case InovatiqaDefaults.Clear:
                    customerPassword.Password = request.NewPassword;
                    break;
                case InovatiqaDefaults.Encrypted:
                    customerPassword.Password = _encryptionService.EncryptText(request.NewPassword);
                    break;
                case InovatiqaDefaults.Hashed:
                    var saltKey = _encryptionService.CreateSaltKey(InovatiqaDefaults.PasswordSaltKeySize);
                    customerPassword.PasswordSalt = saltKey;
                    customerPassword.Password = _encryptionService.CreatePasswordHash(request.NewPassword, saltKey,
                        request.HashedPasswordFormat ?? InovatiqaDefaults.HashedPasswordFormat);
                    break;
            }

            _customerService.InsertCustomerPassword(customerPassword);

            //_eventPublisher.Publish(new CustomerPasswordChangedEvent(customerPassword));

            return result;
        }

        public virtual void SetEmail(Customer customer, string newEmail, bool requireValidation)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            if (newEmail == null)
                throw new InovatiqaException("Email cannot be null");

            newEmail = newEmail.Trim();
            var oldEmail = customer.Email;

            if (!CommonHelper.IsValidEmail(newEmail))
                throw new InovatiqaException("New email is not valid");

            if (newEmail.Length > 100)
                throw new InovatiqaException("E-mail address is too long");

            var customer2 = _customerService.GetCustomerByEmail(newEmail);
            if (customer2 != null && customer.Id != customer2.Id)
                throw new InovatiqaException("The e-mail address is already in use");

            if (requireValidation)
            {
                customer.EmailToRevalidate = newEmail;
                _customerService.UpdateCustomer(customer);

                _genericAttributeService.SaveAttribute(customer.GetType().Name, customer.Id, InovatiqaDefaults.EmailRevalidationTokenAttribute, Guid.NewGuid().ToString(), InovatiqaDefaults.StoreId);

                _workflowMessageService.SendCustomerEmailRevalidationMessage(customer, InovatiqaDefaults.LanguageId);
            }
            else
            {
                customer.Email = newEmail;
                _customerService.UpdateCustomer(customer);

                if (string.IsNullOrEmpty(oldEmail) || oldEmail.Equals(newEmail, StringComparison.InvariantCultureIgnoreCase))
                    return;

                var subscriptionOld = _newsLetterSubscriptionService.GetNewsLetterSubscriptionByEmailAndStoreId(oldEmail, InovatiqaDefaults.StoreId);

                subscriptionOld.Email = newEmail;
                _newsLetterSubscriptionService.UpdateNewsLetterSubscription(subscriptionOld);
            }
        }

        public virtual void SetUsername(Customer customer, string newUsername)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            if (!InovatiqaDefaults.UsernamesEnabled)
                throw new InovatiqaException("Usernames are disabled");

            newUsername = newUsername.Trim();

            if (newUsername.Length > InovatiqaDefaults.CustomerUsernameLength)
                throw new InovatiqaException("Username is too long");

            var user2 = _customerService.GetCustomerByUsername(newUsername);
            if (user2 != null && customer.Id != user2.Id)
                throw new InovatiqaException("The username is already in use");

            customer.Username = newUsername;
            _customerService.UpdateCustomer(customer);
        }
		public virtual CustomerPassword PrepareChildPasswordModel(IFormCollection collection, int CustomerId)
        {
            var model = new CustomerPassword();
            model.CustomerId = CustomerId;
            var saltKey = _encryptionService.CreateSaltKey(InovatiqaDefaults.PasswordSaltKeySize);
            model.PasswordSalt = saltKey;
            model.Password = _encryptionService.CreatePasswordHash(collection["password1"], saltKey,
                                                                    InovatiqaDefaults.HashedPasswordFormat);
            model.PasswordFormatId = InovatiqaDefaults.Hashed;
            model.CreatedOnUtc = DateTime.UtcNow;
            return model;
        }
        #endregion
    }
}