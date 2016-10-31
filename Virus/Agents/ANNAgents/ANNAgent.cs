using DaxCore.ActivationFunctions;
using DaxCore.Networks;
using DaxCore.Learning;
using DaxCore.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace VirusNameSpace.Agents
{
	public class AnnAgent : Agent
	{
		ActivationNetwork network;
		BackPropagationLearning backProp;
		bool learning = true;
		MinimaxAgent teacher;

		public AnnAgent(bool learn, int boardSize, byte player = 1)
		{
			learning = learn;
			playerNumber = player;
			int boardFields = boardSize * boardSize;
			if(File.Exists("ann" + boardSize + ".bin"))
				network = (ActivationNetwork)Serialization.LoadNetwork("ann" + boardSize + ".bin");
			else
				network = new ActivationNetwork(new BipolarSigmoidFunction(), boardFields, 5, boardFields * 2);
			backProp = new BackPropagationLearning(network);
			teacher = new MinimaxAgent(2, player);
		}

		public override Move Move(Virus percept)
		{
			if (learning)
				return LearnFromMinimax(percept);
			return GetAnnMove(percept);
		}

		public override void EndGame(Virus percept)
		{
			Serialization.SaveNetwork(network, "ann" + percept.Size + ".bin");
			using (StreamWriter writer = new StreamWriter("ann" + percept.Size + "log.txt", true))
				writer.WriteLine("New Game : ");
		}

		private Move LearnFromMinimax(Virus percept)
		{
			//lær fra MiniMax
			Move move = teacher.Move(percept);
			VirusBoard currentState = percept.GetBoardCopy();

			backProp.LearningRate = 0.1;
			backProp.Momentum = 0.1;
			Move annMove = OutputsToMove(network.Compute(BoardToInput(currentState)));
			double error = backProp.Run(BoardToInput(currentState), MoveToOutputs(move, currentState.Size));

			if (move.Equals(annMove))
				using (StreamWriter writer = new StreamWriter("ann" + percept.Size + "log.txt", true))
					writer.WriteLine("using right move. E: " + error);
			else
				using (StreamWriter writer = new StreamWriter("ann" + percept.Size + "log.txt", true))
					writer.WriteLine("using wrong move. E: " + error);
			return move;
		}

		private Move GetAnnMove(Virus percept)
		{
			VirusBoard currentState = percept.GetBoardCopy();
			Move[] actions = currentState.GetPossibleMoves(playerNumber);
			Move move = OutputsToMove(network.Compute(BoardToInput(currentState)));

			if (actions.Contains(move))
			{
				using (StreamWriter writer = new StreamWriter("ann" + percept.Size + "log.txt", true))
					writer.WriteLine("using learned move");
				return move;
			}

			using (StreamWriter writer = new StreamWriter("ann" + percept.Size + "log.txt", true))
				writer.WriteLine("using default move");
			return actions[0];
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
					destinationMax = destination[i];
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
