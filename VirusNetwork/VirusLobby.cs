using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading
using System.Threading.Tasks;
using System.Drawing;
using System.Net.Sockets;
using System.Net;
using System.ComponentModel;
using System.IO;

namespace VirusNetwork {
	partial class VirusLobby {
		#region Player Variables
		public readonly List<VirusPlayer> Players = new List<VirusPlayer>();
		public readonly VirusPlayer Player;
		public VirusPlayer[] AllPlayers {
			get {
				VirusPlayer[] array = new VirusPlayer[PlayerCount];
				for (int i = 0; i < Players.Count; i++)
					array[i] = Players[i];
				array[Players.Count] = Player;
				return array;
			}
		}
		public int PlayerCount { get { return Players.Count + 1; } }
		
		public bool Master { 
			get;
			set;
		}
		#endregion

		#region Communication Variables
		private TcpListener tcpListener;
		private UnicodeEncoding encoder = new UnicodeEncoding();
		#endregion

		public bool ConnectionStarted { get; private set; }
		public bool Connected { get; private set; }
		public bool GameStarted { get; private set; }

		public VirusLobby(VirusPlayer player) {
			Player = player;
			ConnectionStarted = false;
			Connected = false;
			GameStarted = false;
		}

		public void StartConnection() {
			
		}

		public void Disconnect() {
			
		}

		#region Update Player
		public void UpdatePlayer(Color color) {
			
		}

		public void UpdatePlayer(string name) {

		}

		public void UpdatePlayer(string name, Color color) {

		}

		public void UpdatePlayer(bool ready) {

		}
		#endregion

		public static MessageType ParseMessageType(string message){
			MessageType result = MessageType.Unknown;
			string type = message.Substring(0, 3);
			switch(type){
				case "INT":
					result = MessageType.Initialise;
					break;
				case "TXT":
					result = MessageType.Text;
					break;
			}
			return result;
		}
	}

	enum MessageType {
		Initialise,
		Text,
		Ready,
		StartGame,
		Unknown
	}
}
