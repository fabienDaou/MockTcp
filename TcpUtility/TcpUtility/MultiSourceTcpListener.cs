using System;
using System.Net;
using System.Net.Sockets;

namespace TcpUtility
{
    public class MultiSourceTcpListener : ITcpListener
    {
        private TcpListener tcpListener;

        public EndPoint LocalEndPoint => tcpListener.LocalEndpoint;

        private MultiSourceTcpListener(int listeningPort)
        {
            tcpListener = new TcpListener(IPAddress.Any, listeningPort);
        }

        public static ITcpListener Create(int listeningPort)
        {
            return new MultiSourceTcpListener(listeningPort);
        }

        public IAsyncResult BeginAcceptTcpClient(AsyncCallback asynCallback, object state)
        {
            return tcpListener.BeginAcceptTcpClient(asynCallback, state);
        }

        public TcpClient EndAcceptTcpClient(IAsyncResult ar)
        {
            return tcpListener.EndAcceptTcpClient(ar);
        }

        public void Start()
        {
            tcpListener.Start();
        }

        public void Stop()
        {
            tcpListener.Stop();
        }
    }
}
