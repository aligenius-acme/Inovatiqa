using Inovatiqa.Core.Interfaces;
using Inovatiqa.Database.Models;
using System;
using System.Collections.Generic;

namespace Inovatiqa.Services.Customers.Interfaces
{
    public partial interface ICustomerService
    {
        #region Customers

        IPagedList<Customer> GetCustomersWithShoppingCarts(int? shoppingCartTypeId = null,
            int storeId = 0, int? productId = null,
            DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int? countryId = null,
            int pageIndex = 0, int pageSize = int.MaxValue);

        void DeleteCustomer(Customer customer);

        bool IsAdmin(Customer customer, bool onlyActiveCustomerRoles = true);

        bool IsB2B(Customer customer, bool onlyActiveCustomerRoles = true);

        bool IsPO(Customer customer, bool onlyActiveCustomerRoles = true);

        IList<Customer> GetCustomersByIds(int[] customerIds);

        void UpdateCustomer(Customer customer);

        Customer GetCustomerByGuid(Guid customerGuid);

        Customer GetCustomerByUsername(string username);

        bool IsRegistered(Customer customer, bool onlyActiveCustomerRoles = true);

        bool IsInCustomerRole(Customer customer, string customerRoleSystemName, bool onlyActiveCustomerRoles = true);

        IList<CustomerRole> GetCustomerRoles(Customer customer, bool showHidden = false);

        Customer InsertGuestCustomer();

        CustomerRole GetCustomerRoleBySystemName(string systemName);

        void AddCustomerRoleMapping(CustomerCustomerRoleMapping roleMapping);
        List<string> GetCustomerRoleMappingByCustomerId(int CustomerId);

        Customer GetCustomerById(int customerId);

        string FormatUsername(Customer customer, bool stripTooLong = false, int maxLength = 0);

        string GetCustomerFullName(Customer customer);

        Customer GetShoppingCartCustomer(IList<ShoppingCartItem> shoppingCart);

        void ResetCheckoutData(Customer customer, int storeId,
            bool clearCouponCodes = false, bool clearCheckoutAttributes = false,
            bool clearRewardPoints = true, bool clearShippingMethod = true,
            bool clearPaymentMethod = true);

        Customer GetOrCreateBackgroundTaskUser();

        void InsertCustomer(Customer customer);

        Customer GetCustomerBySystemName(string systemName);

        Customer GetCustomerByEmail(string email);

        string[] ParseAppliedDiscountCouponCodes(Customer customer);

        IPagedList<Customer> GetAllCustomers(DateTime? createdFromUtc = null, DateTime? createdToUtc = null,
            int affiliateId = 0, int vendorId = 0, int[] customerRoleIds = null,
            string email = null, string username = null, string firstName = null, string lastName = null,
            int dayOfBirth = 0, int monthOfBirth = 0,
            string company = null, string phone = null, string zipPostalCode = null, string ipAddress = null,
            int pageIndex = 0, int pageSize = int.MaxValue, bool getOnlyTotalCount = false);

        IList<Order> GetCustomerUnPaidOrders(Customer customer);

        #endregion

        #region Customer roles

        void InsertCustomerRole(CustomerRole customerRole);

        void UpdateCustomerRole(CustomerRole customerRole);

        void DeleteCustomerRole(CustomerRole customerRole);

        CustomerRole GetCustomerRoleById(int customerRoleId);

        IList<CustomerRole> GetAllCustomerRoles(bool showHidden = false);

        bool IsGuest(Customer customer, bool onlyActiveCustomerRoles = true);

        void RemoveCustomerRoleMapping(Customer customer, CustomerRole role);

        int[] GetCustomerRoleIds(Customer customer, bool showHidden = false);

        bool IsVendor(Customer customer, bool onlyActiveCustomerRoles = true);
        void RemoveAllCustomerRoleMappings(int CustomerId);

        #endregion

        #region Customer passwords

        bool IsPasswordRecoveryLinkExpired(Customer customer);

        bool IsPasswordRecoveryTokenValid(Customer customer, string token);

        IList<CustomerPassword> GetCustomerPasswords(int? customerId = null,
        int? passwordFormat = null, int? passwordsToReturn = null);

        CustomerPassword GetCurrentPassword(int customerId);

        void InsertCustomerPassword(CustomerPassword customerPassword);

        bool PasswordIsExpired(Customer customer);

        #endregion

        #region Customer address mapping

        void RemoveCustomerAddress(Customer customer, Address address);
        void RemoveCustomer(Customer customer);

        void InsertCustomerAddress(Customer customer, Address address);

        Address GetCustomerBillingAddress(Customer customer);

        Address GetCustomerAddress(int customerId, int addressId);

        Address GetCustomerShippingAddress(Customer customer);

        IList<Address> GetAddressesByCustomerId(int customerId);
        List<int> getAllChildAccounts(Customer customer);

        IPagedList<Customer> GetOnlineCustomers(DateTime lastActivityFromUtc,
            int[] customerRoleIds, int pageIndex = 0, int pageSize = int.MaxValue);
        List<int> getAllChildAccountsIds(Customer customer);

        #endregion
    }
}