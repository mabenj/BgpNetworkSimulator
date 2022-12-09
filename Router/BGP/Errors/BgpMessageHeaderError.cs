namespace Router {
	public class BgpMessageHeaderError: BgpError {
		private readonly BgpMessageHeaderErrorSubCode subCode;

		public BgpMessageHeaderError(BgpMessageHeaderErrorSubCode subCode = 0x00) {
			this.subCode = subCode;
		}

		public override byte GetErrorCode() {
			return 0x01;
		}

		public override byte GetErrorSubCode() {
			return (byte) this.subCode;
		}
	}

	public enum BgpMessageHeaderErrorSubCode {
		ConnectionNotSynchronized = 0x01,
		BadMessageLength = 0x02,
		BadMessageType = 0x03,
	}
}