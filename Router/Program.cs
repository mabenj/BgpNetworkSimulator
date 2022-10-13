namespace Router {
	using System;
	using System.Diagnostics;
	using System.Linq;

	using Common;

	public class Program {
		public static int Main(string[] args) {
			try {
				var routerConfig = ProcessArgs(args);
				Run(routerConfig);
				Console.ReadKey();
			} catch(Exception e) {
				Logger.Error($"Exception: {e.Message}", e);
				return 1;
			}
			return 0;
		}

		private static RouterConfiguration ProcessArgs(string[] args) {
			args = args.Select(arg => arg.ToLowerInvariant()).ToArray();
			if(args.Any(arg => arg == "--debug") && !Debugger.IsAttached) {
				Debugger.Launch();
				args = args.Where(arg => arg != "--debug").ToArray();
			}
			if(args.Length < 2) {
				throw new ArgumentException("Invalid arguments. Please provide following the arguments: <router id> <port>.");
			}
			return new RouterConfiguration() {
				Id = args[0],
				Port = int.Parse(args[1]),
			};
		}

		private static void Run(RouterConfiguration config) {
			var router = new Router(config.Id, config.Port);
			Logger.Info($"Starting router [id:{config.Id}] [port:{config.Port}]");
			router.Start();
		}
	}
}