using System;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace TcpUtility
{
    public class AcceptedTcpClient
    {
        private TcpClient tcpClient;

        public IPEndPoint RemoteEndPoint => (IPEndPoint)tcpClient.Client.RemoteEndPoint;

        public AcceptedTcpClient(TcpClient client)
        {
            tcpClient = client;
        }

        public int Receive(byte[] buffer)
        {
            try
            {
                return tcpClient.Client?.Receive(buffer) ?? 0;
            }
            catch (SocketException)
            {
                return 0;
            }
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

        public void Close()
        {
            tcpClient.Close();
        }
    }
}
