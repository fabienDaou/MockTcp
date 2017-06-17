using System;
using System.Collections.Generic;
using System.Linq;
using TcpUtility.CustomEventArgs;

namespace TcpUtility
{
    // WIP
    // TODO: when one side is closing the other side should also close. Add Closed event handler on both sides.
    public sealed class TcpProxy : IDisposable
    {
        private readonly ProxyConfiguration configuration;

        private DataStreamingTcpServer dataStreamingTcpServer;

        private readonly List<ProxySession> allProxySessions = new List<ProxySession>();
        private readonly object allProxySessionsLock = new object();

        private bool isDisposed;

        public TcpProxy(ProxyConfiguration configuration)
        {
            this.configuration = configuration;
            dataStreamingTcpServer = new DataStreamingTcpServer(new TcpServer(configuration.ListeningPort));
            dataStreamingTcpServer.DataReceived += DataStreamingTcpServer_DataReceived;
            dataStreamingTcpServer.Start();
        }

        public void Dispose()
        {
            if (!isDisposed)
            {
                dataStreamingTcpServer.DataReceived -= DataStreamingTcpServer_DataReceived;
                dataStreamingTcpServer.Stop();
                
                allProxySessions.ForEach(ps => ps.Close());
                isDisposed = true;
            }
        }

        private void ForwardDataToDestination(AcceptedTcpClient acceptedTcpClient, byte[] data)
        {
            var proxySession = GetOrCreateProxySession(acceptedTcpClient);
            proxySession.DestinationClient.Send(data);
        }

        private ProxySession GetOrCreateProxySession(AcceptedTcpClient acceptedTcpClient)
        {
            ProxySession proxySession;
            if(!TryFindProxySession(acceptedTcpClient, out proxySession))
            {
                var rule = configuration.FindRule(acceptedTcpClient.RemoteEndPoint.Address);

                // TODO: find a better way to do that so it does not block the server from receiving data.
                var destinationClient = new DataStreamingClient(rule.Destination);
                destinationClient.ConnectAsync().Wait();
                proxySession = new ProxySession(acceptedTcpClient, destinationClient);
                allProxySessions.Add(proxySession);
            }
            return proxySession;
        }

        private bool TryFindProxySession(AcceptedTcpClient acceptedTcpClient, out ProxySession proxySession)
        {
            lock (allProxySessions)
            {
                proxySession = allProxySessions.FirstOrDefault(p => p.SourceClient.RemoteEndPoint == acceptedTcpClient.RemoteEndPoint);
            }
            return proxySession != null;
        }

        private void DataStreamingTcpServer_DataReceived(object sender, DataReceivedEventArgs e)
        {
            if (sender is AcceptedTcpClient acceptedTcpClient)
            {
                ForwardDataToDestination(acceptedTcpClient, e.Data);
            }
        }
    }
}
