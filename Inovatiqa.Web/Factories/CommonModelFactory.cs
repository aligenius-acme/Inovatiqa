using System;
using System.Linq;
using Inovatiqa.Core;
using Inovatiqa.Services.Customers.Interfaces;
using Inovatiqa.Services.Orders.Interfaces;
using Inovatiqa.Services.Security.Interfaces;
using Inovatiqa.Services.WorkContext.Interfaces;
using Inovatiqa.Web.Models.Common;
using Inovatiqa.Services.Security;
using Inovatiqa.Web.Factories.Interfaces;

namespace Inovatiqa.Web.Factories
{
    public partial class CommonModelFactory : ICommonModelFactory
    {
        #region Fields

        private readonly IWorkContextService _workContextService;
        private readonly ICustomerService _customerService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IPermissionService _permissionService;

        #endregion

        #region Ctor

        public CommonModelFactory(IWorkContextService workContextService,
            ICustomerService customerService,
            IShoppingCartService shoppingCartService,
            IPermissionService permissionService)
        {
            _workContextService = workContextService;
            _customerService = customerService;
            _shoppingCartService = shoppingCartService;
            _permissionService = permissionService;
        }

        #endregion

        #region Utilities


        #endregion

        #region Methods

        public virtual HeaderLinksModel PrepareHeaderLinksModel()
        {
            var customer = _workContextService.CurrentCustomer;
            bool IsRegistered = _customerService.IsRegistered(customer);

            var model = new HeaderLinksModel
            {
                IsAuthenticated = IsRegistered,
                CustomerName = IsRegistered ? _customerService.FormatUsername(customer) : string.Empty,
                ShoppingCartEnabled = true,
                WishlistEnabled = true,
                AllowPrivateMessages = false
            };
            if (customer.HasShoppingCartItems)
            {
                var allShoppingCarts = _shoppingCartService.GetShoppingCart(customer, (int)ShoppingCartType.ShoppingCart, InovatiqaDefaults.StoreId, getallShoppingCarts: true);
                model.ShoppingCartItems = allShoppingCarts.Where(x => x.ShoppingCartTypeId == (int)ShoppingCartType.ShoppingCart)
                    .Sum(item => item.Quantity);

                model.WishlistItems = allShoppingCarts.Where(x => x.ShoppingCartTypeId == (int)ShoppingCartType.Wishlist)
                    .Sum(item => item.Quantity);
            }

            return model;
        }

        public virtual AdminHeaderLinksModel PrepareAdminHeaderLinksModel()
        {
            var customer = _workContextService.CurrentCustomer;

            var model = new AdminHeaderLinksModel
            {
                ImpersonatedCustomerName = _customerService.IsRegistered(customer) ? _customerService.FormatUsername(customer) : string.Empty,
                IsCustomerImpersonated = _workContextService.OriginalCustomerIfImpersonated != null,
                DisplayAdminLink = _permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel),
            };

            return model;
        }

        public virtual ContactUsModel PrepareContactUsModel(ContactUsModel model, bool excludeProperties)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            if (!excludeProperties)
            {
                model.Email = _workContextService.CurrentCustomer.Email;
                model.FullName = _customerService.GetCustomerFullName(_workContextService.CurrentCustomer);
            }

            model.SubjectEnabled = InovatiqaDefaults.SubjectFieldOnContactUsForm;
            model.DisplayCaptcha = InovatiqaDefaults.Enabled && InovatiqaDefaults.ShowOnContactUsPage;

            return model;
        }

        #endregion
    }
}