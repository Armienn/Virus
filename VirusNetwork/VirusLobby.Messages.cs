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
	class VirusLobby {

		enum MessageType {
			Initialise,
			Text,
			Ready,
			StartGame,
			Unknown
		}

		public static bool TryParseInitialiseMessage(string message, out string name, out Color color) {
			name = "";
			color = Color.Red;
			bool noerror = InitialMessageCheck(message, MessageType.Initialise, out message);
			if (!noerror)
				return false;

			NeaReader reader = new NeaReader(message);

			try {
				name = reader.ReadWord();
				color = Color.FromName(reader.ReadWord());
			}
			catch { return false; }
			return true;
		}

		private static bool InitialMessageCheck(string message, MessageType type, out string messagebody) {
			messagebody = "";
			MessageType gottype = ParseMessageType(message);
			if (gottype != type || message.Length <= 3)
				return false;

			messagebody = message.Substring(3);
			return true;
		}

		public static MessageType ParseMessageType(string message) {
			MessageType result = MessageType.Unknown;
			if (message.Length < 3)
				return result;
			string type = message.Substring(0, 3);
			switch (type) {
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
}
