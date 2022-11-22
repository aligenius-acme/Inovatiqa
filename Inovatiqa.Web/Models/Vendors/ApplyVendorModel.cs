using Inovatiqa.Web.Framework.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Inovatiqa.Web.Models.Vendors
{
    public partial class ApplyVendorModel : BaseInovatiqaModel
    {
        public ApplyVendorModel()
        {
            VendorAttributes = new List<VendorAttributeModel>();
        }

        [Display(Name = "Vendor name")]
        public string Name { get; set; }

        [DataType(DataType.EmailAddress)]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Display(Name = "Description")]
        public string Description { get; set; }

        public IList<VendorAttributeModel> VendorAttributes { get; set; }

        public bool DisplayCaptcha { get; set; }

        public bool TermsOfServiceEnabled { get; set; }
        public bool TermsOfServicePopup { get; set; }

        public bool DisableFormInput { get; set; }
        public string Result { get; set; }
    }
}
