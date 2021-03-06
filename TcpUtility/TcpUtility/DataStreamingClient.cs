﻿using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using TcpUtility.CustomEventArgs;

namespace TcpUtility
{
    public sealed class DataStreamingClient : IDisposable
    {
        private TcpClient connectedTcpClient;
        private readonly object connectedTcpClientLock = new object();

        private readonly IPEndPoint remoteEndPoint;
        
        private CancellationTokenSource cancelConnectTokenSource;
        private CancellationToken cancelConnectToken;

        private bool isConnectedAlreadyCalled;
        private bool isDisposed;
        private readonly object disposingLock = new object();

        private readonly byte[] receiveBuffer = new byte[1024];

        private bool isConnected;
        private readonly object isConnectedLock = new object();

        public bool IsConnected
        {
            get
            {
                lock (isConnectedLock)
                {
                    return isConnected;
                }
            }
            private set
            {
                lock (isConnectedLock)
                {
                    isConnected = value;
                }
            }
        }
        
        public event EventHandler<ConnectChangedEventArgs> ConnectChanged;
        public event EventHandler<DataReceivedEventArgs> DataReceived;

        public DataStreamingClient(IPEndPoint remoteEndPoint)
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
            var connectTask = Task.Run(() => Connect(), cancelConnectToken)
                .ContinueWith(t => BeginReceive(), cancelConnectToken);
            return connectTask;
        }

        public bool Send(byte[] dataToWrite)
        {
            var isSuccessful = false;
            if (IsConnected)
            {
                try
                {
                    connectedTcpClient.GetStream().Write(dataToWrite, 0, dataToWrite.Length);
                    isSuccessful = true;

                }
                catch (ObjectDisposedException)
                {
                    // This can only happen when the client is purposely disposed.
                }
                catch (Exception ex) when (ex is IOException || ex is InvalidOperationException)
                {
                    // TODO: proper mechanism to recover.
                    Logger.Log("Write action failed, the client is not connected anymore to the remote host.", LogLevel.Warning);
                    Close();
                }
            }
            return isSuccessful;
        }

        // TODO: only creator of object should call Dispose.
        public void Close()
        {
            ThrowIfDisposed();
            cancelConnectTokenSource.Cancel();
            Dispose();
        }

        private void BeginReceive()
        {
            if (IsConnected)
            {
                try
                {
                    var stream = connectedTcpClient.GetStream();
                    stream.BeginRead(receiveBuffer, 0, receiveBuffer.Length, ReceiveAsyncCallback, stream);
                }
                catch (IOException)
                {
                    Logger.Log($"Exception when trying to read the socket. Socket is closed. Remote endpoint: {remoteEndPoint}", LogLevel.Warning);
                    Close();
                }
            }
        }

        private void ReceiveAsyncCallback(IAsyncResult ar)
        {
            try
            {
                var stream = (NetworkStream)ar.AsyncState;
                var receivedBytes = stream.EndRead(ar);

                if (receivedBytes == 0)
                {
                    Close();
                }
                else
                {
                    DataReceived?.Invoke(this, new DataReceivedEventArgs(receiveBuffer.Take(receivedBytes).ToArray()));
                    BeginReceive();
                }
            }
            catch (IOException)
            {
                Close();
            }
            catch (ObjectDisposedException)
            {
            }
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

                IsConnected = true;
                ConnectChanged?.Invoke(this, new ConnectChangedEventArgs(true));
            }
            catch (Exception ex) when (ex is OperationCanceledException || ex is AggregateException)
            {
                IsConnected = false;
                ConnectChanged?.Invoke(this, new ConnectChangedEventArgs(false));
            }
        }

        private void Disconnect()
        {
            lock (connectedTcpClientLock)
            {
                connectedTcpClient?.Close();
                connectedTcpClient = null;
                IsConnected = false;
                ConnectChanged?.Invoke(this, new ConnectChangedEventArgs(false));
            }
        }

        private void ThrowIfDisposed()
        {
            lock (disposingLock)
            {
                if (isDisposed)
                {
                    throw new ObjectDisposedException(nameof(DataStreamingClient));
                }
            }
        }
    }
}
