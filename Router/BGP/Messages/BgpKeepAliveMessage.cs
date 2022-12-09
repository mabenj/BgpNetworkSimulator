namespace Router {
	using System;

	public class BgpKeepAliveMessage: IBgpMessage {
		public byte[] GetBytes() {
			// A KEEPALIVE message consists of only the message header
			return Array.Empty<byte>();
		}
	}
}