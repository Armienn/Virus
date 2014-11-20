using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirusNameSpace.Agents {
	public partial class MemoryQAgent {

		public void ProcessShortTermMemory(bool reverse = false) {
			VirusMemory[][] episodes = ExtractEpisodesFromMemory();
			foreach (VirusMemory[] episode in episodes) {
				double change = LearnFromEpisode(episode, reverse);
				AddToLongTermMemory(episode, change);
			}
			ShortTermMemory.Clear();
		}

		private double LearnFromEpisode(VirusMemory[] episode, bool reverse = false) {
			double significance = 0;
			for (int i = 0; i < episode.Length; i++) {
				int index = reverse ? episode.Length - i - 1 : i;
				significance += Learn(episode[index]);
			}
			foreach (VirusMemory memory in episode) {
				significance += Learn(memory);
			}
			return significance/episode.Length;
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

		/*private void AddToLongTermMemory(VirusMemory memory, double significance) {
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
		}*/

		private void AddToLongTermMemory(VirusMemory[] episode, double significance) {
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
		}
	}
}
