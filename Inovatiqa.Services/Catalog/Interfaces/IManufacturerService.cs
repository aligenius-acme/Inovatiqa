using Inovatiqa.Core.Interfaces;
using Inovatiqa.Database.Models;
using System.Collections.Generic;

namespace Inovatiqa.Services.Catalog.Interfaces
{
    public partial interface IManufacturerService
    {
        IList<ProductManufacturerMapping> GetProductManufacturersByProductId(int productId, bool showHidden = false);

        Manufacturer GetManufacturerById(int manufacturerId);

        IPagedList<Manufacturer> GetAllManufacturers(string manufacturerName = "",
            int storeId = 0,
            int pageIndex = 0,
            int pageSize = int.MaxValue,
            bool showHidden = false,
            bool? overridePublished = null);

        List<Manufacturer> GetAllFeaturedManufacturers(int number);
        //added by hamza
        List<Manufacturer> GetHomePageManufacturers();

        void UpdateManufacturer(Manufacturer manufacturer);

        IPagedList<ProductManufacturerMapping> GetProductManufacturersByManufacturerId(int manufacturerId,
           int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false);

        ProductManufacturerMapping GetProductManufacturerById(int productManufacturerId);

        void UpdateProductManufacturer(ProductManufacturerMapping productManufacturer);

        void DeleteProductManufacturer(ProductManufacturerMapping productManufacturer);

        void InsertProductManufacturer(ProductManufacturerMapping productManufacturer);

        ProductManufacturerMapping FindProductManufacturer(IList<ProductManufacturerMapping> source, int productId, int manufacturerId);
    }
}