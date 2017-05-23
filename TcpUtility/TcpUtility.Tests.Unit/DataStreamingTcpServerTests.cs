using Moq;
using NUnit.Framework;
using Smocks;
using System;
using System.Reflection;

namespace TcpUtility.Tests.Unit
{
    [TestFixture]
    public class DataStreamingTcpServerTests
    {
        [Test]
        public void Ctor_ConstructorCalled_TcpServerInstanciated()
        {
            Smock.Run(context =>
            {
                // Arrange
                var tcpListenerMock = new Mock<ITcpListener>();

                
                context.Setup(() => MultiSourceTcpListener.Create(It.IsAny<int>()))
                    .Returns(tcpListenerMock.Object)
                    .Verifiable();
                
                // Act
                var dataStreamingTcpServer = new DataStreamingTcpServer(0);

                // Assert
                context.Verify();
            });
        }

        [Test]
        public void Start_StartCalled_DataStreamingTcpServerSubscribedToAcceptedClient()
        {
            Smock.Run(context =>
            {
                // Arrange
                var tcpListenerMock = new Mock<ITcpListener>();


                context.Setup(() => MultiSourceTcpListener.Create(It.IsAny<int>()))
                    .Returns(tcpListenerMock.Object);

                // Act
                var dataStreamingTcpServer = new DataStreamingTcpServer(0);
                dataStreamingTcpServer.Start();

                // Assert
                var acceptedClientHandler = GetAcceptedClientHandlerByReflection(dataStreamingTcpServer);

                Assert.IsNotNull(acceptedClientHandler);
                Assert.AreEqual(1, acceptedClientHandler.GetInvocationList().Length);
            });
        }

        [Test]
        public void Start_CallTwice_DoesNotThrow()
        {
            // Arrange
            var dataStreamingTcpServer = new DataStreamingTcpServer(0);

            // Assert
            Assert.DoesNotThrow(() =>
            {
                dataStreamingTcpServer.Start();
                dataStreamingTcpServer.Start();
            });
        }

        [Test]
        public void Stop_StopCalled_DataStreamingTcpServerUnsubscribedFromAcceptedClient()
        {
            Smock.Run(context =>
            {
                // Arrange
                var tcpListenerMock = new Mock<ITcpListener>();

                context.Setup(() => MultiSourceTcpListener.Create(It.IsAny<int>()))
                    .Returns(tcpListenerMock.Object);

                var dataStreamingTcpServer = new DataStreamingTcpServer(0);
                dataStreamingTcpServer.Start();

                // Act
                dataStreamingTcpServer.Stop();

                // Assert
                var acceptedClientHandler = GetAcceptedClientHandlerByReflection(dataStreamingTcpServer);

                Assert.IsNull(acceptedClientHandler);
            });
        }

        [Test]
        public void Stop_CallTwice_DoesNotThrow()
        {
            // Arrange
            var dataStreamingTcpServer = new DataStreamingTcpServer(0);
            dataStreamingTcpServer.Start();

            // Assert
            Assert.DoesNotThrow(() =>
            {
                dataStreamingTcpServer.Stop();
                dataStreamingTcpServer.Stop();
            });
        }

        private Delegate GetAcceptedClientHandlerByReflection(DataStreamingTcpServer dataStreamingTcpServer)
        {
            TcpServer tcpServer = GetTcpServerInstanceByReflection(dataStreamingTcpServer);
            return typeof(TcpServer).GetField(nameof(tcpServer.AcceptedClient), BindingFlags.NonPublic | BindingFlags.Instance).GetValue(tcpServer) as Delegate;
        }

        private TcpServer GetTcpServerInstanceByReflection(DataStreamingTcpServer dataStreamingTcpServer)
        {
            return (TcpServer)typeof(DataStreamingTcpServer).GetField("tcpServer", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(dataStreamingTcpServer);
        }
    }
}
