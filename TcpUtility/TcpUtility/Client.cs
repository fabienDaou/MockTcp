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
        private TcpClient tcpClient;
        private readonly IPEndPoint remoteEndPoint;
        
        private CancellationTokenSource cancelConnectTokenSource;
        private CancellationToken cancelConnectToken;

        private bool isConnectedAlreadyCalled;
        private bool isDisposed;
        private readonly object isDisposedLock = new object();

        public event EventHandler<ConnectChangedEventArgs> ConnectChanged;

        public Client(IPEndPoint remoteEndPoint)
        {
            this.remoteEndPoint = remoteEndPoint;
            cancelConnectTokenSource = new CancellationTokenSource();
            cancelConnectToken = cancelConnectTokenSource.Token;
        }

        public void Dispose()
        {
            lock (isDisposedLock)
            {
                if (!isDisposed)
                {
                    cancelConnectTokenSource.Dispose();
                    tcpClient.Close();
                    isDisposed = true;
                }
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
                tcpClient = new TcpClient();
                var connectTask = tcpClient.ConnectAsync(remoteEndPoint.Address, remoteEndPoint.Port);
                connectTask.Wait(cancelConnectToken);
                ConnectChanged?.Invoke(this, new ConnectChangedEventArgs(true));
            }
            catch (OperationCanceledException)
            {
                ConnectChanged?.Invoke(this, new ConnectChangedEventArgs(false));
            }
        }

        private void ThrowIfDisposed()
        {
            lock (isDisposedLock)
            {
                if (isDisposed)
                {
                    throw new ObjectDisposedException(nameof(Client));
                }
            }
        }
    }
}
