using System.Net;

namespace TcpUtility
{
    public class ProxyRule
    {
        public IPAddress Source { get; }
        public IPEndPoint Destination { get; }

        public ProxyRule(IPAddress source, IPEndPoint destination)
        {
            Source = source;
            Destination = destination;
        }
    }
}
