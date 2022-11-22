namespace Inovatiqa.Web.Models.Checkout
{
    public partial class CheckoutCompletedModel
    {
        public int OrderId { get; set; }
        public string CustomOrderNumber { get; set; }
        public bool OnePageCheckoutEnabled { get; set; }
    }
}