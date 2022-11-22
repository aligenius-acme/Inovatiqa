namespace Inovatiqa.Web.Framework.Models.DataTables
{
    public partial class RenderLink : IRender
    {
        #region Ctor

        public RenderLink(DataUrl url)
        {
            Url = url;
        }

        #endregion

        #region Properties

        public DataUrl Url { get; set; }

        public string Title { get; set; }

        #endregion
    }
}