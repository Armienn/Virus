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

			foreach (VirusLongTermMemory ltmem in LongTermMemory)
			{
				significance = ltmem.Significance.ToString();
				startState = ltmem.Memory.StartState.Save();
				endState = ltmem.Memory.EndState.Save();
				action = ltmem.Memory.Action.Save();
				reward = ltmem.Memory.Reward.ToString();

				data = significance + ":" + startState + ":" + endState + ":" + action + ":" + reward + "\n";
				writer.Write(data);
			}
			writer.Close();
		}

		public void LoadlongTermMermory(String file)
		{
			NeaReader reader = new NeaReader(new StreamReader(file + ".MQ"));

			while (reader.Peek() != -1)
			{
				VirusMemory memory;
				VirusBoard startState = new VirusBoard();
				VirusBoard endState = new VirusBoard();
				Move action = new Move();
				double reward;
				double significance;
				string data;

				data = reader.ReadLine();
				NeaReader r = new NeaReader(data);
				significance = double.Parse(r.ReadUntil(":"));
				startState.Load(r.ReadUntil(":"));
				endState.Load(r.ReadUntil(":"));
				action.Load(r.ReadUntil(":"));
				reward = r.ReadDouble();

				memory = new VirusMemory(startState, action, endState, reward);
				LongTermMemory.Add(new VirusLongTermMemory(memory, significance));
			}
			reader.Close();
		}
	}
}
