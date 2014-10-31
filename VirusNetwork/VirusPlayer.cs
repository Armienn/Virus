using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Net.Sockets;

namespace VirusNetwork {
	public class VirusPlayer {
		public string Name;
		public Color Color;
		public String ID;
		public readonly TcpClient TcpClient;
		public bool Initialised;
		public bool Connected;
		public bool Ready;

		public VirusPlayer(TcpClient client) {
			TcpClient = client;
			Ready = false;
			Initialised = false;
			Connected = true;
		}

		public VirusPlayer(string name, string id, Color color) {
			Name = name;
			ID = id;
			Color = color;
			TcpClient = null;
			Ready = false;
			Initialised = true;
			Connected = true;
		}
	}
}
