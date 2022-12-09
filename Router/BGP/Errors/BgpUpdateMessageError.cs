namespace Router {
	public class BgpUpdateMessageError: BgpError {
		private readonly BgpUpdateMessageErrorSubCode subCode;

		public BgpUpdateMessageError(BgpUpdateMessageErrorSubCode subCode = 0x00) {
			this.subCode = subCode;
		}

		public override byte GetErrorCode() {
			return 0x03;
		}

		public override byte GetErrorSubCode() {
			return (byte) this.subCode;
		}
	}

	public enum BgpUpdateMessageErrorSubCode {
		MalformedAttributeList = 0x01,
		UnrecognizedWellKnownAttribute = 0x02,
		MissingWellKnownAttribute = 0x03,
		AttributeFlagsError = 0x04,
		AttributeLengthError = 0x05,
		InvalidOriginAttribute = 0x06,

		/* 0x07 DEPRECATED */
		InvalidNextHopAttribute = 0x08,
		OptionalAttributeError = 0x09,
		InvalidNetworkField = 0x0A,
		MalformedAsPath = 0x0B,
	}
}