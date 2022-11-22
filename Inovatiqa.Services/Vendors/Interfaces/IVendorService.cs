using Inovatiqa.Core.Interfaces;
using Inovatiqa.Database.Models;
using System.Collections.Generic;

namespace Inovatiqa.Services.Vendors.Interfaces
{
    public partial interface IVendorService
    {
        Vendor GetVendorById(int vendorId);

        Vendor GetVendorByProductId(int productId);

        IList<Vendor> GetVendorsByProductIds(int[] productIds);

        IPagedList<Vendor> GetAllVendors(string name = "", string email = "", int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false);

        void InsertVendor(Vendor vendor);

        void UpdateVendor(Vendor vendor);

        void DeleteVendor(Vendor vendor);
    }
}