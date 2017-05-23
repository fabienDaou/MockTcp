using System;
using System.Net.Sockets;
using TcpUtility.CustomEventArgs;

namespace TcpUtility
{
    public sealed class TcpServer : ITcpServer
    {
        private ITcpListener tcpListener;

        private bool startAccepting;
        private readonly object startAcceptingLock = new object();

        public event EventHandler<AcceptedClientEventArgs> AcceptedClient;

        public TcpServer(int listeningPort)
        {
            tcpListener = MultiSourceTcpListener.Create(listeningPort);
        }

        public void Start()
        {
            lock (startAcceptingLock)
            {
                startAccepting = true;
                tcpListener.Start();
            }

            BeginAccept();
        }

        public void Stop()
        {
            lock (startAcceptingLock)
            {
                startAccepting = false;

                try
                {
                    tcpListener.Stop();
                }
                catch (SocketException ex)
                {
                    Logger.Log($"The Tcp server could not properly stop.{ex.Message}", LogLevel.Warning);
                }
            }
        }

        private void BeginAccept()
        {
            try
            {
                tcpListener.BeginAcceptTcpClient(new AsyncCallback(TcpClientAcceptedCallback), null);
            }
            catch (Exception ex) when (ex is SocketException || ex is ObjectDisposedException)
            {
                Logger.Log(ex.Message, LogLevel.Error);
            }
        }

        private void TcpClientAcceptedCallback(IAsyncResult ar)
        {
            var acceptedTcpClient = tcpListener.EndAcceptTcpClient(ar);

            var args = new AcceptedClientEventArgs(new AcceptedTcpClient(acceptedTcpClient));

            lock (startAcceptingLock)
            {
                if (startAccepting)
                {
                    BeginAccept();
                }
            }
                        
            AcceptedClient?.Invoke(tcpListener, args);
        }
    }
}
