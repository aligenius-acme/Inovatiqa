using System.Collections.Generic;

namespace Inovatiqa.Web.Framework.Models.DataTables
{
    public partial class DataTablesModel : BaseInovatiqaModel
    {
        #region Const

        protected const string DEFAULT_PAGING_TYPE = "simple_numbers";

        #endregion

        #region Ctor

        public DataTablesModel()
        {
            Info = true;
            RefreshButton = true;
            ServerSide = true;
            Processing = true;
            Paging = true;
            PagingType = DEFAULT_PAGING_TYPE;

            Filters = new List<FilterParameter>();
            ColumnCollection = new List<ColumnProperty>();
        }

        #endregion

        #region Properties

        public string Name { get; set; }

        public DataUrl UrlRead { get; set; }

        public DataUrl UrlDelete { get; set; }

        public DataUrl UrlUpdate { get; set; }

        public string SearchButtonId { get; set; }

        public IList<FilterParameter> Filters { get; set; }

        public object Data { get; set; }

        public bool Processing { get; set; }

        public bool ServerSide { get; set; }

        public bool Paging { get; set; }

        public bool Info { get; set; }

        public bool RefreshButton { get; set; }

        public string PagingType { get; set; }

        public int Length { get; set; }

        public string LengthMenu { get; set; }

        public string Dom { get; set; }

        public bool Ordering { get; set; }

        public string HeaderCallback { get; set; }

        public int FooterColumns { get; set; }

        public string FooterCallback { get; set; }

        public bool IsChildTable { get; set; }

        public DataTablesModel ChildTable { get; set; }

        public string PrimaryKeyColumn { get; set; }

        public string BindColumnNameActionDelete { get; set; }

        public IList<ColumnProperty> ColumnCollection { get; set; }
        
        #endregion
    }
}