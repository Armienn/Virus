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
				if (percept.Winner == playerNumber) reward = 1;
				else if (percept.Winner != playerNumber && percept.Winner != 0) reward = -1;

				Learn(prevState, default(VirusBoard), prevAction, reward);
			}

			prevState = default(VirusBoard);
			prevAction = default(Move);
			prevReward = 0;
		}

		public override Move Move(Virus percept) {
			VirusBoard newState = percept.GetBoardCopy();

			if (learn && !prevState.Equals(default(VirusBoard)))
				Learn(prevState, newState, prevAction);

			prevState = newState;
			prevAction = GetMaxExplorationFunctionA(newState);
			prevReward = 0;
			return prevAction;
		}
	}
}
