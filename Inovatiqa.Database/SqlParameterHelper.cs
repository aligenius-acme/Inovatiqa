using Microsoft.Data.SqlClient;
using System;
using System.Data;

namespace Inovatiqa.Database
{
    public static partial class SqlParameterHelper
    {
        #region Utilities

        private static SqlParameter GetParameter(DbType SqlDbType, string parameterName, object parameterValue)
        {
            var parameter = new SqlParameter
            {
                ParameterName = parameterName,
                SqlValue = parameterValue ?? DBNull.Value,
                Value = parameterValue ?? DBNull.Value,
                DbType = SqlDbType
            };

            return parameter;
        }

        private static SqlParameter GetOutputParameter(DbType SqlDbType, string parameterName)
        {
            var parameter = new SqlParameter
            {
                ParameterName = parameterName,
                DbType = SqlDbType,
                Direction = ParameterDirection.Output
            };

            return parameter;
        }

        #endregion

        #region Methods

        public static SqlParameter GetStringParameter(string parameterName, string parameterValue)
        {
            return GetParameter(DbType.String, parameterName, parameterValue);
        }

        public static SqlParameter GetOutputStringParameter(string parameterName)
        {
            return GetOutputParameter(DbType.String, parameterName);
        }

        public static SqlParameter GetInt32Parameter(string parameterName, int? parameterValue)
        {
            return GetParameter(DbType.Int32, parameterName, parameterValue);
        }

        public static SqlParameter GetOutputInt32Parameter(string parameterName)
        {
            return GetOutputParameter(DbType.Int32, parameterName);
        }

        public static SqlParameter GetBooleanParameter(string parameterName, bool? parameterValue)
        {
            return GetParameter(DbType.Boolean, parameterName, parameterValue);
        }

        public static SqlParameter GetDecimalParameter(string parameterName, decimal? parameterValue)
        {
            return GetParameter(DbType.Decimal, parameterName, parameterValue);
        }

        public static SqlParameter GetDateTimeParameter(string parameterName, DateTime? parameterValue)
        {
            return GetParameter(DbType.DateTime, parameterName, parameterValue);
        }

        #endregion
    }
}
