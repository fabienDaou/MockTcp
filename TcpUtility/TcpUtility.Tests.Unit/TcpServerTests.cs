using Moq;
using NUnit.Framework;
using Smocks;
using System;
using System.Net;
using System.Net.Sockets;
using System.Reflection;

namespace TcpUtility.Tests.Unit
{
    [TestFixture]
    public class TcpServerTests
    {
        [TestCase(1000)]
        [TestCase(10000)]
        public void Ctor_ConstructorCalled_TcpListenerIsInstanciatedWithTheRightValues(int listeningPort)
        {
            // Act
            var tcpServer = new TcpServer(listeningPort);
            var actualTcpListener = GetTcpListenerInstanceByReflection(tcpServer);

            // Assert
            Assert.IsNotNull(actualTcpListener);
            Assert.IsInstanceOf(typeof(MultiSourceTcpListener), actualTcpListener);
            Assert.AreEqual(new IPEndPoint(IPAddress.Any, listeningPort), actualTcpListener.LocalEndPoint);
        }

        [Test]
        public void Start_TcpListenerIsNotStartedAndIsNotListening_TcpListenerIsStartedAndListening()
        {
            Smock.Run(context =>
            {
                // Arrange
                var asyncResultMock = new Mock<IAsyncResult>();
                var tcpListenerMock = new Mock<ITcpListener>();
                tcpListenerMock.Setup(mock => mock.Start())
                    .Verifiable();
                tcpListenerMock.Setup(mock => mock.BeginAcceptTcpClient(It.IsAny<AsyncCallback>(), null))
                    .Returns(asyncResultMock.Object)
                    .Verifiable();

                context.Setup(() => MultiSourceTcpListener.Create(It.IsAny<int>()))
                    .Returns(tcpListenerMock.Object);
                
                var tcpServer = new TcpServer(1000);
                
                // Act
                tcpServer.Start();

                // Assert
                tcpListenerMock.Verify();
            });
        }

        [Test]
        public void Start_CallbackCalled_TcpListenerKeepAcceptingIncomingConnection()
        {
            Smock.Run(context =>
            {
                // Arrange
                AsyncCallback callback = null;
                IAsyncResult ar = new Mock<IAsyncResult>().Object;

                var tcpListenerMock = new Mock<ITcpListener>();
                tcpListenerMock
                    .Setup(mock => mock.BeginAcceptTcpClient(It.IsAny<AsyncCallback>(), null))
                    .Callback((AsyncCallback cb, object state) => callback = cb)
                    .Returns(ar);

                context.Setup(() => MultiSourceTcpListener.Create(It.IsAny<int>()))
                    .Returns(tcpListenerMock.Object);

                // Act
                var tcpServer = new TcpServer(0);
                tcpServer.Start();
                callback(ar);

                // Assert
                tcpListenerMock.Verify(mock => mock.BeginAcceptTcpClient(It.IsAny<AsyncCallback>(), null), Times.Exactly(2));
            });
        }

        [Test]
        public void Start_CallbackCalled_AcceptedClientRaisedWithTcpClient()
        {
            Smock.Run(context =>
            {
                // Arrange
                AsyncCallback callback = null;
                var expectedTcpClient = new TcpClient();
                var asyncResultMock = new Mock<IAsyncResult>();
                asyncResultMock.SetupGet(mock => mock.AsyncState).Returns(expectedTcpClient);
                IAsyncResult ar = new Mock<IAsyncResult>().Object;

                var tcpListenerMock = new Mock<ITcpListener>();
                tcpListenerMock
                    .Setup(mock => mock.BeginAcceptTcpClient(It.IsAny<AsyncCallback>(), null))
                    .Callback((AsyncCallback cb, object state) => callback = cb)
                    .Returns(asyncResultMock.Object);
                tcpListenerMock
                    .Setup(mock => mock.EndAcceptTcpClient(It.IsAny<IAsyncResult>()))
                    .Returns(expectedTcpClient);

                context.Setup(() => MultiSourceTcpListener.Create(It.IsAny<int>()))
                    .Returns(tcpListenerMock.Object);

                AcceptedTcpClient actualTcpClient = null;

                // Act
                var tcpServer = new TcpServer(0);
                tcpServer.AcceptedClient += (sender, args) => actualTcpClient = args.TcpClient;
                tcpServer.Start();
                callback(ar);

                // Assert
                Assert.AreEqual(expectedTcpClient, actualTcpClient.TcpClient);
            });
        }

        [Test]
        public void Stop_TcpListenerIsStartedAndIsListening_TcpListenerIsStopped()
        {
            Smock.Run(context =>
            {
                // Arrange
                var tcpListenerMock = new Mock<ITcpListener>();
                tcpListenerMock.Setup(mock => mock.Stop())
                    .Verifiable();

                context.Setup(() => MultiSourceTcpListener.Create(It.IsAny<int>()))
                    .Returns(tcpListenerMock.Object);
                
                var tcpServer = new TcpServer(0);
                tcpServer.Start();

                // Act
                tcpServer.Stop();

                // Assert
                tcpListenerMock.Verify();
            });
        }

        [Test]
        public void Stop_CallbackCalled_TcpListenerStopAcceptingIncomingConnection()
        {
            Smock.Run(context =>
            {
                // Arrange
                AsyncCallback callback = null;
                IAsyncResult ar = new Mock<IAsyncResult>().Object;

                var tcpListenerMock = new Mock<ITcpListener>();
                tcpListenerMock
                    .Setup(mock => mock.BeginAcceptTcpClient(It.IsAny<AsyncCallback>(), null))
                    .Callback((AsyncCallback cb, object state) => callback = cb)
                    .Returns(ar);

                context.Setup(() => MultiSourceTcpListener.Create(It.IsAny<int>()))
                    .Returns(tcpListenerMock.Object);

                // Act
                var tcpServer = new TcpServer(0);
                tcpServer.Start();
                tcpServer.Stop();
                callback(ar);

                // Assert
                tcpListenerMock.Verify(mock => mock.BeginAcceptTcpClient(It.IsAny<AsyncCallback>(), null), Times.Once);
            });
        }

        private ITcpListener GetTcpListenerInstanceByReflection(TcpServer tcpServer)
        {
            return (ITcpListener)typeof(TcpServer).GetField("tcpListener", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(tcpServer);
        }
    }
}
