namespace Inovatiqa.Web.Framework.Models.DataTables
{
    public partial class RenderCheckBox : IRender
    {
        #region Ctor

        public RenderCheckBox(string name, string propertyKeyName = "Id")
        {
            Name = name;
            PropertyKeyName = propertyKeyName;
        }

        #endregion

        #region Properties

        public string Name { get; set; }

        public string PropertyKeyName { get; set; }

        #endregion
    }
}