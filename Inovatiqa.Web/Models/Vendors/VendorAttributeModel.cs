using Inovatiqa.Web.Framework.Models;
using System.Collections.Generic;

namespace Inovatiqa.Web.Models.Vendors
{
    public partial class VendorAttributeModel : BaseInovatiqaEntityModel
    {
        public VendorAttributeModel()
        {
            Values = new List<VendorAttributeValueModel>();
        }

        public string Name { get; set; }

        public bool IsRequired { get; set; }

        public string DefaultValue { get; set; }

        public int AttributeControlTypeId { get; set; }

        public IList<VendorAttributeValueModel> Values { get; set; }

    }

    public partial class VendorAttributeValueModel : BaseInovatiqaEntityModel
    {
        public string Name { get; set; }

        public bool IsPreSelected { get; set; }
    }
}