namespace TelegrafClient.Auxiliary
{
    public class TcpSenderSettings
    {
        public int WriteTimeoutSec { get; set; }
        public int ConnectTimeoutSec { get; set; }
        public int ReconnectPeriodSec { get; set; }
        public int QueueSize { get; set; }
    }
}