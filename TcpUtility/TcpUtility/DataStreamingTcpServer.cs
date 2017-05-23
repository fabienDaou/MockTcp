using System;
using System.Threading;
using TcpUtility.CustomEventArgs;

namespace TcpUtility
{
    public sealed class DataStreamingTcpServer
    {
        private TcpServer tcpServer;

        private CancellationTokenSource cancelReceiveTokenSource;

        private bool isStarted = false;
        private readonly object isStartedLock = new object();

        public event EventHandler<DataReceivedEventArgs> DataReceived;

        public DataStreamingTcpServer(int listeningPort)
        {
            tcpServer = new TcpServer(listeningPort);
        }

        public void Start()
        {
            lock (isStartedLock)
            {
                if (!isStarted)
                {
                    cancelReceiveTokenSource = new CancellationTokenSource();
                    tcpServer.AcceptedClient += TcpServer_AcceptedClient;
                    tcpServer.Start();
                    isStarted = true;
                }
            }
        }

        public void Stop()
        {
            lock (isStartedLock)
            {
                tcpServer.AcceptedClient -= TcpServer_AcceptedClient;
                cancelReceiveTokenSource?.Cancel();
                cancelReceiveTokenSource?.Dispose();
                cancelReceiveTokenSource = null;
                isStarted = false;
            }
        }

        private void TcpServer_AcceptedClient(object source, AcceptedClientEventArgs args)
        {
            var acceptedTcpClient = args.TcpClient;
            Logger.Log($"New client accepted.{args.TcpClient.RemoteEndPoint}", LogLevel.Info);

            ReadFromTcpClient(acceptedTcpClient);
            acceptedTcpClient.Close();
        }

        private void ReadFromTcpClient(AcceptedTcpClient tcpClient)
        {
            bool isConnected = true;

            var buffer = new byte[1024];

            while (isConnected && !cancelReceiveTokenSource.Token.IsCancellationRequested)
            {
                int bytesReceived = tcpClient.Receive(buffer);
                if (bytesReceived > 0)
                {
                    var localBuffer = new byte[bytesReceived];
                    Array.Copy(buffer, localBuffer, bytesReceived);

                    DataReceived?.Invoke(tcpClient, new DataReceivedEventArgs(localBuffer));
                }
                else
                {
                    isConnected = false;
                }
            }
        }
    }
}
