namespace Inovatiqa.Web.Framework.Models.DataTables
{
    public partial class RenderButtonViewAjax : IRender
    {
        #region Ctor

        public RenderButtonViewAjax(string title)
        {
            Title = title;
            ClassName = InovatiqaButtonClassDefaults.Default;
        }

        #endregion

        #region Properties

        public string Title { get; set; }

        public string ClassName { get; set; }

        #endregion
    }
}