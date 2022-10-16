using System;
using System.Diagnostics;

namespace Router {
	using Common;

	public class Router {
		private readonly TcpConnectionManager connectionManager;
		private readonly string id;
		private readonly int port;
        private BgpState state;

		public Router(string id, int port) {
			this.id = id;
			this.port = port;
			this.connectionManager = new TcpConnectionManager();
            this.state = BgpState.Idle;
        }

		public void Start() {
			this.connectionManager.ConnectionReceived += this.HandleConnectionReceived;
			this.connectionManager.StartTcpListener(this.port);
            this.ChangeState(BgpState.Idle);
            //TODO: broadcast keep alive every 30 secs
        }

		private void HandleConnectionReceived(object sender, ConnectionReceivedEventArgs e) {
			Logger.Info($"Connection received: {e.Data}");
            if (this.state == BgpState.Connect)
            {
                this.SendOpenMessage( /* TODO: the peer is the receiver of this message */);
                this.ChangeState(BgpState.OpenSent);
            }
		}

        private void ChangeState(BgpState toState)
        {
            var fromState = this.state;
            switch (this.state)
            {
                case BgpState.Idle:
                {
                    this.connectionManager.InitiatePeerConnections();
                    this.ChangeState(BgpState.Connect);
                    return;
                }
                case BgpState.Active:
                {
                    return;
                }
                case BgpState.Connect:
                {
                    // Do nothing here, send open message when peer tcp connection is negotiated
                    // in HandleConnectionReceived
                    return;
                }
                case BgpState.Established:
                {
                    return;
                }
                case BgpState.OpenSent:
                {
                    return;
                }
                case BgpState.OpenConfirm:
                {
                    return;
                }
                default:
                {
                    throw new ArgumentException($"Unknown BgpState '{state}'");
                }
            }
        }
	}
}