using System;
using TcpUtility.CustomEventArgs;

namespace TcpUtility
{
    public interface ITcpServer
    {
        event EventHandler<AcceptedClientEventArgs> AcceptedClient;
        void Start();
        void Stop();
    }
}
