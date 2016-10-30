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

namespace VirusGUI
{
	public partial class VirusInterface : Form
	{
		Virus virus;
		int tileSize;
		int x = 0, y = 0;
		readonly bool immediateAI = false;
		bool immediateRunning = false;
		bool readyToMove = false;
		bool gameWon = false;
		String message = "Game on!";
		List<String> names = new List<String>();
		Color[] colors;
		Agent[] agents;

		public VirusInterface(Virus virus, int tilesize = 20, bool immediateAI = false, params String[] names) {
			InitializeComponent();
			this.virus = virus;
			this.tileSize = tilesize;
			this.immediateAI = immediateAI;
			this.MouseClick += MouseClickHandler1;
			this.Size = new Size(
				virus.Size * tileSize + 17,
				virus.Size * tileSize + 55);
			this.names.Add("Player 0");
			this.names.AddRange(names);
			while (this.names.Count < virus.Players + 1) {
				this.names.Add("Player " + this.names.Count);
			}
			//Save("Lalalafil");
			agents = new Agent[this.names.Count];
			int n = 1;
			for (byte i = 1; i < this.names.Count; i++) {
				String p = this.names[i];
				switch (p) {
					case "QAI":
						agents[i] = new QAgent(i);
						if (File.Exists("TrainingData.Q") && File.Exists("TrainingData.N")) {
							((QAgent)agents[i]).Load("TrainingData");
							((QAgent)agents[i]).TurnOffExploration();
							((QAgent)agents[i]).TurnOffLearning();
						}
						this.names[i] = "AI " + n;
						n++;
						break;
					case "AnnAI":
						agents[i] = new AnnAgent(virus.Size, i);
						this.names[i] = "AI " + n;
						n++;
						break;
					case "MinimaxAI":
						agents[i] = new MinimaxAgent(4,i);
						this.names[i] = "AI " + n;
						n++;
						break;
					case "MiniMaxMixAI":
						if (File.Exists("TrainingData.Q"))
							agents[i] = new MiniMaxMixAgent("TrainingData", 2, i);
						else
							agents[i] = new BruteForceAgent(i);
						this.names[i] = "AI " + n;
						n++;
						break;
					case "MixedAI":
						agents[i] = new MixedAgent(0.5,false,i);
						this.names[i] = "AI " + n;
						n++;
						break;
					case "BruteAI":
						agents[i] = new BruteForceAgent(i);
						this.names[i] = "AI " + n;
						n++;
						break;
					case "RandomAI":
						agents[i] = new RandomAgent(i);
						this.names[i] = "AI " + n;
						n++;
						break;
					case "SimpleAI":
						agents[i] = new SimpleAgent(i);
						this.names[i] = "AI " + n;
						n++;
						break;
				}
			}

			message = this.names[1] + "'s turn";

			colors = new Color[virus.Players + 1];
			colors[0] = Color.White;
			colors[1] = Color.FromArgb(128, 160, 255);
			colors[2] = Color.FromArgb(96, 255, 96);
			if(virus.Players > 2)
				colors[3] = Color.FromArgb(255, 96, 96);
			if(virus.Players > 3)
				colors[4] = Color.FromArgb(255, 255, 64);
			Random rand = new Random();
			for (int i = 5; i <= virus.Players; i++)
				colors[i] = Color.FromArgb(rand.Next(256), rand.Next(256), rand.Next(256));
		}

		protected override void OnPaint(PaintEventArgs e) {
			Graphics g = e.Graphics;
			Pen pen = new Pen(Color.Black);
			int boardlength = virus.Size;

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

						pen.Color = colors[virus[i, boardlength - j - 1]];
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
				new Pen(colors[virus.CurrentPlayer]).Brush,
				new Rectangle(0, boardlength * tileSize + 1, this.Width, 20)
				);
			g.DrawString(
				message,
				new Font(FontFamily.GenericSansSerif, 10, FontStyle.Regular),
				pen.Brush,
				new Rectangle(0, boardlength * tileSize, 200, 20)
				);
			if ((!immediateAI) && (names[virus.CurrentPlayer].StartsWith("AI"))) {
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

		private void ImmediateHandler() {
			immediateRunning = true;
			while (names[virus.CurrentPlayer].StartsWith("AI")) {
				AIMove();

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
				if (!immediateAI)
					return;
			}
			immediateRunning = false;
		}

		private void MouseClickHandler1(Object sender, MouseEventArgs args) {
			if (names[virus.CurrentPlayer].StartsWith("AI")) {
				if (!immediateAI) {
					if ((args.X > this.Width - 60) && (args.Y > virus.Size * tileSize)) {
						AIMove();
					}
				}
			}
			else if (!immediateRunning) {
				PlayerMove(args);
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

		private void PlayerMove(MouseEventArgs args) {
			int tileX = args.X / tileSize;
			int tileY = virus.Size - (args.Y / tileSize) - 1;
			byte piece = 0;
			Rectangle areaToUpdate;

			if (tileX >= virus.Size || tileY >= virus.Size || tileX < 0 || tileY < 0)
				return;

			if (readyToMove) {
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
				}
				catch {
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

		private void CheckForWinner(byte winner) {
			this.Invalidate(new Rectangle(0, virus.Size * tileSize + 1, this.Width, 40)); //message area

			message = names[winner];

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
					s += names[i] + ": " + pieces[i] + "\n";
				}
				//MessageBox.Show(w + " is the winner\n" + s);
			}
			else {
				message = names[virus.CurrentPlayer];
				message += "'s turn";
				this.Update();
			}
		}
	}
}
