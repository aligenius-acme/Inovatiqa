using Inovatiqa.Core;

namespace Inovatiqa.Web.Framework.Models
{
    public abstract partial class BaseSearchModel : BaseInovatiqaModel, IPagingRequestModel
    {
        #region Ctor

        protected BaseSearchModel()
        {
            Length = 10;
        }

        #endregion

        #region Properties

        public int Page => (Start / Length) + 1;

        public int PageSize => Length;

        public string AvailablePageSizes { get; set; }

        public string Draw { get; set; }

        public int Start { get; set; }

        public int Length { get; set; }

        #endregion

        #region Methods

        public void SetGridPageSize()
        {
            SetGridPageSize(InovatiqaDefaults.DefaultGridPageSize, InovatiqaDefaults.GridPageSizes);
        }

        public void SetPopupGridPageSize()
        {
            SetGridPageSize(InovatiqaDefaults.DefaultGridPageSize, InovatiqaDefaults.GridPageSizes);
        }

        public void SetGridPageSize(int pageSize, string availablePageSizes = null)
        {
            Start = 0;
            Length = pageSize;
            AvailablePageSizes = availablePageSizes;
        }

        #endregion
    }
}