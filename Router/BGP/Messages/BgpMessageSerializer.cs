namespace Router {
	using System;
	using System.Linq;

	public static class BgpMessageSerializer {
		private const int LengthFieldSize = 2;
		private const int MarkerFieldSize = 16;
		private const int TypeFieldSize = 1;
		private static readonly byte[] MarkerField = { 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01 };

		/// <summary>
		/// Parses a BGP message from an array of bytes
		/// </summary>
		/// <param name="bgpMessage">a byte array</param>
		/// <returns>an IBgpMessage</returns>
		public static IBgpMessage Deserialize(byte[] bgpMessage) {
			// find the start and end of the 16-octet marker field
			var markerStartIndex = -1;
			var markerEndIndex = -1;
			var sequentialOnesCount = 0;
			for(var i = 0; i < bgpMessage.Length; i++) {
				if(bgpMessage[i] == 0x01) {
					markerStartIndex = markerStartIndex == -1 ? i : markerStartIndex;
					sequentialOnesCount++;
					markerEndIndex = i;
				} else {
					markerStartIndex = -1;
					sequentialOnesCount = 0;
				}

				if(sequentialOnesCount == MarkerFieldSize) {
					break;
				}
			}

			// get length
			var lengthBytes = bgpMessage.Skip(markerEndIndex + 1).Take(LengthFieldSize);
			var lengthDecimal = BitConverter.ToUInt16(lengthBytes.ToArray());

			// get type
			var type = bgpMessage.Skip(markerEndIndex + 1 + LengthFieldSize).First();

			// get payload
			var payloadStartIndex = markerEndIndex + LengthFieldSize + TypeFieldSize;
			var payloadEndIndex = markerStartIndex + lengthDecimal;
			var payload = bgpMessage.Skip(payloadStartIndex + 1).Take(payloadEndIndex - payloadStartIndex + 1).ToArray();

			return type switch {
				0x01 => new BgpOpenMessage(payload),
				0x02 => new BgpUpdateMessage(payload),
				0x03 => new BgpNotificationMessage(payload, lengthDecimal),
				0x04 => new BgpKeepAliveMessage(),
				_ => throw new ArgumentException($"Unknown BGP message type '{type}'")
			};
		}

		/// <summary>
		/// Converts an IBgpMessage into an array of bytes
		/// </summary>
		/// <param name="bgpMessage">BGP message to convert</param>
		/// <returns>a byte array</returns>
		public static byte[] Serialize(IBgpMessage bgpMessage) {
			// variable sized payload of the message
			var payload = bgpMessage.GetBytes();

			// header: 1-octet for the message type
			var type = bgpMessage switch {
				BgpOpenMessage => new byte[] { 0x01 },
				BgpUpdateMessage => new byte[] { 0x02 },
				BgpNotificationMessage => new byte[] { 0x03 },
				BgpKeepAliveMessage => new byte[] { 0x04 },
				_ => throw new ArgumentException("Unknown BGP message type")
			};

			// header: 2-octets for the total length (including header)
			var length = BitConverter.GetBytes(Convert.ToInt16(payload.Length + MarkerField.Length + type.Length + LengthFieldSize));

			return MarkerField.Concat(length).Concat(type).Concat(payload).ToArray();
		}
	}
}