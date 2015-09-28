using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirusNameSpace.Agents {
	public partial class MemoryQAgent {
		double initvalue = 0;
		readonly double discount = 0.98;
		double learningRateModifier = 1;
		double learningRateStart = 0;
		double explorationModifier = 1;
		double explorationStart = 0;
		double learningRatePower = 1;
		double explorationPower = 0.5;
		bool explore = true;

		double longTermMemorySize = 5000;

		public double MinLearning {
			get;
			set;
		}
		public double RandomRate {
			get;
			set;
		}
		Random random = new Random();

		// Memory:
		List<VirusMemory> ShortTermMemory = new List<VirusMemory>();
		//List<VirusLongTermMemory> LongTermMemory = new List<VirusLongTermMemory>();
		List<VirusMemoryEpisode> LongTermMemory = new List<VirusMemoryEpisode>();

		// Q-values:
		Dictionary<UInt64, Dictionary<UInt32, double>> Q = new Dictionary<UInt64, Dictionary<UInt32, double>>();
		// use with Q[state][action];

		// State-Action frequencies;
		Dictionary<UInt64, Dictionary<UInt32, int>> N = new Dictionary<UInt64, Dictionary<UInt32, int>>();

		VirusBoard prevState = default(VirusBoard);
		Move prevAction = default(Move);
		double prevReward = 0;
	}
}
