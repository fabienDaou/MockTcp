using System;
using System.Net;
using System.Net.Sockets;

namespace TcpUtility
{
    public interface ITcpListener
    {
        EndPoint LocalEndPoint { get; }
        void Start();
        void Stop();
        IAsyncResult BeginAcceptTcpClient(AsyncCallback asynCallback, object state);
        TcpClient EndAcceptTcpClient(IAsyncResult ar);
    }
}
