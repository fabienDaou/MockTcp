using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TcpUtility;
using TcpUtility.CustomEventArgs;

namespace TestClientApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Task.Factory.StartNew(() =>
            {
                var localEndPoint = new IPEndPoint(IPAddress.Loopback, 10002);
                var client = new Client(localEndPoint);
                client.ConnectChanged += Client_ConnectChanged;

                var connectTask = client.ConnectAsync();
                connectTask.Wait();
                Logger.Log($"Finish connection task with {localEndPoint}", LogLevel.Info);
            });

            var manualResetEvent = new ManualResetEvent(false);
            manualResetEvent.WaitOne();
        }

        private static void Client_ConnectChanged(object sender, ConnectChangedEventArgs e)
        {
            Logger.Log($"Connection succesful: {e.Connected}", LogLevel.Info);
        }
    }
}
