using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Dan.IdentityNPocoStores.Test.Util
{
    /// <summary>
    /// Extensions for SQL so we can inspect the DB easily
    /// </summary>
    internal static class SqlExtensions
    {
        /// <summary>
        /// Get a string from the DB, accept NULL values
        /// </summary>
        /// <param name="reader">Data reader currently in use</param>
        /// <param name="col">Column index from input data</param>
        /// <returns>String value (or NULL)</returns>
        public static string GetStringSafe(this SqlDataReader reader, int col)
        {
            if (!reader.IsDBNull(col))
                return reader.GetString(col);
            else
                return null;
        }

        /// <summary>
        /// Get a DateTimeOffset from the DB, accept NULL values
        /// </summary>
        /// <param name="reader">Data reader currently in use</param>
        /// <param name="col">Column index from input data</param>
        /// <returns>DateTimeOffset value (or NULL)</returns>
        public static DateTimeOffset? GetDateTimeOffsetSafe(this SqlDataReader reader, int col)
        {
            if (!reader.IsDBNull(col))
                return reader.GetDateTimeOffset(col);
            else
                return null;
        }
    }
}
