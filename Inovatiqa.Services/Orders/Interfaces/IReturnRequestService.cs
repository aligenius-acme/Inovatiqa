using Inovatiqa.Core;
using Inovatiqa.Core.Interfaces;
using Inovatiqa.Database.Models;
using System;
using System.Collections.Generic;

namespace Inovatiqa.Services.Orders.Interfaces
{
    public partial interface IReturnRequestService
    {
        void UpdateReturnRequest(ReturnRequest returnRequest);

        void DeleteReturnRequest(ReturnRequest returnRequest);

        ReturnRequest GetReturnRequestById(int returnRequestId);

        IPagedList<ReturnRequest> SearchReturnRequests(int storeId = 0, int customerId = 0,
            int orderItemId = 0, string customNumber = "", ReturnRequestStatus? rs = null, DateTime? createdFromUtc = null,
            DateTime? createdToUtc = null, int pageIndex = 0, int pageSize = int.MaxValue, bool getOnlyTotalCount = false, bool orderByDate = true);

        void DeleteReturnRequestAction(ReturnRequestAction returnRequestAction);

        IList<ReturnRequestAction> GetAllReturnRequestActions();

        ReturnRequestAction GetReturnRequestActionById(int returnRequestActionId);

        void InsertReturnRequest(ReturnRequest returnRequest);

        void InsertReturnRequestAction(ReturnRequestAction returnRequestAction);

        void UpdateReturnRequestAction(ReturnRequestAction returnRequestAction);

        void DeleteReturnRequestReason(ReturnRequestReason returnRequestReason);

        IList<ReturnRequestReason> GetAllReturnRequestReasons();

        ReturnRequestReason GetReturnRequestReasonById(int returnRequestReasonId);

        void InsertReturnRequestReason(ReturnRequestReason returnRequestReason);

        void UpdateReturnRequestReason(ReturnRequestReason returnRequestReason);

        void InsertCustomerReturnRequest(CustomerReturnRequest customerReturnRequest);
    }
}
