using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace ElMariachi.Http.Server
{
    public class TcpListenerEx : TcpListener
    {
        public TcpListenerEx(IPAddress ipAddress, in int port) : base(ipAddress, port)
        {
        }

        public bool IsListening => Active;

        public void Start(CancellationToken cancellationToken)
        {
            // NOTE: unfortunately, the AcceptTcpClient method doesn't accept a CancellationToken,
            // the task below is waiting for a the cancellationToken to be signaled in order to stop the server.
            this.Start();

            Task.Run(() =>
            {
                cancellationToken.WaitHandle.WaitOne();
                this.Stop();
            }, cancellationToken);
        }
    }
}