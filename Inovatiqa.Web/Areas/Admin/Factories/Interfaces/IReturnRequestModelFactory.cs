using Inovatiqa.Database.Models;
using Inovatiqa.Web.Areas.Admin.Models.Orders;

namespace Inovatiqa.Web.Areas.Admin.Factories.Interfaces
{
    public partial interface IReturnRequestModelFactory
    {
        ReturnRequestSearchModel PrepareReturnRequestSearchModel(ReturnRequestSearchModel searchModel);

        ReturnRequestListModel PrepareReturnRequestListModel(ReturnRequestSearchModel searchModel);

        ReturnRequestModel PrepareReturnRequestModel(ReturnRequestModel model,
            ReturnRequest returnRequest, bool excludeProperties = false);

        ReturnRequestReasonSearchModel PrepareReturnRequestReasonSearchModel(ReturnRequestReasonSearchModel searchModel);

        ReturnRequestReasonListModel PrepareReturnRequestReasonListModel(ReturnRequestReasonSearchModel searchModel);

        ReturnRequestReasonModel PrepareReturnRequestReasonModel(ReturnRequestReasonModel model,
            ReturnRequestReason returnRequestReason, bool excludeProperties = false);

        ReturnRequestActionSearchModel PrepareReturnRequestActionSearchModel(ReturnRequestActionSearchModel searchModel);

        ReturnRequestActionListModel PrepareReturnRequestActionListModel(ReturnRequestActionSearchModel searchModel);

        ReturnRequestActionModel PrepareReturnRequestActionModel(ReturnRequestActionModel model,
            ReturnRequestAction returnRequestAction, bool excludeProperties = false);
    }
}