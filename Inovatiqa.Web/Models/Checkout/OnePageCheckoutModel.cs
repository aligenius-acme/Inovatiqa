using Inovatiqa.Web.Models.Media;
using System.Collections.Generic;
namespace Inovatiqa.Web.Models.Checkout
{
    public partial class OnePageCheckoutModel
    {
        public OnePageCheckoutModel()
        {
            orderSummaryBox = new OrderSummaryBox();

        }
        public bool ShippingRequired { get; set; }
        public bool DisableBillingAddressCheckoutStep { get; set; }

        public CheckoutBillingAddressModel BillingAddress { get; set; }
        public OrderSummaryBox orderSummaryBox { get; set; }
    }

    public partial class OrderSummaryBox
    {
        public OrderSummaryBox()
        {
            OrderSummaryProducts = new List<CheckoutProducts>();
        }
        public List<CheckoutProducts> OrderSummaryProducts { get; set; }
        public string SubTotal { get; set; }
        public string Tax { get; set; }
        public string TaxPercentage { get; set; }
        public string Shipping { get; set; }
    }

    public partial class CheckoutProducts
    {
        public CheckoutProducts()
        {
            Attributes = new List<string>();
            DefaultPictureModel = new PictureModel();
        }
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string Name { get; set; }
        public List<string> Attributes { get; set; }
        public decimal StartRating { get; set; }
        public string Price { get; set; }
        public bool OldPriceEnabled { get; set; }
        public string OldPrice { get; set; }
        public PictureModel DefaultPictureModel { get; set; }
        public int quantity { get; set; }
        public string SeName { get; set; }
    }
}