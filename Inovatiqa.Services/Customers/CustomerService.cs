using Inovatiqa.Core;
using Inovatiqa.Core.Caching;
using Inovatiqa.Core.Interfaces;
using Inovatiqa.Database.Interfaces;
using Inovatiqa.Database.Models;
using Inovatiqa.Services.Caching.Extensions.Interfaces;
using Inovatiqa.Services.Caching.Interfaces;
using Inovatiqa.Services.Common.Interfaces;
using Inovatiqa.Services.Customers.Interfaces;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml;

namespace Inovatiqa.Services.Customers
{
    public partial class CustomerService : ICustomerService
    {
        #region Fields

        private readonly IRepository<Customer> _customerRepository;
        private readonly IRepository<CustomerRole> _customerRoleRepository;
        private readonly IRepository<CustomerCustomerRoleMapping> _customerCustomerRoleMappingRepository;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IRepository<CustomerPassword> _customerPasswordRepository;
        private readonly IRepository<CustomerAddresses> _customerAddressRepository;
        private readonly IRepository<Address> _addressRepository;
        private readonly ICacheKeyService _cacheKeyService;
        private readonly IStaticCacheManager _staticCacheManager;
        private readonly IRepository<GenericAttribute> _gaRepository;
        private readonly IRepository<ShoppingCartItem> _shoppingCartRepository;
        private readonly IRepository<Order> _orderRepository;

        #endregion

        #region Ctor

        public CustomerService(IRepository<Customer> customerRepository,
            IRepository<CustomerRole> customerRoleRepository,
            IRepository<CustomerCustomerRoleMapping> customerCustomerRoleMappingRepository,
            IGenericAttributeService genericAttributeService,
            IRepository<CustomerPassword> customerPasswordRepository,
            IRepository<CustomerAddresses> customerAddressRepository,
            IRepository<Address> addressRepository,
            ICacheKeyService cacheKeyService,
            IStaticCacheManager staticCacheManager,
            IRepository<GenericAttribute> gaRepository,
            IRepository<ShoppingCartItem> shoppingCartRepository,
            IRepository<Order> orderRepository)
        {
            _customerRepository = customerRepository;
            _customerRoleRepository = customerRoleRepository;
            _customerCustomerRoleMappingRepository = customerCustomerRoleMappingRepository;
            _genericAttributeService = genericAttributeService;
            _customerPasswordRepository = customerPasswordRepository;
            _customerAddressRepository = customerAddressRepository;
            _addressRepository = addressRepository;
            _cacheKeyService = cacheKeyService;
            _staticCacheManager = staticCacheManager;
            _gaRepository = gaRepository;
            _shoppingCartRepository = shoppingCartRepository;
            _orderRepository = orderRepository;
        }

        #endregion

        #region Methods

        #region Customers

        public virtual IPagedList<Customer> GetCustomersWithShoppingCarts(int? shoppingCartTypeId = null,
            int storeId = 0, int? productId = null,
            DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int? countryId = null,
            int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var items = _shoppingCartRepository.Query();

            if (shoppingCartTypeId.HasValue)
                items = items.Where(item => item.ShoppingCartTypeId == (int)shoppingCartTypeId.Value);

            if (storeId > 0)
                items = items.Where(item => item.StoreId == storeId);

            if (productId > 0)
                items = items.Where(item => item.ProductId == productId);

            if (createdFromUtc.HasValue)
                items = items.Where(item => createdFromUtc.Value <= item.CreatedOnUtc);
            if (createdToUtc.HasValue)
                items = items.Where(item => createdToUtc.Value >= item.CreatedOnUtc);

            var customers = _customerRepository.Query().Where(customer => customer.Active && !customer.Deleted);

            if (countryId > 0)
                customers = from c in customers
                            join a in _addressRepository.Query() on c.BillingAddressId equals a.Id
                            where a.CountryId == countryId
                            select c;

            var customersWithCarts = from c in customers
                                     join item in items on c.Id equals item.CustomerId
                                     orderby c.Id
                                     select c;

            return new PagedList<Customer>(customersWithCarts, pageIndex, pageSize);
        }
		public virtual void RemoveCustomer(Customer customer)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            if (customer.IsSystemAccount)
                throw new InovatiqaException($"System customer account ({customer.SystemName}) could not be deleted");

            _customerRepository.Delete(customer);
        }

        public virtual void DeleteCustomer(Customer customer)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            if (customer.IsSystemAccount)
                throw new InovatiqaException($"System customer account ({customer.SystemName}) could not be deleted");

            customer.Deleted = true;

            if (!string.IsNullOrEmpty(customer.Email))
                customer.Email += "-DELETED";
            if (!string.IsNullOrEmpty(customer.Username))
                customer.Username += "-DELETED";

            UpdateCustomer(customer);

            //event notification
            //_eventPublisher.EntityDeleted(customer);
        }
        public virtual bool IsAdmin(Customer customer, bool onlyActiveCustomerRoles = true)
        {
            return IsInCustomerRole(customer, InovatiqaDefaults.AdministratorsRoleName, onlyActiveCustomerRoles);
        }

        public virtual bool IsB2B(Customer customer, bool onlyActiveCustomerRoles = true)
        {
            return IsInCustomerRole(customer, InovatiqaDefaults.B2BRoleName, onlyActiveCustomerRoles);
        }

        public virtual bool IsPO(Customer customer, bool onlyActiveCustomerRoles = true)
        {
            return IsInCustomerRole(customer, InovatiqaDefaults.PORoleName, onlyActiveCustomerRoles);
        }

        public virtual IList<Customer> GetCustomersByIds(int[] customerIds)
        {
            if (customerIds == null || customerIds.Length == 0)
                return new List<Customer>();

            var query = from c in _customerRepository.Query()
                        where customerIds.Contains(c.Id) && !c.Deleted
                        select c;
            var customers = query.ToList();
            var sortedCustomers = new List<Customer>();
            foreach (var id in customerIds)
            {
                var customer = customers.Find(x => x.Id == id);
                if (customer != null)
                    sortedCustomers.Add(customer);
            }

            return sortedCustomers;
        }

        public virtual IPagedList<Customer> GetOnlineCustomers(DateTime lastActivityFromUtc,
            int[] customerRoleIds, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = _customerRepository.Query();
            query = query.Where(c => lastActivityFromUtc <= c.LastActivityDateUtc);
            query = query.Where(c => !c.Deleted);

            if (customerRoleIds != null && customerRoleIds.Length > 0)
                query = query.Where(c => _customerCustomerRoleMappingRepository.Query().Any(ccrm => ccrm.CustomerId == c.Id && customerRoleIds.Contains(ccrm.CustomerRoleId)));

            query = query.OrderByDescending(c => c.LastActivityDateUtc);
            var customers = new PagedList<Customer>(query, pageIndex, pageSize);

            return customers;
        }

        public virtual IPagedList<Customer> GetAllCustomers(DateTime? createdFromUtc = null, DateTime? createdToUtc = null,
            int affiliateId = 0, int vendorId = 0, int[] customerRoleIds = null,
            string email = null, string username = null, string firstName = null, string lastName = null,
            int dayOfBirth = 0, int monthOfBirth = 0,
            string company = null, string phone = null, string zipPostalCode = null, string ipAddress = null,
            int pageIndex = 0, int pageSize = int.MaxValue, bool getOnlyTotalCount = false)
        {
            var query = _customerRepository.Query();
            if (createdFromUtc.HasValue)
                query = query.Where(c => createdFromUtc.Value <= c.CreatedOnUtc);
            if (createdToUtc.HasValue)
                query = query.Where(c => createdToUtc.Value >= c.CreatedOnUtc);
            if (affiliateId > 0)
                query = query.Where(c => affiliateId == c.AffiliateId);
            if (vendorId > 0)
                query = query.Where(c => vendorId == c.VendorId);

            query = query.Where(c => !c.Deleted);

            if (customerRoleIds != null && customerRoleIds.Length > 0)
            {
                query = query.Join(_customerCustomerRoleMappingRepository.Query(), x => x.Id, y => y.CustomerId,
                        (x, y) => new { Customer = x, Mapping = y })
                    .Where(z => customerRoleIds.Contains(z.Mapping.CustomerRoleId))
                    .Select(z => z.Customer)
                    .Distinct();
            }

            if (!string.IsNullOrWhiteSpace(email))
                query = query.Where(c => c.Email.Contains(email));
            if (!string.IsNullOrWhiteSpace(username))
                query = query.Where(c => c.Username.Contains(username));
            if (!string.IsNullOrWhiteSpace(firstName))
            {
                query = query
                    .Join(_gaRepository.Query(), x => x.Id, y => y.EntityId, (x, y) => new { Customer = x, Attribute = y })
                    .Where(z => z.Attribute.KeyGroup == nameof(Customer) &&
                                z.Attribute.Key == InovatiqaDefaults.FirstNameAttribute &&
                                z.Attribute.Value.Contains(firstName))
                    .Select(z => z.Customer);
            }

            if (!string.IsNullOrWhiteSpace(lastName))
            {
                query = query
                    .Join(_gaRepository.Query(), x => x.Id, y => y.EntityId, (x, y) => new { Customer = x, Attribute = y })
                    .Where(z => z.Attribute.KeyGroup == nameof(Customer) &&
                                z.Attribute.Key == InovatiqaDefaults.LastNameAttribute &&
                                z.Attribute.Value.Contains(lastName))
                    .Select(z => z.Customer);
            }

            if (dayOfBirth > 0 && monthOfBirth > 0)
            {
                var dateOfBirthStr = monthOfBirth.ToString("00", CultureInfo.InvariantCulture) + "-" + dayOfBirth.ToString("00", CultureInfo.InvariantCulture);

                query = query
                    .Join(_gaRepository.Query(), x => x.Id, y => y.EntityId, (x, y) => new { Customer = x, Attribute = y })
                    .Where(z => z.Attribute.KeyGroup == nameof(Customer) &&
                                z.Attribute.Key == InovatiqaDefaults.DateOfBirthAttribute &&
                                z.Attribute.Value.Substring(5, 5) == dateOfBirthStr)
                    .Select(z => z.Customer);
            }
            else if (dayOfBirth > 0)
            {
                //only day is specified
                var dateOfBirthStr = dayOfBirth.ToString("00", CultureInfo.InvariantCulture);

                //z.Attribute.Value.Length - dateOfBirthStr.Length = 8
                //dateOfBirthStr.Length = 2
                query = query
                    .Join(_gaRepository.Query(), x => x.Id, y => y.EntityId, (x, y) => new { Customer = x, Attribute = y })
                    .Where(z => z.Attribute.KeyGroup == nameof(Customer) &&
                                z.Attribute.Key == InovatiqaDefaults.DateOfBirthAttribute &&
                                z.Attribute.Value.Substring(8, 2) == dateOfBirthStr)
                    .Select(z => z.Customer);
            }
            else if (monthOfBirth > 0)
            {
                //only month is specified
                var dateOfBirthStr = "-" + monthOfBirth.ToString("00", CultureInfo.InvariantCulture) + "-";
                query = query
                    .Join(_gaRepository.Query(), x => x.Id, y => y.EntityId, (x, y) => new { Customer = x, Attribute = y })
                    .Where(z => z.Attribute.KeyGroup == nameof(Customer) &&
                                z.Attribute.Key == InovatiqaDefaults.DateOfBirthAttribute &&
                                z.Attribute.Value.Contains(dateOfBirthStr))
                    .Select(z => z.Customer);
            }
            //search by company
            if (!string.IsNullOrWhiteSpace(company))
            {
                query = query
                    .Join(_gaRepository.Query(), x => x.Id, y => y.EntityId, (x, y) => new { Customer = x, Attribute = y })
                    .Where(z => z.Attribute.KeyGroup == nameof(Customer) &&
                                z.Attribute.Key == InovatiqaDefaults.CompanyAttribute &&
                                z.Attribute.Value.Contains(company))
                    .Select(z => z.Customer);
            }
            //search by phone
            if (!string.IsNullOrWhiteSpace(phone))
            {
                query = query
                    .Join(_gaRepository.Query(), x => x.Id, y => y.EntityId, (x, y) => new { Customer = x, Attribute = y })
                    .Where(z => z.Attribute.KeyGroup == nameof(Customer) &&
                                z.Attribute.Key == InovatiqaDefaults.PhoneAttribute &&
                                z.Attribute.Value.Contains(phone))
                    .Select(z => z.Customer);
            }
            //search by zip
            if (!string.IsNullOrWhiteSpace(zipPostalCode))
            {
                query = query
                    .Join(_gaRepository.Query(), x => x.Id, y => y.EntityId, (x, y) => new { Customer = x, Attribute = y })
                    .Where(z => z.Attribute.KeyGroup == nameof(Customer) &&
                                z.Attribute.Key == InovatiqaDefaults.ZipPostalCodeAttribute &&
                                z.Attribute.Value.Contains(zipPostalCode))
                    .Select(z => z.Customer);
            }

            //search by IpAddress
            if (!string.IsNullOrWhiteSpace(ipAddress) && CommonHelper.IsValidIpAddress(ipAddress))
            {
                query = query.Where(w => w.LastIpAddress == ipAddress);
            }

            query = query.OrderByDescending(c => c.CreatedOnUtc);

            var customers = new PagedList<Customer>(query, pageIndex, pageSize, getOnlyTotalCount);

            return customers;
        }

        public virtual void UpdateCustomer(Customer customer)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            _customerRepository.Update(customer);

            //_eventPublisher.EntityUpdated(customer);
        }
       
        public virtual Customer GetCustomerByGuid(Guid customerGuid)
        {
            if (customerGuid == Guid.Empty)
                return null;

            var query = from c in _customerRepository.Query()
                        where c.CustomerGuid == customerGuid
                        orderby c.Id
                        select c;
            var customer = query.FirstOrDefault();

            return customer;
        }

        public virtual bool IsRegistered(Customer customer, bool onlyActiveCustomerRoles = true)
        {
            return IsInCustomerRole(customer, InovatiqaDefaults.RegisteredRoleName, onlyActiveCustomerRoles);
        }

        public virtual bool IsInCustomerRole(Customer customer,
            string customerRoleSystemName, bool onlyActiveCustomerRoles = true)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            if (string.IsNullOrEmpty(customerRoleSystemName))
                throw new ArgumentNullException(nameof(customerRoleSystemName));

            var customerRoles = GetCustomerRoles(customer, !onlyActiveCustomerRoles);

            return customerRoles?.Any(cr => cr.SystemName == customerRoleSystemName) ?? false;
        }

        public virtual IList<CustomerRole> GetCustomerRoles(Customer customer, bool showHidden = false)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            var query = from cr in _customerRoleRepository.Query()
                        join crm in _customerCustomerRoleMappingRepository.Query() on cr.Id equals crm.CustomerRoleId
                        where crm.CustomerId == customer.Id &&
                        (showHidden || cr.Active)
                        select cr;

            return query.ToList();
        }

        public virtual Customer InsertGuestCustomer()
        {
            var customer = new Customer
            {
                CustomerGuid = Guid.NewGuid(),
                Active = true,
                CreatedOnUtc = DateTime.UtcNow,
                LastActivityDateUtc = DateTime.UtcNow
            };

            var guestRole = GetCustomerRoleBySystemName(InovatiqaDefaults.GuestsRoleName);
            if (guestRole == null)
                throw new InovatiqaException("'Guests' role could not be loaded");

            _customerRepository.Insert(customer);

            AddCustomerRoleMapping(new CustomerCustomerRoleMapping { CustomerId = customer.Id, CustomerRoleId = guestRole.Id });

            return customer;
        }

        public virtual CustomerRole GetCustomerRoleBySystemName(string systemName)
        {
            if (string.IsNullOrWhiteSpace(systemName))
                return null;

            var query = from cr in _customerRoleRepository.Query()
                        orderby cr.Id
                        where cr.SystemName == systemName
                        select cr;
            var customerRole = query.FirstOrDefault();

            return customerRole;
        }

        public void AddCustomerRoleMapping(CustomerCustomerRoleMapping roleMapping)
        {
            if (roleMapping is null)
                throw new ArgumentNullException(nameof(roleMapping));

            _customerCustomerRoleMappingRepository.Insert(roleMapping);
        }
		public virtual List<string> GetCustomerRoleMappingByCustomerId(int CustomerId)
        {
            var CustomerRoles = _customerCustomerRoleMappingRepository.Query();
            var data = new List<string>();
            foreach(var role in CustomerRoles)
            {
                if(role.CustomerId == CustomerId)
                {
                    var roleData = _customerRoleRepository.GetById(role.CustomerRoleId);
                    data.Add(roleData.Name);
                }
            }
            return data;
        }
        public virtual Customer GetCustomerById(int customerId)
        {
            if (customerId == 0)
                return null;

            return _customerRepository.GetById(customerId);
        }

        public virtual string FormatUsername(Customer customer, bool stripTooLong = false, int maxLength = 0)
        {
            if (customer == null)
                return string.Empty;

            if (IsGuest(customer))
                return "Guest";

            var result = string.Empty;
            result = GetCustomerFullName(customer);

            if (stripTooLong && maxLength > 0)
                result = CommonHelper.EnsureMaximumLength(result, maxLength);

            return result;
        }

        public virtual string GetCustomerFullName(Customer customer)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            var firstName = _genericAttributeService.GetAttribute<string>(customer, InovatiqaDefaults.FirstNameAttribute, customer.Id);
            var lastName = _genericAttributeService.GetAttribute<string>(customer, InovatiqaDefaults.LastNameAttribute, customer.Id);

            var fullName = string.Empty;
            if (!string.IsNullOrWhiteSpace(firstName) && !string.IsNullOrWhiteSpace(lastName))
                fullName = $"{firstName} {lastName}";
            else
            {
                if (!string.IsNullOrWhiteSpace(firstName))
                    fullName = firstName;

                if (!string.IsNullOrWhiteSpace(lastName))
                    fullName = lastName;
            }

            return fullName;
        }
        public virtual Customer GetCustomerByUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return null;

            var query = from c in _customerRepository.Query()
                        orderby c.Id
                        where c.Username == username
                        select c;
            var customer = query.FirstOrDefault();
            return customer;
        }

        public virtual Customer GetShoppingCartCustomer(IList<ShoppingCartItem> shoppingCart)
        {
            var customerId = shoppingCart.FirstOrDefault()?.CustomerId;

            return customerId.HasValue && customerId != 0 ? GetCustomerById(customerId.Value) : null;
        }

        public virtual void ResetCheckoutData(Customer customer, int storeId,
            bool clearCouponCodes = false, bool clearCheckoutAttributes = false,
            bool clearRewardPoints = true, bool clearShippingMethod = true,
            bool clearPaymentMethod = true)
        {
            if (customer == null)
                throw new ArgumentNullException();

            if (clearCheckoutAttributes)
            {
                _genericAttributeService.SaveAttribute<string>(customer.GetType().Name, customer.Id, InovatiqaDefaults.CheckoutAttributes, null, storeId);
            }

            if (clearRewardPoints)
            {
                _genericAttributeService.SaveAttribute(customer.GetType().Name, customer.Id, InovatiqaDefaults.UseRewardPointsDuringCheckoutAttribute, false, storeId);
            }

            if (clearShippingMethod)
            {
                _genericAttributeService.SaveAttribute<ShippingOption>(customer.GetType().Name, customer.Id, InovatiqaDefaults.SelectedShippingOptionAttribute, null, storeId);
                _genericAttributeService.SaveAttribute<ShippingOption>(customer.GetType().Name, customer.Id, InovatiqaDefaults.OfferedShippingOptionsAttribute, null, storeId);
                _genericAttributeService.SaveAttribute<PickupPoint>(customer.GetType().Name, customer.Id, InovatiqaDefaults.SelectedPickupPointAttribute, null, storeId);
            }

            if (clearPaymentMethod)
            {
                _genericAttributeService.SaveAttribute<string>(customer.GetType().Name, customer.Id, InovatiqaDefaults.SelectedPaymentMethodAttribute, null, storeId);
            }

            UpdateCustomer(customer);
        }

        public virtual Customer GetOrCreateBackgroundTaskUser()
        {
            var backgroundTaskUser = GetCustomerBySystemName(InovatiqaDefaults.BackgroundTaskCustomerName);

            if (backgroundTaskUser == null)
            {
                backgroundTaskUser = new Customer
                {
                    Email = "builtin@background-task-record.com",
                    CustomerGuid = Guid.NewGuid(),
                    AdminComment = "Built-in system record used for background tasks.",
                    Active = true,
                    IsSystemAccount = true,
                    SystemName = InovatiqaDefaults.BackgroundTaskCustomerName,
                    CreatedOnUtc = DateTime.UtcNow,
                    LastActivityDateUtc = DateTime.UtcNow,
                    RegisteredInStoreId = InovatiqaDefaults.StoreId
                };
            }

            if (backgroundTaskUser is null)
            {
                InsertCustomer(backgroundTaskUser);

                var guestRole = GetCustomerRoleBySystemName(InovatiqaDefaults.GuestsRoleName);

                if (guestRole is null)
                    throw new InovatiqaException("'Guests' role could not be loaded");

                AddCustomerRoleMapping(new CustomerCustomerRoleMapping { CustomerRoleId = guestRole.Id, CustomerId = backgroundTaskUser.Id });
            }

            return backgroundTaskUser;
        }

        public virtual void InsertCustomer(Customer customer)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            _customerRepository.Insert(customer);

            //_eventPublisher.EntityInserted(customer);
        }

        public virtual Customer GetCustomerBySystemName(string systemName)
        {
            if (string.IsNullOrWhiteSpace(systemName))
                return null;

            var query = from c in _customerRepository.Query()
                        orderby c.Id
                        where c.SystemName == systemName
                        select c;
            var customer = query.FirstOrDefault();
            return customer;
        }

        public virtual Customer GetCustomerByEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return null;

            var query = from c in _customerRepository.Query()
                        orderby c.Id
                        where c.Email == email
                        select c;
            var customer = query.FirstOrDefault();
            return customer;
        }

        public virtual string[] ParseAppliedDiscountCouponCodes(Customer customer)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            var existingCouponCodes = _genericAttributeService.GetAttribute<string>(customer, InovatiqaDefaults.DiscountCouponCodeAttribute, customer.Id);

            var couponCodes = new List<string>();
            if (string.IsNullOrEmpty(existingCouponCodes))
                return couponCodes.ToArray();

            try
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(existingCouponCodes);

                var nodeList1 = xmlDoc.SelectNodes(@"//DiscountCouponCodes/CouponCode");
                foreach (XmlNode node1 in nodeList1)
                {
                    if (node1.Attributes?["Code"] == null)
                        continue;
                    var code = node1.Attributes["Code"].InnerText.Trim();
                    couponCodes.Add(code);
                }
            }
            catch
            {
                // ignored
            }

            return couponCodes.ToArray();
        }

        public virtual IList<Order> GetCustomerUnPaidOrders(Customer customer)
        {
            var query = _orderRepository.Query().Where(x => x.CustomerId == customer.Id && x.PaymentStatusId != (int)PaymentStatus.Paid);

            return query.ToList();
        }

        #endregion

        #region Customer roles
        public virtual void DeleteCustomerRole(CustomerRole customerRole)
        {
            if (customerRole == null)
                throw new ArgumentNullException(nameof(customerRole));

            if (customerRole.IsSystemRole)
                throw new InovatiqaException("System role could not be deleted");

            _customerRoleRepository.Delete(customerRole);

            //event notification
            //_eventPublisher.EntityDeleted(customerRole);
        }


        public virtual void UpdateCustomerRole(CustomerRole customerRole)
        {
            if (customerRole == null)
                throw new ArgumentNullException(nameof(customerRole));

            _customerRoleRepository.Update(customerRole);

            //event notification
            //_eventPublisher.EntityUpdated(customerRole);
        }

        public virtual void InsertCustomerRole(CustomerRole customerRole)
        {
            if (customerRole == null)
                throw new ArgumentNullException(nameof(customerRole));

            _customerRoleRepository.Insert(customerRole);

            //event notification
            //_eventPublisher.EntityInserted(customerRole);
        }

        public virtual CustomerRole GetCustomerRoleById(int customerRoleId)
        {
            if (customerRoleId == 0)
                return null;

            return _customerRoleRepository.GetById(customerRoleId);
        }

        public virtual IList<CustomerRole> GetAllCustomerRoles(bool showHidden = false)
        {
            var key = _cacheKeyService.PrepareKeyForDefaultCache(InovatiqaDefaults.CustomerRolesAllCacheKey, showHidden);


            var query = from cr in _customerRoleRepository.Query()
                        orderby cr.Name
                        where showHidden || cr.Active
                        select cr;

            var customerRoles = query.ToCachedList(key, _staticCacheManager);

            return customerRoles;
        }

        public virtual bool IsGuest(Customer customer, bool onlyActiveCustomerRoles = true)
        {
            return IsInCustomerRole(customer, InovatiqaDefaults.GuestsRoleName, onlyActiveCustomerRoles);
        }

        public void RemoveCustomerRoleMapping(Customer customer, CustomerRole role)
        {
            if (customer is null)
                throw new ArgumentNullException(nameof(customer));

            if (role is null)
                throw new ArgumentNullException(nameof(role));

            var mapping = _customerCustomerRoleMappingRepository.Query().SingleOrDefault(ccrm => ccrm.CustomerId == customer.Id && ccrm.CustomerRoleId == role.Id);

            if (mapping != null)
            {
                _customerCustomerRoleMappingRepository.Delete(mapping);

                //_eventPublisher.EntityDeleted(mapping);
            }
        }
		public void RemoveAllCustomerRoleMappings(int CustomerId)
        {
            var RoleMappingList = new List<CustomerCustomerRoleMapping>();
            foreach( var mapping in _customerCustomerRoleMappingRepository.Query())
            {
                if(mapping.CustomerId == CustomerId)
                {
                    RoleMappingList.Add(mapping);
                }
            }
            _customerCustomerRoleMappingRepository.BulkDelete(RoleMappingList);
        }
        public virtual int[] GetCustomerRoleIds(Customer customer, bool showHidden = false)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            var query = from cr in _customerRoleRepository.Query()
                        join crm in _customerCustomerRoleMappingRepository.Query() on cr.Id equals crm.CustomerRoleId
                        where crm.CustomerId == customer.Id &&
                        (showHidden || cr.Active)
                        select cr.Id;

            //var key = _cacheKeyService.PrepareKeyForShortTermCache(InovatiqaDefaults.CustomerRoleIdsCacheKey, customer, showHidden);

            //return _staticCacheManager.Get(key, () => query.ToArray());

            return query.ToArray();
        }

        public virtual bool IsVendor(Customer customer, bool onlyActiveCustomerRoles = true)
        {
            return IsInCustomerRole(customer, InovatiqaDefaults.VendorsRoleName, onlyActiveCustomerRoles);
        }

        #endregion

        #region Customer passwords

        public virtual bool IsPasswordRecoveryLinkExpired(Customer customer)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            if (InovatiqaDefaults.PasswordRecoveryLinkDaysValid == 0)
                return false;

            var geneatedDate = _genericAttributeService.GetAttribute<DateTime?>(customer, InovatiqaDefaults.PasswordRecoveryTokenDateGeneratedAttribute, customer.Id);
            if (!geneatedDate.HasValue)
                return false;

            var daysPassed = (DateTime.UtcNow - geneatedDate.Value).TotalDays;
            if (daysPassed > InovatiqaDefaults.PasswordRecoveryLinkDaysValid)
                return true;

            return false;
        }

        public virtual bool IsPasswordRecoveryTokenValid(Customer customer, string token)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            var cPrt = _genericAttributeService.GetAttribute<string>(customer, InovatiqaDefaults.PasswordRecoveryTokenAttribute, customer.Id);
            if (string.IsNullOrEmpty(cPrt))
                return false;

            if (!cPrt.Equals(token, StringComparison.InvariantCultureIgnoreCase))
                return false;

            return true;
        }

        public virtual CustomerPassword GetCurrentPassword(int customerId)
        {
            if (customerId == 0)
                return null;

            return GetCustomerPasswords(customerId, passwordsToReturn: 1).FirstOrDefault();
        }

        public virtual IList<CustomerPassword> GetCustomerPasswords(int? customerId = null,
            int? passwordFormatId = null, int? passwordsToReturn = null)
        {
            var query = _customerPasswordRepository.Query();

            if (customerId.HasValue)
                query = query.Where(password => password.CustomerId == customerId.Value);

            if (passwordFormatId.HasValue)
                query = query.Where(password => password.PasswordFormatId == (int)passwordFormatId.Value);

            if (passwordsToReturn.HasValue)
                query = query.OrderByDescending(password => password.CreatedOnUtc).Take(passwordsToReturn.Value);

            return query.ToList();
        }

        public virtual void InsertCustomerPassword(CustomerPassword customerPassword)
        {
            if (customerPassword == null)
                throw new ArgumentNullException(nameof(customerPassword));

            _customerPasswordRepository.Insert(customerPassword);

            //_eventPublisher.EntityInserted(customerPassword);
        }
		public virtual void UpdateCustomerPassword(CustomerPassword password)
        {
            if (password == null)
                throw new ArgumentNullException(nameof(password));
            _customerPasswordRepository.Update(password);
        }
        public virtual bool PasswordIsExpired(Customer customer)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            if (IsGuest(customer))
                return false;

            if (!GetCustomerRoles(customer).Any(role => role.Active && role.EnablePasswordLifetime))
                return false;


            if (InovatiqaDefaults.PasswordLifetime == 0)
                return false;

            var cacheKey = _cacheKeyService.PrepareKeyForShortTermCache(InovatiqaDefaults.CustomerPasswordLifetimeCacheKey, customer);

            var currentLifetime = _staticCacheManager.Get(cacheKey, () =>
            {
                var customerPassword = GetCurrentPassword(customer.Id);
                if (customerPassword == null)
                    return int.MaxValue;

                return (DateTime.UtcNow - customerPassword.CreatedOnUtc).Days;
            });

            return currentLifetime >= InovatiqaDefaults.PasswordLifetime;
        }

        #endregion

        #region Customer address mapping

        public virtual void RemoveCustomerAddress(Customer customer, Address address)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            if (_customerAddressRepository.Query().FirstOrDefault(m => m.AddressId == address.Id && m.CustomerId == customer.Id) is CustomerAddresses mapping)
            {
                if (customer.BillingAddressId == address.Id)
                    customer.BillingAddressId = null;
                if (customer.ShippingAddressId == address.Id)
                    customer.ShippingAddressId = null;

                _customerAddressRepository.Delete(mapping);

                //event notification
                //_eventPublisher.EntityDeleted(mapping);
            }
        }

        public virtual void InsertCustomerAddress(Customer customer, Address address)
        {
            if (customer is null)
                throw new ArgumentNullException(nameof(customer));

            if (address is null)
                throw new ArgumentNullException(nameof(address));

            if (_customerAddressRepository.Query().FirstOrDefault(m => m.AddressId == address.Id && m.CustomerId == customer.Id) is null)
            {
                var mapping = new CustomerAddresses
                {
                    AddressId = address.Id,
                    CustomerId = customer.Id
                };

                _customerAddressRepository.Insert(mapping);

                //_eventPublisher.EntityInserted(mapping);
            }
        }

        public virtual Address GetCustomerBillingAddress(Customer customer)
        {
            if (customer is null)
                throw new ArgumentNullException(nameof(customer));

            return GetCustomerAddress(customer.Id, customer.BillingAddressId ?? 0);
        }

        public virtual Address GetCustomerAddress(int customerId, int addressId)
        {
            if (customerId == 0 || addressId == 0)
                return null;
            var customer = GetCustomerById(customerId);
            if(customer.ParentId != 0 && customer.ParentId != null)
            {
                if(IsInCustomerRole(customer, "Subaccount_FUPA"))
                {
                    customerId = Convert.ToInt32(customer.ParentId);
                }
            }

            var query = from address in _addressRepository.Query()
                        join cam in _customerAddressRepository.Query() on address.Id equals cam.AddressId
                        where cam.CustomerId == customerId && address.Id == addressId
                        select address;

            return query.SingleOrDefault();
        }

        public virtual Address GetCustomerShippingAddress(Customer customer)
        {
            if (customer is null)
                throw new ArgumentNullException(nameof(customer));

            return GetCustomerAddress(customer.Id, customer.ShippingAddressId ?? 0);
        }

        public virtual IList<Address> GetAddressesByCustomerId(int customerId)
        {
            var query = from address in _addressRepository.Query()
                        join cam in _customerAddressRepository.Query() on address.Id equals cam.AddressId
                        where cam.CustomerId == customerId
                        select address;

            return query.ToList();
        }

        public virtual List<int> getAllChildAccounts(Customer customer)
        {
            List<int> ChildAccounts = new List<int>();
            //by hamza replace customer.ParentId to customer.Id
            //var allCustomers = _customerRepository.Query().Where(c => c.ParentId != null && c.ParentId != 0 && c.ParentId == customer.ParentId && !c.Deleted).Select(c => c.Id).ToList();
            var allCustomers = _customerRepository.Query().Where(c => c.ParentId != null && c.ParentId != 0 && c.ParentId == customer.Id && !c.Deleted).ToList();
            foreach (var c in allCustomers)
            {
                if(customer.ParentId != null && customer.ParentId != 0 && !customer.Deleted) // if current customer is a child, then get all sibling account otherwise get all child Accounts
                {
                    if (c.ParentId == customer.ParentId && !c.Deleted)    // get sibling
                    {
                        ChildAccounts.Add(c.Id);
                    }
                }
                else
                {
                    if (c.ParentId == customer.Id && !c.Deleted)      // get child
                    {
                        ChildAccounts.Add(c.Id);
                    }
                }
                
            }
            if (!ChildAccounts.Contains(customer.Id))
                ChildAccounts.Add(customer.Id);
            return ChildAccounts;
        }
        public virtual List<int> getAllChildAccountsIds(Customer customer)
        {
            List<int> ChildAccounts = new List<int>();
            var allCustomers = _customerRepository.Query().Where(c => c.ParentId != null && c.ParentId != 0 && c.ParentId == customer.Id && !c.Deleted).ToList();
            foreach(var c in allCustomers)
            {
                ChildAccounts.Add(c.Id);
            }
            return ChildAccounts;
        }

        #endregion

        #endregion
    }
}