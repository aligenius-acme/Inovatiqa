using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;

namespace Inovatiqa.Database.Interfaces
{
    public interface IRepository<TEntity> where TEntity : class
    {
        #region Methods

        IQueryable<TEntity> Query();

        TEntity GetById(int id);

        void Insert(TEntity entity);

        void Update(TEntity entity);

        void Update(IEnumerable<TEntity> entities);

        void Delete(TEntity entity);

        IEnumerable<TEntity> EntityFromSql(string storeProcedureName, params SqlParameter[] parameters);

        void Truncate(string table);

        void BulkUpdate(List<TEntity> entities);

        void BulkDelete(List<TEntity> entities);

        #region Product By MSku

        Inovatiqa.Database.Models.Product GetProductByMSku(int mSku);

        #endregion

        #endregion

    }
}
