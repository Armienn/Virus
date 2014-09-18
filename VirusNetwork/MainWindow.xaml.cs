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
		private TcpListener tcpListener;
		private Thread listenThread;

		public MainWindow() {
			InitializeComponent();
			this.tcpListener = new TcpListener(IPAddress.Any, 3000);
			this.listenThread = new Thread(new ThreadStart(ListenForClients));
			this.listenThread.Start();
		}

		private void StartButton_Click(object sender, RoutedEventArgs e) {
			TcpClient client = new TcpClient();
			IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Parse(IpBox.Text), int.Parse(PortBox.Text));
			client.Connect(serverEndPoint);
			NetworkStream clientStream = client.GetStream();
			ASCIIEncoding encoder = new ASCIIEncoding();
			byte[] buffer = encoder.GetBytes("Hello");

			clientStream.Write(buffer, 0, buffer.Length);
			clientStream.Flush();
			clientStream.Close();
		}
	}
}
