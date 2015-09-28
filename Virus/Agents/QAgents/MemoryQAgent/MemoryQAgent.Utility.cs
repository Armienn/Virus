using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Nea;

namespace VirusNameSpace.Agents {
	public partial class MemoryQAgent {
		public void TurnOnExploration() {
			explore = true;
		}

		public void TurnOffExploration() {
			explore = false;
		}

		public void ForgetShortTerm() {
			ShortTermMemory.Clear();
		}

		public void Save(String file) {
			BinaryWriter writer = new BinaryWriter(new FileStream(file + ".Q", FileMode.Create));
			foreach (KeyValuePair<UInt64, Dictionary<UInt32, double>> kvp in Q) {
				foreach (KeyValuePair<UInt32, double> kv2 in kvp.Value) {
					writer.Write(kvp.Key);
					writer.Write(kv2.Key);
					writer.Write(kv2.Value);
				}
			}
			writer.Close();

			writer = new BinaryWriter(new FileStream(file + ".N", FileMode.Create));
			foreach (KeyValuePair<UInt64, Dictionary<UInt32, int>> kvp in N) {
				foreach (KeyValuePair<UInt32, int> kv2 in kvp.Value) {
					writer.Write(kvp.Key);
					writer.Write(kv2.Key);
					writer.Write(kv2.Value);
				}
			}
			writer.Close();
		}

		public void Load(String file) {
			BinaryReader reader = new BinaryReader(new FileStream(file + ".Q", FileMode.Open, FileAccess.Read));
			while (reader.BaseStream.Position < reader.BaseStream.Length) {
				UInt64 key = reader.ReadUInt64();
				if (!Q.ContainsKey(key))
					Q.Add(key, new Dictionary<uint, double>());
				UInt32 key2 = reader.ReadUInt32();
				Q[key].Add(key2, reader.ReadDouble());
			}
			reader.Close();

			reader = new BinaryReader(new FileStream(file + ".N", FileMode.Open, FileAccess.Read));
			while (reader.BaseStream.Position < reader.BaseStream.Length) {
				UInt64 key = reader.ReadUInt64();
				if (!N.ContainsKey(key))
					N.Add(key, new Dictionary<uint, int>());
				UInt32 key2 = reader.ReadUInt32();
				N[key].Add(key2, reader.ReadInt32());
			}
			reader.Close();
		}

		public void SaveLongTermMemory(String file)
		{
			StreamWriter writer = new StreamWriter(new FileStream(file + ".MQ", FileMode.Create));
			string significance;
			string startState;
			string endState;
			string action;
			string reward;
			string data;

			foreach (VirusMemoryEpisode episode in LongTermMemory)
			{
				significance = episode.Significance.ToString();
				data = significance;
				foreach (VirusMemory memory in episode.Memories) {
					startState = memory.StartState.Save();
					endState = memory.EndState.Save();
					action = memory.Action.Save();
					reward = memory.Reward.ToString();
					data += ":" + startState + ":" + endState + ":" + action + ":" + reward;
				}

				data += "\n";
				writer.Write(data);
			}
			writer.Close();
		}

		public void LoadlongTermMermory(String file)
		{
			NeaReader reader = new NeaReader(new StreamReader(file + ".MQ"));

			while (reader.Peek() != -1)
			{
				List<VirusMemory> memories = new List<VirusMemory>();
				VirusBoard startState = new VirusBoard();
				VirusBoard endState = new VirusBoard();
				Move action = new Move();
				double reward;
				double significance;
				string data;

				data = reader.ReadLine();
				NeaReader r = new NeaReader(data);
				significance = double.Parse(r.ReadUntil(":"));
				while (r.Peek() != -1) {
					startState.Load(r.ReadUntil(":"));
					endState.Load(r.ReadUntil(":"));
					action.Load(r.ReadUntil(":"));
					reward = double.Parse(r.ReadUntil(":"));
					memories.Add(new VirusMemory(startState, action, endState, reward));
				}
				

				//memory = new VirusMemory(startState, action, endState, reward);
				LongTermMemory.Add(new VirusMemoryEpisode(memories.ToArray(), significance));//new VirusLongTermMemory(memory, significance));
			}
			reader.Close();
		}

		public void TellOfMemoryTo(MemoryQAgent agent, bool fullepisode = false) {
			VirusMemory[] memories = LongTermMemory[random.Next(LongTermMemory.Count)].Memories;
			VirusMemory memory = memories[0];
			if (fullepisode) {
				foreach (VirusMemory m in memories)
					agent.Learn(m);
			}
			else {
				foreach (VirusMemory m in memories)
					if (Math.Abs(m.Reward) > Math.Abs(memory.Reward))
						memory = m;
				agent.Learn(memory);
			}
		}

		public int TellOfMemoryToExt(MemoryQAgent agent, bool fullepisode = false) {
			VirusMemory[] memories = LongTermMemory[random.Next(LongTermMemory.Count)].Memories;
			VirusMemory memory = memories[0];
			if (fullepisode) {
				agent.LearnFromEpisode(memories, true);
				//foreach (VirusMemory m in memories)
				//	agent.Learn(m);
				return memories.Length;
			}
			else {
				foreach (VirusMemory m in memories)
					if (Math.Abs(m.Reward) > Math.Abs(memory.Reward))
						memory = m;
				agent.Learn(memory);
				return 1;
			}
		}
	}
}
