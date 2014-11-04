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

namespace VirusNetwork {
	partial class VirusLobby {

		#region Master

		private void ListenForClients() {
			this.tcpListener.Start();

			while (ListeningStarted) {
				try {
					TcpClient cl = this.tcpListener.AcceptTcpClient(); //blocks until a client has connected to the server
					VirusPlayer client = new VirusPlayer(cl);
					//Players.Add(client);

					//create a thread to handle communication with connected client
					Thread clientThread = new Thread(new ParameterizedThreadStart(AwaitCommunicationMaster));
					clientThread.Start(client);
				}
				catch { }
			}
			this.tcpListener.Stop();
		}

		private void AwaitCommunicationMaster(object client) {
			VirusPlayer player = (VirusPlayer)client;
			TcpClient tcpClient = player.TcpClient;
			NetworkStream clientStream = tcpClient.GetStream();

			while (player.Connected) {
				String message = "";

				try { //blocks until a client sends a message
					message = ReadMessage(clientStream);
				}
				catch { //a socket error has occured
					player.Connected = false;
					break;
				}

				if (message == "")
					continue;

				MessageType type = VirusLobby.ParseMessageType(message);

				// -- The player should start by initialising
				if (!player.Initialised) {
					if (type != MessageType.Initialise) {
						player.Connected = false;
						break;
					}
					bool success = TryParseInitialiseMessage(message, out player.ID, out player.Name, out player.Color);
					if (!success) {
						player.Connected = false;
						break;
					}
					player.Initialised = true;
					Players.Add(player);
					Connected = true;
					if (OnPlayerConnected != null)
						OnPlayerConnected(player);
					List<VirusPlayer> list = new List<VirusPlayer>();
					List<VirusPlayer> list2 = new List<VirusPlayer>();
					list.Add(player);
					foreach (VirusPlayer p in Players) {
						if (p.ID != player.ID) {
							SendInitialiseMessage(p, list); //send init message to new player about the existing players
							list2.Add(p);
						}
					}
					SendInitialiseMessage(Player, list); //send init message to new player about the host
					SendInitialiseMessage(player, list2); //send init message to existing players about the new player

					continue;
				}

				// -- Player is already initialised, so the message needs other handling
				HandleMessageMaster(player, message, type);
			}

			if (OnPlayerDisconnected != null)
				OnPlayerDisconnected(player);
			Players.Remove(player);
			SendDisconnectMessage(player);
		}

		private void HandleMessageMaster(VirusPlayer player, string message, MessageType type) {
			string text = "";
			string id = "";
			string name = "";
			Color color = Color.Red;
			bool ready;
			int x, y, dx, dy = 0;
			switch (type) {

				// ###### ------ Initialise ------ ###### //
				case MessageType.Initialise:
					if (OnBadMessageRecieved != null)
						OnBadMessageRecieved("Got a second initialise message from player with id " + player.ID);
					break;

				// ###### ------ Text ------ ###### //
				case MessageType.Text:
					if (TryParseTextMessage(message, out id, out text)) {
						if (player.ID != id) {
							if (OnBadMessageRecieved != null)
								OnBadMessageRecieved("Got text message with id " + id + " from player with id " + player.ID);
						}
						else {
							if (OnTextMessageRecieved != null)
								OnTextMessageRecieved(player, text);
							SendTextMessage(player, text);
						}
					}
					else {
						if (OnBadMessageRecieved != null)
							OnBadMessageRecieved("Couldn't parse text message from player with id " + player.ID);
					}
					break;

				// ###### ------ Color ------ ###### //
				case MessageType.Color:
					if (TryParseColorMessage(message, out id, out color)) {
						if (player.ID != id) {
							if (OnBadMessageRecieved != null)
								OnBadMessageRecieved("Got color message with id " + id + " from player with id " + player.ID);
						}
						else {
							if (OnColorChanged != null)
								OnColorChanged(player, color);
							player.Color = color;
							SendColorMessage(player);
						}
					}
					else {
						if (OnBadMessageRecieved != null)
							OnBadMessageRecieved("Couldn't parse color message from player with id " + player.ID);
					}
					break;

				// ###### ------ Name ------ ###### //
				case MessageType.Name:
					if (TryParseNameMessage(message, out id, out name)) {
						if (player.ID != id) {
							if (OnBadMessageRecieved != null)
								OnBadMessageRecieved("Got name message with id " + id + " from player with id " + player.ID);
						}
						else {
							if (OnNameChanged != null)
								OnNameChanged(player, name);
							player.Name = name;
							SendNameMessage(player);
						}
					}
					else {
						if (OnBadMessageRecieved != null)
							OnBadMessageRecieved("Couldn't parse name message from player with id " + player.ID);
					}
					break;

				// ###### ------ Ready ------ ###### //
				case MessageType.Ready:
					if (TryParseReadyMessage(message, out id, out ready)) {
						if (player.ID != id) {
							if (OnBadMessageRecieved != null)
								OnBadMessageRecieved("Got ready message with id " + id + " from player with id " + player.ID);
						}
						else {
							bool allreadyprev = true;
							foreach (VirusPlayer p in Players)
								if (!p.Ready)
									allreadyprev = false;
							if (OnReadyChanged != null)
								OnReadyChanged(player, ready);
							player.Ready = ready;
							SendReadyMessage(player);
							bool allreadynow = true;
							foreach (VirusPlayer p in Players)
								if (!p.Ready)
									allreadynow = false;
							if (allreadynow != allreadyprev)
								if (OnEveryoneReadyChanged != null)
									OnEveryoneReadyChanged(allreadynow);
						}
					}
					else {
						if (OnBadMessageRecieved != null)
							OnBadMessageRecieved("Couldn't parse ready message from player with id " + player.ID);
					}
					break;

				// ###### ------ StartGame ------ ###### //
				case MessageType.StartGame:
					if (OnBadMessageRecieved != null)
						OnBadMessageRecieved("Got start game message from player with id " + player.ID);
					break;

				// ###### ------ GameMessage ------ ###### //
				case MessageType.GameMessage:
					if (TryParseGameMessage(message, out id, out x, out y, out dx, out dy)) {
						if (player.ID != id) {
							if (OnBadMessageRecieved != null)
								OnBadMessageRecieved("Got game message with id " + id + " from player with id " + player.ID);
						}
						else {
							if (OnGameMove != null)
								OnGameMove(player, x, y, dx, dy);
							SendGameMessage(player, x, y, dx, dy);
						}
					}
					else {
						if (OnBadMessageRecieved != null)
							OnBadMessageRecieved("Couldn't parse game message from player with id " + player.ID);
					}
					break;

				// ###### ------ Disconnect ------ ###### //
				case MessageType.Disconnect:
					if (OnBadMessageRecieved != null)
						OnBadMessageRecieved("Got disconnect message from player with id " + player.ID);
					break;

				// ###### ------ default ------ ###### //
				default:
					if (OnBadMessageRecieved != null)
						OnBadMessageRecieved("Couldn't recognise message from player with id " + player.ID);
					break;
			}
		}

		#endregion

		#region Client

		private void AwaitCommunicationClient(object client) {
			VirusPlayer player = (VirusPlayer)client;
			TcpClient tcpClient = player.TcpClient;
			NetworkStream clientStream = tcpClient.GetStream();

			while (player.Connected) {
				String message = "";

				try { //blocks until a client sends a message
					message = ReadMessage(clientStream);
				}
				catch { //a socket error has occured
					player.Connected = false;
					break;
				}

				if (message == "")
					continue;

				MessageType type = VirusLobby.ParseMessageType(message);

				// -- The player should start by initialising
				if (!player.Initialised) {
					if (type != MessageType.Initialise) {
						player.Connected = false;
						break;
					}
					bool success = TryParseInitialiseMessage(message, out player.ID, out player.Name, out player.Color);
					if (!success) {
						player.Connected = false;
						break;
					}
					player.Initialised = true;
					Players.Add(player);
					MasterPlayer = player;
					Connected = true;
					if (OnPlayerConnected != null)
						OnPlayerConnected(player);
					continue;
				}

				// -- Player is already initialised, so the message needs other handling
				HandleMessageClient(message, type);
			}
			if (OnPlayerDisconnected != null)
				OnPlayerDisconnected(player);
			Players.Clear();
		}

		private void HandleMessageClient(string message, MessageType type) {
			VirusPlayer origin = null;
			string text = "";
			string id = "";
			string name = "";
			Color color = Color.Red;
			bool ready;
			string[] sequence;
			int x, y, dx, dy = 0;
			switch (type) {
				case MessageType.Initialise:
					if (TryParseInitialiseMessage(message, out id, out name, out color)) {
						origin = GetPlayerFromID(id);

						if (origin == null) {
							Players.Add(new VirusPlayer(name, id, color));
						}
						else {
							if (OnBadMessageRecieved != null)
								OnBadMessageRecieved("Got a second initialise message for player id: " + origin.ID);
						}
					}
					else {
						if (OnBadMessageRecieved != null)
							OnBadMessageRecieved("Couldn't parse new initialise message from master");
					}
					break;
				case MessageType.Text:
					if (TryParseTextMessage(message, out id, out text)) {
						origin = GetPlayerFromID(id);
						if (origin == null) {
							if (OnBadMessageRecieved != null)
								OnBadMessageRecieved("Got text message without origin");
						}
						else
							if (OnTextMessageRecieved != null)
								OnTextMessageRecieved(origin, text);
					}
					else {
						if (OnBadMessageRecieved != null)
							OnBadMessageRecieved("Couldn't parse text message from master");
					}
					break;
				case MessageType.Color:
					if (TryParseColorMessage(message, out id, out color)) {
						origin = GetPlayerFromID(id);
						if (origin == null) {
							if (OnBadMessageRecieved != null)
								OnBadMessageRecieved("Got color message without origin");
						}
						else {
							if (OnColorChanged != null)
								OnColorChanged(origin, color);
							origin.Color = color;
						}
					}
					else {
						if (OnBadMessageRecieved != null)
							OnBadMessageRecieved("Couldn't parse color message from master");
					}
					break;
				case MessageType.Name:
					if (TryParseNameMessage(message, out id, out name)) {
						origin = GetPlayerFromID(id);
						if (origin == null) {
							if (OnBadMessageRecieved != null)
								OnBadMessageRecieved("Got name message without origin");
						}
						else {
							if (OnNameChanged != null)
								OnNameChanged(origin, name);
							origin.Name = name;
						}
					}
					else {
						if (OnBadMessageRecieved != null)
							OnBadMessageRecieved("Couldn't parse name message from master");
					}
					break;
				case MessageType.Ready:
					if (TryParseReadyMessage(message, out id, out ready)) {
						origin = GetPlayerFromID(id);
						if (origin == null) {
							if (OnBadMessageRecieved != null)
								OnBadMessageRecieved("Got ready message without origin");
						}
						else {
							if (OnReadyChanged != null)
								OnReadyChanged(origin, ready);
							origin.Ready = ready;
						}
					}
					else {
						if (OnBadMessageRecieved != null)
							OnBadMessageRecieved("Couldn't parse ready message from master");
					}
					break;
				case MessageType.StartGame:
					if (TryParseStartGameMessage(message, out sequence)) {
						if(PlayerCount == sequence.Length){
							VirusPlayer[] list = new VirusPlayer[PlayerCount];
							VirusPlayer[] all = AllPlayers;
							for (int i = 0; i < PlayerCount; i++ ) {
								foreach (VirusPlayer p in all) {
									if (p.ID == sequence[i]) {
										list[i] = p;
										break;
									}
								}
							}
							GameStarted = true;
							if (OnStartGame != null)
								OnStartGame(list);
						}
						else
							if (OnBadMessageRecieved != null)
								OnBadMessageRecieved("Start game message from master had different number of players than locally");
					}
					else {
						if (OnBadMessageRecieved != null)
							OnBadMessageRecieved("Couldn't parse start game message from master");
					}
					break;
				case MessageType.GameMessage:
					if (TryParseGameMessage(message, out id, out x, out y, out dx, out dy)) {
						if (id == Player.ID)
							break;
						origin = GetPlayerFromID(id);
						if (origin == null) {
							if (OnBadMessageRecieved != null)
								OnBadMessageRecieved("Got game message without origin");
						}
						else
							if (OnGameMove != null)
								OnGameMove(origin, x, y, dx, dy);
					}
					else {
						if (OnBadMessageRecieved != null)
							OnBadMessageRecieved("Couldn't parse game message from master");
					}
					break;
				case MessageType.Disconnect:
					if (TryParseDisconnectMessage(message, out id)) {
						origin = GetPlayerFromID(id);
						if (origin == null) {
							if (OnBadMessageRecieved != null)
								OnBadMessageRecieved("Got disconnect message without origin");
						}
						else {
							if (OnPlayerDisconnected != null)
								OnPlayerDisconnected(origin);
							Players.Remove(origin);
						}
					}
					else {
						if (OnBadMessageRecieved != null)
							OnBadMessageRecieved("Couldn't parse ready message from master");
					}
					break;
				default:
					if (OnBadMessageRecieved != null)
						OnBadMessageRecieved("Couldn't recognise message from master");
					break;
			}
		}

		#endregion

		static string ReadMessage(NetworkStream stream) {
			byte[] sizeinfo = new byte[4];

			//read the size of the message
			int totalread = 0, currentread = 0;

			currentread = totalread = stream.Read(sizeinfo, 0, 4);// socket.Receive(sizeinfo);

			while (totalread < sizeinfo.Length && currentread > 0) {
				currentread = stream.Read(sizeinfo,
									totalread, //offset into the buffer
									sizeinfo.Length - totalread //max amount to read
									);

				totalread += currentread;
			}

			int messagesize = 0;

			//could optionally call BitConverter.ToInt32(sizeinfo, 0);
			messagesize |= sizeinfo[0];
			messagesize |= (((int)sizeinfo[1]) << 8);
			messagesize |= (((int)sizeinfo[2]) << 16);
			messagesize |= (((int)sizeinfo[3]) << 24);

			//create a byte array of the correct size
			//note:  there really should be a size restriction on
			//              messagesize because a user could send
			//              Int32.MaxValue and cause an OutOfMemoryException
			//              on the receiving side.  maybe consider using a short instead
			//              or just limit the size to some reasonable value
			byte[] data = new byte[messagesize];

			//read the first chunk of data
			totalread = 0;
			currentread = totalread = stream.Read(data,
									 totalread, //offset into the buffer
									data.Length - totalread //max amount to read
									);

			//if we didn't get the entire message, read some more until we do
			while (totalread < messagesize && currentread > 0) {
				currentread = stream.Read(data,
								 totalread, //offset into the buffer
								data.Length - totalread //max amount to read
								);
				totalread += currentread;
			}

			return encoder.GetString(data, 0, totalread);
		}
	}
}
