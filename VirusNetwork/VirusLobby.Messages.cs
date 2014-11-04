using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Drawing;
using System.Net.Sockets;
using System.Net;
using System.ComponentModel;
using System.IO;
using Nea;

namespace VirusNetwork {
	partial class VirusLobby {

		const string CodeInitialise = "INI";
		const string CodeText = "TXT";
		const string CodeColor = "CLR";
		const string CodeName = "NME";
		const string CodeReady = "RDY";
		const string CodeStartGame = "STG";
		const string CodeGameMessage = "GMS";
		const string CodeDisconnect = "DIS";

		enum MessageType {
			Initialise,
			Text,
			Color,
			Name,
			Ready,
			StartGame,
			GameMessage,
			Disconnect,
			Unknown
		}

		private static MessageType ParseMessageType(string message) {
			MessageType result = MessageType.Unknown;
			if (message.Length < 3)
				return result;
			string type = message.Substring(0, 3);
			switch (type) {
				case CodeInitialise:
					result = MessageType.Initialise;
					break;
				case CodeText:
					result = MessageType.Text;
					break;
				case CodeColor:
					result = MessageType.Color;
					break;
				case CodeName:
					result = MessageType.Name;
					break;
				case CodeReady:
					result = MessageType.Ready;
					break;
				case CodeStartGame:
					result = MessageType.StartGame;
					break;
				case CodeGameMessage:
					result = MessageType.GameMessage;
					break;
				case CodeDisconnect:
					result = MessageType.Disconnect;
					break;
				default:
					result = MessageType.Unknown;
					break;
			}
			return result;
		}

		private static bool InitialMessageCheck(string message, MessageType type, out string messagebody) {
			messagebody = "";
			MessageType gottype = ParseMessageType(message);
			if (gottype != type || message.Length <= 3)
				return false;

			messagebody = message.Substring(3);
			return true;
		}

		public void SendMessage(byte[] message) {
			SendMessage(message, Players);
		}

		public void SendMessage(byte[] message, List<VirusPlayer> players) {
			//if (Connected) {
			byte[] sizeinfo = new byte[4];

			//could optionally call BitConverter.GetBytes(data.length);
			sizeinfo[0] = (byte)message.Length;
			sizeinfo[1] = (byte)(message.Length >> 8);
			sizeinfo[2] = (byte)(message.Length >> 16);
			sizeinfo[3] = (byte)(message.Length >> 24);
				if (Master) {
					foreach (VirusPlayer pc in players) {
						TcpClient ns = pc.TcpClient;
						ns.GetStream().Write(sizeinfo, 0, 4);
						ns.GetStream().Write(message, 0, message.Length);
						ns.GetStream().Flush();
					}
				}
				else {
					TcpClient ns = MasterPlayer.TcpClient;
					ns.GetStream().Write(sizeinfo, 0, 4);
					ns.GetStream().Write(message, 0, message.Length);
					ns.GetStream().Flush();
				}
			//}
		}

		#region Send Messages

		private void SendInitialiseMessage(VirusPlayer player, List<VirusPlayer> players) {
			byte[] buffer = encoder.GetBytes(CodeInitialise + player.ID + " [" + player.Name + "] " + player.Color.Name);
			SendMessage(buffer, players);
		}

		private void SendInitialiseMessage(VirusPlayer player) {
			byte[] buffer = encoder.GetBytes(CodeInitialise + player.ID + " [" + player.Name + "] " + player.Color.Name);
			SendMessage(buffer);
		}

		public void SendTextMessage(string text) {
			SendTextMessage(Player, text);
			if (Master)
				OnTextMessageRecieved(Player, text);
		}

		private void SendTextMessage(VirusPlayer player, string text) {
			byte[] buffer = encoder.GetBytes(CodeText + player.ID + " [" + text + "]");
			SendMessage(buffer);
		}

		private void SendColorMessage(VirusPlayer player) {
			byte[] buffer = encoder.GetBytes(CodeColor + player.ID + " " + player.Color.Name);
			SendMessage(buffer);
		}

		private void SendNameMessage(VirusPlayer player) {
			byte[] buffer = encoder.GetBytes(CodeName + player.ID + " [" + player.Name + "]");
			SendMessage(buffer);
		}

		private void SendReadyMessage(VirusPlayer player) {
			byte[] buffer = encoder.GetBytes(CodeReady + player.ID + " " + (player.Ready ? "1" : "0"));
			SendMessage(buffer);
		}

		private void SendStartGameMessage() {
			VirusPlayer[] list = AllPlayers;
			string sequence = "";
			foreach (VirusPlayer player in list) {
				sequence += " " + player.ID;
			}
			byte[] buffer = encoder.GetBytes(CodeStartGame + sequence);
			SendMessage(buffer);
		}

		public void SendGameMessage(int x, int y, int dx, int dy) { SendGameMessage(Player, x, y, dx, dy); }

		private void SendGameMessage(VirusPlayer player, int x, int y, int dx, int dy) {
			byte[] buffer = encoder.GetBytes(CodeGameMessage + player.ID + " " + x + " " + y + " " + dx + " " + dy);
			SendMessage(buffer);
		}

		private void SendDisconnectMessage(VirusPlayer player) {
			byte[] buffer = encoder.GetBytes(CodeDisconnect + player.ID);
			SendMessage(buffer);
		}

		#endregion

		#region Parse Messages

		public static bool TryParseInitialiseMessage(string message, out string id, out string name, out Color color) {
			id = "";
			name = "";
			color = Color.Red;
			bool success = InitialMessageCheck(message, MessageType.Initialise, out message);
			if (!success)
				return false;

			NeaReader reader = new NeaReader(message);

			try {
				id = reader.ReadWord();
				name = reader.ReadSection('[', ']');
				color = Color.FromName(reader.ReadWord());
			}
			catch { return false; }
			return true;
		}

		public static bool TryParseTextMessage(string message, out string id, out string text) {
			id = "";
			text = "";
			bool success = InitialMessageCheck(message, MessageType.Text, out message);
			if (!success)
				return false;

			NeaReader reader = new NeaReader(message);

			try {
				id = reader.ReadWord();
				text = reader.ReadSection('[', ']');
			}
			catch { return false; }
			return true;
		}

		public static bool TryParseColorMessage(string message, out string id, out Color color) {
			id = "";
			color = Color.Red;
			bool success = InitialMessageCheck(message, MessageType.Color, out message);
			if (!success)
				return false;

			NeaReader reader = new NeaReader(message);

			try {
				id = reader.ReadWord();
				color = Color.FromName(reader.ReadWord());
			}
			catch { return false; }
			return true;
		}

		public static bool TryParseNameMessage(string message, out string id, out string name) {
			id = "";
			name = "";
			bool success = InitialMessageCheck(message, MessageType.Name, out message);
			if (!success)
				return false;

			NeaReader reader = new NeaReader(message);

			try {
				id = reader.ReadWord();
				name = reader.ReadSection('[', ']');
			}
			catch { return false; }
			return true;
		}

		public static bool TryParseReadyMessage(string message, out string id, out bool ready) {
			id = "";
			ready = false;
			bool success = InitialMessageCheck(message, MessageType.Ready, out message);
			if (!success)
				return false;

			NeaReader reader = new NeaReader(message);

			try {
				id = reader.ReadWord();
				ready = reader.ReadInt() == 1 ? true : false;
			}
			catch { return false; }
			return true;
		}

		public static bool TryParseStartGameMessage(string message, out string[] sequence) {
			sequence = new string[0];
			bool success = InitialMessageCheck(message, MessageType.StartGame, out message);
			if (!success)
				return false;

			NeaReader reader = new NeaReader(message);

			try {
				List<string> list = new List<string>();
				reader.SkipWhiteSpace();
				while (reader.Peek() != -1) {
					list.Add(reader.ReadWord());
				}
				sequence = list.ToArray();
			}
			catch { return false; }
			return true;
		}

		public static bool TryParseGameMessage(string message, out string id, out int x, out int y, out int dx, out int dy) {
			id = "";
			x = y = dx = dy = 0;
			bool success = InitialMessageCheck(message, MessageType.GameMessage, out message);
			if (!success)
				return false;

			NeaReader reader = new NeaReader(message);

			try {
				id = reader.ReadWord();
				x = reader.ReadInt();
				y = reader.ReadInt();
				dx = reader.ReadInt();
				dy = reader.ReadInt();
			}
			catch { return false; }
			return true;
		}

		public static bool TryParseDisconnectMessage(string message, out string id) {
			id = "";
			bool success = InitialMessageCheck(message, MessageType.Disconnect, out message);
			if (!success)
				return false;

			NeaReader reader = new NeaReader(message);

			try {
				id = reader.ReadWord();
			}
			catch { return false; }
			return true;
		}

		#endregion
	}
}
