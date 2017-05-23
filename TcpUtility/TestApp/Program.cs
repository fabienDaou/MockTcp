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
                var server = new DataStreamingTcpServer(new TcpServer(10002));
                server.DataReceived += DataStreamingTcpServer_DataReceived;

                server.Start();
            });

            var manualResetEvent = new ManualResetEvent(false);
            manualResetEvent.WaitOne();
        }

        private static void DataStreamingTcpServer_DataReceived(object source, DataReceivedEventArgs args)
        {
            var acceptedTcpClient = source as AcceptedTcpClient;
            Logger.Log($"DataReceived from {acceptedTcpClient.RemoteEndPoint}:{Encoding.Default.GetString(args.Data)}", LogLevel.Info);
        }
    }
}
