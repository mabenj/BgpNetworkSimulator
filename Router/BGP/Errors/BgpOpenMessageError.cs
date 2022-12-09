namespace Router {
	public class BgpOpenMessageError: BgpError {
		private readonly BgpOpenMessageErrorSubCode subCode;

		public BgpOpenMessageError(BgpOpenMessageErrorSubCode subCode = 0x00) {
			this.subCode = subCode;
		}

		public override byte GetErrorCode() {
			return 0x02;
		}

		public override byte GetErrorSubCode() {
			return (byte) this.subCode;
		}
	}

	public enum BgpOpenMessageErrorSubCode {
		UnsupportedVersionNumber = 0x01,
		BadPeerAs = 0x02,
		BadBgpIdentifier = 0x03,
		UnsupportedOptionalParameter = 0x04,

		/* 0x05 DEPRECATED */
		UnacceptableHoldTime = 0x06,
	}
}