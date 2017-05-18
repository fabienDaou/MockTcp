using System;
using System.Net;
using System.Net.Sockets;
using TcpUtility.CustomEventArgs;

namespace TcpUtility
{
    public class TcpServer
    {
        private readonly TcpListener tcpListener;

        public event EventHandler<AcceptedClientEventArgs> AcceptedClient;

        public TcpServer(int listeningPort)
        {
            tcpListener = new TcpListener(IPAddress.Any, listeningPort);
        }

        public void Start()
        {
            try
            {
                tcpListener.Start();

                tcpListener.BeginAcceptTcpClient(new AsyncCallback(TcpClientAcceptedCallback), tcpListener);
            }
            catch(SocketException ex)
            {
                Logger.Log(ex.Message, LogLevel.Error);
            }
        }

        public void Stop()
        {
            try
            {
                tcpListener.Stop();
            }
            catch (SocketException ex)
            {
                Logger.Log($"The Tcp server could not properly stop.{ex.Message}", LogLevel.Warning);
            }
        }

        private void TcpClientAcceptedCallback(IAsyncResult ar)
        {
            var listener = ar.AsyncState as TcpListener;
            var acceptedTcpClient = listener.EndAcceptTcpClient(ar);

            var args = new AcceptedClientEventArgs(new AcceptedTcpClient(acceptedTcpClient));
            AcceptedClient?.Invoke(listener, args);

            try
            {
                listener.BeginAcceptTcpClient(new AsyncCallback(TcpClientAcceptedCallback), listener);
            }
            catch(Exception ex) when (ex is SocketException || ex is ObjectDisposedException)
            {
                Logger.Log(ex.Message, LogLevel.Error);
            }
        }
    }
}
