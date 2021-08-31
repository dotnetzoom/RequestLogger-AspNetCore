using Microsoft.Data.SqlClient;
using System;

namespace RequestLogger.AppCode
{
    public static class SqlHelper
    {
        /// <summary>
        /// Create database if not exists
        /// Example 1 : databaseName: null(optional) and connectionString: "Data Source=.;Initial Catalog=MyDatabaseName;Integrated Security=true"
        /// Example 1 : databaseName: "MyDatabaseName" and connectionString: "Data Source=.;Initial Catalog=master;Integrated Security=true"
        /// </summary>
        /// <param name="connectionString">Connection string to connect</param>
        /// <param name="databaseName">Database name to check or create</param>
        public static void CreateDatabaseIfNotExists(string connectionString, string databaseName = null)
        {
            if (!DatabaseExists(connectionString, databaseName))
                CreateDatabase(connectionString, databaseName);
        }

        /// <summary>
        /// Checks the existence of a database
        /// Example 1 : databaseName: null(optional) and connectionString: "Data Source=.;Initial Catalog=MyDatabaseName;Integrated Security=true"
        /// Example 1 : databaseName: "MyDatabaseName" and connectionString: "Data Source=.;Initial Catalog=master;Integrated Security=true"
        /// </summary>
        /// <param name="connectionString">Connection string to connect</param>
        /// <param name="databaseName">Database name to check</param>
        /// <returns>Whether or not the database is exist</returns>
        public static bool DatabaseExists(string connectionString, string databaseName = null)
        {
            var builder = new SqlConnectionStringBuilder(connectionString);

            var isMaster = builder.InitialCatalog.Equals("master", StringComparison.OrdinalIgnoreCase);
            if (string.IsNullOrWhiteSpace(databaseName))
            {
                if (isMaster)
                    throw new InvalidOperationException($"If {nameof(databaseName)} hasn't value then current InitialCatalog shouldn't be 'master'");

                databaseName = builder.InitialCatalog;
                builder.InitialCatalog = "master";
            }
            else
            {
                if (!isMaster)
                    throw new InvalidOperationException($"If {nameof(databaseName)} has value ({databaseName}) then current InitialCatalog should be 'master'");
            }

            var command = "select count(*) from master.dbo.sysdatabases where name=@database";
            using (var sqlConnection = new SqlConnection(builder.ConnectionString))
            {
                using (var sqlCommand = new SqlCommand(command, sqlConnection))
                {
                    sqlCommand.Parameters.Add("@database", System.Data.SqlDbType.NVarChar).Value = databaseName;
                    sqlConnection.Open();
                    return Convert.ToInt32(sqlCommand.ExecuteScalar()) == 1;
                }
            }
        }

        /// <summary>
        /// Create database
        /// Example 1 : databaseName: null(optional) and connectionString: "Data Source=.;Initial Catalog=MyDatabaseName;Integrated Security=true"
        /// Example 1 : databaseName: "MyDatabaseName" and connectionString: "Data Source=.;Initial Catalog=master;Integrated Security=true"
        /// </summary>
        /// <param name="connectionString">Connection string to connect</param>
        /// <param name="databaseName">Database name to create</param>
        public static void CreateDatabase(string connectionString, string databaseName = null)
        {
            var builder = new SqlConnectionStringBuilder(connectionString);

            var isMaster = builder.InitialCatalog.Equals("master", StringComparison.OrdinalIgnoreCase);
            if (string.IsNullOrWhiteSpace(databaseName))
            {
                if (isMaster)
                    throw new InvalidOperationException($"If {nameof(databaseName)} hasn't value then current InitialCatalog shouldn't be 'master'");

                databaseName = builder.InitialCatalog;
                builder.InitialCatalog = "master";
            }
            else
            {
                if (!isMaster)
                    throw new InvalidOperationException($"If {nameof(databaseName)} has value ({databaseName}) then current InitialCatalog should be 'master'");
            }

            var command = $"CREATE DATABASE {databaseName}";

            #region More info
            //https://stackoverflow.com/questions/39499810/how-to-create-database-if-not-exist-in-c-sharp-winforms
            //https://www.codeproject.com/Questions/666651/How-to-create-a-database-using-csharp-code-in-net
            //https://support.microsoft.com/en-us/help/307283/how-to-create-a-sql-server-database-programmatically-by-using-ado-net

            // ConnectionString examples
            //"server=(local)\\SQLEXPRESS;Trusted_Connection=yes"
            //"Server=localhost;Integrated security=SSPI;database=master"

            //var command = $"CREATE DATABASE {"MyDatabase"} ON PRIMARY " +
            //    $"(NAME = {"MyDatabase_Data"}, " +
            //    $"FILENAME = '{"C:\\MyDatabaseData.mdf"}', " +
            //    $"SIZE = {2}MB, " +
            //    $"MAXSIZE = {10}MB, " +
            //    $"FILEGROWTH = {10}%) " +
            //    $"LOG ON (NAME = {"MyDatabase_Log"}, " +
            //    $"FILENAME = '{"C:\\MyDatabaseLog.ldf"}', " +
            //    $"SIZE = {1}MB, " +
            //    $"MAXSIZE = {5}MB, " +
            //    $"FILEGROWTH = {10}%)";
            #endregion

            using (var sqlConnection = new SqlConnection(builder.ConnectionString))
            {
#pragma warning disable CA2100 // Review SQL queries for security vulnerabilities
                using (var sqlCommand = new SqlCommand(command, sqlConnection))
#pragma warning restore CA2100 // Review SQL queries for security vulnerabilities
                {
                    sqlConnection.Open();
                    sqlCommand.ExecuteNonQuery();
                }
            }
        }
    }
}
