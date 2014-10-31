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
			get { return master; }
			set {
				if(ListeningStarted && value == false)
					throw new ApplicationException("Attempt at changing master status while listening");
				if(Connected || ConnectionStarted)
					throw new ApplicationException("Attempt at changing master status while connected");
				master = value;
			}
		}
		private bool master = false;
		private VirusPlayer MasterPlayer;

		#endregion

		#region Communication Variables

		private const int Port = 3000;
		private TcpListener tcpListener;
		private Thread listenThread;
		private static UnicodeEncoding encoder = new UnicodeEncoding();

		#endregion

		#region Connection Information

		public bool ListeningStarted { get; private set; }
		public bool ConnectionStarted { get; private set; }
		public bool Connected { get; private set; }
		public bool GameStarted { get; private set; }

		#endregion

		#region Callback Functions

		public delegate void StartFunction(VirusPlayer[] players);
		public delegate void TextFunction(string text);
		public delegate void BoolFunction(bool boolean);
		public delegate void TextMessageFunction(VirusPlayer player, string text);
		public delegate void PlayerUpdateFunction(VirusPlayer player);
		public delegate void PlayerUpdateTextFunction(VirusPlayer player, string text);
		public delegate void PlayerUpdateColorFunction(VirusPlayer player, Color color);
		public delegate void PlayerUpdateBoolFunction(VirusPlayer player, bool boolean);
		public delegate void GameMoveFunction(VirusPlayer player, int x, int y, int dx, int dy);
		public TextFunction OnBadMessageRecieved;
		public StartFunction OnStartGame;
		public TextMessageFunction OnTextMessageRecieved;
		public PlayerUpdateColorFunction OnColorChanged;
		public PlayerUpdateTextFunction OnNameChanged;
		public PlayerUpdateBoolFunction OnReadyChanged;
		public BoolFunction OnEveryoneReadyChanged;
		public PlayerUpdateFunction OnPlayerConnected;
		public GameMoveFunction OnGameMove;

		#endregion

		public VirusLobby(VirusPlayer player) {
			Player = player;
			ListeningStarted = false;
			ConnectionStarted = false;
			Connected = false;
			GameStarted = false;
		}

		public void StartGame() {
			if (!Master)
				return;
			SendStartGameMessage();
			GameStarted = true;
		}

		#region Start/Stop Connection

		public void StartListening(){
			if (!Master)
				throw new ApplicationException("Client tried to listen for clients");
			this.tcpListener = new TcpListener(IPAddress.Any, Port);
			this.listenThread = new Thread(new ThreadStart(ListenForClients));
			this.listenThread.Start();
			ListeningStarted = true;
		}

		public void StopListening(){
			if(!Master)
				return;
			ListeningStarted = false;
			listenThread.Join(20);
			if(listenThread.IsAlive)
				listenThread.Abort();
		}

		public void StartConnection(IPAddress ip) {
			if(Master)
				throw new ApplicationException("Master tried to connect to another master");

			TcpClient client = new TcpClient();
			IPEndPoint serverEndPoint = new IPEndPoint(ip, Port);
			client.Connect(serverEndPoint);
			VirusPlayer pclient = new VirusPlayer(client);
			SendInitialiseMessage(pclient);

			Thread clientThread = new Thread(new ParameterizedThreadStart(AwaitCommunicationClient));
			clientThread.Start(pclient);
			ConnectionStarted = true;
		}

		public void Disconnect() {
			StopListening();
			foreach(VirusPlayer player in Players){
				player.TcpClient.Close();
			}
			ConnectionStarted = false;
			Connected = false;
			Players.Clear();
		}

		#endregion

		#region Update Player
		public void UpdatePlayer(Color color) {
			if (Master)
				OnColorChanged(Player, color);
			Player.Color = color;
			SendColorMessage(Player);
		}

		public void UpdatePlayer(string name) {
			if (Master)
				OnNameChanged(Player, name);
			Player.Name = name;
			SendNameMessage(Player);
		}

		public void UpdatePlayer(bool ready) {
			if (Master)
				OnReadyChanged(Player, ready);
			Player.Ready = ready;
			SendReadyMessage(Player);
		}
		#endregion

		public VirusPlayer GetPlayerFromID(string id){
			foreach(VirusPlayer player in Players){
				if(player.ID == id)
					return player;
			}
			if(Player.ID == id)
				return Player;
			return null;
		}
	}
}
