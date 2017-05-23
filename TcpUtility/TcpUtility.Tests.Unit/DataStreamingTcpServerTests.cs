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
                TcpServer tcpServer = GetTcpServerInstanceByReflection(dataStreamingTcpServer);
                var acceptedClientHandler = typeof(TcpServer).GetField(nameof(tcpServer.AcceptedClient), BindingFlags.NonPublic | BindingFlags.Instance).GetValue(tcpServer) as Delegate;

                Assert.IsNotNull(acceptedClientHandler);
                Assert.AreEqual(1, acceptedClientHandler.GetInvocationList().Length);
                context.Verify();
            });
        }

        private TcpServer GetTcpServerInstanceByReflection(DataStreamingTcpServer dataStreamingTcpServer)
        {
            return (TcpServer)typeof(DataStreamingTcpServer).GetField("tcpServer", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(dataStreamingTcpServer);
        }
    }
}
