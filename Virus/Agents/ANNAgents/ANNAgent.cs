using DaxCore.ActivationFunctions;
using DaxCore.Networks;
using DaxCore.Learning;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirusNameSpace.Agents
{
	public class AnnAgent : Agent
	{
		ActivationNetwork network;
		bool learning = true;
		MinimaxAgent teacher;

		public AnnAgent(int boardSize, byte player = 1)
		{
			playerNumber = player;
			int boardFields = boardSize * boardSize;
			network = new ActivationNetwork(new SigmoidFunction(), boardFields, 30, 30, boardFields * 2);
			teacher = new MinimaxAgent(4, player);
		}

		public override Move Move(Virus percept)
		{
			if (learning)
				return LearnFromMinimax(percept);
			return GetAnnMove(percept);
		}

		public override void EndGame(Virus percept)
		{

		}

		private Move LearnFromMinimax(Virus percept)
		{
			Move move = teacher.Move(percept);
			BackPropagationLearning backProp = new BackPropagationLearning(network);

			backProp.Run(boardToInput(currentState), MoveToOutputs(move));

			//lær fra MiniMax

			return move;
		}

		private Move GetAnnMove(Virus percept)
		{
			VirusBoard currentState = percept.GetBoardCopy();
			Move[] actions = currentState.GetPossibleMoves(playerNumber);

			Move action = actions[0];
			return new Move();
		}

		private double[] MoveToOutputs(Move move)
		{
			double[] someArray = new double[10]; 
			return someArray;
		}
	}
}
