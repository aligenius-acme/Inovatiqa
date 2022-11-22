using Inovatiqa.Database.Models;
using System.Collections.Generic;
using System.IO;

namespace Inovatiqa.Services.Common.Interfaces
{
    public partial interface IPdfService
    {
        string PrintOrderToPdf(Order order, int languageId = 0, int vendorId = 0);

        void PrintOrdersToPdf(Stream stream, IList<Order> orders, int languageId = 0, int vendorId = 0, int shipmentId = 0);

        void PrintPackagingSlipsToPdf(Stream stream, IList<Shipment> shipments, int languageId = 0);
        
        void PrintProductsToPdf(Stream stream, IList<Product> products);
    }
}