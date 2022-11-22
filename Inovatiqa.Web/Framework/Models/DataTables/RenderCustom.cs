namespace Inovatiqa.Web.Framework.Models.DataTables
{
    public partial class RenderCustom : IRender
    {
        #region Ctor

        public RenderCustom(string functionName)
        {
            FunctionName = functionName;
        }

        #endregion

        #region Properties

        public string FunctionName { get; set; }

        #endregion
    }
}