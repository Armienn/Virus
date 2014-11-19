using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirusNameSpace {
	struct VirusMemory {
		public readonly VirusBoard StartState;
		public readonly Move Action;
		public readonly VirusBoard EndState;
		public readonly double Reward;

		public VirusMemory(VirusBoard start, Move action, VirusBoard end) {
			StartState = start;
			Action = action;
			EndState = end;
			Reward = VirusNameSpace.Agents.MemoryQAgent.Reward(start, end);
		}

		public VirusMemory(VirusBoard start, Move action, VirusBoard end, double reward) {
			StartState = start;
			Action = action;
			EndState = end;
			Reward = reward;
		}
	}

	struct VirusLongTermMemory {
		public readonly VirusMemory Memory;
		public readonly double Significance;

		public VirusLongTermMemory(VirusMemory memory, double significance) {
			Memory = memory;
			Significance = significance;
		}
	}

	struct VirusMemoryEpisode {
		public readonly VirusMemory[] Memories;
		public readonly double Significance;
		public readonly VirusMemory Memory;

		public VirusMemoryEpisode(VirusMemory[] memories, double significance, VirusMemory memory) {
			Memories = memories;
			Significance = significance;
			Memory = memory;
		}
	}
}
