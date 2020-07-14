using Serilog;
using Serilog.Configuration;
using Serilog.Filters;

namespace RequestLogger.AppCode
{
    public static class SerilogExtensions
    {
        /// <summary>
        /// Filter log events to include only those that has specified property
        /// </summary>
        /// <param name="configuration">configuration</param>
        /// <param name="propertyName">Name of property</param>
        /// <param name="propertyValue">Expected property value</param>
        /// <returns>LoggerConfiguration</returns>
        public static LoggerConfiguration FilterByProperty(this LoggerFilterConfiguration configuration, string propertyName, string propertyValue)
        {
            return configuration.ByIncludingOnly(Matching.WithProperty(propertyName, propertyValue)); //new ScalarValue(propertyValue)

            //var scalarValue = new ScalarValue(propertyValue);
            //return configuration.ByIncludingOnly(logEvent =>
            //{
            //    if (logEvent.Properties.TryGetValue(propertyName, out var propertyValue) && propertyValue is ScalarValue stValue)
            //        return scalarValue.Equals(stValue);
            //    return false;
            //});
        }

        /// <summary>
        /// Filter log events to include only those that has specified property
        /// </summary>
        /// <param name="configuration">configuration</param>
        /// <param name="propertyName">Name of property</param>
        /// <returns>LoggerConfiguration</returns>
        public static LoggerConfiguration FilterByHasProperty(this LoggerFilterConfiguration configuration, string propertyName)
        {
            return configuration.ByIncludingOnly(Matching.WithProperty(propertyName));

            //return configuration.ByIncludingOnly(logEvent => logEvent.Properties.ContainsKey(propertyName));
        }

        /// <summary>
        /// Filter log events to include only those that has specified property
        /// </summary>
        /// <param name="configuration">configuration</param>
        /// <param name="propertyName">Name of property</param>
        /// <param name="propertyValue">Expected property value</param>
        /// <returns>LoggerConfiguration</returns>
        public static LoggerConfiguration FilterOnlyLogRequests(this LoggerFilterConfiguration configuration)
        {
            return configuration.ByIncludingOnly(logEvent =>
            {
                var propertyName = nameof(LogRequestAttribute);
                var contains = logEvent.Properties.ContainsKey(propertyName); //Matching.WithProperty(propertyName)(logEvent);
                logEvent.RemovePropertyIfPresent(propertyName);
                return contains;
            });
        }
    }
}
