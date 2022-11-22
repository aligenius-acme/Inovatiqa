using Inovatiqa.Database.Models;
using System.Collections.Generic;

namespace Inovatiqa.Services.Shipping.Interfaces
{
    public partial interface IDateRangeService
    {
        #region Delivery dates

        DeliveryDate GetDeliveryDateById(int deliveryDateId);

        IList<DeliveryDate> GetAllDeliveryDates();

        #endregion

        #region Product availability ranges

        ProductAvailabilityRange GetProductAvailabilityRangeById(int productAvailabilityRangeId);

        IList<ProductAvailabilityRange> GetAllProductAvailabilityRanges();

        #endregion
    }
}
