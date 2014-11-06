using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using VirusNameSpace;
using VirusNameSpace.Agents;
using System.IO;

namespace VirusNetwork
{
	public partial class VirusInterfaceMod : UserControl
	{
		Virus virus;
		int tileSize;
		int x = 0, y = 0;
		bool immediateAI = false;
		bool immediateRunning = false;
		bool readyToMove = false;
		bool gameWon = false;
		String message = "Game on!";
		List<VirusPlayer> players = new List<VirusPlayer>();
		//Color[] colors;
		Agent[] agents;
		PerformedMoveCallback PerformedMove;
		UpdatePiecesCallback UpdatePieces;
		EndCallback End;
		string PlayerID;

		public delegate void PerformedMoveCallback(int x, int y, int dx, int dy);
		public delegate void UpdatePiecesCallback(int pieces);
		public delegate void EndCallback();

		public VirusInterfaceMod() {
			InitializeComponent();
		}

		public void StartGame(
			Virus virus, 
			PerformedMoveCallback callback, 
			UpdatePiecesCallback piecesCallback,
			EndCallback end,
			string id, 
			params VirusPlayer[] players) 
		{
			Random rand = new Random();
			PerformedMove = callback;
			UpdatePieces = piecesCallback;
			End = end;
			PlayerID = id;
			this.virus = virus;
			this.immediateAI = true;
			this.MouseClick += MouseClickHandler1;
			tileSize = 49;
			this.Size = new Size(
				virus.Size * tileSize + 17,
				virus.Size * tileSize + 55);
			int smallestSide = this.Size.Height < this.Size.Width ? this.Size.Height : this.Size.Width;
			tileSize = smallestSide / virus.Size;
			this.players.Add(new VirusPlayer("Player 0", "", Color.White));
			this.players.AddRange(players);
			while (this.players.Count < virus.Players + 1) {
				this.players.Add(new VirusPlayer("BruteAI","AI",Color.FromArgb(rand.Next(256), rand.Next(256), rand.Next(256))));
			}
			//Save("Lalalafil");
			agents = new Agent[this.players.Count];
			int n = 1;
			for (byte i = 1; i < this.players.Count; i++) {
				String p = this.players[i].Name;
				switch (p) {
					case "AIQ":
						agents[i] = new QAgent(i);
						if (File.Exists("TrainingData.Q") && File.Exists("TrainingData.N")) {
							((QAgent)agents[i]).Load("TrainingData");
							((QAgent)agents[i]).TurnOffExploration();
							((QAgent)agents[i]).TurnOffLearning();
						}
						//this.players[i].Name = "AI " + n;
						n++;
						break;
					case "AIMQ":
						agents[i] = new MemoryQAgent(i);
						if (File.Exists("TrainingData.Q") && File.Exists("TrainingData.N")) {
							((MemoryQAgent)agents[i]).Load("TrainingData");
							((MemoryQAgent)agents[i]).TurnOffExploration();
						}
						//this.players[i].Name = "AI " + n;
						n++;
						break;
					case "AIMinimax":
						agents[i] = new MinimaxAgent(4,i);
						//this.players[i].Name = "AI " + n;
						n++;
						break;
					case "AIMiniMaxMix":
						if (File.Exists("TrainingData.Q"))
							agents[i] = new MiniMaxMixAgent("TrainingData", 2, i);
						else
							agents[i] = new BruteForceAgent(i);
						//this.players[i].Name = "AI " + n;
						n++;
						break;
					case "AIMixed":
						agents[i] = new MixedAgent(0.5,false,i);
						//this.players[i].Name = "AI " + n;
						n++;
						break;
					case "AIBrute":
						agents[i] = new BruteForceAgent(i);
						//this.players[i].Name = "AI " + n;
						n++;
						break;
					case "AIRandom":
						agents[i] = new RandomAgent(i);
						//this.players[i].Name = "AI " + n;
						n++;
						break;
					case "AISimple":
						agents[i] = new SimpleAgent(i);
						//this.players[i].Name = "AI " + n;
						n++;
						break;
				}
			}

			message = this.players[1].Name + "'s turn";

			/*colors = new Color[virus.Players + 1];
			colors[0] = Color.White;
			colors[1] = Color.FromArgb(128, 160, 255);
			colors[2] = Color.FromArgb(96, 255, 96);
			if(virus.Players > 2)
				colors[3] = Color.FromArgb(255, 96, 96);
			if(virus.Players > 3)
				colors[4] = Color.FromArgb(255, 255, 64);
			
			for (int i = 5; i <= virus.Players; i++)
				colors[i] = Color.FromArgb(rand.Next(256), rand.Next(256), rand.Next(256));*/
		}

		protected override void OnPaint(PaintEventArgs e) {
			if (virus != null) {
				Graphics g = e.Graphics;
				Pen pen = new Pen(Color.Black);
				int boardlength = virus.Size;
				int smallestSide = this.Size.Height - 20 < this.Size.Width ? this.Size.Height - 20 : this.Size.Width;
				tileSize = smallestSide/virus.Size;

				int xStart = e.ClipRectangle.Left / tileSize;
				int yStart = e.ClipRectangle.Top / tileSize;
				int xEnd = e.ClipRectangle.Right / tileSize;
				int yEnd = e.ClipRectangle.Bottom / tileSize;
				xStart = xStart < 0 ? 0 : xStart;
				yStart = yStart < 0 ? 0 : yStart;

				if (xStart < boardlength && yStart < boardlength) {

					for (int i = xStart; i < boardlength && i < xEnd; i++) {
						for (int j = yStart; j < boardlength && j < yEnd; j++) {

							g.DrawRectangle(pen, i * tileSize, j * tileSize, tileSize, tileSize);

							pen.Color = players[virus[i, boardlength - j - 1]].Color;
							g.FillRectangle(pen.Brush, i * tileSize + 1, j * tileSize + 1, tileSize - 1, tileSize - 1);

							if (readyToMove && i == x && (boardlength - j - 1) == y) {
								pen.Color = Color.White;
								g.DrawRectangle(pen, i * tileSize + 1, j * tileSize + 1, tileSize - 2, tileSize - 2);
							}

							pen.Color = Color.Black;
						}
					}
				}

				g.FillRectangle(
					new Pen(players[virus.CurrentPlayer].Color).Brush,
					new Rectangle(0, boardlength * tileSize + 1, this.Width, 20)
					);
				g.DrawString(
					message,
					new Font(FontFamily.GenericSansSerif, 10, FontStyle.Regular),
					pen.Brush,
					new Rectangle(0, boardlength * tileSize, 200, 20)
					);
				if ((!immediateAI) && (players[virus.CurrentPlayer].Name.StartsWith("AI")) && players[virus.CurrentPlayer].ID == PlayerID) {
					g.FillRectangle(
						new Pen(Color.LightGray).Brush,
						new Rectangle(this.Width - 60, boardlength * tileSize + 2, 43, 13)
						);
					g.DrawString(
						"Run AI",
						new Font(FontFamily.GenericSansSerif, 10, FontStyle.Regular),
						pen.Brush,
						new Rectangle(this.Width - 60, boardlength * tileSize, 60, 20)
						);
				}
			}
		}

		private void ImmediateHandler() {
			immediateRunning = true;
			while (players[virus.CurrentPlayer].Name.StartsWith("AI") && players[virus.CurrentPlayer].ID == PlayerID) {
				AINetworkMove();

				if (gameWon) {
					foreach (Agent a in agents) {
						if (a != null) {
							a.EndGame(virus);
						}
					}
					gameWon = false;
					//virus = new Virus(virus.Players, virus.Size);
					immediateAI = false;
					this.Refresh();
				}
				if (!immediateAI)
					return;
			}
			immediateRunning = false;
		}

		private void MouseClickHandler1(Object sender, MouseEventArgs args) {
			if (players[virus.CurrentPlayer].Name.StartsWith("AI")) {
				if (!immediateAI) {
					if ((args.X > this.Width - 60) && (args.Y > virus.Size * tileSize)) {
						if (players[virus.CurrentPlayer].ID == PlayerID) {
							AINetworkMove();
						}
					}
				}
			}
			else if (!immediateRunning) {
				int tileX = args.X / tileSize;
				int tileY = virus.Size - (args.Y / tileSize) - 1;
				PlayerMove(tileX, tileY);
			}

			if (gameWon) {
				foreach (Agent a in agents) {
					if (a != null) {
						a.EndGame(virus);
					}
				}
				gameWon = false;
				virus = new Virus(virus.Players, virus.Size);
				this.Refresh();
			}

			if (immediateAI && !immediateRunning) {
				ImmediateHandler();
			}
		}

		private void PlayerMove(int tileX, int tileY) {
			byte piece = 0;
			Rectangle areaToUpdate;

			if (tileX >= virus.Size || tileY >= virus.Size || tileX < 0 || tileY < 0)
				return;

			if (readyToMove) {
				if (players[virus.CurrentPlayer].ID == PlayerID) {
					try {
						virus.Move(x, y, tileX, tileY);
						piece = virus.Winner;
						int smallestX = x < tileX ? x : (tileX - 1);
						int smallestY = y < tileY ? y : (tileY - 1);
						int greatestX = x > tileX ? x : (tileX + 1);
						int greatestY = y > tileY ? y : (tileY + 1);
						Point startpoint = new Point(tileSize * smallestX, tileSize * (virus.Size - greatestY - 1));
						Size size = new Size((greatestX - smallestX + 1) * tileSize, (greatestY - smallestY + 1) * tileSize);
						areaToUpdate = new Rectangle(startpoint, size);
						PerformedMove(x, y, tileX, tileY);
					}
					catch {
						areaToUpdate = new Rectangle(x * tileSize, (virus.Size - y - 1) * tileSize, tileSize, tileSize);
					}
				}
				else {
					areaToUpdate = new Rectangle(x * tileSize, (virus.Size - y - 1) * tileSize, tileSize, tileSize);
				}
				readyToMove = false;
			}
			else {
				x = tileX;
				y = tileY;
				if(virus[x,y] == virus.CurrentPlayer)
					readyToMove = true;
				areaToUpdate = new Rectangle(x * tileSize, (virus.Size - y - 1) * tileSize, tileSize, tileSize);
			}
			this.Invalidate(areaToUpdate); //tile area
			Update();

			CheckForWinner(piece);
		}

		public void NetworkMove(int x, int y, int dx, int dy) {
			if (gameWon)
				return;

			int tileX = dx;
			int tileY = dy;
			byte piece = 0;
			Rectangle areaToUpdate;

			virus.Move(x, y, tileX, tileY);
			piece = virus.Winner;
			int smallestX = x < tileX ? x : (tileX - 1);
			int smallestY = y < tileY ? y : (tileY - 1);
			int greatestX = x > tileX ? x : (tileX + 1);
			int greatestY = y > tileY ? y : (tileY + 1);
			Point startpoint = new Point(tileSize * smallestX, tileSize * (virus.Size - greatestY - 1));
			Size size = new Size((greatestX - smallestX + 1) * tileSize, (greatestY - smallestY + 1) * tileSize);
			areaToUpdate = new Rectangle(startpoint, size);
			this.Invalidate(areaToUpdate); //tile area
			Update();

			CheckForWinner(piece);
			if (immediateAI && !immediateRunning) {
				ImmediateHandler();
			}
		}

		private void AIMove() {
			VirusNameSpace.Move a = agents[virus.CurrentPlayer].Move(virus);

			if (gameWon)
				return;

			int x = a.sx;
			int y = a.sy;
			int tileX = a.ex;
			int tileY = a.ey;
			byte piece = 0;
			Rectangle areaToUpdate;

			virus.Move(x, y, tileX, tileY);
			piece = virus.Winner;
			int smallestX = x < tileX ? x : (tileX - 1);
			int smallestY = y < tileY ? y : (tileY - 1);
			int greatestX = x > tileX ? x : (tileX + 1);
			int greatestY = y > tileY ? y : (tileY + 1);
			Point startpoint = new Point(tileSize * smallestX, tileSize * (virus.Size - greatestY - 1));
			Size size = new Size((greatestX - smallestX + 1) * tileSize, (greatestY - smallestY + 1) * tileSize);
			areaToUpdate = new Rectangle(startpoint, size);
			this.Invalidate(areaToUpdate); //tile area
			Update();

			CheckForWinner(piece);
		}

		private void AINetworkMove() {
			VirusNameSpace.Move a = agents[virus.CurrentPlayer].Move(virus);

			if (gameWon)
				return;

			int x = a.sx;
			int y = a.sy;
			int tileX = a.ex;
			int tileY = a.ey;
			byte piece = 0;
			Rectangle areaToUpdate;

			virus.Move(x, y, tileX, tileY);
			PerformedMove(x, y, tileX, tileY);
			piece = virus.Winner;
			int smallestX = x < tileX ? x : (tileX - 1);
			int smallestY = y < tileY ? y : (tileY - 1);
			int greatestX = x > tileX ? x : (tileX + 1);
			int greatestY = y > tileY ? y : (tileY + 1);
			Point startpoint = new Point(tileSize * smallestX, tileSize * (virus.Size - greatestY - 1));
			Size size = new Size((greatestX - smallestX + 1) * tileSize, (greatestY - smallestY + 1) * tileSize);
			areaToUpdate = new Rectangle(startpoint, size);
			this.Invalidate(areaToUpdate); //tile area
			Update();

			CheckForWinner(piece);
			if(agents[virus.CurrentPlayer] is MemoryQAgent){
				((MemoryQAgent)agents[virus.CurrentPlayer]).ForgetShortTerm();
			}
		}

		private void CheckForWinner(byte winner) {
			this.Invalidate(new Rectangle(0, virus.Size * tileSize + 1, this.Width, 40)); //message area

			message = players[winner].Name;

			if (message != "Player 0") {
				gameWon = true;
				String w = message;
				message += " has won";
				this.Update();

				// calculating some data for the messagebox
				int[] pieces = new int[virus.Players + 1];

				for (int i = 0; i < virus.Size; i++) {
					for (int j = 0; j < virus.Size; j++) {
						pieces[virus[i, j]]++;
					}
				}

				String s = "Free: " + pieces[0] + "\n";
				for (int i = 1; i <= virus.Players; i++) {
					s += players[i].Name + ": " + pieces[i] + "\n";
					if (players[i].ID == PlayerID)
					{
						if(UpdatePieces != null)
							UpdatePieces(pieces[i]);
					}
				}
				MessageBox.Show(w + " is the winner\n" + s);
				if (End != null)
					End();
			}
			else {
				message = players[virus.CurrentPlayer].Name;
				message += "'s turn";
				this.Update();
			}
		}
	}
}
