using Inovatiqa.Database.Models;
using System.Collections.Generic;

namespace Inovatiqa.Services.Common.Interfaces
{
    public partial interface IGenericAttributeService
    {
        void UpdateAttribute(GenericAttribute attribute);

        void DeleteAttribute(GenericAttribute attribute);

        TPropType GetAttribute<TPropType>(object entity, string key, int id, int storeId = 0, TPropType defaultValue = default);

        IList<GenericAttribute> GetAttributesForEntity(int entityId, string keyGroup);

        void SaveAttribute<TPropType>(string entityName, int id, string key, TPropType value, int storeId = 0);

        void InsertAttribute(GenericAttribute attribute);
    }
}