using System.ComponentModel.DataAnnotations;

namespace Inovatiqa.Web.Models.Newsletter
{
    public partial class NewsletterBoxModel
    {
        [DataType(DataType.EmailAddress)]
        public string NewsletterEmail { get; set; }
        public bool AllowToUnsubscribe { get; set; }
    }
}