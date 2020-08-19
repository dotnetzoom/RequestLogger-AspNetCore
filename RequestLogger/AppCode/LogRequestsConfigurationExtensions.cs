using BookingSystem.WebServiceCache.Application.Utilities;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.MSSqlServer;
using Serilog.Sinks.MSSqlServer.Sinks.MSSqlServer.Options;
using System;
using System.Collections.ObjectModel;
using System.Data;

namespace RequestLogger.AppCode
{
    public static class LogRequestsConfigurationExtensions
    {
        /// <summary>
        /// Log action requests to .../sink
        /// </summary>
        /// <param name="loggerConfiguration">loggerConfiguration</param>
        /// <param name="configureLogger">An action that configures the sub-logger.</param>
        /// <returns>LoggerConfiguration</returns>
        public static LoggerConfiguration LogActionRequestsWriteTo(this LoggerConfiguration loggerConfiguration, Action<LoggerConfiguration> configureLogger)
        {
            loggerConfiguration.WriteTo.Logger(config =>
            {
                config.Filter.FilterOnlyLogRequests();
                configureLogger(config);
            });
            return loggerConfiguration;
        }

        /// <summary>
        /// Log action requests to sql server
        /// </summary>
        /// <param name="loggerConfiguration">loggerConfiguration</param>
        /// <param name="connectionString">Connection string to sql server database</param>
        /// <param name="tableName">Name of table in sql server database</param>
        public static void LogActionRequestsToSqlServer(this LoggerConfiguration loggerConfiguration, string connectionString, string tableName = "RequestLogs")
        {
            SqlHelper.CreateDatabaseIfNotExists(connectionString);

            loggerConfiguration.WriteTo.Logger(subLoggerConfigurations =>
            {
                var options = new ColumnOptions();
                options.Store.Remove(StandardColumn.Properties);
                options.Store.Add(StandardColumn.LogEvent);
                options.LogEvent.ExcludeAdditionalProperties = true;
                options.LogEvent.ExcludeStandardColumns = true;
                //options.TimeStamp.ConvertToUtc = true;
                options.AdditionalColumns = new Collection<SqlColumn>
                {
                    new SqlColumn {ColumnName = "RequestPath", DataType = SqlDbType.NVarChar, DataLength = 500, AllowNull = false},
                    new SqlColumn {ColumnName = "RequestMethod", DataType = SqlDbType.NVarChar, DataLength = 10, AllowNull = false},
                    new SqlColumn {ColumnName = "StatusCode", DataType = SqlDbType.Int, AllowNull = false},
                    new SqlColumn {ColumnName = "Elapsed", DataType = SqlDbType.Float, AllowNull = false},
                    new SqlColumn {ColumnName = "IpAddress", DataType = SqlDbType.NVarChar, DataLength = 100, AllowNull = false},
                };

                subLoggerConfigurations
                    .Filter.FilterOnlyLogRequests()
                    //.Enrich.FromLogContext()
                    .WriteTo.MSSqlServer(
                        connectionString: connectionString,
                        sinkOptions: new SinkOptions { AutoCreateSqlTable = true, TableName = "Logs" },
                        restrictedToMinimumLevel: LogEventLevel.Information,
                        columnOptions: options);
            });
        }
    }
}
