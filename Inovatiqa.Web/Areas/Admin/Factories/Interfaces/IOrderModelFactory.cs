using Inovatiqa.Database.Models;
using Inovatiqa.Web.Areas.Admin.Models.Orders;
using Inovatiqa.Web.Areas.Admin.Models.Reports;

namespace Inovatiqa.Web.Areas.Admin.Factories.Interfaces
{
    public partial interface IOrderModelFactory
    {
        OrderSearchModel PrepareOrderSearchModel(OrderSearchModel searchModel);

        BestsellerBriefSearchModel PrepareBestsellerBriefSearchModel(BestsellerBriefSearchModel searchModel);

        OrderAverageReportListModel PrepareOrderAverageReportListModel(OrderAverageReportSearchModel searchModel);

        OrderIncompleteReportListModel PrepareOrderIncompleteReportListModel(OrderIncompleteReportSearchModel searchModel);

        OrderListModel PrepareOrderListModel(OrderSearchModel searchModel);

        BestsellerBriefListModel PrepareBestsellerBriefListModel(BestsellerBriefSearchModel searchModel);

        OrderAggreratorModel PrepareOrderAggregatorModel(OrderSearchModel searchModel);

        OrderModel PrepareOrderModel(OrderModel model, Order order, bool excludeProperties = false);

        ShipmentModel PrepareShipmentModel(ShipmentModel model, Shipment shipment, Order order, bool excludeProperties = false);

        OrderShipmentListModel PrepareOrderShipmentListModel(OrderShipmentSearchModel searchModel, Order order);

        ShipmentItemListModel PrepareShipmentItemListModel(ShipmentItemSearchModel searchModel, Shipment shipment);

        OrderNoteListModel PrepareOrderNoteListModel(OrderNoteSearchModel searchModel, Order order);

        OrderAddressModel PrepareOrderAddressModel(OrderAddressModel model, Order order, Address address);

        ShipmentSearchModel PrepareShipmentSearchModel(ShipmentSearchModel searchModel);

        ShipmentListModel PrepareShipmentListModel(ShipmentSearchModel searchModel);

        ShipmentSearchModel PrepareInvoicedShipmentSearchModel(ShipmentSearchModel searchModel);

        ShipmentListModel PrepareInvoicedShipmentListModel(ShipmentSearchModel searchModel);
    }
}