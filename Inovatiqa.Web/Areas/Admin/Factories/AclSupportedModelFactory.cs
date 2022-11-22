using System;
using System.Linq;
using Inovatiqa.Core;
using Inovatiqa.Services.Customers.Interfaces;
using Inovatiqa.Services.Security;
using Inovatiqa.Web.Framework.Factories.Interfaces;
using Inovatiqa.Web.Framework.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Inovatiqa.Web.Framework.Factories
{
    public partial class AclSupportedModelFactory : IAclSupportedModelFactory
    {
        #region Fields

        private readonly IAclService _aclService;
        private readonly ICustomerService _customerService;

        #endregion

        #region Ctor

        public AclSupportedModelFactory(IAclService aclService,
            ICustomerService customerService)
        {
            _aclService = aclService;
            _customerService = customerService;
        }

        #endregion

        #region Methods

        public virtual void PrepareModelCustomerRoles<TModel>(TModel model) where TModel : IAclSupportedModel
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            var availableRoles = _customerService.GetAllCustomerRoles(showHidden: true);
            model.AvailableCustomerRoles = availableRoles.Select(role => new SelectListItem
            {
                Text = role.Name,
                Value = role.Id.ToString(),
                Selected = model.SelectedCustomerRoleIds.Contains(role.Id)
            }).ToList();
        }

        public virtual void PrepareModelCustomerRoles<TModel, TEntity>(TModel model, TEntity entity, bool ignoreAclMappings, int id)
            where TModel : IAclSupportedModel where TEntity : IAclSupported
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            if (!ignoreAclMappings && entity != null)
                model.SelectedCustomerRoleIds = _aclService.GetCustomerRoleIdsWithAccess(entity, id).ToList();

            PrepareModelCustomerRoles(model);
        }

        #endregion
    }
}