namespace Inovatiqa.Web.Models.Catalog
{
    public partial class ProductSpecificationModel
    {
        public int SpecificationAttributeId { get; set; }

        public string SpecificationAttributeName { get; set; }

        public string ValueRaw { get; set; }

        public string ColorSquaresRgb { get; set; }

        public int AttributeTypeId { get; set; }
    }
}