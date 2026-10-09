using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;

namespace ProjectCore.Template
{
    public sealed class NtpTimeProvider : ITimeProvider
    {
        private const string DefaultServer = "pool.ntp.org";
        private const int Port = 123;
        private const int TimeoutMilliseconds = 3000;

        private DateTime _baseTime = DateTime.UtcNow;
        private long _baseTimestamp = System.Diagnostics.Stopwatch.GetTimestamp();

        public async UniTask InitializeAsync(
            CancellationToken cancellationToken,
            string server = DefaultServer)
        {
            try
            {
                DateTime networkTime = await Task.Run(
                    () => GetNetworkTime(server),
                    cancellationToken);

                _baseTime = networkTime;
                _baseTimestamp = System.Diagnostics.Stopwatch.GetTimestamp();
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch
            {
                _baseTime = DateTime.UtcNow;
                _baseTimestamp = System.Diagnostics.Stopwatch.GetTimestamp();
            }
        }

        public DateTime GetCurrentTime()
        {
            long elapsedTicks = System.Diagnostics.Stopwatch.GetTimestamp() - _baseTimestamp;
            double elapsedSeconds = (double)elapsedTicks / System.Diagnostics.Stopwatch.Frequency;
            return _baseTime.AddSeconds(elapsedSeconds);
        }

        private static DateTime GetNetworkTime(string server)
        {
            using (Socket socket = new Socket(
                       AddressFamily.InterNetwork,
                       SocketType.Dgram,
                       ProtocolType.Udp))
            {
                socket.ReceiveTimeout = TimeoutMilliseconds;
                socket.SendTimeout = TimeoutMilliseconds;

                IPAddress address = Dns.GetHostEntry(server).AddressList[0];
                socket.Connect(new IPEndPoint(address, Port));

                byte[] packet = new byte[48];
                packet[0] = 0x1B;
                socket.Send(packet);
                socket.Receive(packet);

                ulong integerPart = SwapEndianness(BitConverter.ToUInt32(packet, 40));
                ulong fractionalPart = SwapEndianness(BitConverter.ToUInt32(packet, 44));
                ulong milliseconds = integerPart * 1000 + fractionalPart * 1000 / 0x100000000L;

                return new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                    .AddMilliseconds((long)milliseconds);
            }
        }

        private static uint SwapEndianness(ulong value)
        {
            return (uint)(((value & 0x000000ff) << 24) +
                          ((value & 0x0000ff00) << 8) +
                          ((value & 0x00ff0000) >> 8) +
                          ((value & 0xff000000) >> 24));
        }
    }
}
