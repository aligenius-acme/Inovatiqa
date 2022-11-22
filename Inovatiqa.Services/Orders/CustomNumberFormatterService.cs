using Inovatiqa.Core;
using Inovatiqa.Database.Models;
using Inovatiqa.Services.Orders.Interfaces;

namespace Inovatiqa.Services.Orders
{
    public partial class CustomNumberFormatterService : ICustomNumberFormatterService
    {
        #region Fields

        #endregion

        #region Ctor

        #endregion

        #region Methods

        public virtual string GenerateReturnRequestCustomNumber(ReturnRequest returnRequest)
        {
            string customNumber;

            if (string.IsNullOrEmpty(InovatiqaDefaults.ReturnRequestNumberMask))
            {
                customNumber = returnRequest.Id.ToString();
            }
            else
            {
                customNumber = InovatiqaDefaults.ReturnRequestNumberMask
                    .Replace("{ID}", returnRequest.Id.ToString())
                    .Replace("{YYYY}", returnRequest.CreatedOnUtc.ToString("yyyy"))
                    .Replace("{YY}", returnRequest.CreatedOnUtc.ToString("yy"))
                    .Replace("{MM}", returnRequest.CreatedOnUtc.ToString("MM"))
                    .Replace("{DD}", returnRequest.CreatedOnUtc.ToString("dd"));

            }

            return customNumber;
        }

        public virtual string GenerateOrderCustomNumber(Order order)
        {
            if (string.IsNullOrEmpty(InovatiqaDefaults.CustomOrderNumberMask))
                return order.Id.ToString();

            var customNumber = InovatiqaDefaults.CustomOrderNumberMask
                .Replace("{ID}", order.Id.ToString())
                .Replace("{YYYY}", order.CreatedOnUtc.ToString("yyyy"))
                .Replace("{YY}", order.CreatedOnUtc.ToString("yy"))
                .Replace("{MM}", order.CreatedOnUtc.ToString("MM"))
                .Replace("{DD}", order.CreatedOnUtc.ToString("dd")).Trim();

            return customNumber;
        }

        #endregion
    }
}