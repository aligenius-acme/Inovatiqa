using System.Collections.Generic;

namespace Inovatiqa.Web.Models.Common
{
    public partial class AddressAttributeModel
    {
        public AddressAttributeModel()
        {
            Values = new List<AddressAttributeValueModel>();
        }

        public int Id { get; set; }

        public string Name { get; set; }

        public bool IsRequired { get; set; }

        public string DefaultValue { get; set; }

        public int AttributeControlTypeId { get; set; }

        public IList<AddressAttributeValueModel> Values { get; set; }
    }

    public partial class AddressAttributeValueModel
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public bool IsPreSelected { get; set; }
    }
}