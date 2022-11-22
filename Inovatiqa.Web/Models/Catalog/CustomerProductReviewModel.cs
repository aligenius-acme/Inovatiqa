using Inovatiqa.Web.Models.Common;
using System.Collections.Generic;

namespace Inovatiqa.Web.Models.Catalog
{
    public class CustomerProductReviewModel
    {
        public CustomerProductReviewModel()
        {
            AdditionalProductReviewList = new List<ProductReviewReviewTypeMappingModel>();
        }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductSeName { get; set; }
        public string Title { get; set; }
        public string ReviewText { get; set; }
        public string ReplyText { get; set; }
        public int Rating { get; set; }
        public string WrittenOnStr { get; set; }
        public string ApprovalStatus { get; set; }
        public IList<ProductReviewReviewTypeMappingModel> AdditionalProductReviewList { get; set; }
    }

    public class CustomerProductReviewsModel
    {
        public CustomerProductReviewsModel()
        {
            ProductReviews = new List<CustomerProductReviewModel>();
        }

        public IList<CustomerProductReviewModel> ProductReviews { get; set; }
        public PagerModel PagerModel { get; set; }

        #region Nested class

        public partial class CustomerProductReviewsRouteValues : IRouteValues
        {
            public int pageNumber { get; set; }
        }

        #endregion
    }
}