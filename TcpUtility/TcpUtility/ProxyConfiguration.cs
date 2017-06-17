using System.Collections.Generic;
using System.Net;

namespace TcpUtility
{
    public class ProxyConfiguration
    {
        private readonly Dictionary<IPAddress, ProxyRule> rules = new Dictionary<IPAddress, ProxyRule>();

        public int ListeningPort { get; }

        public ProxyConfiguration(int listeningPort)
        {
            ListeningPort = listeningPort;
        }

        public void AddRule(ProxyRule rule)
        {
            rules.Add(rule.Source, rule);
        }

        public ProxyRule FindRule(IPAddress source)
        {
            rules.TryGetValue(source, out ProxyRule ruleFound);
            return ruleFound;
        }
    }
}
