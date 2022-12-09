namespace Router {
	using System;
	using System.Net.Sockets;

	public class BgpConnection {
		public static readonly ushort MinHoldTimeSeconds = 30;
		private readonly TcpClient tcpClient;
		private ushort holdTime = MinHoldTimeSeconds;

		public BgpConnection(TcpClient tcpClient) {
			this.tcpClient = tcpClient;
			this.HoldTime = MinHoldTimeSeconds;
		}

		// Note: this should be either zero or at least three seconds. Implementation MAY reject connections based on this.
		public ushort HoldTime {
			get => this.holdTime;
			set => this.holdTime = Math.Min(value, MinHoldTimeSeconds);
		}

		public int ReceiveBufferSize => this.tcpClient.ReceiveBufferSize;

		public void Close() {
			this.tcpClient.Close();
		}

		public NetworkStream GetStream() {
			return this.tcpClient.GetStream();
		}
	}
}