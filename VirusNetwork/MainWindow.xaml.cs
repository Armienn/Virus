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

namespace VirusNetwork {
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window {
		private bool master = false;
		private TcpListener tcpListener;
		private Thread listenThread;
		List<TcpClient> clientList = new List<TcpClient>();
		VirusInterfaceMod viruscontrol;

		public MainWindow() {
			InitializeComponent();
			messageBox.KeyDown += new KeyEventHandler(messageBox_keyDown);
		}

		private void ListenForClients() {
			this.tcpListener.Start();

			while (true) {
				//blocks until a client has connected to the server
				TcpClient client = this.tcpListener.AcceptTcpClient();
				clientList.Add(client);

				//create a thread to handle communication 
				//with connected client
				Thread clientThread = new Thread(new ParameterizedThreadStart(HandleClientComm));
				clientThread.Start(client);
			}
		}

		private void HandleClientComm(object client) {
			TcpClient tcpClient = (TcpClient)client;
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
				AddText(InTextBox, intext);
				if (master) {
					byte[] buffer = encoder.GetBytes(intext);

					foreach (TcpClient ns in clientList) {
						ns.GetStream().Write(buffer, 0, buffer.Length);
						ns.GetStream().Flush();
					}
				}
			}

			tcpClient.Close();
			clientList.Remove(tcpClient);
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
				foreach (TcpClient ns in clientList) {
					ns.Close();
				}
				clientList = new List<TcpClient>();
				StartButton.Content = "Start Listening";
			}
			else if (((String)StartButton.Content)=="Connect") {   // Connecting
				
				IPAddress ip;
				if (IPAddress.TryParse(IpBox.Text, out ip)) {
					try {
						TcpClient client = new TcpClient();
						IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Parse(IpBox.Text), 3000);
						client.Connect(serverEndPoint);
						clientList.Add(client);

						Thread clientThread = new Thread(new ParameterizedThreadStart(HandleClientComm));
						clientThread.Start(client);

						StartButton.Content = "Disconnect";
						MasterCheckbox.IsEnabled = false;
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
				foreach (TcpClient ns in clientList) {
					ns.Close();
				}
				clientList = new List<TcpClient>();
			}
		}

		private void MasterCheckbox_Checked(object sender, RoutedEventArgs e) {
			master = true;
			StartButton.Content = "Start Listening";
			IpBox.IsEnabled = false;
			IpBox.Text = GetOwnIP();
		}

		private void MasterCheckbox_UnChecked(object sender, RoutedEventArgs e) {
			master = false;
			StartButton.Content = "Connect";
			IpBox.IsEnabled = true;
			IpBox.Text = "";
		}

		private void SendButton_Click(object sender, RoutedEventArgs e) {
			viruscontrol.StartGame(new VirusNameSpace.Virus(),);
			UnicodeEncoding encoder = new UnicodeEncoding();
			String message = PlayerNameBox.Text + ":\n  " + messageBox.Text + "\n";
			byte[] buffer = encoder.GetBytes(message);
			if(master)
				AddText(InTextBox, message);
			messageBox.Text = "";

			foreach (TcpClient ns in clientList) {
				ns.GetStream().Write(buffer, 0, buffer.Length);
				ns.GetStream().Flush();
			}
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
			foreach (TcpClient ns in clientList) {
				ns.Close();
			}
		}

		private void messageBox_keyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter && messageBox.Text != "")
			{
				UnicodeEncoding encoder = new UnicodeEncoding();
				String message = PlayerNameBox.Text + ":\n  " + messageBox.Text + "\n";
				byte[] buffer = encoder.GetBytes(message);
				if (master)
					AddText(InTextBox, message);
				messageBox.Text = "";

				foreach (TcpClient ns in clientList)
				{
					ns.GetStream().Write(buffer, 0, buffer.Length);
					ns.GetStream().Flush();
				}
			}
		}
	}
}
