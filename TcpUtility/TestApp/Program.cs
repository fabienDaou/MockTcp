using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TcpUtility;
using TcpUtility.CustomEventArgs;

namespace TestApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Task.Factory.StartNew(() =>
            {
                var server = new TcpServer(10002);
                server.AcceptedClient += Server_AcceptedClient;

                server.Start();
            });

            var manualResetEvent = new ManualResetEvent(false);
            manualResetEvent.WaitOne();
        }

        private static void Server_AcceptedClient(object source, AcceptedClientEventArgs args)
        {
            Logger.Log($"New client accepted.{args.TcpClient.RemoteEndPoint}" , LogLevel.Info);
            args.TcpClient.DataReceived += AcceptedTcpClient_DataReceived;
        }

        private static void AcceptedTcpClient_DataReceived(object source, DataReceivedEventArgs args)
        {
            var acceptedTcpClient = source as AcceptedTcpClient;
            Logger.Log($"DataReceived from {acceptedTcpClient.RemoteEndPoint}:{Encoding.Default.GetString(args.Data)}", LogLevel.Info);
        }
    }
}
