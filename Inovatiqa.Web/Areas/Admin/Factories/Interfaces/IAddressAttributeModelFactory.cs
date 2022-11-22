using Inovatiqa.Database.Models;
using Inovatiqa.Web.Areas.Admin.Models.Common;
using System.Collections.Generic;

namespace Inovatiqa.Web.Areas.Admin.Factories.Interfaces
{
    public partial interface IAddressAttributeModelFactory
    {
        void PrepareCustomAddressAttributes(IList<AddressModel.AddressAttributeModel> models, Address address);
    }
}