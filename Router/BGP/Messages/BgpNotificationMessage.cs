namespace Router {
	using System;
	using System.Collections.Generic;
	using System.Linq;

	public class BgpNotificationMessage: IBgpMessage {
		private const byte ErrorCodeFieldSize = 1;
		private const byte ErrorSubCodeFieldSize = 1;

		private readonly byte[] data;
		private readonly BgpError error;

		/// <summary>
		/// Creates a new instance of BGP NOTIFICATION message.
		/// </summary>
		/// <param name="error">the BGP error</param>
		/// <param name="data">the data associated with the error</param>
		public BgpNotificationMessage(BgpError error, byte[] data = null) {
			this.error = error;
			this.data = data ?? Array.Empty<byte>();
		}

		/// <summary>
		/// Creates a new instance of BGP NOTIFICATION message from binary data.
		/// </summary>
		/// <param name="message">BGP message as a byte array</param>
		/// <param name="messageTotalLength">total length of the message (including header)</param>
		public BgpNotificationMessage(byte[] message, ushort messageTotalLength) {
			var errorCode = message.First();
			var errorSubCode = message.Skip(ErrorCodeFieldSize).First();

			this.error = errorCode switch {
				0x01 => new BgpMessageHeaderError((BgpMessageHeaderErrorSubCode) errorSubCode),
				0x02 => new BgpOpenMessageError((BgpOpenMessageErrorSubCode) errorSubCode),
				0x03 => new BgpUpdateMessageError((BgpUpdateMessageErrorSubCode) errorSubCode),
				0x04 => new BgpHoldTimerExpired(),
				0x05 => new BgpFiniteStateMachineError(),
				0x06 => new BgpCease(),
				_ => throw new ArgumentException($"Unknown error code '{errorCode}'"),
			};

			// Message Length = 21 + Data Length, the minimum length of the NOTIFICATION message is 21 octets (including message header)
			var dataLength = messageTotalLength - 21;
			this.data = message.Skip(ErrorCodeFieldSize + ErrorSubCodeFieldSize).Take(dataLength).ToArray();
		}

		public byte[] GetBytes() {
			var payload = new List<byte>();

			// error code (1-octet)
			payload.Add(this.error.GetErrorCode());

			// error sub code (1-octet)
			payload.Add(this.error.GetErrorSubCode());

			// data (variable)
			payload.AddRange(this.data);

			return payload.ToArray();
		}
	}
}