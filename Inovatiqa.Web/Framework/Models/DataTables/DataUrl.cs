using Microsoft.AspNetCore.Routing;

namespace Inovatiqa.Web.Framework.Models.DataTables
{
    public partial class DataUrl
    {
        #region Ctor

        public DataUrl(string actionName, string controllerName, RouteValueDictionary routeValues)
        {
            ActionName = actionName;
            ControllerName = controllerName;
            RouteValues = routeValues;
        }

        public DataUrl(string url)
        {
            Url = url;
        }

        public DataUrl(string url, string dataId)
        {
            Url = url;
            DataId = dataId;
        }

        public DataUrl(string url, bool trimEnd)
        {
            Url = url;
            TrimEnd = trimEnd;
        }

        #endregion

        #region Properties

        public string ActionName { get; set; }

        public string ControllerName { get; set; }

        public string Url { get; set; }

        public RouteValueDictionary RouteValues { get; set; }

        public string DataId { get; set; }

        public bool TrimEnd { get; set; }

        #endregion
    }
}