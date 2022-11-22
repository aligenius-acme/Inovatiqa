using Inovatiqa.Core;
using Inovatiqa.Core.Interfaces;
using Inovatiqa.Domain.Models;
using System;

namespace Inovatiqa.Services.Customers.Interfaces
{
    public partial interface ICustomerReportService
    {
        IPagedList<BestCustomerReportLine> GetBestCustomersReport(DateTime? createdFromUtc,
            DateTime? createdToUtc, OrderStatus? os, PaymentStatus? ps, ShippingStatus? ss, OrderByEnum orderBy,
            int pageIndex = 0, int pageSize = 214748364);
        
        int GetRegisteredCustomersReport(int days);
    }
}