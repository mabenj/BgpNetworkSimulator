namespace Simulator {
	using System.Diagnostics;

	using Common;

	public class Program {
		public static void Main(string[] args) {
			//var topologyConfig = TopologyConfiguration.CreateFromFile();

			//var open = new BgpOpenMessage(4200, 69, IPAddress.Loopback);
			//var serialized = BgpMessageSerializer.Serialize(open);
			//var deserialized = BgpMessageSerializer.Deserialize(serialized);

			Logger.Info("Launching routers");
			//var router1 = LaunchRouter(1, 7001);
			var router2 = LaunchRouter(2, 7002, 2, 30);
			//router1.WaitForExit();
			router2.WaitForExit();
		}

		private static Process LaunchRouter(int id, int port, int asNumber, int holdTime, bool debug = false) {
			var startInfo = new ProcessStartInfo("Router.exe") {
				UseShellExecute = true,
				Arguments = $"{id} {port} {asNumber} {holdTime} {(debug ? "--debug" : "")}"
			};
			return Process.Start(startInfo);
		}
	}
}