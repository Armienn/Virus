using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace VirusNameSpace
{
	public struct VirusBoard : IEquatable<VirusBoard>
	{
		byte players;
		public byte winner;
		public byte[,] board;
		public int Size { get { return board.GetLength(0); } }

		public byte this[int x, int y] {
			get {
				return board[x, y];
			}
			set {
				board[x, y] = value;
			}
		}

		public VirusBoard(int size, byte players) {
			board = new byte[size, size];
			this.players = players;
			winner = 0;
		}

		public VirusBoard(byte[,] source, byte players, byte winner) {
			this.players = players;
			this.winner = winner;
			board = new byte[source.GetLength(0), source.GetLength(0)];
			for (int i = 0; i < this.Size; i++) {
				for (int j = 0; j < this.Size; j++) {
					board[i, j] = source[i, j];
				}
			}
		}

		public VirusBoard(VirusBoard virusBoard) : this(virusBoard.board, virusBoard.players, 0) { }

		public VirusBoard GetUpdated(Move action) {
			VirusBoard result = new VirusBoard(this);
			byte playerNumber = board[action.sx, action.sy];

			if(action.IsLongMove){
				result[action.sx, action.sy] = 0;
			}
			result[action.ex, action.ey] = playerNumber;

			int startx = action.ex - 1;
			int slutx = action.ex + 1;
			int starty = action.ey - 1;
			int sluty = action.ey + 1;
			if (startx < 0)
				startx = 0;
			if (starty < 0)
				starty = 0;
			if (slutx >= this.Size)
				slutx = this.Size - 1;
			if (sluty >= this.Size)
				sluty = this.Size - 1;

			for (int n = startx; n <= slutx; n++) {
				for (int m = starty; m <= sluty; m++) {
					if (result[n, m] != 0 && result[n, m] != playerNumber)
						result[n, m] = playerNumber;
				}
			}
			result.CalculateWinner();
			return result;
		}

		private void CalculateWinner() {
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
				winner = 0;

				for (byte i = 1; i < pieces.Length; i++) {
					if (pieces[i] > max) {
						max = pieces[i];
						winner = i;
					}
				}
			}
			else {
				winner = 0;
			}
		}

		public Move[] GetPossibleMoves(byte player) {
			List<Move> actions = new List<Move>();
			for (int i = 0; i < Size; i++) {
				for (int j = 0; j < Size; j++) {
					if (this[i, j] == 0) {
						int startx = i - 2;
						int slutx = i + 2;
						int starty = j - 2;
						int sluty = j + 2;
						if (startx < 0)
							startx = 0;
						if (starty < 0)
							starty = 0;
						if (slutx >= Size)
							slutx = Size - 1;
						if (sluty >= Size)
							sluty = Size - 1;

						bool exists = false;

						for (int n = startx; n <= slutx; n++) {
							for (int m = starty; m <= sluty; m++) {
								if (this[n, m] == player) {
									Move move = new Move(n, m, i, j);
									if (move.IsLongMove) {
										actions.Add(move);
									}
									else if (!exists) {
										actions.Add(move);
										exists = true;
									}
								}
							}
						}
					}
				}
			}
			return actions.ToArray();
		}

		public int TakeablePieces(Move action) {
			int result = 0;

			byte playerNumber = board[action.sx, action.sy];

			int startx = action.ex - 1;
			int slutx = action.ex + 1;
			int starty = action.ey - 1;
			int sluty = action.ey + 1;
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
					if (this[n, m] != 0 && this[n, m] != playerNumber)
						result++;
				}
			}
			return result;
		}

		public bool Equals(VirusBoard other) {
			if (board == null) {
				if (other.board == null)
					return true;
				return false;
			}
			if (other.board == null)
				return false;
			int ownsize = board.GetLength(0);
			if (ownsize != other.Size)
				return false;
			for (int i = 0; i < ownsize; i++) {
				for (int j = 0; j < ownsize; j++) {
					if (other[i, j] != this[i, j])
						return false;
				}
			}
			return true;
		}

		public override bool Equals(object other) {
			return other is VirusBoard ? Equals((VirusBoard)other) : false;
		}

		public override int GetHashCode() {
			int hash = 0;
			int i = 0;
			foreach (byte b in board) {
				int temp = 1;
				for (int n = 0; n < i; n++) {
					temp *= 3;
				}
				hash += b * temp;
				i++;
			}
			return hash;
		}

		public byte[] SimpleHash() {
			byte[] hash = new byte[(Size*Size/5) + 1];
			for (int i = 0; i < Size; i++) {
				for (int j = 0; j < Size; j++) {
					int pos = (i * Size + j)/5;
					hash[pos] += (byte)(board[i, j] * (byte)(Math.Pow(3, (i * Size + j) - (pos * 5))));
				}
			}
			return hash;
		}

		public byte[] MD5() {
			return new MD5CryptoServiceProvider().ComputeHash(SimpleHash());
		}

		public UInt64 CustomHash() {
			UInt64 h = 0;
			byte[] md5 = MD5();
			for (int i = 0; i < 8; i++) {
				h += ((UInt64)md5[i]) * (UInt64)Math.Pow(256, i);
				h += ((UInt64)md5[i+8]) * (UInt64)Math.Pow(256, i);
			}
			return h;
		}
	}

	public struct Move : IEquatable<Move>
	{
		public int sx, sy, ex, ey;
		public bool IsLongMove {
			get {
				int difx = ex - sx;
				int dify = ey - sy;

				if (difx > 1 || difx < -1 || dify < -1 || dify > 1) {
					return true;
				}
				else
					return false;
			}
		}

		public Move(int startx, int starty, int endx, int endy) {
			sx = startx;
			sy = starty;
			ex = endx;
			ey = endy;
		}

		public bool Equals(Move other) {
			if ((sx == other.sx) && (sy == other.sy) && (ex == other.ex) && (ey == other.ey))
				return true;
			return false;
		}

		public override bool Equals(object other) {
			return other is Move ? Equals((Move)other) : false;
		}

		public override int GetHashCode() {
			int hash = sx + sy * 256 + ex * 256 * 256 + ey * 256 * 256 * 256;
			return hash;
		}

		public UInt32 CustomHash() {
			return (UInt32)GetHashCode();
		}
	}
}
