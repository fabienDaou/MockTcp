using System;

namespace TcpUtility.CustomEventArgs
{
    public class ConnectChangedEventArgs : EventArgs
    {
        public bool Connected { get; }

        public ConnectChangedEventArgs(bool connected)
        {
            Connected = connected;
        }
    }
}
