using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace TelegrafClient.Auxiliary
{
    public class TcpSender
    {
        private readonly TcpSink _sink;
        private readonly RingBuffer<byte[]> _queue;
        private readonly TimeSpan _reconnectPeriod;
        private readonly object _syncObj = new object();

        private NetworkStream _stream;

        private TcpSender(TcpSink sink, RingBuffer<byte[]> queue, TimeSpan reconnectPeriod)
        {
            _sink = sink;
            _queue = queue;
            _reconnectPeriod = reconnectPeriod;
        }

        public static TcpSender Initialize(string hostname, int port, TcpSenderSettings senderSets)
        {
            var sink = new TcpSink(hostname, port, TimeSpan.FromSeconds(senderSets.WriteTimeoutSec), TimeSpan.FromSeconds(senderSets.ConnectTimeoutSec));
            var queue = new RingBuffer<byte[]>(senderSets.QueueSize);

            var sender = new TcpSender(sink, queue, TimeSpan.FromSeconds(senderSets.ReconnectPeriodSec))
            {
                _stream = sink.TryInitializeStream()
            };

            if (sender._stream == null)
                sender.ScheduleReconnect();

            return sender;
        }

        private void ScheduleReconnect()
        {
            Task.Run(async () => 
            {
                while (true)
                {
                    await Task.Delay(_reconnectPeriod).ConfigureAwait(false);

                    var stream = _sink.TryInitializeStream();

                    if (stream != null)
                    {
                        lock (_syncObj)
                        {
                            _stream = stream;

                            byte[] data;

                            while ((data = _queue.Get()) != null)
                            {
                                if (!TrySendData(data))
                                    break;
                            }

                            if (_stream != null)
                                return;
                        }
                    }
                }
            });
        }

        public void Send(byte[] data)
        {
            lock (_syncObj)
            {
                if (_stream != null)
                {
                    if (!TrySendData(data))
                        ScheduleReconnect();
                }
                else
                    _queue.Put(data);
            }
        }

        private bool TrySendData(byte[] data)
        {
            if (_sink.TrySendData(_stream, data))
                return true;

            _queue.TryInsertFirst(data);

            _stream = null;

            return false;
        }
    }
}
