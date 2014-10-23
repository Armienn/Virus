﻿using System;
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

namespace VirusNetwork {
	class PlayerClient {
		public readonly TcpClient TcpClient;
		public readonly String ID;
		public Color Color;
		public String Name;
		public bool Ready;

		public PlayerClient(TcpClient cl, String id) {
			this.TcpClient = cl;
			this.ID = id;
			this.Color = Colors.Red;
			this.Name = "PlayerX";
			this.Ready = false;
		}
	}

	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window {
		private bool master = false;
		private bool ready = false;
		private TcpListener tcpListener;
		private Thread listenThread;
		List<PlayerClient> playerList = new List<PlayerClient>();
		VirusInterfaceMod viruscontrol;
		Random rand = new Random();

		public MainWindow() {
			InitializeComponent();
			messageBox.KeyDown += new KeyEventHandler(messageBox_keyDown);
		}

		private void ListenForClients() {
			this.tcpListener.Start();

			while (true) {
				//blocks until a client has connected to the server
				PlayerClient client = new PlayerClient(this.tcpListener.AcceptTcpClient(), rand.Next().ToString());
				playerList.Add(client);

				//create a thread to handle communication 
				//with connected client
				Thread clientThread = new Thread(new ParameterizedThreadStart(HandleClientComm));
				clientThread.Start(client);
			}
		}

		private void HandleClientComm(object client) {
			PlayerClient player = (PlayerClient)client;
			TcpClient tcpClient = player.TcpClient;
			NetworkStream clientStream = tcpClient.GetStream();

			byte[] message = new byte[4096];
			int bytesRead;

			while (true) {
				bytesRead = 0;

				try {
					//blocks until a client sends a message
					bytesRead = clientStream.Read(message, 0, 4096);
				}
				catch {
					//a socket error has occured
					break;
				}

				if (bytesRead == 0) {
					//the client has disconnected from the server
					break;
				}

				//message has successfully been received
				UnicodeEncoding encoder = new UnicodeEncoding();
				String intext = encoder.GetString(message, 0, bytesRead);
				String messagetype = intext.Substring(0, 3);
				if(intext.Length > 3)
					intext = intext.Substring(3);
				switch (messagetype) {
					case "STG": // STart Game
						this.Dispatcher.Invoke(() => { viruscontrol.StartGame(new VirusNameSpace.Virus()); });
						this.Dispatcher.Invoke(() => { ReadyButton.IsEnabled = false; });
						break;
					case "MES": // MESsage
						if (master) {
							AddText(InTextBox, player.Name + ":\n  " + intext + "\n");
							byte[] buffer = encoder.GetBytes("MES" + player.Name + ":\n  " + intext + "\n");
							foreach (PlayerClient pc in playerList) {
								TcpClient ns = pc.TcpClient;
								ns.GetStream().Write(buffer, 0, buffer.Length);
								ns.GetStream().Flush();
							}
						}
						else {
							AddText(InTextBox, intext);
						}
						break;
					case "RDY": // ReaDY
						player.Ready = true;
						break;
					case "NRD": // Not ReaDy
						player.Ready = false;
						break;
					case "NME": // NaME
						if (master) {
							string temp = player.Name + " changed name to:  " + intext + "\n";
							player.Name = intext;
							AddText(InTextBox, temp);
							byte[] buffer = encoder.GetBytes("MES" + temp);
							foreach (PlayerClient pc in playerList) {
								TcpClient ns = pc.TcpClient;
								ns.GetStream().Write(buffer, 0, buffer.Length);
								ns.GetStream().Flush();
							}
						}
						else {
							AddText(InTextBox, "ERROR: Got name message while not master");
						}
						break;
					case "CLR": // CoLoR
						if (master) {
							string temp = player.Name + " changed color to:  " + intext + "\n";
							switch (intext) {
								case "Red":
									player.Color = Colors.Red;
									break;
								case "Blue":
									player.Color = Colors.Blue;
									break;
								case "Black":
									player.Color = Colors.Black;
									break;
								case "Green":
									player.Color = Colors.Green;
									break;
								case "Gold":
									player.Color = Colors.Gold;
									break;
							}
							AddText(InTextBox, temp);
							byte[] buffer = encoder.GetBytes("MES" + temp);
							foreach (PlayerClient pc in playerList) {
								TcpClient ns = pc.TcpClient;
								ns.GetStream().Write(buffer, 0, buffer.Length);
								ns.GetStream().Flush();
							}
						}
						else {
							AddText(InTextBox, "ERROR: Got color message while not master");
						}
						break;
				}
				if (master) {
					ready = true;
					foreach (PlayerClient pc in playerList) {
						if (!pc.Ready)
							ready = false;
					}
					if (ready) {
						this.Dispatcher.Invoke(() => { ReadyButton.IsEnabled = true; });
					}
					else {
						this.Dispatcher.Invoke(() => { ReadyButton.IsEnabled = false; });
					}
				}
			}

			tcpClient.Close();
			playerList.Remove(player);
			Thread.CurrentThread.Abort();
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
			if (master && ((String)StartButton.Content) == "Start Listening") {   // Listening
				MasterCheckbox.IsEnabled = false;
				this.tcpListener = new TcpListener(IPAddress.Any, 3000);
				this.listenThread = new Thread(new ThreadStart(ListenForClients));
				this.listenThread.Start();
				StartButton.Content = "Stop Listening";
			}
			else if (master) {   // Disconnecting
				MasterCheckbox.IsEnabled = true;
				tcpListener.Stop();
				listenThread.Abort();
				foreach (PlayerClient pc in playerList) {
					TcpClient ns = pc.TcpClient;
					ns.Close();
				}
				playerList = new List<PlayerClient>();
				StartButton.Content = "Start Listening";
			}
			else if (((String)StartButton.Content)=="Connect") {   // Connecting
				
				IPAddress ip;
				if (IPAddress.TryParse(IpBox.Text, out ip)) {
					try {
						TcpClient client = new TcpClient();
						IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Parse(IpBox.Text), 3000);
						client.Connect(serverEndPoint);
						PlayerClient pclient = new PlayerClient(client, "master");
						playerList.Add(pclient);

						Thread clientThread = new Thread(new ParameterizedThreadStart(HandleClientComm));
						clientThread.Start(pclient);

						StartButton.Content = "Disconnect";
						MasterCheckbox.IsEnabled = false;
						ReadyButton.IsEnabled = true;
					}
					catch (Exception exc) {
						IpBox.Text = "Failed to connect";
					}
				}
				else {
					IpBox.Text = "Not valid IP";
				}
				
			}
			else {   // Disconnecting
				MasterCheckbox.IsEnabled = true;
				StartButton.Content = "Connect";
				foreach (PlayerClient pc in playerList) {
					TcpClient ns = pc.TcpClient;
					ns.Close();
				}
				playerList = new List<PlayerClient>();
			}
		}

		private void MasterCheckbox_Checked(object sender, RoutedEventArgs e) {
			master = true;
			StartButton.Content = "Start Listening";
			ReadyButton.Content = "Start Game";
			IpBox.IsEnabled = false;
			IpBox.Text = GetOwnIP();
		}

		private void MasterCheckbox_UnChecked(object sender, RoutedEventArgs e) {
			master = false;
			StartButton.Content = "Connect";
			ReadyButton.Content = "Ready";
			IpBox.IsEnabled = true;
			IpBox.Text = "";
		}

		private void SendMessage() {
			UnicodeEncoding encoder = new UnicodeEncoding();
			String message = messageBox.Text;
			if (master) {
				message = PlayerNameBox.Text + ":\n  " + messageBox.Text + "\n";
				AddText(InTextBox, message);
			}
				
			messageBox.Text = "";
			byte[] buffer = encoder.GetBytes("MES" + message);

			foreach (PlayerClient pc in playerList) {
				TcpClient ns = pc.TcpClient;
				ns.GetStream().Write(buffer, 0, buffer.Length);
				ns.GetStream().Flush();
			}
		}

		private void SendButton_Click(object sender, RoutedEventArgs e) {
			SendMessage();
		}

		public string GetOwnIP()
		{
			string localIP = "?";
			IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
			foreach (IPAddress ip in host.AddressList)
			{
				if (ip.AddressFamily == AddressFamily.InterNetwork)
				{
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
			if (master) {
				if (tcpListener != null) {
					tcpListener.Stop();
					listenThread.Abort();
				}
			}
			foreach (PlayerClient pc in playerList) {
				TcpClient ns = pc.TcpClient;
				ns.Close();
			}
		}

		private void messageBox_keyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter && messageBox.Text != "")
			{
				SendMessage();
			}
		}

		private void redColor_Checked(object sender, RoutedEventArgs e)
		{
			SendColorMessage("Red");
		}

		private void blueColor_Checked(object sender, RoutedEventArgs e)
		{
			SendColorMessage("Blue");
		}

		private void greenColor_Checked(object sender, RoutedEventArgs e)
		{
			SendColorMessage("Green");
		}

		private void blackColor_Checked(object sender, RoutedEventArgs e)
		{
			SendColorMessage("Black");
		}

		private void goldColor_Checked(object sender, RoutedEventArgs e)
		{
			SendColorMessage("Gold");
		}

		private void SendColorMessage(String color) {
			UnicodeEncoding encoder = new UnicodeEncoding();
			String message = "CLR" + color;
			byte[] buffer = encoder.GetBytes(message);

			foreach (PlayerClient pc in playerList) {
				TcpClient ns = pc.TcpClient;
				ns.GetStream().Write(buffer, 0, buffer.Length);
				ns.GetStream().Flush();
			}
		}

		private void ReadyButton_Click(object sender, RoutedEventArgs e) {
			if (master) {
				viruscontrol.StartGame(new VirusNameSpace.Virus());
				UnicodeEncoding encoder = new UnicodeEncoding();
				String message = "STG";
				byte[] buffer = encoder.GetBytes(message);


				foreach (PlayerClient pc in playerList) {
					TcpClient ns = pc.TcpClient;
					ns.GetStream().Write(buffer, 0, buffer.Length);
					ns.GetStream().Flush();
				}
			}
			else {
				ready = !ready;
				if (ready) {
					ReadyLabel.Content = "Ready";
					ReadyButton.Content = "Not ready";
				}
				else {
					ReadyLabel.Content = "Not ready";
					ReadyButton.Content = "Ready";
				}

				UnicodeEncoding encoder = new UnicodeEncoding();
				String message = ready ? "RDY" : "NRD";
				byte[] buffer = encoder.GetBytes(message);

				foreach (PlayerClient pc in playerList) {
					TcpClient ns = pc.TcpClient;
					ns.GetStream().Write(buffer, 0, buffer.Length);
					ns.GetStream().Flush();
				}
			}
		}

		private void PlayerNameBox_LostFocus(object sender, RoutedEventArgs e) {
			if (!master && PlayerNameBox.Text!=PlayerNameBox.Text) {
				UnicodeEncoding encoder = new UnicodeEncoding();
				String message = "NME" + PlayerNameBox.Text;
				byte[] buffer = encoder.GetBytes(message);

				foreach (PlayerClient pc in playerList) {
					TcpClient ns = pc.TcpClient;
					ns.GetStream().Write(buffer, 0, buffer.Length);
					ns.GetStream().Flush();
				}
			}
		}
	}
}
