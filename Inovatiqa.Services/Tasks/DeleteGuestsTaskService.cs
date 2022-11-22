using Dapper;
using Inovatiqa.Core;
using Inovatiqa.Database;
using Inovatiqa.Database.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace Inovatiqa.Services.Tasks
{
    public class DeleteGuestsTaskService : BackgroundTaskService
    {

        #region Fields
        private readonly IConfiguration _configuration;
        #endregion

        #region Ctor
        public DeleteGuestsTaskService(
            IConfiguration configuration)
        {
            _configuration = configuration;
        }
        #endregion

        #region Utilities
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var olderThanMinutes = InovatiqaDefaults.DeleteGuestTaskOlderThanMinutes;
                //prepare parameters
                var pOnlyWithoutShoppingCart = SqlParameterHelper.GetBooleanParameter("OnlyWithoutShoppingCart", true);
                var pCreatedFromUtc = SqlParameterHelper.GetDateTimeParameter("CreatedFromUtc", null);
                var pCreatedToUtc = SqlParameterHelper.GetDateTimeParameter("CreatedToUtc", DateTime.UtcNow.AddMinutes(-olderThanMinutes));
                var pTotalRecordsDeleted = SqlParameterHelper.GetOutputInt32Parameter("TotalRecordsDeleted");

                //invoke stored procedure
                EntityFromSql("DeleteGuests", pOnlyWithoutShoppingCart,
                    pCreatedFromUtc,
                    pCreatedToUtc,
                    pTotalRecordsDeleted);

                var totalRecordsDeleted = pTotalRecordsDeleted.Value != DBNull.Value ? Convert.ToInt32(pTotalRecordsDeleted.Value) : 0;
                await Task.Delay(5000000, stoppingToken); // Time to pause this background logic
            }
        }
        #endregion

        #region Methods
        public virtual IEnumerable<Customer> EntityFromSql(string storeProcedureName, params SqlParameter[] parameters)
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

                var results = connection.Query<Customer>(storeProcedureName, dynamicParameters, commandType: CommandType.StoredProcedure);
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

        #endregion
    }
}
