namespace Inovatiqa.Web.Models.Customer
{
    public partial class GdprConsentModel
    {
        public int Id { get; set; }
        public string Message { get; set; }

        public bool IsRequired { get; set; }

        public string RequiredMessage { get; set; }

        public bool Accepted { get; set; }
    }
}