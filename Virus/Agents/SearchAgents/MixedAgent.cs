using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirusNameSpace
{
	public class MixedAgent : Agent
	{
		bool deterministic;
		double randomRatio;
		Random random = new Random();

		public MixedAgent(double randomratio, bool deterministic = false, byte player = 1) {
			this.deterministic = deterministic;
			randomRatio = randomratio;
			playerNumber = player;
		}

		public override void EndGame(Virus percept) { }

		public override Move Move(Virus percept) {
			VirusBoard state = percept.GetBoardCopy();
			Move[] actions = state.GetPossibleMoves(playerNumber);
			if (actions.Length < 1)
				return default(Move);
			Move action;
			if (random.NextDouble() > randomRatio) { // bruteforce
				List<Move> list = new List<Move>();
				int maxtaken = -1;

				foreach (Move a in actions) {
					int temp = state.TakeablePieces(a);
					if (a.IsLongMove)
						temp--;

					if (temp > maxtaken) {
						maxtaken = temp;
						list.Clear();
						list.Add(a);
					}
					else if (temp == maxtaken) {
						list.Add(a);
					}
				}

				if (deterministic) {
					action = list[0];
				}
				else {
					action = list[random.Next(list.Count)];
				}
			}
			else { // random
				if (deterministic)
					action = actions[0];
				else
					action = actions[random.Next(actions.Length)];
			}
			return action;
		}
	}

	public class BruteForceAgent : MixedAgent
	{
		public BruteForceAgent(byte player = 1, bool deterministic = false)
			: base(0,deterministic,player) { }
	}

	public class RandomAgent : MixedAgent
	{
		public RandomAgent(byte player = 1)
			: base(1, false, player) { }
	}

	public class SimpleAgent : MixedAgent
	{
		public SimpleAgent(byte player = 1)
			: base(1, true, player) { }
	}
}
