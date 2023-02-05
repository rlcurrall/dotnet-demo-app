using System.ComponentModel.DataAnnotations;

namespace Acme.Core.Logging
{
    public class WebLoggerOptions
    {
        /// <summary>
        /// Unique identifier for app, to be used in AI queries 
        /// </summary>
        [Required]
        public string AppId { get; set; }

        public string CorrelationIdHeaderKey { get; set; } = "x-correlation-id";

        /// <summary>
        /// AI Connection String
        /// </summary>
        [Required]
        public string AppInsightsConnectionString { get; set; }

        internal bool IsValid()
        {
            return !string.IsNullOrEmpty(AppId) && !string.IsNullOrEmpty(AppInsightsConnectionString);
        }
    }
}