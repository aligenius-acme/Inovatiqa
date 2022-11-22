using Inovatiqa.Core;
using Inovatiqa.Database.Interfaces;
using Inovatiqa.Database.Models;
using Inovatiqa.Services.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Inovatiqa.Services.Common
{
    public partial class GenericAttributeService : IGenericAttributeService
    {
        #region Fields

        private readonly IRepository<GenericAttribute> _genericAttributeRepository;

        #endregion

        #region Ctor

        public GenericAttributeService(IRepository<GenericAttribute> genericAttributeRepository)
        {
            _genericAttributeRepository = genericAttributeRepository;
        }

        #endregion

        #region Methods

        public virtual void InsertAttribute(GenericAttribute attribute)
        {
            if (attribute == null)
                throw new ArgumentNullException(nameof(attribute));

            attribute.CreatedOrUpdatedDateUtc = DateTime.UtcNow;
            _genericAttributeRepository.Insert(attribute);

            //_eventPublisher.EntityInserted(attribute);
        }

        public virtual void UpdateAttribute(GenericAttribute attribute)
        {
            if (attribute == null)
                throw new ArgumentNullException(nameof(attribute));

            attribute.CreatedOrUpdatedDateUtc = DateTime.UtcNow;
            _genericAttributeRepository.Update(attribute);

            //_eventPublisher.EntityUpdated(attribute);
        }

        public virtual void DeleteAttribute(GenericAttribute attribute)
        {
            if (attribute == null)
                throw new ArgumentNullException(nameof(attribute));

            _genericAttributeRepository.Delete(attribute);

            //_eventPublisher.EntityDeleted(attribute);
        }

        public virtual TPropType GetAttribute<TPropType>(object entity, string key, int id, int storeId = 0, TPropType defaultValue = default)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var keyGroup = entity.GetType().Name;
            //var keyGroup = entity.ToString();
            var props = GetAttributesForEntity(id, keyGroup);

            if (props == null)
                return defaultValue;

            props = props.Where(x => x.StoreId == storeId).ToList();
            if (!props.Any())
                return defaultValue;

            var prop = props.FirstOrDefault(ga =>
                ga.Key.Equals(key, StringComparison.InvariantCultureIgnoreCase));    

            if (prop == null || string.IsNullOrEmpty(prop.Value))
                return defaultValue;

            return CommonHelper.To<TPropType>(prop.Value);
        }

        public virtual IList<GenericAttribute> GetAttributesForEntity(int entityId, string keyGroup)
        {
            var query = from ga in _genericAttributeRepository.Query()
                        where ga.EntityId == entityId &&
                              ga.KeyGroup == keyGroup
                        select ga;
            var attributes = query.ToList();

            return attributes;
        }
        public virtual void SaveAttribute<TPropType>(string entityName, int id, string key, TPropType value, int storeId = 0)
        {
            if (entityName == null)
                throw new ArgumentNullException(nameof(entityName));

            if (key == null)
                throw new ArgumentNullException(nameof(key));

            var keyGroup = entityName;

            var props = GetAttributesForEntity(id, keyGroup)
                .Where(x => x.StoreId == storeId)
                .ToList();
            var prop = props.FirstOrDefault(ga =>
                ga.Key.Equals(key, StringComparison.InvariantCultureIgnoreCase));    

            var valueStr = CommonHelper.To<string>(value);

            if (prop != null)
            {
                if (string.IsNullOrWhiteSpace(valueStr))
                {
                    DeleteAttribute(prop);
                }
                else
                {
                    prop.Value = valueStr;
                    UpdateAttribute(prop);
                }
            }
            else
            {
                if (string.IsNullOrWhiteSpace(valueStr))
                    return;

                prop = new GenericAttribute
                {
                    EntityId = id,
                    Key = key,
                    KeyGroup = keyGroup,
                    Value = valueStr,
                    StoreId = storeId
                };

                InsertAttribute(prop);
            }
        }

        #endregion
    }
}