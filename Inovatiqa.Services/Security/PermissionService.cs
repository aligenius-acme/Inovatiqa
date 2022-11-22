using Inovatiqa.Database.Interfaces;
using Inovatiqa.Database.Models;
using Inovatiqa.Services.Customers.Interfaces;
using Inovatiqa.Services.Security.Interfaces;
using Inovatiqa.Services.WorkContext.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Inovatiqa.Services.Security
{
    public partial class PermissionService : IPermissionService
    {
        #region Fields

        private readonly ICustomerService _customerService;
        private readonly IRepository<PermissionRecord> _permissionRecordRepository;
        private readonly IRepository<PermissionRecordRoleMapping> _permissionRecordCustomerRoleMappingRepository;
        private readonly IWorkContextService _workContextService;

        #endregion

        #region Ctor

        public PermissionService(ICustomerService customerService,
            IRepository<PermissionRecord> permissionRecordRepository,
            IRepository<PermissionRecordRoleMapping> permissionRecordCustomerRoleMappingRepository,
            IWorkContextService workContextService)
        {
            _customerService = customerService;
            _permissionRecordRepository = permissionRecordRepository;
            _permissionRecordCustomerRoleMappingRepository = permissionRecordCustomerRoleMappingRepository;
            _workContextService = workContextService;
        }

        #endregion

        #region Utilities

        protected virtual IList<PermissionRecord> GetPermissionRecordsByCustomerRoleId(int customerRoleId)
        {
            var query = from pr in _permissionRecordRepository.Query()
                        join prcrm in _permissionRecordCustomerRoleMappingRepository.Query() on pr.Id equals prcrm
                            .PermissionRecordId
                        where prcrm.CustomerRoleId == customerRoleId
                        orderby pr.Id
                        select pr;

            return query.ToList();
        }

        #endregion

        #region Methods

        public virtual bool Authorize(PermissionRecord permission)
        {
            return Authorize(permission, _workContextService.CurrentCustomer);
        }

        public virtual bool Authorize(PermissionRecord permission, Customer customer)
        {
            if (permission == null)
                return false;

            if (customer == null)
                return false;

            return Authorize(permission.SystemName, customer);
        }

        public virtual bool Authorize(string permissionRecordSystemName)
        {
            return Authorize(permissionRecordSystemName, _workContextService.CurrentCustomer);
        }

        public virtual bool Authorize(string permissionRecordSystemName, Customer customer)
        {
            if (string.IsNullOrEmpty(permissionRecordSystemName))
                return false;

            var customerRoles = _customerService.GetCustomerRoles(customer);
            foreach (var role in customerRoles)
                if (Authorize(permissionRecordSystemName, role.Id))
                    return true;

            return false;
        }

        public virtual bool Authorize(string permissionRecordSystemName, int customerRoleId)
        {
            if (string.IsNullOrEmpty(permissionRecordSystemName))
                return false;

            var permissions = GetPermissionRecordsByCustomerRoleId(customerRoleId);
            foreach (var permission in permissions)
                if (permission.SystemName.Equals(permissionRecordSystemName, StringComparison.InvariantCultureIgnoreCase))
                    return true;

            return false;
        }

        #endregion
    }
}