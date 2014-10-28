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
			string startState;
			string endState;
			string action;
			string reward;
			string data;

			foreach (VirusMemory memory in LongTermMemory)
			{
				startState = memory.StartState.Save();
				endState = memory.EndState.Save();
				action = memory.Action.Save();
				reward = memory.Reward.ToString();

				data = startState + ":" + endState + ":" + action + ":" + reward + "/n";
				writer.Write(data);
			}
			writer.Close();
		}

		public void LoadlongTermMermory(String file)
		{
			NeaReader reader = new NeaReader(new NeaStreamReader(file + ".MQ"));

			while (reader.Peek() != -1)
			{
				VirusMemory memory = new VirusMemory();
				VirusBoard startState = new VirusBoard();
				VirusBoard endState = new VirusBoard();
				Move action = new Move();
				double reward;
				string data;

				data = reader.ReadLine();
				NeaReader r = new NeaReader(data);
				startState.Load(r.ReadUntil(":"));
				endState.Load(r.ReadUntil(":"));
				action.Load(r.ReadUntil(":"));
				reward = r.ReadDouble();

				memory = new VirusMemory(startState, action, endState, reward);
				LongTermMemory.Add(memory);
			}
			reader.Close();
		}
	}
}
