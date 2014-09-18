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

namespace VirusNetwork {
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window {
		private bool master = false;
		private TcpListener tcpListener;
		private Thread listenThread;
		NetworkStream clientStream;

		public MainWindow() {
			InitializeComponent();
			
		}

		private void ListenForClients() {
			this.tcpListener.Start();

			while (true) {
				//blocks until a client has connected to the server
				TcpClient client = this.tcpListener.AcceptTcpClient();

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
				ASCIIEncoding encoder = new ASCIIEncoding();
				SetText(InTextBox, encoder.GetString(message, 0, bytesRead));
				//System.Diagnostics.Debug.WriteLine(encoder.GetString(message, 0, bytesRead));
			}

			tcpClient.Close();
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

		private void StartButton_Click(object sender, RoutedEventArgs e) {
			if (master) {
				this.tcpListener = new TcpListener(IPAddress.Any, 3000);
				this.listenThread = new Thread(new ThreadStart(ListenForClients));
				this.listenThread.Start();
				StartButton.Content = "Listening";
				StartButton.IsEnabled = false;
			}
			else if (StartButton.Content=="Connect") {
				TcpClient client = new TcpClient();
				IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Parse(IpBox.Text), 3000);
				client.Connect(serverEndPoint);
				clientStream = client.GetStream();
				
				Thread clientThread = new Thread(new ParameterizedThreadStart(HandleClientComm));
				clientThread.Start(client);

				StartButton.Content = "Disconnect";
				
				/*ASCIIEncoding encoder = new ASCIIEncoding();
				byte[] buffer = encoder.GetBytes("Hello");

				clientStream.Write(buffer, 0, buffer.Length);
				clientStream.Flush();
				clientStream.Close();*/
			}
			else {
				clientStream.Close();
			}
		}

		private void MasterCheckbox_Checked(object sender, RoutedEventArgs e) {
			master = true;
			StartButton.Content = "Start Listening";
			IpBox.IsEnabled = false;
			IpBox.Text = "x.x.x.x";
		}

		private void MasterCheckbox_UnChecked(object sender, RoutedEventArgs e) {
			master = false;
			StartButton.Content = "Connect";
			IpBox.IsEnabled = true;
			IpBox.Text = "";
		}

		private void SendButton_Click(object sender, RoutedEventArgs e) {
			ASCIIEncoding encoder = new ASCIIEncoding();
			byte[] buffer = encoder.GetBytes("Hello");

			clientStream.Write(buffer, 0, buffer.Length);
			clientStream.Flush();
		}
	}
}
