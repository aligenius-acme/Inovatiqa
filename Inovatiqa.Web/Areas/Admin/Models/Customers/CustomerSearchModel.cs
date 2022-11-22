using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Inovatiqa.Web.Framework.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Inovatiqa.Web.Areas.Admin.Models.Customers
{
    public partial class CustomerSearchModel : BaseSearchModel, IAclSupportedModel
    {
        #region Ctor

        public CustomerSearchModel()
        {
            SelectedCustomerRoleIds = new List<int>();
            AvailableCustomerRoles = new List<SelectListItem>();
        }

        #endregion

        #region Properties

        [Display(Name = "Customer roles")]
        public IList<int> SelectedCustomerRoleIds { get; set; }

        public IList<SelectListItem> AvailableCustomerRoles { get; set; }

        [Display(Name = "Email")]
        public string SearchEmail { get; set; }

        [Display(Name = "Username")]
        public string SearchUsername { get; set; }

        public bool UsernamesEnabled { get; set; }

        [Display(Name = "First name")]
        public string SearchFirstName { get; set; }
        public bool FirstNameEnabled { get; set; }

        [Display(Name = "Last name")]
        public string SearchLastName { get; set; }
        public bool LastNameEnabled { get; set; }

        [Display(Name = "Date of birth")]
        public string SearchDayOfBirth { get; set; }

        [Display(Name = "Date of birth")]
        public string SearchMonthOfBirth { get; set; }

        public bool DateOfBirthEnabled { get; set; }

        [Display(Name = "Company")]
        public string SearchCompany { get; set; }

        public bool CompanyEnabled { get; set; }

        [Display(Name = "Phone")]
        public string SearchPhone { get; set; }

        public bool PhoneEnabled { get; set; }

        [Display(Name = "Zip code")]
        public string SearchZipPostalCode { get; set; }

        public bool ZipPostalCodeEnabled { get; set; }

        [Display(Name = "IP address")]
        public string SearchIpAddress { get; set; }

        public bool AvatarEnabled { get; internal set; }

        #endregion
    }
}