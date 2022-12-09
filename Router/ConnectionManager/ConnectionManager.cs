namespace Router {
	using System;
	using System.Collections.Concurrent;
	using System.Collections.Generic;
	using System.Net;
	using System.Net.Sockets;
	using System.Threading;

	using Common;

	using Timer = System.Timers.Timer;

	public class ConnectionManager {
		private const int SecondsToMilliseconds = 1000;
		private readonly ConcurrentDictionary<Guid, BgpConnection> connections = new();
		private readonly string routerId;
		private readonly ConcurrentDictionary<Guid, Timer> timersByConnectionId = new();

		public ConnectionManager(string routerId) {
			this.routerId = routerId;
		}

		public event EventHandler<BgpMessageReceivedEventArgs<BgpKeepAliveMessage>> BgpKeepAliveReceived;
		public event EventHandler<BgpMessageReceivedEventArgs<BgpNotificationMessage>> BgpNotificationReceived;
		public event EventHandler<BgpMessageReceivedEventArgs<BgpOpenMessage>> BgpOpenReceived;
		public event EventHandler<BgpMessageReceivedEventArgs<BgpUpdateMessage>> BgpUpdateReceived;

		public event EventHandler<TcpConnectionReceivedEventArgs> TcpConnectionStarted;

		/// <summary>
		/// Attempts to form a TCP connection with each of the neighbors.
		/// After a successful connection, a client handler thread takes control of the TCP connection.
		/// </summary>
		public async void DiscoverNeighbors() {
			foreach(var neighbor in TopologyManager.GetNeighbors(this.routerId)) {
				var tcpClient = new TcpClient();
				try {
					// because this is a simulator and the routers are all running on the same machine, we will use loopback as the IP address
					await tcpClient.ConnectAsync(IPAddress.Loopback, neighbor.Port);

					var thread = new Thread(this.ClientHandler) { IsBackground = true };
					thread.Start(new BgpConnection(tcpClient));
				} catch {
					Logger.Error($"Could not reach neighbor '{neighbor.Address}:{neighbor.Port}'");
					tcpClient.Close();
				}
			}
		}

		public void SetConnectionHoldTime(ushort holdTime, Guid connectionId) {
			var connection = this.GetConnection(connectionId);
			connection.HoldTime = holdTime;
		}

		public void StartKeepAliveInterval(Guid connectionId) {
			var connection = this.GetConnection(connectionId);
			var timer = new Timer(connection.HoldTime * SecondsToMilliseconds);
			timer.Elapsed += (_, _) => this.SendMessage(new BgpKeepAliveMessage(), connectionId);
			this.timersByConnectionId[connectionId] = timer;
			timer.AutoReset = true;
			timer.Enabled = true;
		}

		/// <summary>
		/// Starts a TCP server that listens for incoming connections.
		/// After a successful connection, a client handler thread takes control of the TCP connection.
		/// </summary>
		/// <param name="port">port to listen for connections</param>
		public async void StartTcpListener(int port) {
			var server = new TcpListener(new IPEndPoint(IPAddress.Loopback, port));
			try {
				server.Start();
				Logger.Info($"Waiting for connections on port {port}...");

				while(true /* TODO: add cancellation token support ? */) {
					var client = await server.AcceptTcpClientAsync();

					var thread = new Thread(this.ClientHandler) { IsBackground = true };
					thread.Start(new BgpConnection(client));
				}
			} catch(Exception e) {
				throw new Exception($"TCP Listener error: {e.Message}", e);
			} finally {
				server.Stop();
			}
		}

		internal async void SendMessage(IBgpMessage bgpMessage, Guid receiverId) {
			var connection = this.GetConnection(receiverId);
			try {
				var stream = connection.GetStream();
				await stream.WriteAsync(BgpMessageSerializer.Serialize(bgpMessage));
			} catch(Exception error) {
				Logger.Error($"Could not send BGP message to '{receiverId}'", error);
			}
		}

		private async void ClientHandler(object client) {
			var clientConnection = (BgpConnection) client;
			var connectionId = Guid.NewGuid();
			this.connections[connectionId] = clientConnection;
			var stream = clientConnection.GetStream();

			this.TcpConnectionStarted?.Invoke(this, new TcpConnectionReceivedEventArgs() { ClientId = connectionId });

			var connected = true;
			while(connected) {
				try {
					Thread.Sleep(10);

					var buffer = new byte[clientConnection.ReceiveBufferSize];
					var received = await stream.ReadAsync(buffer);

					if(received == 0) {
						return;
					}

					var bgpMessage = BgpMessageSerializer.Deserialize(buffer);
					switch(bgpMessage) {
					case BgpOpenMessage openMessage:
						this.BgpOpenReceived?.Invoke(
							this,
							new BgpMessageReceivedEventArgs<BgpOpenMessage>() {
								BgpMessage = openMessage,
								SenderId = connectionId
							});
						break;
					case BgpUpdateMessage updateMessage:
						this.BgpUpdateReceived?.Invoke(
							this,
							new BgpMessageReceivedEventArgs<BgpUpdateMessage>() {
								BgpMessage = updateMessage,
								SenderId = connectionId
							});
						break;
					case BgpNotificationMessage notificationMessage:
						this.BgpNotificationReceived?.Invoke(
							this,
							new BgpMessageReceivedEventArgs<BgpNotificationMessage>() {
								BgpMessage = notificationMessage,
								SenderId = connectionId
							});
						break;
					case BgpKeepAliveMessage keepAliveMessage:
						this.BgpKeepAliveReceived?.Invoke(
							this,
							new BgpMessageReceivedEventArgs<BgpKeepAliveMessage>() {
								BgpMessage = keepAliveMessage,
								SenderId = connectionId
							});
						break;
					}
				} catch(Exception e) {
					connected = false;
					Logger.Error("Error in client handler", e);
				}
			}

			Logger.Info($"Connection '{connectionId}' disconnected");
			this.CloseConnection(connectionId);
		}

		private void CloseConnection(Guid connectionId) {
			try {
				var connection = this.GetConnection(connectionId);
				connection.Close();
				this.connections.Remove(connectionId, out _);

				var timer = this.GetTimer(connectionId);
				timer.Stop();
				timer.Dispose();
				this.timersByConnectionId.Remove(connectionId, out _);
			} catch {
				// do nothing
			}
		}

		private BgpConnection GetConnection(Guid connectionId) {
			if(!this.connections.TryGetValue(connectionId, out var connection)) {
				throw new KeyNotFoundException($"Could not resolve BGP connection '{connectionId}'");
			}
			return connection;
		}

		private Timer GetTimer(Guid connectionId) {
			if(!this.timersByConnectionId.TryGetValue(connectionId, out var timer)) {
				throw new KeyNotFoundException($"Could not resolve BGP connection timer '{connectionId}'");
			}
			return timer;
		}
	}
}