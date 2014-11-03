using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.ComponentModel;
using System.IO;
using Nea;

namespace VirusNetwork {

	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window {
		VirusLobby lobby;
		VirusInterfaceMod viruscontrol;
		Random rand = new Random();

		VirusPlayer player;

		public MainWindow() {
			InitializeComponent();
			messageBox.KeyDown += new KeyEventHandler(messageBox_keyDown);
			player = new VirusPlayer("Player1", rand.Next().ToString(), System.Drawing.Color.Red);
			lobby = new VirusLobby(player);
			lobby.OnBadMessageRecieved += new VirusLobby.TextFunction(BadMessage);
			lobby.OnColorChanged += new VirusLobby.PlayerUpdateColorFunction(ColorUpdated);
			lobby.OnEveryoneReadyChanged += new VirusLobby.BoolFunction(EveryoneReadyUpdated);
			lobby.OnGameMove += new VirusLobby.GameMoveFunction(GameMove);
			lobby.OnNameChanged += new VirusLobby.PlayerUpdateTextFunction(NameUpdated);
			lobby.OnPlayerConnected += new VirusLobby.PlayerUpdateFunction(PlayerConnected);
			lobby.OnStartGame += new VirusLobby.StartFunction(StartGame);
			lobby.OnTextMessageRecieved += new VirusLobby.TextMessageFunction(TextMessageRecieved);
		}

		void BadMessage(string text) {
			AddText(InTextBox, text + "\n");
		}

		private void StartGame(VirusPlayer[] players) {
			Dispatcher.Invoke(() => { 
				viruscontrol.StartGame(new VirusNameSpace.Virus(lobby.PlayerCount, 10), PerformedMoveCallback, player.ID, players);
			});
		}

		private void TextMessageRecieved(VirusPlayer player, string text) {
			Dispatcher.Invoke(() => {
				InTextBox.Text += player.Name + ":\n" + text + "\n";
			});
		}

		private void NameUpdated(VirusPlayer player, string name) {
			string original = player.Name;
			Dispatcher.Invoke(() => {
				InTextBox.Text += "Player " + original + " changed name to " + name + "\n";
			});
		}

		private void ColorUpdated(VirusPlayer player, System.Drawing.Color color) {
			Dispatcher.Invoke(() => {
				InTextBox.Text += "Player " + player.Name + " changed color to " + color.Name + "\n";
			});
		}

		private void EveryoneReadyUpdated(bool ready) {
			Dispatcher.Invoke(() => {
				if (lobby.Master) {
					if (ready) {
						ReadyButton.IsEnabled = true;
					}
					else {
						ReadyButton.IsEnabled = false;
					}
				}
			});
		}

		private void PlayerConnected(VirusPlayer player) {
			Dispatcher.Invoke(() => {
				InTextBox.Text += "Player " + player.Name + " connected with color " + player.Color.Name + "\n";
			});
		}

		private void GameMove(VirusPlayer player, int x, int y, int dx, int dy) {
			Dispatcher.Invoke(() => {
				viruscontrol.NetworkMove(x, y, dx, dy);
			});
		}

		delegate void SetTextCallback(TextBlock control, string text);

		private void SetText(TextBlock control, string text) {
			if (control.Dispatcher.CheckAccess()) {
				control.Text = text;
			}
			else {
				SetTextCallback d = new SetTextCallback(SetText);
				control.Dispatcher.Invoke(d, new object[] { control, text });
			}
		}

		delegate void AddTextCallback(TextBlock control, string text);

		private void AddText(TextBlock control, string text) {
			if (control.Dispatcher.CheckAccess()) {
				control.Text += text;
			}
			else {
				AddTextCallback d = new AddTextCallback(AddText);
				control.Dispatcher.Invoke(d, new object[] { control, text });
			}
		}

		private void StartButton_Click(object sender, RoutedEventArgs e) {
			if (lobby.Master && ((String)StartButton.Content) == "Start Listening") {   // Listening
				MasterCheckbox.IsEnabled = false;
				lobby.StartListening();
				StartButton.Content = "Stop Listening";
			}
			else if (lobby.Master) {   // Disconnecting
				MasterCheckbox.IsEnabled = true;
				lobby.Disconnect();
				ReadyButton.IsEnabled = false;
				StartButton.Content = "Start Listening";
			}
			else if (((String)StartButton.Content) == "Connect") {   // Connecting

				IPAddress ip;
				if (IPAddress.TryParse(IpBox.Text, out ip)) {
					try {
						lobby.StartConnection(ip);
						StartButton.Content = "Disconnect";
						MasterCheckbox.IsEnabled = false;
						ReadyButton.IsEnabled = true;
					}
					catch { IpBox.Text = "Failed to connect"; }
				}
				else
					IpBox.Text = "Not valid IP";

			}
			else {   // Disconnecting
				MasterCheckbox.IsEnabled = true;
				StartButton.Content = "Connect";
				lobby.Disconnect();
				ReadyButton.IsEnabled = false;
			}
		}

		private void MasterCheckbox_Checked(object sender, RoutedEventArgs e) {
			StartButton.Content = "Start Listening";
			ReadyButton.Content = "Start Game";
			IpBox.IsEnabled = false;
			IpBox.Text = GetOwnIP();
			lobby.Master = true;
		}

		private void MasterCheckbox_UnChecked(object sender, RoutedEventArgs e) {
			StartButton.Content = "Connect";
			ReadyButton.Content = "Ready";
			IpBox.IsEnabled = true;
			IpBox.Text = "";
			lobby.Master = false;
		}

		private void SendButton_Click(object sender, RoutedEventArgs e) {
			if (messageBox.Text != "") {
				lobby.SendTextMessage(messageBox.Text);
				messageBox.Text = "";
			}
		}

		public string GetOwnIP() {
			string localIP = "?";
			IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
			foreach (IPAddress ip in host.AddressList) {
				if (ip.AddressFamily == AddressFamily.InterNetwork) {
					localIP = ip.ToString();
				}
			}
			return localIP;
		}

		private void Window_Loaded(object sender, RoutedEventArgs e) {
			// Create the interop host control.
			System.Windows.Forms.Integration.WindowsFormsHost host =
					new System.Windows.Forms.Integration.WindowsFormsHost();
			viruscontrol = new VirusInterfaceMod();
			host.Child = viruscontrol;
			this.VirusGrid.Children.Add(host);
		}

		void MainWindow_Closing(object sender, CancelEventArgs e) {
			lobby.Disconnect();
		}

		private void messageBox_keyDown(object sender, KeyEventArgs e) {
			if (e.Key == Key.Enter && messageBox.Text != "") {
				lobby.SendTextMessage(messageBox.Text);
				messageBox.Text = "";
			}
		}

		private void redColor_Checked(object sender, RoutedEventArgs e) {
			lobby.UpdatePlayer(System.Drawing.Color.FromName("Red"));
		}

		private void blueColor_Checked(object sender, RoutedEventArgs e) {
			lobby.UpdatePlayer(System.Drawing.Color.FromName("Blue"));
		}

		private void greenColor_Checked(object sender, RoutedEventArgs e) {
			lobby.UpdatePlayer(System.Drawing.Color.FromName("Green"));
		}

		private void blackColor_Checked(object sender, RoutedEventArgs e) {
			lobby.UpdatePlayer(System.Drawing.Color.FromName("Black"));
		}

		private void goldColor_Checked(object sender, RoutedEventArgs e) {
			lobby.UpdatePlayer(System.Drawing.Color.FromName("Gold"));
		}

		private void ReadyButton_Click(object sender, RoutedEventArgs e) {
			if (lobby.Master) {
				ReadyButton.IsEnabled = false;
				lobby.StartGame();
				viruscontrol.StartGame(new VirusNameSpace.Virus(lobby.PlayerCount, 10), PerformedMoveCallback, player.ID, lobby.AllPlayers);
			}
			else {
				bool ready = !player.Ready;
				if (ready) {
					ReadyLabel.Content = "Ready";
					ReadyButton.Content = "Not ready";
				}
				else {
					ReadyLabel.Content = "Not ready";
					ReadyButton.Content = "Ready";
				}

				lobby.UpdatePlayer(ready);
			}
		}

		public void PerformedMoveCallback(int x, int y, int dx, int dy) {
			lobby.SendGameMessage(x, y, dx, dy);
		}

		private void PlayerNameBox_LostFocus(object sender, RoutedEventArgs e) {
			if (player.Name != PlayerNameBox.Text)
				lobby.UpdatePlayer(PlayerNameBox.Text);
		}
	}
}
