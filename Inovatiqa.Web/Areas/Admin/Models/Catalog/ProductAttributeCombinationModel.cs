using Inovatiqa.Core;
using Inovatiqa.Web.Framework.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Inovatiqa.Web.Areas.Admin.Models.Catalog
{
    public partial class ProductAttributeCombinationModel : BaseInovatiqaEntityModel
    {
        #region Ctor

        public ProductAttributeCombinationModel()
        {
            ProductAttributes = new List<ProductAttributeModel>();
            ProductPictureModels = new List<ProductPictureModel>();
            Warnings = new List<string>();
        }

        #endregion

        #region Properties

        public int ProductId { get; set; }

        [Display(Name = "Attributes")]
        public string AttributesXml { get; set; }

        [Display(Name = "Stock quantity")]
        public int StockQuantity { get; set; }

        [Display(Name = "Allow out of stock")]
        public bool AllowOutOfStockOrders { get; set; }

        [Display(Name = "SKU")]
        public string Sku { get; set; }

        [Display(Name = "Manufacturer part number")]
        public string ManufacturerPartNumber { get; set; }

        [Display(Name = "GTIN")]
        public string Gtin { get; set; }

        [Display(Name = "Overridden price")]
        [UIHint("DecimalNullable")]
        public decimal? OverriddenPrice { get; set; }

        [Display(Name = "Notify admin for quantity below")]
        public int NotifyAdminForQuantityBelow { get; set; }

        [Display(Name = "Picture")]
        public int PictureId { get; set; }

        public string PictureThumbnailUrl { get; set; }

        public IList<ProductAttributeModel> ProductAttributes { get; set; }

        public IList<ProductPictureModel> ProductPictureModels { get; set; }

        public IList<string> Warnings { get; set; }

        #endregion

        #region Nested classes

        public partial class ProductAttributeModel : BaseInovatiqaEntityModel
        {
            public ProductAttributeModel()
            {
                Values = new List<ProductAttributeValueModel>();
            }

            public int ProductAttributeId { get; set; }

            public string Name { get; set; }

            public string TextPrompt { get; set; }

            public bool IsRequired { get; set; }

            public AttributeControlType AttributeControlType { get; set; }

            public IList<ProductAttributeValueModel> Values { get; set; }
        }

        public partial class ProductAttributeValueModel : BaseInovatiqaEntityModel
        {
            public string Name { get; set; }

            public bool IsPreSelected { get; set; }

            public string Checked { get; set; }
        }

        #endregion
    }
}
