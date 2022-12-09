namespace Router {
	using System;

	public class BgpUpdateMessage: IBgpMessage {
		private readonly BgpNLRI[] nlri;
		private readonly BgpPathAttribute[] pathAttributes;
		private readonly ushort totalPathAttributeLength;
		private readonly BgpRoute[] withdrawnRoutes;
		private readonly ushort withdrawnRoutesLength;

		public BgpUpdateMessage(byte[] message) {
			throw new NotImplementedException();
		}

		public enum BgpAttributeTypeCode {
			ORIGIN = 0x01,
			AS_PATH = 0x02,
			NEXT_HOP = 0x03,
			MULTI_EXIT_DISC = 0x04,
			LOCAL_PREF = 0x05,
			ATOMIC_AGGREGATE = 0x06,
			AGGREGATOR = 0x07
		}

		public byte[] GetBytes() {
			throw new NotImplementedException();
		}

		public class BgpAttributeType {
			public byte AttributeFlags {
				get;
				init;
			}

			public BgpAttributeTypeCode AttributeTypeCode {
				get;
				init;
			}
		}

		public class BgpNLRI {
			/// <summary>
			/// Length of IP address prefix in <b>bits</b>
			/// </summary>
			public byte Length {
				get;
				init;
			}

			public byte[] Prefix {
				get;
				init;
			}
		}

		public class BgpPathAttribute {
			public ushort AttributeLength {
				get;
				init;
			}

			public BgpAttributeType AttributeType {
				get;
				init;
			}

			public byte[] AttributeValue {
				get;
				init;
			}
		}

		public class BgpRoute {
			public byte Length {
				get;
				init;
			}

			public byte[] Prefix {
				get;
				init;
			}
		}
	}
}