using System.ComponentModel.DataAnnotations;

namespace Inovatiqa.Web.Models.Common
{
    public class EmailModel
    {

        [Display(Name = "First Name")]
        public string CustomerFirstName { get; set; }

        [Display(Name = "Last Name")]
        public string CustomerLastName { get; set; }


        [Display(Name = "Email")]
        public string CustomerEmail { get; set; }


        [RegularExpression(@"\({0,1}[\d+]{3}\){0,1}(\-){0,1}[\d+]{3}(\-){0,1}[\d+]{4}$", ErrorMessage = "XXX-XXX-XXXX OR (XXX)-XXX-XXXX Expected.")]
        [DataType(DataType.PhoneNumber)]
        [Display(Name = "Phone Number")]
        public string PhoneNumber{ get; set; }

        [Display(Name = "Releated To")]
        public string ReleatedTo    { get; set; }

        [Required]
        public string CaptchaCode { get; set; }

        [RegularExpression(@"([\d+]+)", ErrorMessage = "Order number must only contain numbers.")]
        [Display(Name = "Order Number")]
        public string OrderNumber   { get; set; }
        public string RequestType   { get; set; }
        public string Company   { get; set; }
        public string Address1   { get; set; }
        public string Address2   { get; set; }
        public string City { get; set; }
        public string State { get; set; }

        [RegularExpression(@"(^\d{5}$)|(^\d{9}$)|(^\d{5}-\d{4}$)", ErrorMessage = "XXXXX OR XXXXX-XXXX Expected.")]
        [DataType(DataType.PhoneNumber)]
        [Display(Name = "ZipCode")]
        public string ZipCode   { get; set; }
        public string Phone { get; set; }
        public bool PrivacyPolicy { get; set; }
        public bool MedicalEquipment { get; set; }
        public bool PhysicalTherapy { get; set; }
        
        [RegularExpression(@"((([A-Za-z]{3,9}:(?:\/\/)?)(?:[-;:&=\+\$,\w]+@)?[A-Za-z0-9.-]+(:[0-9]+)?|(?:www.|[-;:&=\+\$,\w]+@)[A-Za-z0-9.-]+)((?:\/[\+~%\/.\w-_]*)?\??(?:[-\+=&;%@.\w_]*)#?(?:[\w]*))?)", ErrorMessage = "URL is not valid.")]
        public string URL { get; set; }

        [RegularExpression(@"\({0,1}[\d+]{3}\){0,1}(\-){0,1}[\d+]{3}(\-){0,1}[\d+]{4}$", ErrorMessage = "XXX-XXX-XXXX OR (XXX)-XXX-XXXX Expected.")]
        [DataType(DataType.PhoneNumber)]
        [Display(Name = "Fax")]
        public string Fax { get; set; }
        public string Quantity { get; set; }
        public string ManufacturerProductNumber { get; set; }
        public string Description { get; set; }
        public string Title { get; set; }
        public string FacilityGPOId { get; set; }
        public string FederalTaxID { get; set; }
        public string GPOYourFacilityCanAccess { get; set; }
        public string ProductsInterested { get; set; }
        public string AdditionalInformation { get; set; }


        [Display(Name = "Message")]
        public string Message  { get; set; }

        [Display(Name = "Subject")]
        public string EmailSubject { get; set; }

        [Display(Name = "Email body")]
        public string EmailBody { get; set; }

        // used for back redirection instead of home page redirection
        public string Controller { get; set; }
        public string Method { get; set; }


    }
}
