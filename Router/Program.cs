namespace Router {
	using System;
	using System.Net;
	using System.Net.Sockets;
	using System.Text;

	using Common;

	public class Program {
		public static void Main(string[] args) {
			var routerId = args[0];
			var port = int.Parse(args[1]);
			var isServer = args[2] == "1";

			Run(routerId, port, isServer);
			Console.ReadKey();
		}

		private static async void Run(string routerId, int port, bool isServer) {
			var router = new Router(routerId, port);

			Logger.Info($"Starting router [id:{routerId}] [port:{port}] [is_server:{isServer}]");

			if(isServer) {
				router.Start();
			} else {
				// not a server

				try {
					var messageString = "FOOBAR :DD";

					using var client = new TcpClient();
					await client.ConnectAsync(IPAddress.Loopback, port);
					await using NetworkStream stream = client.GetStream();

					var messageBytes = Encoding.UTF8.GetBytes(messageString);
					await stream.WriteAsync(messageBytes);
					Logger.Info($"Sent message: '{messageString}'");

					var buffer = new byte[1024];
					int received = await stream.ReadAsync(buffer);
					var responseString = Encoding.UTF8.GetString(buffer, 0, received);
					Logger.Info($"Received response message: '{responseString}'");

					// Close everything.
					stream.Close();
					client.Close();
				} catch(Exception e) {
					Logger.Error($"Exception {e}", e);
				}
			}
		}
	}
}