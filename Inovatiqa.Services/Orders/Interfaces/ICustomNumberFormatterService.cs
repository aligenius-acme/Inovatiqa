using Inovatiqa.Database.Models;

namespace Inovatiqa.Services.Orders.Interfaces
{
    public partial interface ICustomNumberFormatterService
    {
        string GenerateReturnRequestCustomNumber(ReturnRequest returnRequest);

        string GenerateOrderCustomNumber(Order order);
    }
}