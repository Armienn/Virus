using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;

namespace VirusNameSpace
{
	public class MinimaxAgent : Agent
	{
		int searchLength = 4;
		public MinimaxAgent(int searchlength = 4, byte player = 1) {
			playerNumber = player;
			searchLength = searchlength;
		}

		public override Move Move(Virus percept) {
			//Stopwatch watch = new Stopwatch();
			//watch.Start();
			VirusBoard currentState = percept.GetBoardCopy();
			Move[] actions = currentState.GetPossibleMoves(playerNumber);
			Move action = actions[0];

			double max = double.NegativeInfinity;
			foreach (Move a in actions)
			{
				VirusBoard newState = currentState.GetUpdated(a);
				double q = Utility(currentState, newState);
				q += MinValue(newState, 0);
				if (q > max)
				{
					max = q;
					action = a;
				}
				if (max == double.PositiveInfinity) {
					break;
				}
			}
			//watch.Stop();

			//StreamWriter timeWriter = new StreamWriter("mmTimeLog",true);
			//timeWriter.WriteLine(watch.ElapsedMilliseconds); // + " ; " + watch.ElapsedTicks);
			//timeWriter.Close();
			return action;
		}

		public override void EndGame(Virus percept) {

		}

		//Calc maxValue
		private double MaxValue(VirusBoard state, int iteration)
		{
			iteration++;
			if (state.winner == playerNumber)
			{
				return double.PositiveInfinity;
			}
			if (state.winner != playerNumber && state.winner != 0)
			{
				return double.NegativeInfinity;
			}

			if (iteration < searchLength) {
				Move[] actions = state.GetPossibleMoves(playerNumber);

				double max = double.NegativeInfinity;
				foreach (Move a in actions) {
					VirusBoard newState = state.GetUpdated(a);
					double q = Utility(state, newState);
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
		private double MinValue(VirusBoard state, int iteration)
		{
			iteration++;
			if (state.winner == playerNumber)
			{
				return double.PositiveInfinity;
			}
			if (state.winner != playerNumber && state.winner != 0)
			{
				return double.NegativeInfinity;
			}

			if (iteration < searchLength) {
				byte opponent = (playerNumber == 1) ? (byte)2 : (byte)1;
				Move[] actions = state.GetPossibleMoves(opponent);

				double min = double.PositiveInfinity;
				foreach (Move a in actions)
				{
					VirusBoard newState = state.GetUpdated(a);
					double q = Utility(state, newState);
					q += MaxValue(newState, iteration);
					if (q < min)
					{
						min = q;
					}
					if (min == double.NegativeInfinity)
					{
						return min;
					}
				}

				return min;
			}
			else {
				return 0;
			}
		}

		//Utilities for Minimax search algorithm
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
			foreach (byte b in nextState.board)
			{
				if (b == playerNumber)
				{
					newPieces++;
					//newPieces += newPieces + 2;
				}
				else if (b != playerNumber && b != 0)
				{
					newPieces--;
				}
			}

			int difference = newPieces - orgPieces;
			return (double)difference;
		}
	}
}
