namespace Inovatiqa.Web.Framework.Models.DataTables
{
    public partial class RenderPicture : IRender
    {
        #region Ctor

        public RenderPicture(string srcPrefix = "")
        {
            SrcPrefix = srcPrefix;
        }

        #endregion

        #region Properties

        public string SrcPrefix { get; set; }

        public string Src { get; set; }

        #endregion
    }
}