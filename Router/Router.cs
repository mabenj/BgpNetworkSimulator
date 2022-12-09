namespace Router {
	using System.Net;

	using Common;

	public class Router {
		private readonly int asNumber;
		private readonly IPAddress bgpIdentifier;
		private readonly ConnectionManager connectionManager;
		private readonly int holdTime;
		private readonly int port;

		public Router(string id, int port, int asNumber, int holdTime) {
			this.port = port;
			this.connectionManager = new ConnectionManager(id);
			this.asNumber = asNumber;
			this.holdTime = holdTime;
			this.bgpIdentifier = IPAddress.Loopback; // TODO: resolve this somehow ?
		}

		public void Start() {
			this.connectionManager.TcpConnectionStarted += this.HandleTcpConnectionStarted;
			this.connectionManager.BgpOpenReceived += this.HandleBgpOpenReceived;
			this.connectionManager.BgpUpdateReceived += this.HandleBgpUpdateReceived;
			this.connectionManager.BgpNotificationReceived += this.HandleBgpNotificationReceived;
			this.connectionManager.BgpKeepAliveReceived += this.HandleBgpKeepAliveReceived;
			this.connectionManager.StartTcpListener(this.port);
			this.connectionManager.DiscoverNeighbors();
		}

		private void HandleBgpKeepAliveReceived(object sender, BgpMessageReceivedEventArgs<BgpKeepAliveMessage> e) {
			Logger.Info($"KEEPALIVE received from '{e.SenderId}'");
		}

		private void HandleBgpNotificationReceived(object sender, BgpMessageReceivedEventArgs<BgpNotificationMessage> e) {
			Logger.Info($"NOTIFICATION received from '{e.SenderId}'");
		}

		private void HandleBgpOpenReceived(object sender, BgpMessageReceivedEventArgs<BgpOpenMessage> e) {
			Logger.Info($"OPEN received from '{e.SenderId}'");
			this.connectionManager.SetConnectionHoldTime(e.BgpMessage.HoldTime, e.SenderId);
			this.connectionManager.StartKeepAliveInterval(e.SenderId);
		}

		private void HandleBgpUpdateReceived(object sender, BgpMessageReceivedEventArgs<BgpUpdateMessage> e) {
			Logger.Info($"UPDATE received from '{e.SenderId}'");
		}

		private void HandleTcpConnectionStarted(object sender, TcpConnectionReceivedEventArgs e) {
			Logger.Info($"TCP connection formed with '{e.ClientId}'. Replying with OPEN message.");
			// After the TCP connection is formed, both parties send the other one an OPEN message.
			// Here we, the receiver, send the sender an OPEN message.
			var openMessage = new BgpOpenMessage((ushort) this.asNumber, (ushort) this.holdTime, this.bgpIdentifier);
			this.connectionManager.SendMessage(openMessage, e.ClientId);
		}
	}
}