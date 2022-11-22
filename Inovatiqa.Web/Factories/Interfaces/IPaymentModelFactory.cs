using Inovatiqa.Database.Models;
using Inovatiqa.Web.Models.Payment;

namespace Inovatiqa.Web.Factories.Interfaces
{
    public partial interface IPaymentModelFactory
    {
        PaymentInfoModel PreparePaymentInfoModel(Customer customer,
            decimal totalPayment,
            decimal amountToPay,
            string invoiceIds,
            string invoiceIdsAmounts,
            string orderIds = "");

        PaymentShipmentModel PreparePaymentPortalShipmentListModel();
    }
}
