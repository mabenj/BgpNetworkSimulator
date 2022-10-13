namespace Router {
	using Common;

	public class Router {
		private readonly TcpConnectionManager connectionManager;
		private readonly string id;
		private readonly int port;

		public Router(string id, int port) {
			this.id = id;
			this.port = port;
			this.connectionManager = new TcpConnectionManager();
		}

		public void Start() {
			this.connectionManager.ConnectionReceived += this.HandleConnectionReceived;
			this.connectionManager.StartTcpListener(this.port);
		}

		private void HandleConnectionReceived(object sender, ConnectionReceivedEventArgs e) {
			Logger.Info($"Connection received: {e.Data}");
		}
	}
}