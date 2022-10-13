namespace Router {
	using System;

	public class ConnectionReceivedEventArgs: EventArgs {
		public int BytesRead {
			get;
			init;
		}

		public byte[] Data {
			get;
			init;
		}
	}
}