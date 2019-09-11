using System;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Syslog.Framework.Logging;
using TelegrafClient.Auxiliary;
using TelegrafClient.Logs;

namespace TelegrafClient
{
    public class TelegrafClientSettings
    {
        private TelegrafClientSettings SetDefaults()
        {
            if (ServerHost == null)
                ServerHost = GetDefaultGateway()?.ToString() ?? "127.0.0.1";

            if (Hostname == null)
                Hostname = Environment.MachineName;

            if (Appname == null)
                Appname = Assembly.GetEntryAssembly()?.GetName().Name.Replace(' ', '_');

            return this;
        }

        private TelegrafClientSettings() { }

        public static TelegrafClientSettings Default() => new TelegrafClientSettings().SetDefaults();

        private static IPAddress GetDefaultGateway()
        {
            return NetworkInterface
                .GetAllNetworkInterfaces()
                .Where(n => n.OperationalStatus == OperationalStatus.Up)
                .Where(n => n.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                .SelectMany(n => n.GetIPProperties()?.GatewayAddresses)
                .Select(g => g?.Address)
                .Where(a => a != null)
                .Where(a => a.AddressFamily == AddressFamily.InterNetwork)
                .Where(a => Array.FindIndex(a.GetAddressBytes(), b => b != 0) >= 0)
                .FirstOrDefault();
        }


        /// <summary>
        /// Gets or sets the protocol used to send messages to Telegraf server.
        /// </summary>
        public TransportProtocol MessageTransportProtocol { get; set; } = TransportProtocol.Tcp;

        /// <summary>
        /// Gets or sets the host for the Telegraf server.
        /// </summary>
        public string ServerHost { get; set; }

        /// <summary>
        /// Gets or sets the port for the Syslog server.
        /// </summary>
        public int LogServerPort { get; set; } = 6514;

        /// <summary>
        /// Gets or sets the port for the Telegraf metrics server.
        /// </summary>
        public int MetricServerPort { get; set; } = 8094;

        public string Hostname { get; set; }

        public string Appname { get; set; }


        public TcpSenderSettings TcpSenderSettings { get; set; } = new TcpSenderSettings
        {
            ConnectTimeoutSec = 5,
            QueueSize = 1000,
            ReconnectPeriodSec = 10,
            WriteTimeoutSec = 2
        };

        public SyslogSettings SyslogSettings { get; set; } = new SyslogSettings
        {
            FacilityType = FacilityType.Local0,
            MinLogLevel = LogLevel.Trace,
            UseUtc = true,
            UseOctetCounting = true
        };
    }
}
