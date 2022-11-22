using Newtonsoft.Json;
using System.Collections.Generic;

namespace Inovatiqa.Web.Framework.Models
{
    public abstract partial class BasePagedListModel<T> : BaseInovatiqaModel, IPagedModel<T> where T : BaseInovatiqaModel
    {
        public IEnumerable<T> Data { get; set; }

        [JsonProperty(PropertyName = "draw")]
        public string Draw { get; set; }

        [JsonProperty(PropertyName = "recordsFiltered")]
        public int RecordsFiltered { get; set; }

        [JsonProperty(PropertyName = "recordsTotal")]
        public int RecordsTotal { get; set; }        
    }
}