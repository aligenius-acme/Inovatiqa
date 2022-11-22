using System.Collections.Generic;

namespace Inovatiqa.Web.Models.Catalog
{
    public partial class NewProductsModel
    {
        public NewProductsModel()
        {
            Products = new List<ProductOverviewModel>();
            PagingFilteringContext = new CatalogPagingFilteringModel();
        }
        public CatalogPagingFilteringModel PagingFilteringContext { get; set; }

        public IList<ProductOverviewModel> Products { get; set; }
    }
}