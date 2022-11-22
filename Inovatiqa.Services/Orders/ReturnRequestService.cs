using Inovatiqa.Core;
using Inovatiqa.Core.Interfaces;
using Inovatiqa.Database.Interfaces;
using Inovatiqa.Database.Models;
using Inovatiqa.Services.Orders.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Inovatiqa.Services.Orders
{
    public partial class ReturnRequestService : IReturnRequestService
    {
        #region Fields

        private readonly IRepository<ReturnRequest> _returnRequestRepository;
        private readonly IRepository<ReturnRequestAction> _returnRequestActionRepository;
        private readonly IRepository<ReturnRequestReason> _returnRequestReasonRepository;
        private readonly IRepository<CustomerReturnRequest> _customerReturnRequestRepository;

        #endregion

        #region Ctor

        public ReturnRequestService(IRepository<ReturnRequest> returnRequestRepository,
            IRepository<ReturnRequestAction> returnRequestActionRepository,
            IRepository<ReturnRequestReason> returnRequestReasonRepository,
            IRepository<CustomerReturnRequest> customerReturnRequestRepository)
        {
            _returnRequestRepository = returnRequestRepository;
            _returnRequestActionRepository = returnRequestActionRepository;
            _returnRequestReasonRepository = returnRequestReasonRepository;
            _customerReturnRequestRepository = customerReturnRequestRepository;
        }

        #endregion

        #region Methods

        public virtual void DeleteReturnRequest(ReturnRequest returnRequest)
        {
            if (returnRequest == null)
                throw new ArgumentNullException(nameof(returnRequest));

            _returnRequestRepository.Delete(returnRequest);

            //_eventPublisher.EntityDeleted(returnRequest);
        }

        public virtual ReturnRequest GetReturnRequestById(int returnRequestId)
        {
            if (returnRequestId == 0)
                return null;

            return _returnRequestRepository.GetById(returnRequestId);
        }

        public IPagedList<ReturnRequest> SearchReturnRequests(int storeId = 0, int customerId = 0,
            int orderItemId = 0, string customNumber = "", ReturnRequestStatus? rs = null, DateTime? createdFromUtc = null,
            DateTime? createdToUtc = null, int pageIndex = 0, int pageSize = int.MaxValue, bool getOnlyTotalCount = false, bool orderByDate = true)
        {
            var query = _returnRequestRepository.Query();
            if (storeId > 0)
                query = query.Where(rr => storeId == rr.StoreId);
            if (customerId > 0)
                query = query.Where(rr => customerId == rr.CustomerId);
            if (rs.HasValue)
            {
                var returnStatusId = (int)rs.Value;
                query = query.Where(rr => rr.ReturnRequestStatusId == returnStatusId);
            }

            if (orderItemId > 0)
                query = query.Where(rr => rr.OrderItemId == orderItemId);

            if (!string.IsNullOrEmpty(customNumber))
                query = query.Where(rr => rr.CustomNumber == customNumber);

            if (createdFromUtc.HasValue)
                query = query.Where(rr => createdFromUtc.Value <= rr.CreatedOnUtc);
            if (createdToUtc.HasValue)
                query = query.Where(rr => createdToUtc.Value >= rr.CreatedOnUtc);

            if(orderByDate) 
                query = query.OrderByDescending(rr => rr.CreatedOnUtc);
            else
                query = query.OrderByDescending(rr => rr.CreatedOnUtc).ThenByDescending(rr => rr.Id);

            var returnRequests = new PagedList<ReturnRequest>(query, pageIndex, pageSize, getOnlyTotalCount);

            return returnRequests;
        }

        public virtual void DeleteReturnRequestAction(ReturnRequestAction returnRequestAction)
        {
            if (returnRequestAction == null)
                throw new ArgumentNullException(nameof(returnRequestAction));

            _returnRequestActionRepository.Delete(returnRequestAction);

            //_eventPublisher.EntityDeleted(returnRequestAction);
        }

        public virtual IList<ReturnRequestAction> GetAllReturnRequestActions()
        {
            var query = from rra in _returnRequestActionRepository.Query()
                        orderby rra.DisplayOrder, rra.Id
                        select rra;

            return query.ToList();
        }

        public virtual ReturnRequestAction GetReturnRequestActionById(int returnRequestActionId)
        {
            if (returnRequestActionId == 0)
                return null;

            return _returnRequestActionRepository.GetById(returnRequestActionId);
        }

        public virtual void InsertReturnRequest(ReturnRequest returnRequest)
        {
            if (returnRequest == null)
                throw new ArgumentNullException(nameof(returnRequest));

            _returnRequestRepository.Insert(returnRequest);

            //_eventPublisher.EntityInserted(returnRequest);
        }

        public virtual void InsertReturnRequestAction(ReturnRequestAction returnRequestAction)
        {
            if (returnRequestAction == null)
                throw new ArgumentNullException(nameof(returnRequestAction));

            _returnRequestActionRepository.Insert(returnRequestAction);

            //_eventPublisher.EntityInserted(returnRequestAction);
        }

        public virtual void UpdateReturnRequest(ReturnRequest returnRequest)
        {
            if (returnRequest == null)
                throw new ArgumentNullException(nameof(returnRequest));

            _returnRequestRepository.Update(returnRequest);

            //_eventPublisher.EntityUpdated(returnRequest);
        }

        public virtual void UpdateReturnRequestAction(ReturnRequestAction returnRequestAction)
        {
            if (returnRequestAction == null)
                throw new ArgumentNullException(nameof(returnRequestAction));

            _returnRequestActionRepository.Update(returnRequestAction);

            //_eventPublisher.EntityUpdated(returnRequestAction);
        }

        public virtual void DeleteReturnRequestReason(ReturnRequestReason returnRequestReason)
        {
            if (returnRequestReason == null)
                throw new ArgumentNullException(nameof(returnRequestReason));

            _returnRequestReasonRepository.Delete(returnRequestReason);

            //_eventPublisher.EntityDeleted(returnRequestReason);
        }

        public virtual IList<ReturnRequestReason> GetAllReturnRequestReasons()
        {
            var query = from rra in _returnRequestReasonRepository.Query()
                        orderby rra.DisplayOrder, rra.Id
                        select rra;

            return query.ToList();
        }

        public virtual ReturnRequestReason GetReturnRequestReasonById(int returnRequestReasonId)
        {
            if (returnRequestReasonId == 0)
                return null;

            return _returnRequestReasonRepository.GetById(returnRequestReasonId);
        }

        public virtual void InsertReturnRequestReason(ReturnRequestReason returnRequestReason)
        {
            if (returnRequestReason == null)
                throw new ArgumentNullException(nameof(returnRequestReason));

            _returnRequestReasonRepository.Insert(returnRequestReason);

            //_eventPublisher.EntityInserted(returnRequestReason);
        }

        public virtual void UpdateReturnRequestReason(ReturnRequestReason returnRequestReason)
        {
            if (returnRequestReason == null)
                throw new ArgumentNullException(nameof(returnRequestReason));

            _returnRequestReasonRepository.Update(returnRequestReason);

            //_eventPublisher.EntityUpdated(returnRequestReason);
        }
        public virtual void InsertCustomerReturnRequest(CustomerReturnRequest customerReturnRequest)
        {
            if (customerReturnRequest == null)
                throw new ArgumentNullException(nameof(customerReturnRequest));
            
            _customerReturnRequestRepository.Insert(customerReturnRequest);
        }
        #endregion
    }
}