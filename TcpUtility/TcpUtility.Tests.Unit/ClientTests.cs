﻿using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace TcpUtility.Tests.Unit
{
    [TestFixture]
    public class ClientTests
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
            using (var client = new Client(new IPEndPoint(IPAddress.Loopback, LISTENING_PORT)))
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
            using(var client = new Client(new IPEndPoint(IPAddress.Loopback, LISTENING_PORT + 1)))
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
        public void Close_ClientConnected_ClientDisonnected()
        {
            // Arrange
            int timesConnectedChangedRaised = 0;
            var actualConnectedStates = new List<bool>();

            var client = new Client(new IPEndPoint(IPAddress.Loopback, LISTENING_PORT));
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
            using (var client = new Client(new IPEndPoint(IPAddress.Loopback, LISTENING_PORT)))
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
            using (var client = new Client(new IPEndPoint(IPAddress.Loopback, LISTENING_PORT)))
            {
                // Assert
                Assert.DoesNotThrow(() =>
                {
                    client.Dispose();
                    client.Dispose();
                });
            }
        }
    }
}
