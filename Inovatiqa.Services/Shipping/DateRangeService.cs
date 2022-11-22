using Inovatiqa.Services.Shipping.Interfaces;
using System.Collections.Generic;
using System.Linq;
using Inovatiqa.Database.Interfaces;
using Inovatiqa.Database.Models;

namespace Inovatiqa.Services.Shipping
{
    public partial class DateRangeService : IDateRangeService
    {
        #region Fields

        private readonly IRepository<DeliveryDate> _deliveryDateRepository;
        private readonly IRepository<ProductAvailabilityRange> _productAvailabilityRangeRepository;

        #endregion

        #region Ctor

        public DateRangeService(IRepository<DeliveryDate> deliveryDateRepository,
            IRepository<ProductAvailabilityRange> productAvailabilityRangeRepository)
        {
            _deliveryDateRepository = deliveryDateRepository;
            _productAvailabilityRangeRepository = productAvailabilityRangeRepository;
        }

        #endregion

        #region Methods

        #region Delivery dates

        public virtual DeliveryDate GetDeliveryDateById(int deliveryDateId)
        {
            if (deliveryDateId == 0)
                return null;

            return _deliveryDateRepository.GetById(deliveryDateId);
        }

        public virtual IList<DeliveryDate> GetAllDeliveryDates()
        {
            var query = from dd in _deliveryDateRepository.Query()
                        orderby dd.DisplayOrder, dd.Id
                        select dd;
            var deliveryDates = query.ToList();

            return deliveryDates;
        }

        #endregion

        #region Product availability ranges

        public virtual ProductAvailabilityRange GetProductAvailabilityRangeById(int productAvailabilityRangeId)
        {
            return productAvailabilityRangeId != 0 ? _productAvailabilityRangeRepository.GetById(productAvailabilityRangeId) : null;
        }

        public virtual IList<ProductAvailabilityRange> GetAllProductAvailabilityRanges()
        {
            var query = from par in _productAvailabilityRangeRepository.Query()
                        orderby par.DisplayOrder, par.Id
                        select par;

            return query.ToList();
        }

        #endregion

        #endregion
    }
}