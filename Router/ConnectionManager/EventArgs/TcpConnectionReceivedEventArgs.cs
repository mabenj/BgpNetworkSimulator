namespace Router {
	using System;

	public class TcpConnectionReceivedEventArgs {
		public Guid ClientId {
			get;
			init;
		}
	}
}