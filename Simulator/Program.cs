namespace Simulator {
	using Common;

	public class Program {
		public static void Main(string[] args) {
			var topologyConfig = TopologyConfiguration.CreateFromFile();
			//Logger.Info("Launching routers");
			//var router1 = LaunchRouter(1, 7000, true);
			//var router2 = LaunchRouter(2, 7000, false);
			//router1.WaitForExit();
			//router2.WaitForExit();
		}

		//private static Process LaunchRouter(int id, int port, bool isServer) {
		//	var startInfo = new ProcessStartInfo("Router.exe") {
		//		UseShellExecute = true,
		//		Arguments = $"{id} {port} {(isServer ? 1 : 0)}"
		//	};
		//	return Process.Start(startInfo);
		//}
	}
}