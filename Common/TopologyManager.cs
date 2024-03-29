﻿namespace Common {
	using System.Collections.Generic;
	using System.Linq;
	using System.Net;

	public static class TopologyManager {
		private static readonly Dictionary<string, RouterNode> RouterNodeMap;

		static TopologyManager() {
			var topologyConfig = TopologyConfiguration.CreateFromFile();
			RouterNodeMap = topologyConfig.Nodes.ToDictionary(node => node.Id, node => node);
		}

		public static IPEndPoint[] GetNeighbors(string routerId) {
			var neighborIds = RouterNodeMap[routerId].Neighbors;
			return neighborIds.Select(id => new IPEndPoint(IPAddress.Parse(RouterNodeMap[id].MockIp), RouterNodeMap[id].Port)).ToArray();
		}
	}
}