namespace Router {
	public class BgpCease: BgpError {
		public override byte GetErrorCode() {
			return 0x06;
		}

		public override byte GetErrorSubCode() {
			return 0x00;
		}
	}
}