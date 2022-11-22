using Inovatiqa.Database.Models;
using System.Collections.Generic;

namespace Inovatiqa.Services.Catalog.Interfaces
{
    public partial interface ICompareProductsService
    {
        void ClearCompareProducts();

        IList<Product> GetComparedProducts();

        void RemoveProductFromCompareList(int productId);

        void AddProductToCompareList(int productId);
    }
}
