namespace Router {
	using System;

	public class BgpMessageReceivedEventArgs<T> {
		public T BgpMessage {
			get;
			init;
		}

		public Guid SenderId {
			get;
			init;
		}
	}
}