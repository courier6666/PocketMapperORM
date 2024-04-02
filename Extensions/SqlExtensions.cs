using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PocketMapperORM.Extensions
{
    public static class SqlExtensions
    {
        public static SqlConnection OpenConnection(this SqlConnection sqlConnection)
        {
            sqlConnection.Open();
            return sqlConnection;
        }
        public static async Task<SqlConnection> OpenConnectionAsync(this SqlConnection sqlConnection)
        {
            await sqlConnection.OpenAsync();
            return sqlConnection;
        }
        public static async Task<SqlConnection> OpenConnectionAsync(this SqlConnection sqlConnection, CancellationToken token)
        {
            await sqlConnection.OpenAsync(cancellationToken: token);
            return sqlConnection;
        }
    }
}
