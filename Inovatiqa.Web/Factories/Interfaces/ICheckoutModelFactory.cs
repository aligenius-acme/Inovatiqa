using Inovatiqa.Database.Models;
using Inovatiqa.Web.Models.Checkout;
using Inovatiqa.Web.Models.Media;
using System.Collections.Generic;

namespace Inovatiqa.Web.Factories.Interfaces
{
    public partial interface ICheckoutModelFactory
    {
        OnePageCheckoutModel PrepareOnePageCheckoutModel(IList<ShoppingCartItem> cart);
        PictureModel PrepareProductOverviewPictureModel(Product product, int? productThumbPictureSize = null);

        CheckoutBillingAddressModel PrepareBillingAddressModel(IList<ShoppingCartItem> cart,
            int? selectedCountryId = null,
            bool prePopulateNewAddressWithCustomerFields = false,
            string overrideAttributesXml = "");

        CheckoutShippingMethodModel PrepareShippingMethodModel(IList<ShoppingCartItem> cart, Address shippingAddress);

        CheckoutPaymentMethodModel PreparePaymentMethodModel(IList<ShoppingCartItem> cart, int filterByCountryId);

        CheckoutPaymentInfoModel PreparePaymentInfoModel(string paymentMethod);

        CheckoutConfirmModel PrepareConfirmOrderModel(IList<ShoppingCartItem> cart);

        CheckoutShippingAddressModel PrepareShippingAddressModel(IList<ShoppingCartItem> cart, int? selectedCountryId = null,
            bool prePopulateNewAddressWithCustomerFields = false, string overrideAttributesXml = "");

        CheckoutProgressModel PrepareCheckoutProgressModel(CheckoutProgressStep step);

        CheckoutCompletedModel PrepareCheckoutCompletedModel(Order order);
    }
}
