namespace Router {
	public class BgpFiniteStateMachineError: BgpError {
		public override byte GetErrorCode() {
			return 0x05;
		}

		public override byte GetErrorSubCode() {
			return 0x00;
		}
	}
}