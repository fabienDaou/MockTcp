using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TcpUtility;

namespace TestProxyApp
{
    class Program
    {
        private static ProxyConfiguration configuration = new ProxyConfiguration(1005);

        static void Main(string[] args)
        {
            // Packets from 127.0.0.1 are forwarded to 127.0.0.1:1007
            var newRule = new ProxyRule(IPAddress.Parse("127.0.0.1"), new IPEndPoint(IPAddress.Parse("127.0.0.1"), 1007));
            configuration.AddRule(newRule);
            var proxy = new TcpProxy(configuration);

            Console.ReadKey();
        }
    }
}
