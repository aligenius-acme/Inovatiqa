using Inovatiqa.Core;
using Inovatiqa.Web.Framework.Models;

namespace Inovatiqa.Web.Framework.Factories.Interfaces
{
    public partial interface IAclSupportedModelFactory
    {
        void PrepareModelCustomerRoles<TModel>(TModel model) where TModel : IAclSupportedModel;

        void PrepareModelCustomerRoles<TModel, TEntity>(TModel model, TEntity entity, bool ignoreAclMappings, int id)
            where TModel : IAclSupportedModel where TEntity : IAclSupported;
    }
}