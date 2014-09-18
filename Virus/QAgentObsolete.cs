using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirusNameSpace
{
	public class QAgentObsolete : Agent
	{
		//readonly double learningRate = 0.5;
		readonly double discount = 0.9;
		// Q-values:
		Dictionary<VirusBoard, Dictionary<Move, double>> Q = new Dictionary<VirusBoard, Dictionary<Move, double>>();
		// use with Q[state][action];

		// State-Action frequencies;
		Dictionary<VirusBoard, Dictionary<Move, int>> N = new Dictionary<VirusBoard, Dictionary<Move, int>>();

		VirusBoard prevState = default(VirusBoard);
		Move prevAction = default(Move);
		double prevReward = 0;

		public QAgentObsolete(byte player = 1) {
			playerNumber = player;
		}

		public override void EndGame(Virus percept) {
			Move(percept);
			prevState = default(VirusBoard);
			prevAction = default(Move);
			prevReward = 0;
		}

		public override Move Move(Virus percept) {

			//Checking if we're at an terminal state
			byte winner = percept.Winner;
			VirusBoard newState = percept.GetBoardCopy();
			if (!Q.ContainsKey(newState))
				Q.Add(newState, new Dictionary<Move, double>());

			if (!prevState.Equals(default(VirusBoard))) {
				if (!N.ContainsKey(prevState))
					N.Add(prevState, new Dictionary<Move, int>());
				if (!N[prevState].ContainsKey(prevAction))
					N[prevState].Add(prevAction, 0);
				if (!Q.ContainsKey(prevState))
					Q.Add(prevState, new Dictionary<Move, double>());
				if (!Q[prevState].ContainsKey(prevAction))
					Q[prevState].Add(prevAction, 0);

				if (winner == playerNumber) {
					if (!Q[newState].ContainsKey(default(Move)))
						Q[newState].Add(default(Move), 1);
				}
				else if (winner != playerNumber && winner != 0) {
					if (!Q[newState].ContainsKey(default(Move)))
						Q[newState].Add(default(Move), -1);
				}

				N[prevState][prevAction]++;
				Q[prevState][prevAction] =
					Q[prevState][prevAction]
					+ LearningRate(N[prevState][prevAction])
					* (prevReward + discount * GetMaxQ(newState) - Q[prevState][prevAction]);
			}

			prevState = newState;
			prevAction = GetMaxExplorationFunctionA(newState);
			prevReward = 0;
			return prevAction;
		}

		private double GetMaxQ(VirusBoard state) {
			double max = -10;
			Move[] actions = state.GetPossibleMoves(playerNumber);
			foreach (Move a in actions) {
				double value = 0;
				if (!Q[state].ContainsKey(a)) {
					value = 0;
				}
				else {
					value = Q[state][a];
				}
				if (value > max)
					max = value;
			}
			if (Q[state].ContainsKey(default(Move))) {
				if (Q[state][default(Move)] > max)
					max = Q[state][default(Move)];
			}
			return max;
		}

		private Move GetMaxExplorationFunctionA(VirusBoard state) {
			double max = -10;
			Move action = default(Move);
			Move[] actions = state.GetPossibleMoves(playerNumber);
			foreach (Move a in actions) {
				double value = 0;
				if (!Q.ContainsKey(state)) {
					Q.Add(state, new Dictionary<Move, double>());
				}
				if (Q[state].ContainsKey(a)) {
					value = Q[state][a] + 1 / (double)N[state][a];
				}
				else {
					value = 1;
				}
				if (value > max) {
					max = value;
					action = a;
				}
			}
			return action;
		}

		private double LearningRate(int frequency) {
			return 1 / Math.Sqrt((double)frequency);
		}
	}
}
