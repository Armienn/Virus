using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace VirusNameSpace
{
	public class QAgent : Agent
	{
		double initvalue = 0;
		readonly double discount = 1;
		double learningRateModifier = 1;
		double learningRateStart = 0;
		double explorationModifier = 1;
		double explorationStart = 0;
		double learningRatePower = 1;
		double explorationPower = 0.5;
		bool explore = true;
		bool learn = true;

		public double MinLearning {
			get;
			set;
		}
		public double RandomRate {
			get;
			set;
		}
		Random random = new Random();

		// Q-values:
		Dictionary<UInt64, Dictionary<UInt32, double>> Q = new Dictionary<UInt64, Dictionary<UInt32, double>>();
		// use with Q[state][action];

		// State-Action frequencies;
		Dictionary<UInt64, Dictionary<UInt32, int>> N = new Dictionary<UInt64, Dictionary<UInt32, int>>();

		VirusBoard prevState = default(VirusBoard);
		Move prevAction = default(Move);
		double prevReward = 0;

		public QAgent(byte player, double disc, double lrmod, double lrstrt, double exmod, double exstrt, double lrpow, double expow, double initvalue = 0) {
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

		public QAgent(byte player = 1) {
			playerNumber = player;
		}

		public override void EndGame(Virus percept) {
			if (learn) {
				double reward = 0;
				byte winner = percept.Winner;
				if (winner == playerNumber)
					reward = 1;
				else if (winner != playerNumber && winner != 0)
					reward = -1;
				else
					reward = 0;
				if (!N.ContainsKey(prevState.CustomHash()))
					N.Add(prevState.CustomHash(), new Dictionary<UInt32, int>());
				if (!N[prevState.CustomHash()].ContainsKey(prevAction.CustomHash()))
					N[prevState.CustomHash()].Add(prevAction.CustomHash(), 0);

				N[prevState.CustomHash()][prevAction.CustomHash()]++;
				Q[prevState.CustomHash()][prevAction.CustomHash()] =
					Q[prevState.CustomHash()][prevAction.CustomHash()]
					+ LearningRate(N[prevState.CustomHash()][prevAction.CustomHash()])
					* (reward - Q[prevState.CustomHash()][prevAction.CustomHash()]);
			}

			prevState = default(VirusBoard);
			prevAction = default(Move);
			prevReward = 0;
		}

		public override Move Move(Virus percept) {
			VirusBoard newState = percept.GetBoardCopy();
			if (!Q.ContainsKey(newState.CustomHash()))
				Q.Add(newState.CustomHash(), new Dictionary<UInt32, double>());

			if (learn && !prevState.Equals(default(VirusBoard))) {

				if (!N.ContainsKey(prevState.CustomHash()))
					N.Add(prevState.CustomHash(), new Dictionary<UInt32, int>());
				if (!N[prevState.CustomHash()].ContainsKey(prevAction.CustomHash()))
					N[prevState.CustomHash()].Add(prevAction.CustomHash(), 0);

				N[prevState.CustomHash()][prevAction.CustomHash()]++;
				Q[prevState.CustomHash()][prevAction.CustomHash()] =
					Q[prevState.CustomHash()][prevAction.CustomHash()]
					+ LearningRate(N[prevState.CustomHash()][prevAction.CustomHash()])
					* (prevReward + discount * GetMaxQ(newState) - Q[prevState.CustomHash()][prevAction.CustomHash()]);
			}

			prevState = newState;
			prevAction = GetMaxExplorationFunctionA(newState);
			prevReward = 0;
			if (learn && !Q[prevState.CustomHash()].ContainsKey(prevAction.CustomHash()))
				Q[prevState.CustomHash()].Add(prevAction.CustomHash(), initvalue);
			return prevAction;
		}

		private double GetMaxQ(VirusBoard state) {
			double max = -10;
			Move[] actions = state.GetPossibleMoves(playerNumber);
			foreach (Move a in actions) {
				double value = 0;
				if (!Q[state.CustomHash()].ContainsKey(a.CustomHash())) {
					value = initvalue;
				}
				else {
					value = Q[state.CustomHash()][a.CustomHash()];
				}
				if (value > max)
					max = value;
			}
			return max;
		}

		private Move GetMaxExplorationFunctionA(VirusBoard state) {
			double max = double.NegativeInfinity;
			Move action = default(Move);
			Move[] actions = state.GetPossibleMoves(playerNumber);

			bool berandom = random.NextDouble() < RandomRate;
			foreach (Move a in actions) {
				double value = 0;

				if (Q[state.CustomHash()].ContainsKey(a.CustomHash())) {
					if (Q[state.CustomHash()][a.CustomHash()] >= 1) {
						value = 1;
						max = value;
						action = a;
						break;
					}
					else if (Q[state.CustomHash()][a.CustomHash()] <= -1) {
						value = -1;
					}
					else {
						if (berandom)
							value = random.NextDouble();
						else
							value = Q[state.CustomHash()][a.CustomHash()] + ((explore && !(RandomRate > 0)) ? ExplorationRate(N[state.CustomHash()][a.CustomHash()]) : 0);
					}
				}
				else {
					value = 1;
				}
				if (value > max) {
					max = value;
					action = a;
				}
			}
			return action;
		}

		private double ExplorationRate(int frequency) {
			return explorationModifier / Math.Pow((double)frequency + explorationStart, explorationPower);
		}

		private double LearningRate(int frequency) {
			double result = learningRateModifier / Math.Pow((double)frequency + learningRateStart, learningRatePower);
			return result < MinLearning ? MinLearning : result;
		}

		public void TurnOnExploration() {
			explore = true;
		}

		public void TurnOffExploration() {
			explore = false;
		}

		public void TurnOnLearning() {
			learn = true;
		}

		public void TurnOffLearning() {
			learn = false;
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
				if(!Q.ContainsKey(key))
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
	}
}
