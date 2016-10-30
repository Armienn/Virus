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
			//lær fra MiniMax
			Move move = teacher.Move(percept);
			VirusBoard currentState = percept.GetBoardCopy();
			BackPropagationLearning backProp = new BackPropagationLearning(network);

			backProp.Run(BoardToInput(currentState), MoveToOutputs(move, currentState.Size));

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

		private double[] BoardToInput(VirusBoard board)
		{
			double[] inputs = new double[board.Size * board.Size];
			for(int i = 0; i < board.Size; i++)
			{
				for (int j = 0; j < board.Size; j++)
				{
					byte fieldState = board.board[i, j];
					if (fieldState == 0)
						inputs[i * board.Size + j] = 0;
					else if (fieldState == playerNumber)
						inputs[i * board.Size + j] = 1;
					else
						inputs[i * board.Size + j] = -1;
				}
			}
			return inputs;
		}

		private Move OutputsToMove(double[] outputs)
		{
			int boardFields = outputs.Length / 2;
			int boardSize = (int)Math.Sqrt(boardFields);
			double[] source = new double[boardFields];
			double[] destination = new double[boardFields];
			for (int i = 0; i < outputs.Length; i++)
			{
				if (i < boardFields)
					source[i] = outputs[i];
				else
					destination[i - boardFields] = outputs[i];
			}

			int sourcePosition = 0;
			int destinationPosition = 0;
			double sourceMax = double.MinValue;
			double destinationMax = double.MinValue;

			for (int i = 0; i < source.Length; i++)
			{
				if(source[i]> sourceMax)
				{
					sourceMax = source[i];
					sourcePosition = i;
				}
				if (destination[i] > destinationMax)
				{
					destinationMax = source[i];
					destinationPosition = i;
				}
			}

			int sourceX = sourcePosition / boardSize;
			int sourceY = sourcePosition % boardSize;
			int destinationX = destinationPosition / boardSize;
			int destinationY = destinationPosition % boardSize;
			return new Move(sourceX, sourceY, destinationX, destinationY);
		}

		private double[] MoveToOutputs(Move move, int boardSize)
		{
			int boardFields = boardSize * boardSize;
			double[] outputs = new double[boardFields * 2];
			outputs[move.sx * boardSize + move.sy] = 1;
			outputs[boardFields + move.ex * boardSize + move.ey] = 1;
			return outputs;
		}
	}
}
