using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace VirusNameSpace.Agents
{
	public partial class QAgentAdvanced : Agent
	{
		public QAgentAdvanced(byte player, double disc, double lrmod, double lrstrt, double exmod, double exstrt, double lrpow, double expow, double initvalue = 0) {
			playerNumber = player;
			discount = disc;
			learningRateModifier = lrmod;
			learningRateStart = lrstrt;
			learningRatePower = lrpow;
			explorationModifier = exmod;
			explorationStart = exstrt;
			explorationPower = expow;
			this.initvalue = initvalue;
			RandomRate = 0;
			MinLearning = 0;
		}

		public QAgentAdvanced(byte player = 1) {
			playerNumber = player;
		}

		public override void EndGame(Virus percept) {
			if (learn) {
				double reward = 0;
				byte winner = percept.Winner;
				if (winner == playerNumber)
					reward = 1;
				else if (winner != playerNumber && winner != 0)
					reward = -1;
				else
					reward = 0;
				if (!N.ContainsKey(prevState.CustomHash()))
					N.Add(prevState.CustomHash(), new Dictionary<UInt32, int>());
				if (!N[prevState.CustomHash()].ContainsKey(prevAction.CustomHash()))
					N[prevState.CustomHash()].Add(prevAction.CustomHash(), 0);

				N[prevState.CustomHash()][prevAction.CustomHash()]++;
				Q[prevState.CustomHash()][prevAction.CustomHash()] =
					Q[prevState.CustomHash()][prevAction.CustomHash()]
					+ LearningRate(N[prevState.CustomHash()][prevAction.CustomHash()])
					* (reward - Q[prevState.CustomHash()][prevAction.CustomHash()]);
			}

			prevState = default(VirusBoard);
			prevAction = default(Move);
			prevReward = 0;
		}

		public override Move Move(Virus percept) {
			VirusBoard newState = percept.GetBoardCopy();
			if (!Q.ContainsKey(newState.CustomHash()))
				Q.Add(newState.CustomHash(), new Dictionary<UInt32, double>());

			if (learn && !prevState.Equals(default(VirusBoard))) {

				if (!N.ContainsKey(prevState.CustomHash()))
					N.Add(prevState.CustomHash(), new Dictionary<UInt32, int>());
				if (!N[prevState.CustomHash()].ContainsKey(prevAction.CustomHash()))
					N[prevState.CustomHash()].Add(prevAction.CustomHash(), 0);

				N[prevState.CustomHash()][prevAction.CustomHash()]++;
				Q[prevState.CustomHash()][prevAction.CustomHash()] =
					Q[prevState.CustomHash()][prevAction.CustomHash()]
					+ LearningRate(N[prevState.CustomHash()][prevAction.CustomHash()])
					* (prevReward + discount * GetMaxQ(newState) - Q[prevState.CustomHash()][prevAction.CustomHash()]);
			}

			prevState = newState;
			prevAction = GetMaxExplorationFunctionA(newState);
			prevReward = 0;
			if (learn && !Q[prevState.CustomHash()].ContainsKey(prevAction.CustomHash()))
				Q[prevState.CustomHash()].Add(prevAction.CustomHash(), initvalue);
			return prevAction;
		}

		private double GetMaxQ(VirusBoard state) {
			double max = -10;
			Move[] actions = state.GetPossibleMoves(playerNumber);
			foreach (Move a in actions) {
				double value = 0;
				if (!Q[state.CustomHash()].ContainsKey(a.CustomHash())) {
					value = initvalue;
				}
				else {
					value = Q[state.CustomHash()][a.CustomHash()];
				}
				if (value > max)
					max = value;
			}
			return max;
		}

		private Move GetMaxExplorationFunctionA(VirusBoard state) {
			double max = double.NegativeInfinity;
			Move action = default(Move);
			Move[] actions = state.GetPossibleMoves(playerNumber);

			bool berandom = random.NextDouble() < RandomRate;
			foreach (Move a in actions) {
				double value = 0;

				if (Q[state.CustomHash()].ContainsKey(a.CustomHash())) {
					if (Q[state.CustomHash()][a.CustomHash()] >= 1) {
						value = 1;
						max = value;
						action = a;
						break;
					}
					else if (Q[state.CustomHash()][a.CustomHash()] <= -1) {
						value = -1;
					}
					else {
						if (berandom)
							value = random.NextDouble();
						else
							value = Q[state.CustomHash()][a.CustomHash()] + ((explore && !(RandomRate > 0)) ? ExplorationRate(N[state.CustomHash()][a.CustomHash()]) : 0);
					}
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

		private double ExplorationRate(int frequency) {
			return explorationModifier / Math.Pow((double)frequency + explorationStart, explorationPower);
		}

		private double LearningRate(int frequency) {
			double result = learningRateModifier / Math.Pow((double)frequency + learningRateStart, learningRatePower);
			return result < MinLearning ? MinLearning : result;
		}
	}
}
