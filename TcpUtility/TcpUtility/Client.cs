using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using TcpUtility.CustomEventArgs;

namespace TcpUtility
{
    public sealed class Client : IDisposable
    {
        private TcpClient connectedTcpClient;
        private readonly object connectedTcpClientLock = new object();

        private readonly IPEndPoint remoteEndPoint;
        
        private CancellationTokenSource cancelConnectTokenSource;
        private CancellationToken cancelConnectToken;

        private bool isConnectedAlreadyCalled;
        private bool isDisposed;
        private readonly object disposingLock = new object();

        public event EventHandler<ConnectChangedEventArgs> ConnectChanged;

        public Client(IPEndPoint remoteEndPoint)
        {
            this.remoteEndPoint = remoteEndPoint;
            cancelConnectTokenSource = new CancellationTokenSource();
            cancelConnectToken = cancelConnectTokenSource.Token;
        }

        public void Dispose()
        {
            lock (disposingLock)
            {
                cancelConnectTokenSource?.Dispose();
                cancelConnectTokenSource = null;

                Disconnect();

                isDisposed = true;
            }
        }

        public Task ConnectAsync()
        {
            ThrowIfDisposed();
            if (isConnectedAlreadyCalled)
            {
                return Task.FromResult(0);
            }
            isConnectedAlreadyCalled = true;
            return Task.Factory.StartNew(() => Connect(), cancelConnectToken);
        }

        public void Close()
        {
            ThrowIfDisposed();
            cancelConnectTokenSource.Cancel();
            Dispose();
        }

        private void Connect()
        {
            try
            {
                var connectingTcpClient = new TcpClient();
                var connectTask = connectingTcpClient.ConnectAsync(remoteEndPoint.Address, remoteEndPoint.Port);
                connectTask.Wait(cancelConnectToken);

                lock (connectedTcpClientLock)
                {
                    connectedTcpClient = connectingTcpClient;
                }

                ConnectChanged?.Invoke(this, new ConnectChangedEventArgs(true));
            }
            catch (Exception ex) when (ex is OperationCanceledException || ex is AggregateException)
            {
                ConnectChanged?.Invoke(this, new ConnectChangedEventArgs(false));
            }
        }

        private void Disconnect()
        {
            lock (connectedTcpClientLock)
            {
                connectedTcpClient?.Close();
                connectedTcpClient = null;
            }
        }

        private void ThrowIfDisposed()
        {
            lock (disposingLock)
            {
                if (isDisposed)
                {
                    throw new ObjectDisposedException(nameof(Client));
                }
            }
        }
    }
}
