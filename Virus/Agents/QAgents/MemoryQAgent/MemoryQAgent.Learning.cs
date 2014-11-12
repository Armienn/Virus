using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirusNameSpace.Agents {
	public partial class MemoryQAgent {

		#region Learning

		private double Learn(VirusMemory memory) {
			VirusBoard startstate = memory.StartState;
			VirusBoard endstate = memory.EndState;
			Move action = memory.Action;
			double reward = memory.Reward;

			// -- Make sure the entries for the state and action exist -- 
			if (!Q.ContainsKey(startstate.CustomHash()))
				Q.Add(startstate.CustomHash(), new Dictionary<UInt32, double>());
			if (!Q[startstate.CustomHash()].ContainsKey(action.CustomHash()))
				Q[startstate.CustomHash()].Add(action.CustomHash(), initvalue);

			if (!N.ContainsKey(startstate.CustomHash()))
				N.Add(startstate.CustomHash(), new Dictionary<UInt32, int>());
			if (!N[startstate.CustomHash()].ContainsKey(action.CustomHash()))
				N[startstate.CustomHash()].Add(action.CustomHash(), 0);

			// -- Perform the update of Q-values --
			N[startstate.CustomHash()][action.CustomHash()]++;
			double change = LearningRate(N[startstate.CustomHash()][action.CustomHash()])
				* (reward + discount * GetMaxQ(endstate) - Q[startstate.CustomHash()][action.CustomHash()]);
			Q[startstate.CustomHash()][action.CustomHash()] =
				Q[startstate.CustomHash()][action.CustomHash()] + change;
			return Math.Abs(change);
		}

		public static double Reward(VirusBoard startstate, VirusBoard endstate) {
			return 0;
		}

		/// <summary>
		/// Returns the maximum Q-value found for any moves performable in the given state.
		/// If there is no data for a move, it will be considered having [initvalue].
		/// If there is no data for the state, the return value will be 0.
		/// </summary>
		/// <param name="state"></param>
		/// <returns></returns>
		private double GetMaxQ(VirusBoard state) {
			if (state.Equals(default(VirusBoard)) || !Q.ContainsKey(state.CustomHash()))
				return 0;

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

		private double LearningRate(int frequency) {
			double result = learningRateModifier / Math.Pow((double)frequency + learningRateStart, learningRatePower);
			return result < MinLearning ? MinLearning : result;
		}

		#endregion

		#region Exploration

		private Move GetMaxExplorationFunctionA(VirusBoard state) {
			double max = double.NegativeInfinity;
			Move action = default(Move);
			Move[] actions = state.GetPossibleMoves(playerNumber);
			if (!Q.ContainsKey(state.CustomHash()))
				return actions.Length > 0 ? actions[0] : action;

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

		#endregion
	}
}
