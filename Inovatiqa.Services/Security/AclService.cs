using Inovatiqa.Core;
using Inovatiqa.Database.Interfaces;
using Inovatiqa.Database.Models;
using Inovatiqa.Services.Customers.Interfaces;
using Inovatiqa.Services.WorkContext.Interfaces;
using System;
using System.Linq;

namespace Inovatiqa.Services.Security
{
    public partial class AclService : IAclService
    {
        #region Fields

        private readonly ICustomerService _customerService;
        private readonly IRepository<AclRecord> _aclRecordRepository;
        private readonly IWorkContextService _workContextService;

        #endregion

        #region Ctor

        public AclService(ICustomerService customerService,
            IRepository<AclRecord> aclRecordRepository,
            IWorkContextService workContextService)
        {
            _customerService = customerService;
            _aclRecordRepository = aclRecordRepository;
            _workContextService = workContextService;
        }

        #endregion

        #region Methods

        
        public virtual int[] GetCustomerRoleIdsWithAccess<T>(T entity, int id) where T : IAclSupported
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));


            var entityId = id;
            var entityName = entity.GetType().Name;

            var query = from ur in _aclRecordRepository.Query()
                where ur.EntityId == entityId &&
                      ur.EntityName == entityName
                select ur.CustomerRoleId;

            return query.ToArray();
        }

        #endregion
    }
}