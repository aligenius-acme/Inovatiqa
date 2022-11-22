using System;

namespace Inovatiqa.Web.Framework.Models.DataTables
{
    public partial class FilterParameter
    {
        #region Ctor

        public FilterParameter(string name)
        {
            Name = name;
            Type = typeof(string);
        }

        public FilterParameter(string name, string modelName)
        {
            Name = name;
            ModelName = modelName;
            Type = typeof(string);
        }

        public FilterParameter(string name, Type type)
        {
            Name = name;
            Type = type;
        }

        public FilterParameter(string name, object value)
        {
            Name = name;
            Type = value.GetType();
            Value = value;
        }

        public FilterParameter(string name, string parentName, bool isParentChildParameter = true)
        {
            Name = name;
            ParentName = parentName;
            Type = typeof(string);
        }

        #endregion

        #region Properties

        public string Name { get; }

        public string ModelName { get; }

        public Type Type { get; }

        public object Value { get; set; }

        public string ParentName { get; set; }

        #endregion
    }
}
