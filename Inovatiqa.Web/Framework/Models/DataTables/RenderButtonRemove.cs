namespace Inovatiqa.Web.Framework.Models.DataTables
{
    public partial class RenderButtonRemove : IRender
    {
        #region Ctor

        public RenderButtonRemove(string title)
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