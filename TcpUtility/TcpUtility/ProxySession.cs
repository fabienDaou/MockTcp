using TcpUtility.CustomEventArgs;

namespace TcpUtility
{
    public class ProxySession
    {
        public AcceptedTcpClient SourceClient { get; }
        public DataStreamingClient DestinationClient { get; }

        public ProxySession(AcceptedTcpClient sourceClient, DataStreamingClient destinationClient)
        {
            SourceClient = sourceClient;
            DestinationClient = destinationClient;
            DestinationClient.DataReceived += DestinationClient_DataReceived;
        }

        public void Close()
        {
            // Closing the AcceptedTcpClient is the responsibility of the DataStreamingTcpServer.
            DestinationClient.DataReceived -= DestinationClient_DataReceived;
            DestinationClient.Close();
        }

        private void DestinationClient_DataReceived(object sender, DataReceivedEventArgs e)
        {
            SourceClient.Send(e.Data);
        }
    }
}
