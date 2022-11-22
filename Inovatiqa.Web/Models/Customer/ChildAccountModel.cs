using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace Inovatiqa.Web.Models.Customer
{
    public partial class ChildAccountModel
    {
        public ChildAccountModel()
        {
            Messages = new List<KeyValuePair<string, string>>();
            Accounts = new List<ChildAccountModel>();
            AvailableCountries = new List<SelectListItem>();
            AvailableStates = new List<SelectListItem>();

        }
        public List<KeyValuePair<string, string>> Messages { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public bool isEditing { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string email { get; set; }
        public bool isActive { get; set; }
        public string Password { get; set; }
        public bool Subaccount_MAD { get; set; }
        public bool Subaccount_MAB { get; set; }
        public bool Subaccount_DAOH { get; set; }
        public bool Subaccount_CO { get; set; }
        public bool Subaccount_RABCO { get; set; }
        public bool Subaccount_GTCC { get; set; }
        public bool Subaccount_GTC { get; set; }
        public bool Subaccount_OPN { get; set; }
        public bool Subaccount_FUPCN { get; set; }
        public bool Subaccount_FUPCVAT { get; set; }
        public bool Subaccount_FUPA { get; set; }
        public bool Subaccount_CAO { get; set; }
        public bool Subaccount_CMS { get; set; }
        public string City { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string Company { get; set; }
        public string Country { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public string Phone { get; set; }
        public string MaxOrderApprovalValue { get; set; }
        public string MinValueToRequestApproval { get; set; }
        public IList<SelectListItem> AvailableCountries { get; set; }
        public IList<SelectListItem> AvailableStates { get; set; }

        public int CountryId { get; set; }
        public int StateProvinceId { get; set; }
        public List<ChildAccountModel> Accounts { get; set; }

    }
}
