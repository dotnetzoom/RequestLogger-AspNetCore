using System;
using System.Threading.Tasks;
using RequestLogger.AppCode;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Filters;
using Serilog.Sinks.MSSqlServer;
using Microsoft.Extensions.Primitives;
using System.Linq;
using Serilog.Sinks.MSSqlServer.Sinks.MSSqlServer.Options;

namespace RequestLogger
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
               .MinimumLevel.Debug()
               .WriteTo.Console()
               .WriteTo.Debug()
               .CreateLogger();
            try
            {
                Log.Information("Starting web host");
                await CreateHostBuilder(args).Build().RunAsync();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
           Host.CreateDefaultBuilder(args)
               .ConfigureWebHostDefaults(webBuilder =>
               {
                   webBuilder.UseStartup<Startup>();
               })
               .UseSerilog((hostingContext, loggerConfiguration) =>
               {
                   loggerConfiguration
                       .Enrich.FromLogContext();

                   //Log requests
                   loggerConfiguration.LogActionRequestsToSqlServer("Data Source=.;Initial Catalog=SerilogDb;Integrated Security=true");

                   //Log other logs/exceptions
                   loggerConfiguration
                        .WriteTo.Logger(subLoggerConfiguration =>
                        {
                            var options = new ColumnOptions();
                            options.Store.Remove(StandardColumn.Properties);
                            options.Store.Add(StandardColumn.LogEvent);

                            subLoggerConfiguration
                                .Filter.ByExcluding(Matching.WithProperty(nameof(LogRequestAttribute)))
                                .WriteTo.MSSqlServer(
                                    connectionString: "Data Source=.;Initial Catalog=SerilogDb;Integrated Security=true",
                                    sinkOptions: new SinkOptions { AutoCreateSqlTable = true, TableName = "Logs" },
                                    restrictedToMinimumLevel: LogEventLevel.Warning,
                                    columnOptions: options);
                        });
               });
    }


}
