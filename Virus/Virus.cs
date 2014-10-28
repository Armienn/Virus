using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace VirusNameSpace
{
	public class Virus
	{
		private byte[,] board;
		private readonly byte players;
		public byte Players { get { return players; } }
		public byte CurrentPlayer { get; private set; }
		public byte Winner { get; private set; }
		public int Size { get { return board.GetLength(0); } }
		private bool[] skip;

		public bool SaveReplay { get; set; }
		private string replayfile = "game0";

		public byte this[int x, int y] {
			get {
				return board[x, y];
			}
		}

		public Virus(int players = 2, int x = 5) {
			if (x < 3)
				throw new ArgumentException("Too small a board");
			if (players < 2)
				throw new ArgumentException("Too few players");
			if(players > x*x || players > 255)
				throw new ArgumentException("Too many players");
			this.players = (byte)players;
			SaveReplay = false;
			int n = 0;
			while (File.Exists("game" + n)) {
				n++;
			}
			replayfile = "game" + n;
			skip = new bool[players + 1];

			board = new byte[x, x];
			for (int i = 0; i < x; i++) {
				for (int j = 0; j < x; j++) {
					board[i, j] = 0;
				}
			}

			board[0, x - 1] = 1;
			board[x - 1, x - 1] = 2;
			if (players == 2) {
				board[0, 0] = 2;
				board[x - 1, 0] = 1;
			}
			else if (players == 3) {
				board[0, 0] = 3;
			}
			else if (players > 3) {
				board[0, 0] = 3;
				board[x - 1, 0] = 4;
			}
			if (players > 4) {
				Random random = new Random();
				for (byte i = 5; i <= players; i++) {
					int xx = random.Next(x);
					int yy = random.Next(x);
					while (board[xx, yy] != 0) {
						xx = random.Next(x);
						yy = random.Next(x);
					}
					board[xx, yy] = i;
				}
			}

			CurrentPlayer = 1;
			Winner = 0;
		}

		public void Move(int x, int y, int destX, int destY) {
			int difx = x - destX;
			int dify = y - destY;

			if (!(IsWithinBoard(x) && IsWithinBoard(y) && IsWithinBoard(destX) && IsWithinBoard(destY)))
				throw new ArgumentException("Position is not within the board");
			if (board[x, y] != CurrentPlayer)
				throw new ArgumentException("Position does not have the current player's piece");
			if (board[destX, destY] != 0)
				throw new ArgumentException("Position is already taken");

			if (difx > 2 || difx < -2 || dify < -2 || dify > 2) {
				throw new ArgumentException("Illegal move");
			}
			else if (difx > 1 || difx < -1 || dify < -1 || dify > 1) {
				// long move
				board[x, y] = 0;
			}

			if (SaveReplay) {
				StreamWriter w = new StreamWriter(replayfile, true);
				w.WriteLine("{0} {1} {2} {3}", x, y, destX, destY);
				w.Close();
			}

			board[destX, destY] = CurrentPlayer;
			TurnPiecesAround(destX, destY);

			Winner = GetWinner();
			if (Winner != 0)
				return;
			CurrentPlayer++;
			if (CurrentPlayer > players)
				CurrentPlayer = 1;
			while (skip[CurrentPlayer]) {
				CurrentPlayer++;
				if (CurrentPlayer > players)
					CurrentPlayer = 1;
			}
		}

		private byte GetWinner() {
			bool[] canMove = new bool[players + 1];
			int[] pieces = new int[players + 1];
			int moveable = 0;

			for (int i = 0; i < board.GetLength(0); i++) {
				for (int j = 0; j < board.GetLength(0); j++) {
					pieces[board[i, j]]++;

					if (board[i, j] == 0) {

						int startx = i - 2;
						int slutx = i + 2;
						int starty = j - 2;
						int sluty = j + 2;
						if (startx < 0)
							startx = 0;
						if (starty < 0)
							starty = 0;
						if (slutx >= board.GetLength(0))
							slutx = board.GetLength(0) - 1;
						if (sluty >= board.GetLength(0))
							sluty = board.GetLength(0) - 1;

						for (int n = startx; n <= slutx; n++) {
							for (int m = starty; m <= sluty; m++) {
								canMove[board[n, m]] = true;

								moveable = 0;
								for (int k = 1; k <= players; k++) {
									if (canMove[k])
										moveable++;
								}
								if (moveable == players)
									break;
							}
							if (moveable == players)
								break;
						}
					}
					if (moveable == players)
						break;
				}
				if (moveable == players)
					break;
			}

			for (int i = 1; i <= players; i++) {
				skip[i] = !canMove[i];
			}


			if (moveable <= 1) {

				int lastplayer = 0;
				for (int i = 1; i < canMove.Length; i++) {
					if (canMove[i]) {
						lastplayer = i;
						break;
					}
				}

				pieces[lastplayer] += pieces[0]; //lægger de frie felter til den sidste spiller, der kan bevæge sig

				int max = 0;
				byte winner = 0;

				for (byte i = 1; i < pieces.Length; i++) {
					if (pieces[i] > max) {
						max = pieces[i];
						winner = i;
					}
				}

				return winner;
			}
			else {
				return 0;
			}
		}

		private void TurnPiecesAround(int x, int y) {
			int startx = x - 1;
			int slutx = x + 1;
			int starty = y - 1;
			int sluty = y + 1;
			if (startx < 0)
				startx = 0;
			if (starty < 0)
				starty = 0;
			if (slutx >= Size)
				slutx = Size - 1;
			if (sluty >= Size)
				sluty = Size - 1;

			for (int n = startx; n <= slutx; n++) {
				for (int m = starty; m <= sluty; m++) {
					if (board[n, m] != 0)
						board[n, m] = CurrentPlayer;
				}
			}
		}

		private bool IsWithinBoard(int x) {
			if ((x >= board.GetLength(0)) || (x < 0)) {
				return false;
			}
			return true;
		}

		public VirusBoard GetBoardCopy() {
			return new VirusBoard(board, this.players, this.Winner);
		}
	}
}