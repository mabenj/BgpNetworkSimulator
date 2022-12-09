namespace Router {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Net;

	public class BgpOpenMessage: IBgpMessage {
		private const int BgpIdFieldSize = 4;
		private const byte CurrentBgpVersion = 4;
		private const int HoldTimeFieldSize = 2;
		private const int MyAsFieldSize = 2;
		private const int OptParamsLengthFieldSize = 1;
		private const int VersionFieldSize = 1;
		private readonly IPAddress bgpIdentifier;
		private readonly ushort myAs;
		private readonly BgpOptionalParameter[] optionalParameters;

		/// <summary>
		/// Creates a new instance of BGP OPEN message
		/// </summary>
		/// <param name="myAs">AS number of the router</param>
		/// <param name="holdTime">sender's suggestion of interval for KEEPALIVE and UPDATE messages in seconds</param>
		/// <param name="bgpIdentifier">BGP router's IP address</param>
		/// <param name="optionalParameters">optional capabilities of the router</param>
		public BgpOpenMessage(ushort myAs, ushort holdTime, IPAddress bgpIdentifier, BgpOptionalParameter[] optionalParameters = null) {
			this.myAs = myAs;
			this.HoldTime = holdTime;
			this.bgpIdentifier = bgpIdentifier;
			this.optionalParameters = optionalParameters ?? Array.Empty<BgpOptionalParameter>();
		}

		/// <summary>
		/// Creates a new instance of BGP OPEN message from binary data
		/// </summary>
		/// <param name="message">the bgp message as an array of bytes</param>
		public BgpOpenMessage(byte[] message) {
			var version = message.First();
			if(version != CurrentBgpVersion) {
				throw new BgpOpenMessageError(BgpOpenMessageErrorSubCode.UnsupportedVersionNumber);
			}
			this.myAs = BitConverter.ToUInt16(message.Skip(VersionFieldSize).Take(MyAsFieldSize).ToArray());
			this.HoldTime = BitConverter.ToUInt16(message.Skip(VersionFieldSize + MyAsFieldSize).Take(HoldTimeFieldSize).ToArray());
			this.bgpIdentifier = new IPAddress(
				message.Skip(VersionFieldSize + MyAsFieldSize + HoldTimeFieldSize)
					.Take(BgpIdFieldSize).ToArray());

			var optParamsLength = message.Skip(VersionFieldSize + MyAsFieldSize + HoldTimeFieldSize + BgpIdFieldSize)
				.First();
			var optParamsBytes = message
				.Skip(VersionFieldSize + MyAsFieldSize + HoldTimeFieldSize + BgpIdFieldSize + OptParamsLengthFieldSize)
				.Take(optParamsLength)
				.ToList();
			var optParams = new List<BgpOptionalParameter>();
			while(optParamsBytes.Count > 3) {
				var optParamType = optParamsBytes[0];
				var optParamLength = optParamsBytes[1];
				var optParamValue = optParamsBytes.Skip(2).Take(optParamLength);
				optParams.Add(new BgpOptionalParameter() { ParamType = optParamType, ParamLength = optParamLength, ParamValue = optParamValue.ToArray() });

				optParamsBytes.RemoveRange(0, 2 + optParamLength);
			}

			this.optionalParameters = optParams.ToArray();
		}

		public ushort HoldTime {
			get;
		}

		public byte[] GetBytes() {
			var payload = new List<byte>();

			// version (1-octet)
			payload.Add(CurrentBgpVersion);

			// my autonomous system (2-octets)
			payload.AddRange(BitConverter.GetBytes(this.myAs));

			// hold time (2-octets)
			payload.AddRange(BitConverter.GetBytes(this.HoldTime));

			// BGP ID (4-octets)
			payload.AddRange(this.bgpIdentifier.GetAddressBytes());

			// optional params length (1-octet)
			var parameters = this.optionalParameters.SelectMany(param => new[] { param.ParamType, param.ParamLength }.Concat(param.ParamValue)).ToList();
			payload.Add((byte) parameters.Count);

			// optional params
			payload.AddRange(parameters);

			return payload.ToArray();
		}

		public class BgpOptionalParameter {
			public byte ParamLength {
				get;
				init;
			}

			public byte ParamType {
				get;
				init;
			}

			public byte[] ParamValue {
				get;
				init;
			}
		}
	}
}