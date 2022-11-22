using System.ComponentModel.DataAnnotations;

namespace Inovatiqa.Web.Models.Common
{
    public partial class ContactUsModel
    {
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        
        public string Subject { get; set; }

        public bool SubjectEnabled { get; set; }

        public string Enquiry { get; set; }

        public string FullName { get; set; }

        public bool SuccessfullySent { get; set; }
        public string Result { get; set; }

        public bool DisplayCaptcha { get; set; }
    }
}