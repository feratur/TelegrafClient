using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Syslog.Framework.Logging;

namespace TelegrafClient.Logs
{
    public class SyslogSettings
    {
        public LogLevel MinLogLevel { get; set; }

        /// <summary>
        /// Gets or sets whether to use TCP octet counting or non-transparent framing method. Defaults to true (octet counting).
        /// </summary>
        /// <remarks>
        /// </remarks>
        public bool UseOctetCounting { get; set; }

        /// <summary>
        /// Gets or sets the facility type.
        /// </summary>
        public FacilityType FacilityType { get; set; }

        /// <summary>
        /// Structured data that is sent with every request. Only for RFC 5424.
        /// </summary>
        public IEnumerable<SyslogStructuredData> StructuredData { get; set; }

        /// <summary>
        /// Gets or sets whether to log messages using UTC or local time. Defaults to true (use UTC).
        /// </summary>
        public bool UseUtc { get; set; }
    }
}