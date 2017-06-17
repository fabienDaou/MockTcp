using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace TcpUtility.Tests.Unit
{
    [TestFixture]
    public class DataStreamingClientTests
    {
        private const int LISTENING_PORT = 3000;
        private TcpListener tcpListener;

        [SetUp]
        public void SetUp()
        {
            // TODO: dynamically choose a port to avoid collision
            tcpListener = new TcpListener(new IPEndPoint(IPAddress.Loopback, LISTENING_PORT));
            tcpListener.Start();
        }

        [TearDown]
        public void TearDown()
        {
            tcpListener.Stop();
        }

        [Test]
        [Category("Integration")]
        public void ConnectAsync_TcpListenerAvailable_ClientConnects()
        {
            using (var client = new DataStreamingClient(new IPEndPoint(IPAddress.Loopback, LISTENING_PORT)))
            {
                // Arrange
                int timesConnectedChangedRaised = 0;
                var isConnected = false;

                client.ConnectChanged += (sender, args) =>
                {
                    timesConnectedChangedRaised++;
                    isConnected = args.Connected;
                };

                // Act
                var connectionTask = client.ConnectAsync();
                connectionTask.Wait(TimeSpan.FromSeconds(1));

                // Assert
                Assert.IsTrue(isConnected);
                Assert.AreEqual(1, timesConnectedChangedRaised);
            }
        }

        [Test]
        [Category("Integration")]
        public void ConnectAsync_TcpListenerNotAvailable_ClientDoesNotConnect()
        {
            using(var client = new DataStreamingClient(new IPEndPoint(IPAddress.Loopback, LISTENING_PORT + 1)))
            {
                // Arrange
                int timesConnectedChangedRaised = 0;
                var isConnected = false;


                client.ConnectChanged += (sender, args) =>
                {
                    timesConnectedChangedRaised++;
                    isConnected = args.Connected;
                };

                // Act
                var connectionTask = client.ConnectAsync();
                connectionTask.Wait(TimeSpan.FromSeconds(2));

                // Assert
                Assert.IsFalse(isConnected);
                Assert.AreEqual(1, timesConnectedChangedRaised);
            }
        }

        [Test]
        [Category("Integration")]
        public void Close_ClientConnected_ClientDisconnected()
        {
            // Arrange
            int timesConnectedChangedRaised = 0;
            var actualConnectedStates = new List<bool>();

            var client = new DataStreamingClient(new IPEndPoint(IPAddress.Loopback, LISTENING_PORT));
            client.ConnectChanged += (sender, args) =>
            {
                timesConnectedChangedRaised++;
                actualConnectedStates.Add(args.Connected);
            };
            
            var connectionTask = client.ConnectAsync();
            connectionTask.Wait(TimeSpan.FromSeconds(1));

            // Act
            client.Close();

            // Assert
            var expectedConnectedStates = new List<bool>
            {
                true,
                false
            };
            CollectionAssert.AreEquivalent(expectedConnectedStates, actualConnectedStates);
            Assert.AreEqual(2, timesConnectedChangedRaised);
        }

        [Test]
        public void Close_CallTwice_ThrowObjectDisposedException()
        {
            // Arrange
            using (var client = new DataStreamingClient(new IPEndPoint(IPAddress.Loopback, LISTENING_PORT)))
            {
                // Assert
                Assert.Throws<ObjectDisposedException>(() =>
                {
                    client.Close();
                    client.Close();
                });
            }
        }

        [Test]
        public void Dispose_CallTwice_DoesNotThrow()
        {
            // Arrange
            using (var client = new DataStreamingClient(new IPEndPoint(IPAddress.Loopback, LISTENING_PORT)))
            {
                // Assert
                Assert.DoesNotThrow(() =>
                {
                    client.Dispose();
                    client.Dispose();
                });
            }
        }

        [Test]
        [Category("Integration")]
        public void Write_TcpListenerAvailable_WriteSuccessful()
        {
            using (var client = new DataStreamingClient(new IPEndPoint(IPAddress.Loopback, LISTENING_PORT)))
            {
                // Arrange
                var dataToWrite = new byte[]
                {
                    0x00,
                    0x01,
                    0x02
                };
                var acceptTcpClientTask = tcpListener.AcceptTcpClientAsync();
                var connectionTask = client.ConnectAsync();
                connectionTask.Wait(TimeSpan.FromSeconds(1));

                var acceptedTcpClient = acceptTcpClientTask.Result;

                var buffer = new byte[1024];
                var readTask = acceptedTcpClient.GetStream().ReadAsync(buffer, 0, buffer.Length);

                // Act
                var result = client.Write(dataToWrite);
                readTask.Wait(TimeSpan.FromMilliseconds(100));
                // Assert
                Assert.IsTrue(result);
                var receivedData = new byte[readTask.Result];
                Array.Copy(buffer, receivedData, receivedData.Length);
                Assert.AreEqual(dataToWrite, receivedData);
            }
        }
    }
}
