using System;

namespace TcpUtility.CustomEventArgs
{
    public class AcceptedClientEventArgs : EventArgs
    {
        public AcceptedTcpClient TcpClient { get; }

        public AcceptedClientEventArgs(AcceptedTcpClient client)
        {
            TcpClient = client;
        }
    }
}
