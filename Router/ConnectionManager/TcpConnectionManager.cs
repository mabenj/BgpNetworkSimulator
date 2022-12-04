using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace Router {
	using System;
	using System.Net;
	using System.Net.Sockets;

	using Common;

	public class TcpConnectionManager {
        private readonly ConcurrentBag<TcpClient> tcpClients;
        private readonly string routerId;
        private readonly ConcurrentDictionary<int, TcpClient> connections;

		public TcpConnectionManager(string routerId) {
            this.tcpClients = new ConcurrentBag<TcpClient>();
            this.routerId = routerId;
            this.connections = new ConcurrentDictionary<int, TcpClient>();
        }

		public event EventHandler<ConnectionReceivedEventArgs> ConnectionReceived;

		public event EventHandler<BgpMessageReceivedEventArgs<BgpOpenMessage>> BgpOpenReceived;
		public event EventHandler<BgpMessageReceivedEventArgs<BgpUpdateMessage>> BgpUpdateReceived;
		public event EventHandler<BgpMessageReceivedEventArgs<BgpNotificationMessage>> BgpNotificationReceived;
		public event EventHandler<BgpMessageReceivedEventArgs<BgpKeepAliveMessage>> BgpKeepAliveReceived;

        public async void StartTcpListener(int port) {
			var server = new TcpListener(new IPEndPoint(IPAddress.Loopback, port));
			try {
				server.Start();
                Logger.Info($"Waiting for connections on port {port}...");

                while (true /* TODO: add cancellation token support ? */) {
                    var client = await server.AcceptTcpClientAsync();
                    Logger.Info("Connection received");

                    var thread = new Thread(this.ClientHandler)
                    {
                        IsBackground = true
                    };
                    thread.Start(client);

					//await using var stream = handler.GetStream();
					////Logger.Info("Incoming connection");

					//var buffer = new byte[4096];
					//var received = await stream.ReadAsync(buffer);
					//var stringData = Encoding.UTF8.GetString(buffer, 0, received);
					////Logger.Info($"Received message: '{stringData}'");

					//var responseString = $"Message received ({stringData})";
					//var responseBytes = Encoding.UTF8.GetBytes(responseString);
					//await stream.WriteAsync(responseBytes);
					//Logger.Info($"Sent response: '{responseString}'");

					//this.OnConnectionReceived(
					//	new ConnectionReceivedEventArgs() {
					//		BytesRead = received, Data = buffer
					//	});
				}
			} catch(Exception e) {
				throw new Exception($"TCP Listener error: {e.Message}", e);
			} finally {
				server.Stop();
			}
		}

        private void ClientHandler(object client)
        {
            var tcpClient = (TcpClient)client;
            var clientId = tcpClient.GetHashCode();
            this.connections[clientId] = tcpClient;
            var stream = tcpClient.GetStream();
            var connected = true;
            while (connected)
            {
                try
                {
                    Thread.Sleep(10);
                    var buffer = new byte[tcpClient.ReceiveBufferSize];
                    var received = stream.Read(buffer);
                    if (received == 0)
                    {
                        continue;
                    }
                    var bgpMessage = BgpMessageSerializer.Deserialize(buffer);
                    switch (bgpMessage)
                    {
                        case BgpOpenMessage openMessage:
                            //this.connections[openMessage.Id] = tcpClient;
                            this.BgpOpenReceived?.Invoke(this, new BgpMessageReceivedEventArgs<BgpOpenMessage>()
                            {
                                BgpMessage = openMessage,
                                SenderId = clientId
                            });
                            break;
                        case BgpUpdateMessage updateMessage:
                            this.BgpUpdateReceived?.Invoke(this, new BgpMessageReceivedEventArgs<BgpUpdateMessage>()
                            {
                                BgpMessage = updateMessage,
                                SenderId = clientId
                            });
                            break;
                        case BgpNotificationMessage notificationMessage:
                            this.BgpNotificationReceived?.Invoke(this, new BgpMessageReceivedEventArgs<BgpNotificationMessage>()
                            {
                                BgpMessage = notificationMessage,
                                SenderId = clientId
                            });
                            break;
                        case BgpKeepAliveMessage keepAliveMessage:
                            this.BgpKeepAliveReceived?.Invoke(this, new BgpMessageReceivedEventArgs<BgpKeepAliveMessage>()
                            {
                                BgpMessage = keepAliveMessage,
                                SenderId = clientId
                            });
                            break;
                    }
                }
                catch (Exception e)
                {
                    connected = false;
                    Logger.Error("Error in client handler", e);
                    this.connections.Remove(clientId, out _);
                }
            }
        }

		protected virtual void OnConnectionReceived(ConnectionReceivedEventArgs e) {
			var handler = this.ConnectionReceived;
			handler?.Invoke(this, e);
		}

        public async void Broadcast(BgpOpenMessage bgpMessage)
        {
            foreach (var peer in TopologyManager.GetNeighbors(this.routerId))
            {
                try
                {

                    var tcpClient = new TcpClient();
                    await tcpClient.ConnectAsync(peer.Address, peer.Port);
                    await using var stream = tcpClient.GetStream();

                    await stream.WriteAsync(BgpMessageSerializer.Serialize(bgpMessage));
                    Logger.Info("Message sent");
                }
                catch
                {
					Logger.Error($"Could not reach neighbor '{peer.Address}:{peer.Port}'");
                }
            }
        }

		/// <summary>
		/// Attempts to form a TCP connection with each of the neighbors. After a successful connection, a BGP OPEN message is sent.
		/// </summary>
        public async void DiscoverNeighbors()
        {
            foreach (var neighbor in TopologyManager.GetNeighbors(this.routerId))
            {
                try
                {
                    var tcpClient = new TcpClient();
                    await tcpClient.ConnectAsync(neighbor.Address, neighbor.Port);
                    await using var stream = tcpClient.GetStream();

                    var openMessage = new BgpOpenMessage(1, 30, IPAddress.Loopback);
                    await stream.WriteAsync(BgpMessageSerializer.Serialize(openMessage));
					Logger.Info($"OPEN message sent to '{neighbor.Address}:{neighbor.Port}'");
                }
                catch (Exception e)
                {
					Logger.Error($"Could not reach neighbor '{neighbor.Address}:{neighbor.Port}'", e);
                }
            }
        }
    }
}