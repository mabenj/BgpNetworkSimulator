namespace Router {
	using System;
	using System.Net;
	using System.Net.Sockets;
	using System.Text;

	using Common;

	public class Router {
		private readonly string id;
		private readonly int port;

		public Router(string id, int port) {
			this.id = id;
			this.port = port;
		}

		public void Start() {
			this.LaunchTcpServer();
		}

		private async void LaunchTcpServer() {
			var ipEndPoint = new IPEndPoint(IPAddress.Loopback, this.port);
			var server = new TcpListener(ipEndPoint);
			try {
				server.Start();

				while(true /* TODO: add cancellation token support ? */) {
					Logger.Info($"Waiting for connections on port {this.port}...");

					using TcpClient handler = await server.AcceptTcpClientAsync();
					await using NetworkStream stream = handler.GetStream();
					Logger.Info("Incoming connection");

					var buffer = new byte[1024];
					int received = await stream.ReadAsync(buffer);
					var stringData = Encoding.UTF8.GetString(buffer, 0, received);
					Logger.Info($"Received message: '{stringData}'");

					var responseString = $"Message received ({stringData})";
					var responseBytes = Encoding.UTF8.GetBytes(responseString);
					await stream.WriteAsync(responseBytes);
					Logger.Info($"Sent response: '{responseString}'");
				}
			} catch(Exception e) {
				Logger.Error($"Exception {e}", e);
			} finally {
				server.Stop();
			}
		}
	}
}