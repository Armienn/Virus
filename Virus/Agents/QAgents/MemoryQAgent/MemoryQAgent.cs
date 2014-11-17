using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace VirusNameSpace.Agents
{
	public partial class MemoryQAgent : Agent
	{
		public MemoryQAgent(byte player, double disc, double lrmod, double lrstrt, double exmod, double exstrt, double lrpow, double expow, double initvalue = 0) {
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

		public MemoryQAgent(byte player = 1) {
			playerNumber = player;
		}

		public override void EndGame(Virus percept) {
			VirusBoard newState = percept.GetBoardCopy();
			double reward = 0;
			if (percept.Winner == playerNumber) reward = 1;
			else if (percept.Winner != playerNumber && percept.Winner != 0) reward = -1;

			ShortTermMemory.Add(new VirusMemory(prevState, prevAction, newState, reward));

			prevState = default(VirusBoard);
			prevAction = default(Move);
			prevReward = 0;
		}

		public override Move Move(Virus percept) {
			VirusBoard newState = percept.GetBoardCopy();

			if (!prevState.Equals(default(VirusBoard))) {
				ShortTermMemory.Add(new VirusMemory(prevState, prevAction, newState));
			}

			prevState = newState;
			prevAction = GetMaxExplorationFunctionA(newState);
			prevReward = 0;
			return prevAction;
		}

		public void ProcessShortTermMemory() {
			foreach (VirusMemory memory in ShortTermMemory) {
				double change = Learn(memory);
				AddToLongTermMemory(memory, change);
			}
			/*VirusMemory[][] episodes = ExtractEpisodesFromMemory();
			foreach (VirusMemory[] episode in episodes) {
				double change = LearnFromEpisode(episode);
				AddToLongTermMemory(episode, change);
			}*/
			ShortTermMemory.Clear();
		}

		private double LearnFromEpisode(VirusMemory[] episode) {
			double significance = 0;
			foreach (VirusMemory memory in episode) {
				significance += Learn(memory);
			}
			return significance;
		}

		private VirusMemory[][] ExtractEpisodesFromMemory() {
			List<VirusMemory[]> episodes = new List<VirusMemory[]>();
			List<VirusMemory> episode = new List<VirusMemory>();
			int n = 0;
			for (int i = 0; i < ShortTermMemory.Count; i++) {
				VirusMemory memory = ShortTermMemory[i];
				byte winner = memory.EndState.winner;
				episode.Add(memory);
				if (winner != 0) {
					episodes.Add(episode.ToArray());
					episode.Clear();
					n = i;
				}
			}
			ShortTermMemory.RemoveRange(0, n + 1);
			return episodes.ToArray();
		}

		private void AddToLongTermMemory(VirusMemory memory, double significance) {
			if (LongTermMemory.Count < longTermMemorySize) {
				for (int i = 0; i <= LongTermMemory.Count; i++) {
					if (i == LongTermMemory.Count) {
						LongTermMemory.Add(new VirusLongTermMemory(memory, significance));
						break;
					}
					else {
						if (LongTermMemory[i].Significance < significance) {
							LongTermMemory.Insert(i, new VirusLongTermMemory(memory, significance));
							break;
						}
					}
				}
			}
			else {
				if (LongTermMemory[LongTermMemory.Count - 1].Significance > significance)
					return;

				for (int i = 0; i < LongTermMemory.Count; i++) {
					if (LongTermMemory[i].Significance <= significance) {
						LongTermMemory.Insert(i, new VirusLongTermMemory(memory, significance));
						LongTermMemory.RemoveAt(LongTermMemory.Count - 1);
						break;
					}
				}
			}
		}

		/*private void AddToLongTermMemory(VirusMemory[] episode, double significance) {
			if (LongTermMemory.Count < longTermMemorySize) {
				for (int i = 0; i <= LongTermMemory.Count; i++) {
					if (i == LongTermMemory.Count) {
						LongTermMemory.Add(new VirusMemoryEpisode(episode, significance));
						break;
					}
					else {
						if (LongTermMemory[i].Significance < significance) {
							LongTermMemory.Insert(i, new VirusMemoryEpisode(episode, significance));
							break;
						}
					}
				}
			}
			else {
				if (LongTermMemory[LongTermMemory.Count - 1].Significance > significance)
					return;

				for (int i = 0; i < LongTermMemory.Count; i++) {
					if (LongTermMemory[i].Significance <= significance) {
						LongTermMemory.Insert(i, new VirusMemoryEpisode(episode, significance));
						LongTermMemory.RemoveAt(LongTermMemory.Count - 1);
						break;
					}
				}
			}
		}*/
	}
}
