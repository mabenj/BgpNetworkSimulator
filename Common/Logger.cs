namespace Common {
	using System;

	public class Logger {
		static Logger() {
			// do nothing
		}

		public static void Error(string message, Exception e = null) {
			Console.WriteLine(message);
		}

		public static void Info(string message) {
			Console.WriteLine(message);
		}

		public static void Warning(string message) {
			Console.WriteLine(message);
		}
	}
}