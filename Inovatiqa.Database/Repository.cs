using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
using Inovatiqa.Database.DbContexts;
using Inovatiqa.Database.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Inovatiqa.Database
{
    public partial class Repository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        #region Fields

        private readonly InovatiqaContext _dbContext;
        private readonly IConfiguration _configuration;
        protected DbSet<TEntity> DbSet { get; }

        #endregion

        #region Ctor

        public Repository(InovatiqaContext dataContext,
            IConfiguration configuration)
        {
            _dbContext = dataContext;
            _configuration = configuration;
            DbSet = _dbContext.Set<TEntity>();
        }

        #endregion

        #region Methods

        public virtual TEntity GetById(int id)
        {
            //changes by hamza
            return DbSet.Find(id);
            //return _dbContext.Customer.Where(c => c.Id == id).FirstOrDefault();
        }

        public virtual void Insert(TEntity entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            DbSet.Add(entity);
            _dbContext.SaveChanges();
        }

        public virtual void Update(TEntity entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            DbSet.Update(entity);
            _dbContext.SaveChanges();
        }

        public virtual void Update(IEnumerable<TEntity> entities)
        {
            if (entities == null)
                throw new ArgumentNullException(nameof(entities));

            foreach (var entity in entities)
            {
                Update(entity);
            }
        }

        public virtual void Delete(TEntity entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            DbSet.Remove(entity);
            _dbContext.SaveChanges();
        }

        public IQueryable<TEntity> Query()
        {
            return DbSet;
        }

        public virtual IEnumerable<TEntity> EntityFromSql(string storeProcedureName, params SqlParameter[] parameters)
        {
            using (var connection = new SqlConnection(_configuration.GetConnectionString("InovatiqaConnection")))
            {
                var dynamicParameters = new DynamicParameters();
                foreach (var parameter in parameters)
                {
                    if (parameter.Direction == ParameterDirection.Output)
                    {
                        if (parameter.DbType == DbType.String)
                            dynamicParameters.Add(parameter.ParameterName, dbType: parameter.DbType, direction: ParameterDirection.Output, size: int.MaxValue);
                        else
                            dynamicParameters.Add(parameter.ParameterName, dbType: parameter.DbType, direction: ParameterDirection.Output);
                    }
                    else
                        dynamicParameters.Add(parameter.ParameterName, parameter.Value, parameter.DbType);
                }
                connection.Open();

                var results = connection.Query<TEntity>(storeProcedureName, dynamicParameters, commandType: CommandType.StoredProcedure);
                foreach (var parameter in parameters)
                {
                    if (parameter.Direction == ParameterDirection.Output)
                    {
                        if (parameter.DbType == DbType.String)
                            parameter.Value = dynamicParameters.Get<string>(parameter.ParameterName);
                        else if (parameter.DbType == DbType.Int32)
                            parameter.Value = dynamicParameters.Get<int>(parameter.ParameterName);
                    }
                }
                return results;
            }
        }

        public virtual void Truncate(string table)
        {
            _dbContext.Database.ExecuteSqlCommand("TRUNCATE TABLE " + table);
        }

        public virtual void BulkUpdate(List<TEntity> entities)
        {
            if (entities == null)
                throw new ArgumentNullException(entities.GetType().Name);
            foreach (var entity in entities)
            {
                DbSet.Update(entity);
            }
            _dbContext.SaveChanges();
        }

        public virtual void BulkDelete(List<TEntity> entities)
        {
            if (entities == null)
                throw new ArgumentNullException(entities.GetType().Name);
            foreach (var entity in entities)
            {
                DbSet.Remove(entity);
            }
            _dbContext.SaveChanges();
        }

        #region Product By MSku

        public virtual Inovatiqa.Database.Models.Product GetProductByMSku(int mSku)
        {
            return _dbContext.Product.Where(x => x.Msku == mSku).FirstOrDefault();
        }

        #endregion

        #endregion
    }
}