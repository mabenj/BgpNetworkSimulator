namespace Common {
	using System;

	public class Logger {
		static Logger() {
			// do nothing
		}

		public static void Error(string message, Exception e = null) {
			Console.WriteLine($"{GetTimeStamp()} [ERROR] {message}");
			if(e != null) {
				Console.WriteLine($"{GetTimeStamp()} [ERROR] Stacktrace: {e.StackTrace}");
			}
		}

		public static void Info(string message) {
			Console.WriteLine($"{GetTimeStamp()} [INFO] {message}");
		}

		public static void Warning(string message) {
			Console.WriteLine($"{GetTimeStamp()} [WARN] {message}");
		}

		private static string GetTimeStamp() {
			return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
		}
	}
}