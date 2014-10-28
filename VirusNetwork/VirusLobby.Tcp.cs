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

		private void ListenForClients() {
			this.tcpListener.Start();

			while (ConnectionStarted) {
				TcpClient cl = this.tcpListener.AcceptTcpClient(); //blocks until a client has connected to the server
				VirusPlayer client = new VirusPlayer(cl);
				//Players.Add(client);

				//create a thread to handle communication with connected client
				Thread clientThread = new Thread(new ParameterizedThreadStart(AwaitCommunication));
				clientThread.Start(client);
			}
		}

		private void AwaitCommunication(object client) {
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
					bool success = TryParseInitialiseMessage(message, out player.Name, out player.Color);
					if (!success) {
						player.Connected = false;
						break;
					}
					player.Initialised = true;
					Players.Add(player);
					continue;
				}

				// -- Player is already initialised, so the message needs other handling
				HandleMessage(message, type);
			}
		}

		private void HandleMessage(string message, MessageType type) {
			
		}
	}
}
