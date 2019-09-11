using System;
using System.Net.Sockets;

namespace TelegrafClient.Auxiliary
{
    public class TcpSink
    {
        private readonly string _hostname;
        private readonly int _port;
        private readonly TimeSpan _connectTimeout;
        private readonly TimeSpan _writeTimeout;

        public TcpSink(string hostname, int port, TimeSpan writeTimeout, TimeSpan connectTimeout)
        {
            _hostname = hostname;
            _port = port;
            _writeTimeout = writeTimeout;
            _connectTimeout = connectTimeout;
        }

        public NetworkStream TryInitializeStream()
        {
            var client = new TcpClient();

            try
            {
                var asyncResult = client.BeginConnect(_hostname, _port, null, null);

                asyncResult.AsyncWaitHandle.WaitOne(_connectTimeout);

                if (!client.Connected)
                    throw new TimeoutException();

                client.EndConnect(asyncResult);

                client.NoDelay = true;

                client.SendTimeout = (int)_writeTimeout.TotalMilliseconds;

                return client.GetStream();
            }
            catch
            {
                client.Close();

                return null;
            }
        }

        public bool TrySendData(NetworkStream stream, byte[] data)
        {
            if (stream == null)
                return false;

            try
            {
                stream.Write(data, 0, data.Length);

                return true;
            }
            catch
            {
                stream.Close(0);

                return false;
            }
        }
    }
}
