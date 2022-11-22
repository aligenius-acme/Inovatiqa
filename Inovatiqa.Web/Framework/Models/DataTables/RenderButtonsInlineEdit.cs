namespace Inovatiqa.Web.Framework.Models.DataTables
{
    public partial class RenderButtonsInlineEdit : IRender
    {
        #region Ctor

        public RenderButtonsInlineEdit()
        {
            ClassName = InovatiqaButtonClassDefaults.Default;
        }

        #endregion

        #region Properties

        public string ClassName { get; set; }

        #endregion
    }
}