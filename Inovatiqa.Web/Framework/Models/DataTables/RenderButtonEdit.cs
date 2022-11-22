namespace Inovatiqa.Web.Framework.Models.DataTables
{
    public partial class RenderButtonEdit : IRender
    {
        #region Ctor

        public RenderButtonEdit(DataUrl url)
        {
            Url = url;
            ClassName = InovatiqaButtonClassDefaults.Default;
        }

        #endregion

        #region Properties

        public DataUrl Url { get; set; }

        public string ClassName { get; set; }

        #endregion
    }
}