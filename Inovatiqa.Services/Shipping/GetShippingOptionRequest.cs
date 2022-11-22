using Inovatiqa.Database.Models;
using System.Collections.Generic;

namespace Inovatiqa.Services.Shipping
{
    public partial class GetShippingOptionRequest
    {
        #region Ctor

        public GetShippingOptionRequest()
        {
            Items = new List<PackageItem>();
        }

        #endregion

        #region Properties

        public virtual Customer Customer { get; set; }

        public IList<PackageItem> Items { get; set; }

        public Address ShippingAddress { get; set; }

        public Warehouse WarehouseFrom { get; set; }

        public Country CountryFrom { get; set; }

        public StateProvince StateProvinceFrom { get; set; }

        public string ZipPostalCodeFrom { get; set; }

        public string CountyFrom { get; set; }

        public string CityFrom { get; set; }

        public string AddressFrom { get; set; }

        public int StoreId { get; set; }

        #endregion

        #region Nested classes

        public class PackageItem
        {
            public PackageItem(ShoppingCartItem sci, Product product, int? qty = null)
            {
                ShoppingCartItem = sci;
                Product = product;
                OverriddenQuantity = qty;
            }

            public ShoppingCartItem ShoppingCartItem { get; set; }

            public Product Product { get; set; }

            public int? OverriddenQuantity { get; set; }

            public int GetQuantity()
            {
                if (OverriddenQuantity.HasValue)
                    return OverriddenQuantity.Value;

                return ShoppingCartItem.Quantity;
            }
        }

        #endregion
    }
}