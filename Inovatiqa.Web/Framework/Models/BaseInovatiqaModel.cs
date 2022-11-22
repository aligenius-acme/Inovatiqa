using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Inovatiqa.Web.Framework.Models
{
    public partial class BaseInovatiqaModel
    {
        #region Ctor

        public BaseInovatiqaModel()
        {
            CustomProperties = new Dictionary<string, object>();
            PostInitialize();
        }

        #endregion

        #region Methods

        public virtual void BindModel(ModelBindingContext bindingContext)
        {
        }

        protected virtual void PostInitialize()
        {
        }

        #endregion

        #region Properties

        [XmlIgnore]
        public Dictionary<string, object> CustomProperties { get; set; }

        #endregion

    }
}