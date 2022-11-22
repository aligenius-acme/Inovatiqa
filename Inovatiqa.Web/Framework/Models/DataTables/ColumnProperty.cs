namespace Inovatiqa.Web.Framework.Models.DataTables
{
    public partial class ColumnProperty
    {
        #region Ctor

        public ColumnProperty(string data)
        {
            Data = data;
            Visible = true;
            Encode = true;
        }

        #endregion

        #region Properties

        public string Data { get; set; }

        public string Title { get; set; }

        public IRender Render { get; set; }

        public string Width { get; set; }

        public bool AutoWidth { get; set; }

        public bool IsMasterCheckBox { get; set; }

        public string ClassName { get; set; }

        public bool Visible { get; set; }

        public bool Searchable { get; set; }

        public bool Editable { get; set; }

        public EditType EditType { get; set; }

        public bool Encode { get; set; }

        #endregion
    }
}