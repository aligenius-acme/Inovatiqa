using Inovatiqa.Database.Models;
using System.Collections.Generic;

namespace Inovatiqa.Services.Catalog.Interfaces
{
    public partial interface IRecentlyViewedProductsService
    {
        IList<Product> GetRecentlyViewedProducts(int number);

        void AddProductToRecentlyViewedList(int productId);
    }
}
