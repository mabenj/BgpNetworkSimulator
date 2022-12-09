namespace Common {
	using System;
	using System.IO;
	using System.Linq;

	using YamlDotNet.Serialization;
	using YamlDotNet.Serialization.NamingConventions;

	public class TopologyConfiguration {
		private const string DefaultConfigFilePath = "topology.yaml";

		private RouterNode[] nodes;

		public RouterNode[] Nodes {
			get => this.nodes;
			set {
				var ids = value.Select(node => node.Id);
				if(value.Length != ids.Distinct().Count()) {
					throw new ArgumentException("Topology configuration contains nodes with duplicate IDs");
				}
				this.nodes = value.Select(
					node => {
						node.Neighbors = node.Neighbors.Distinct().ToArray();
						return node;
					}).ToArray();
			}
		}

		public string Version {
			get;
			set;
		}

		public static TopologyConfiguration CreateFromFile() {
			var deserializer = new DeserializerBuilder()
				.WithNamingConvention(UnderscoredNamingConvention.Instance)
				.Build();
			try {
				return deserializer.Deserialize<TopologyConfiguration>(File.ReadAllText(DefaultConfigFilePath));
			} catch(Exception e) {
				throw new Exception($"Error while deserializing topology configuration: {e.InnerException?.InnerException}", e);
			}
		}
	}

	public class RouterNode {
		public int As {
			get;
			set;
		}

		public string Id {
			get;
			set;
		}

		public string MockIp {
			get;
			set;
		}

		public string[] Neighbors {
			get;
			set;
		}

		public int Port {
			get;
			set;
		}
	}
}