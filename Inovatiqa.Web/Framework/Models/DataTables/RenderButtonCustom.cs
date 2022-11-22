namespace Inovatiqa.Web.Framework.Models.DataTables
{
    public partial class RenderButtonCustom : IRender
    {
        #region Ctor

        public RenderButtonCustom(string className, string title)
        {
            ClassName = className;
            Title = title;
        }

        #endregion

        #region Properties

        public string Url { get; set; }

        public string ClassName { get; set; }

        public string Title { get; set; }

        public string OnClickFunctionName { get; set; }

        #endregion
    }
}
