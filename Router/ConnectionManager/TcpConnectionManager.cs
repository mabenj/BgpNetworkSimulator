namespace Router {
	using System;
	using System.Net;
	using System.Net.Sockets;

	using Common;

	public class TcpConnectionManager {
		private readonly TopologyConfiguration topologyConfig;

		public TcpConnectionManager() {
			this.topologyConfig = TopologyConfiguration.CreateFromFile();
		}

		public event EventHandler<ConnectionReceivedEventArgs> ConnectionReceived;

		public async void StartTcpListener(int port) {
			var server = new TcpListener(new IPEndPoint(IPAddress.Loopback, port));
			try {
				server.Start();

				while(true /* TODO: add cancellation token support ? */) {
					Logger.Info($"Waiting for connections on port {port}...");

					using var handler = await server.AcceptTcpClientAsync();
					await using var stream = handler.GetStream();
					//Logger.Info("Incoming connection");

					var buffer = new byte[1024];
					var received = await stream.ReadAsync(buffer);
					//var stringData = Encoding.UTF8.GetString(buffer, 0, received);
					////Logger.Info($"Received message: '{stringData}'");

					//var responseString = $"Message received ({stringData})";
					//var responseBytes = Encoding.UTF8.GetBytes(responseString);
					//await stream.WriteAsync(responseBytes);
					//Logger.Info($"Sent response: '{responseString}'");

					this.OnConnectionReceived(
						new ConnectionReceivedEventArgs() {
							BytesRead = received, Data = buffer
						});
				}
			} catch(Exception e) {
				throw new Exception($"TCP Listener error: {e.Message}", e);
			} finally {
				server.Stop();
			}
		}

		protected virtual void OnConnectionReceived(ConnectionReceivedEventArgs e) {
			var handler = this.ConnectionReceived;
			handler?.Invoke(this, e);
		}
	}
}