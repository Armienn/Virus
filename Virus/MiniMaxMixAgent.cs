using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace VirusNameSpace
{
	public class MiniMaxMixAgent : Agent
	{
		int searchLength = 4;
		Dictionary<UInt64, Dictionary<UInt32, double>> Q = new Dictionary<UInt64, Dictionary<UInt32, double>>();

		public MiniMaxMixAgent(String file, int searchlength = 4, byte player = 1) {
			Load(file);
			playerNumber = player;
			searchLength = searchlength;
		}

		public override Move Move(Virus percept) {
			VirusBoard currentState = percept.GetBoardCopy();
			Move[] actions = currentState.GetPossibleMoves(playerNumber);
			Move action = actions[0];

			double max = double.NegativeInfinity;
			foreach (Move a in actions) {
				VirusBoard newState = currentState.GetUpdated(a);
				double q = 0;
				if (Q.ContainsKey(currentState.CustomHash())) {
					if (Q[currentState.CustomHash()].ContainsKey(a.CustomHash())) {
						q = Q[currentState.CustomHash()][a.CustomHash()];
					}
				}
				q += MinValue(newState, 0);
				if (q > max) {
					max = q;
					action = a;
				}
				if (max == 1) {
					break;
				}
			}

			return action;
		}

		public override void EndGame(Virus percept) {

		}

		//Calc maxValue
		private double MaxValue(VirusBoard state, int iteration) {
			iteration++;
			if (state.winner == playerNumber) {
				return double.PositiveInfinity;
			}
			if (state.winner != playerNumber && state.winner != 0) {
				return double.NegativeInfinity;
			}

			if (iteration < searchLength) {
				Move[] actions = state.GetPossibleMoves(playerNumber);

				double max = double.NegativeInfinity;
				foreach (Move a in actions) {
					VirusBoard newState = state.GetUpdated(a);
					
					double q = Utility(state, newState);
					if (Q.ContainsKey(state.CustomHash())) {
						if (Q[state.CustomHash()].ContainsKey(a.CustomHash())) {
							q = Q[state.CustomHash()][a.CustomHash()];
						}
					}

					q += MinValue(newState, iteration);
					if (q > max) {
						max = q;
					}
					if (max == double.PositiveInfinity) {
						return max;
					}
				}

				return max;
			}
			else {
				return 0;
			}
		}

		// Calc minValue
		private double MinValue(VirusBoard state, int iteration) {
			iteration++;
			if (state.winner == playerNumber) {
				return double.PositiveInfinity;
			}
			if (state.winner != playerNumber && state.winner != 0) {
				return double.NegativeInfinity;
			}

			if (iteration < searchLength) {
				byte opponent = (playerNumber == 1) ? (byte)2 : (byte)1;
				Move[] actions = state.GetPossibleMoves(opponent);

				double min = double.PositiveInfinity;
				foreach (Move a in actions) {
					VirusBoard newState = state.GetUpdated(a);
					double q = Utility(state, newState);
					if (Q.ContainsKey(state.CustomHash())) {
						if (Q[state.CustomHash()].ContainsKey(a.CustomHash())) {
							q = -Q[state.CustomHash()][a.CustomHash()];
						}
					}

					q += MaxValue(newState, iteration);
					if (q < min) {
						min = q;
					}
					if (min == double.NegativeInfinity) {
						return min;
					}
				}

				return min;
			}
			else {
				return 0;
			}
		}

		private double Utility(VirusBoard currentState, VirusBoard nextState)
		{
			int orgPieces = 0;
			foreach (byte b in currentState.board)
			{
				if (b == playerNumber)
				{
					orgPieces++;
					//orgPieces += orgPieces + 2;
				}
				else if (b != playerNumber && b!=0)
				{
					orgPieces--;
				}
			}

			int newPieces = 0;
			foreach (byte b in nextState.board) {
				if (b == playerNumber) {
					newPieces++;
					//newPieces += newPieces + 2;
				}
				else if (b != playerNumber && b != 0) {
					newPieces--;
				}
			}

			double difference = newPieces - orgPieces;
			difference *= 0.1;
			return difference;
		}

		public void Load(String file) {
			BinaryReader reader = new BinaryReader(new FileStream(file + ".Q", FileMode.Open, FileAccess.Read));
			while (reader.BaseStream.Position < reader.BaseStream.Length) {
				UInt64 key = reader.ReadUInt64();
				if (!Q.ContainsKey(key))
					Q.Add(key, new Dictionary<uint, double>());
				UInt32 key2 = reader.ReadUInt32();
				Q[key].Add(key2, reader.ReadDouble());
			}
			reader.Close();
		}
	}
}
