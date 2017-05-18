using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using TcpUtility.CustomEventArgs;

namespace TcpUtility
{
    public class AcceptedTcpClient
    {
        private TcpClient tcpClient;

        private byte[] buffer = new byte[1024];

        public IPEndPoint RemoteEndPoint => (IPEndPoint)tcpClient.Client.RemoteEndPoint;

        public event EventHandler<DataReceivedEventArgs> DataReceived;

        public AcceptedTcpClient(TcpClient client)
        {
            tcpClient = client;
            StartReceiving();
        }

        public bool Send(byte[] data)
        {
            bool isSuccessful = false;

            try
            {
                tcpClient.GetStream().Write(data, 0, data.Length);
                isSuccessful = true;
            }
            catch (Exception ex) when (ex is IOException || ex is ObjectDisposedException)
            {
                Logger.Log($"Failed to write data to the network. {ex.Message}", LogLevel.Warning);
            }

            return isSuccessful;
        }

        private void StartReceiving()
        {
            var state = new StateObject(tcpClient.Client, buffer);
            // TODO: handle exceptions
            tcpClient.Client.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(ReadCallback), state);
        }

        private void ReadCallback(IAsyncResult ar)
        {
            var state = ar.AsyncState as StateObject;

            var socket = state.Socket;

            var bytesReceived = socket.EndReceive(ar);

            if(bytesReceived > 0)
            {
                var localBuffer = new byte[bytesReceived];
                Array.Copy(state.Buffer, localBuffer, bytesReceived);

                DataReceived?.Invoke(this, new DataReceivedEventArgs(localBuffer));

                // TODO: handle exceptions
                socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(ReadCallback), state);
            }
            else
            {
                socket.Close();
            }
        }

        private class StateObject
        {
            public Socket Socket { get; }
            public byte[] Buffer { get; }

            public StateObject(Socket socket, byte[] buffer)
            {
                Socket = socket;
                Buffer = buffer;
            }
        }
    }
}
