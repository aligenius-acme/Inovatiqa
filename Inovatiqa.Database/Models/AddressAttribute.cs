using System.Collections.Generic;

namespace Inovatiqa.Database.Models
{
    public partial class AddressAttribute
    {
        public AddressAttribute()
        {
            AddressAttributeValue = new HashSet<AddressAttributeValue>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsRequired { get; set; }
        public int AttributeControlTypeId { get; set; }
        public int DisplayOrder { get; set; }

        public virtual ICollection<AddressAttributeValue> AddressAttributeValue { get; set; }
    }
}
