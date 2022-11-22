namespace Inovatiqa.Web.Framework.Models.DataTables
{
    public partial class RenderDate : IRender
    {
        #region Constants

        private string DEFAULT_DATE_FORMAT = "MM/DD/YYYY HH:mm:ss";

        #endregion

        #region Ctor

        public RenderDate()
        {
            Format = DEFAULT_DATE_FORMAT;
        }

        #endregion

        #region Properties

        public string Format { get; set; }

        #endregion
    }
}