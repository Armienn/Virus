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
				TcpClient cl = this.tcpListener.AcceptTcpClient(); //blocks until a client has connected to the server
				VirusPlayer client = new VirusPlayer(cl);
				//Players.Add(client);

				//create a thread to handle communication with connected client
				Thread clientThread = new Thread(new ParameterizedThreadStart(AwaitCommunicationMaster));
				clientThread.Start(client);
			}
		}

		private void AwaitCommunicationMaster(object client) {
			VirusPlayer player = (VirusPlayer)client;
			TcpClient tcpClient = player.TcpClient;
			NetworkStream clientStream = tcpClient.GetStream();

			byte[] incoming = new byte[4096];
			int bytesRead;

			while (player.Connected) {
				bytesRead = 0;

				try { //blocks until a client sends a message
					bytesRead = clientStream.Read(incoming, 0, 4096);
				}
				catch { //a socket error has occured
					player.Connected = false;
					break;
				}

				if (bytesRead == 0) { //the client has disconnected from the server
					player.Connected = false;
					break;
				}

				String message = encoder.GetString(incoming, 0, bytesRead);
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
					continue;
				}

				// -- Player is already initialised, so the message needs other handling
				HandleMessageMaster(player, message, type);
			}
		}

		private void HandleMessageMaster(VirusPlayer player, string message, MessageType type) {
			string text = "";
			string id = "";
			string name = "";
			Color color = Color.Red;
			bool ready;
			int x, y, dx, dy = 0;
			switch (type) {
				case MessageType.Initialise:
					OnBadMessageRecieved("Got a second initialise message from player with id " + player.ID);
					break;
				case MessageType.Text:
					if (TryParseTextMessage(message, out id, out text)) {
						if (player.ID != id)
							OnBadMessageRecieved("Got text message with id " + id + " from player with id " + player.ID);
						else {
							OnTextMessageRecieved(player, text);
							SendTextMessage(player, text);
						}
					}
					else {
						OnBadMessageRecieved("Couldn't parse text message from player with id " + player.ID);
					}
					break;
				case MessageType.Color:
					if (TryParseColorMessage(message, out id, out color)) {
						if (player.ID != id)
							OnBadMessageRecieved("Got color message with id " + id + " from player with id " + player.ID);
						else {
							OnColorChanged(player, color);
							player.Color = color;
							SendColorMessage(player);
						}
					}
					else {
						OnBadMessageRecieved("Couldn't parse color message from player with id " + player.ID);
					}
					break;
				case MessageType.Name:
					if (TryParseNameMessage(message, out id, out name)) {
						if (player.ID != id)
							OnBadMessageRecieved("Got name message with id " + id + " from player with id " + player.ID);
						else {
							OnNameChanged(player, name);
							player.Name = name;
							SendNameMessage(player);
						}
					}
					else {
						OnBadMessageRecieved("Couldn't parse name message from player with id " + player.ID);
					}
					break;
				case MessageType.Ready:
					if (TryParseReadyMessage(message, out id, out ready)) {
						if (player.ID != id)
							OnBadMessageRecieved("Got ready message with id " + id + " from player with id " + player.ID);
						else {
							bool allreadyprev = false;
							foreach (VirusPlayer p in Players)
								if (!p.Ready)
									allreadyprev = false;
							OnReadyChanged(player, ready);
							player.Ready = ready;
							SendReadyMessage(player);
							bool allreadynow = true;
							foreach (VirusPlayer p in Players)
								if (!p.Ready)
									allreadynow = false;
							if (allreadynow != allreadyprev)
								OnEveryoneReadyChanged(allreadynow);
						}
					}
					else {
						OnBadMessageRecieved("Couldn't parse ready message from player with id " + player.ID);
					}
					break;
				case MessageType.StartGame:
					OnBadMessageRecieved("Got start game message from player with id " + player.ID);
					break;
				case MessageType.GameMessage:
					if (TryParseGameMessage(message, out id, out x, out y, out dx, out dy)) {
						if (player.ID != id)
							OnBadMessageRecieved("Got game message with id " + id + " from player with id " + player.ID);
						else {
							OnGameMove(player, x, y, dx, dy);
							SendGameMessage(player, x, y, dx, dy);
						}
					}
					else {
						OnBadMessageRecieved("Couldn't parse game message from player with id " + player.ID);
					}
					break;
				default:
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

			byte[] incoming = new byte[4096];
			int bytesRead;

			while (player.Connected) {
				bytesRead = 0;

				try { //blocks until a client sends a message
					bytesRead = clientStream.Read(incoming, 0, 4096);
				}
				catch { //a socket error has occured
					player.Connected = false;
					break;
				}

				if (bytesRead == 0) { //the client has disconnected from the server
					player.Connected = false;
					break;
				}

				String message = encoder.GetString(incoming, 0, bytesRead);
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
					continue;
				}

				// -- Player is already initialised, so the message needs other handling
				HandleMessageClient(message, type);
			}
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
							OnBadMessageRecieved("Got a second initialise message for player id: " + origin.ID);
						}
					}
					else {
						OnBadMessageRecieved("Couldn't parse new initialise message from master");
					}
					break;
				case MessageType.Text:
					if (TryParseTextMessage(message, out id, out text)) {
						origin = GetPlayerFromID(id);
						if (origin == null)
							OnBadMessageRecieved("Got text message without origin");
						else
							OnTextMessageRecieved(origin, text);
					}
					else {
						OnBadMessageRecieved("Couldn't parse text message from master");
					}
					break;
				case MessageType.Color:
					if (TryParseColorMessage(message, out id, out color)) {
						origin = GetPlayerFromID(id);
						if (origin == null)
							OnBadMessageRecieved("Got color message without origin");
						else {
							OnColorChanged(origin, color);
							origin.Color = color;
						}
					}
					else {
						OnBadMessageRecieved("Couldn't parse color message from master");
					}
					break;
				case MessageType.Name:
					if (TryParseNameMessage(message, out id, out name)) {
						origin = GetPlayerFromID(id);
						if (origin == null)
							OnBadMessageRecieved("Got name message without origin");
						else {
							OnNameChanged(origin, name);
							origin.Name = name;
						}
					}
					else {
						OnBadMessageRecieved("Couldn't parse name message from master");
					}
					break;
				case MessageType.Ready:
					if (TryParseReadyMessage(message, out id, out ready)) {
						origin = GetPlayerFromID(id);
						if (origin == null)
							OnBadMessageRecieved("Got ready message without origin");
						else {
							OnReadyChanged(origin, ready);
							origin.Ready = ready;
						}
					}
					else {
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
							OnStartGame(list);
						}
						else
							OnBadMessageRecieved("Start game message from master had different number of players than locally");
					}
					else {
						OnBadMessageRecieved("Couldn't parse start game message from master");
					}
					break;
				case MessageType.GameMessage:
					if (TryParseGameMessage(message, out id, out x, out y, out dx, out dy)) {
						origin = GetPlayerFromID(id);
						if (origin == null)
							OnBadMessageRecieved("Got game message without origin");
						else
							OnGameMove(origin, x, y, dx, dy);
					}
					else {
						OnBadMessageRecieved("Couldn't parse game message from master");
					}
					break;
				default:
					OnBadMessageRecieved("Couldn't recognise message from master");
					break;
			}
		}

		#endregion
	}
}
