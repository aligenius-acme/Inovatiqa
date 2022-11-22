using Inovatiqa.Core;
using Inovatiqa.Core.Domain.Orders;
using Inovatiqa.Core.Interfaces;
using Inovatiqa.Core.Orders;
using Inovatiqa.Database.Models;
using Inovatiqa.Domain.Models;
using System;
using System.Collections.Generic;

namespace Inovatiqa.Services.Orders.Interfaces
{
    public partial interface IOrderReportService
    {
        IPagedList<BestsellersReportLine> BestSellersReport(
            int categoryId = 0, int manufacturerId = 0, 
            int storeId = 0, int vendorId = 0,
            DateTime? createdFromUtc = null, DateTime? createdToUtc = null,
            OrderStatus? os = null, PaymentStatus? ps = null, ShippingStatus? ss = null,
            int billingCountryId = 0,
            OrderByEnum orderBy = OrderByEnum.OrderByQuantity,
            int pageIndex = 0, int pageSize = int.MaxValue,
            bool showHidden = false);

        OrderAverageReportLineSummary OrderAverageReport(int storeId, OrderStatus os);

        OrderAverageReportLine GetOrderAverageReportLine(int storeId = 0, int vendorId = 0, int productId = 0,
            int warehouseId = 0, int billingCountryId = 0, int orderId = 0, string paymentMethodSystemName = null,
            List<int> osIds = null, List<int> psIds = null, List<int> ssIds = null,
            DateTime? startTimeUtc = null, DateTime? endTimeUtc = null,
            string billingPhone = null, string billingEmail = null, string billingLastName = "", string orderNotes = null);

        decimal ProfitReport(int storeId = 0, int vendorId = 0, int productId = 0,
            int warehouseId = 0, int billingCountryId = 0, int orderId = 0, string paymentMethodSystemName = null,
            List<int> osIds = null, List<int> psIds = null, List<int> ssIds = null,
            DateTime? startTimeUtc = null, DateTime? endTimeUtc = null,
            string billingPhone = null, string billingEmail = null, string billingLastName = "", string orderNotes = null);

        IPagedList<Product> ProductsNeverSold(int vendorId = 0, int storeId = 0,
            int categoryId = 0, int manufacturerId = 0,
            DateTime? createdFromUtc = null, DateTime? createdToUtc = null,
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false);

        IList<OrderByCountryReportLine> GetCountryReport(int storeId = 0, OrderStatus? os = null,
            PaymentStatus? ps = null, ShippingStatus? ss = null,
            DateTime? startTimeUtc = null, DateTime? endTimeUtc = null);
    }
}
