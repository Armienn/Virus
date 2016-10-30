using DaxCore.ActivationFunctions;
using DaxCore.Networks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirusNameSpace.Agents.ANNAgents
{
	public class ANNAgent : Agent
	{
		ActivationNetwork network = new ActivationNetwork(new SigmoidFunction(), 26, 20, 20);
		public override Move Move(Virus percept)
		{
			VirusBoard currentState = percept.GetBoardCopy();
			Move[] actions = currentState.GetPossibleMoves(playerNumber);
			Move action = actions[0];

			return action;
		}

		public override void EndGame(Virus percept)
		{

		}
	}
}
