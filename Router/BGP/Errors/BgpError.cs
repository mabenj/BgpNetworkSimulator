namespace Router {
	using System;

	public abstract class BgpError: Exception {
		public abstract byte GetErrorCode();

		public abstract byte GetErrorSubCode();
	}
}