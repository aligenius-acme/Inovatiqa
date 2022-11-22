using Inovatiqa.Database.Models;
using System.Collections.Generic;

namespace Inovatiqa.Services.Vendors
{
    public partial interface IVendorAttributeService
    {
        #region Vendor attributes

        IList<VendorAttribute> GetAllVendorAttributes();

        VendorAttribute GetVendorAttributeById(int vendorAttributeId);

        void InsertVendorAttribute(VendorAttribute vendorAttribute);

        void UpdateVendorAttribute(VendorAttribute vendorAttribute);

        void DeleteVendorAttribute(VendorAttribute vendorAttribute);

        #endregion

        #region Vendor attribute values

        IList<VendorAttributeValue> GetVendorAttributeValues(int vendorAttributeId);

        VendorAttributeValue GetVendorAttributeValueById(int vendorAttributeValueId);

        void InsertVendorAttributeValue(VendorAttributeValue vendorAttributeValue);

        void UpdateVendorAttributeValue(VendorAttributeValue vendorAttributeValue);

        void DeleteVendorAttributeValue(VendorAttributeValue vendorAttributeValue);

        #endregion
    }
}