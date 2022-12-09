namespace Router {
	public class BgpHoldTimerExpired: BgpError {
		public override byte GetErrorCode() {
			return 0x04;
		}

		public override byte GetErrorSubCode() {
			return 0x00;
		}
	}
}