using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirusNameSpace;
using VirusNameSpace.Agents;
using System.IO;

namespace VirusConsole {
	partial class Program {
		static void comTrainMemQ(bool load) {
			Console.WriteLine("Q is player..?");
			byte temp3 = byte.Parse(Console.ReadLine());
			MemoryQAgent mqagent = new MemoryQAgent(temp3);
			if (load)
				mqagent.Load("TrainingData");
			Console.WriteLine("How many iterations?");
			int temp1 = int.Parse(Console.ReadLine());
			Console.WriteLine("Save how often?");
			int temp2 = int.Parse(Console.ReadLine());
			Console.WriteLine("Board size?");
			int temp4 = int.Parse(Console.ReadLine());
			Console.WriteLine("Which opponent?");
			string command = Console.ReadLine();
			TrainMemoryQ(temp4, temp3, mqagent, command, "log", "TrainingData", temp1, temp2);
		}

		static void comGatherMemQData() {
			Console.WriteLine("Q is player..?");
			byte temp3 = byte.Parse(Console.ReadLine());
			Console.WriteLine("How many iterations?");
			int temp1 = int.Parse(Console.ReadLine());
			Console.WriteLine("Save how often?");
			int temp2 = int.Parse(Console.ReadLine());
			Console.WriteLine("Which opponent?");
			string command = Console.ReadLine();
			GatherMemQData(command, temp1, temp2, temp3);
		}

		static void GatherMemQData(String opponent, int iterations, int saveinterval, byte player) {
			MemoryQAgent agent;
			String name = opponent + "MemQ";
			Console.WriteLine("Gathering data for Q: " + name + ":");
			for (int i = 0; i < 10; i++) {
				if (!File.Exists(name + ".log" + i)) {
					agent = new MemoryQAgent(player);
					TrainMemoryQ(5, player, agent, opponent, name + ".log" + i, name + ".sav" + i, iterations, saveinterval);
				}
			}
		}

		static void TrainMemoryQ(int size, byte qnumber, MemoryQAgent agent, String opponent, String logname, String savename, int iterations, int saveinterval = 360) {
			Virus virus = new Virus(2, size);
			int wins = 0, wins2 = 0;
			byte oppnumber = qnumber == 1 ? (byte)2 : (byte)1;
			Agent opp = new BruteForceAgent(oppnumber);
			StreamWriter writer = new StreamWriter(logname);
			for (int i = 1; i <= iterations; i++) {
				switch (opponent) {
					case "brute":
						break;
					case "minimax4":
						opp = new MinimaxAgent(4, oppnumber);
						break;
					case "minimax3":
						opp = new MinimaxAgent(3, oppnumber);
						break;
					case "minimax2":
						opp = new MinimaxAgent(2, oppnumber);
						break;
					default:
						opp = new BruteForceAgent(oppnumber);
						break;
				}

				int winner = RunGame(virus, qnumber == 1 ? (Agent)agent : opp, qnumber == 2 ? (Agent)agent : opp);
				wins += winner == 1 ? 1 : 0;
				wins2 += winner == 1 ? 1 : 0;

				agent.ProcessShortTermMemory();

				if (i % saveinterval == 0) {
					agent.Save(savename);
					Console.WriteLine("Iteration: " + i);
					Console.WriteLine("Wins: " + wins);
					wins = 0;
				}
				virus = new Virus(2, size);
			}
			for (int n = 0; n < 10; n++) {
				MemoryQAgent ag = new MemoryQAgent();
				for (int i = 0; i < 1000; i++) {
					agent.TellOfMemoryTo(ag);
				}
				wins = 0;
				opp = new BruteForceAgent(oppnumber);
				for (int i = 1; i <= 10000; i++) {
					switch (opponent) {
						case "brute":
							break;
						case "minimax4":
							opp = new MinimaxAgent(4, oppnumber);
							break;
						case "minimax3":
							opp = new MinimaxAgent(3, oppnumber);
							break;
						case "minimax2":
							opp = new MinimaxAgent(2, oppnumber);
							break;
						default:
							opp = new BruteForceAgent(oppnumber);
							break;
					}

					int winner = RunGame(virus, qnumber == 1 ? (Agent)ag : opp, qnumber == 2 ? (Agent)ag : opp);
					wins += winner == 1 ? 1 : 0;
					virus = new Virus(2, size);
				}
				Console.WriteLine("After 1000 memories");
				Console.WriteLine("Wins: " + wins);
				writer.Write(((double)wins) / 10000.0 + ";");
				for (int i = 0; i < 9000; i++) {
					agent.TellOfMemoryTo(ag);
				}
				wins = 0;
				opp = new BruteForceAgent(oppnumber);
				for (int i = 1; i <= 10000; i++) {
					switch (opponent) {
						case "brute":
							break;
						case "minimax4":
							opp = new MinimaxAgent(4, oppnumber);
							break;
						case "minimax3":
							opp = new MinimaxAgent(3, oppnumber);
							break;
						case "minimax2":
							opp = new MinimaxAgent(2, oppnumber);
							break;
						default:
							opp = new BruteForceAgent(oppnumber);
							break;
					}

					int winner = RunGame(virus, qnumber == 1 ? (Agent)ag : opp, qnumber == 2 ? (Agent)ag : opp);
					wins += winner == 1 ? 1 : 0;
					virus = new Virus(2, size);
				}
				Console.WriteLine("After 10000 memories");
				Console.WriteLine("Wins: " + wins);
				writer.Write(((double)wins) / 10000.0 + ";");
				for (int i = 0; i < 90000; i++) {
					agent.TellOfMemoryTo(ag);
				}
				wins = 0;
				opp = new BruteForceAgent(oppnumber);
				for (int i = 1; i <= 10000; i++) {
					switch (opponent) {
						case "brute":
							break;
						case "minimax4":
							opp = new MinimaxAgent(4, oppnumber);
							break;
						case "minimax3":
							opp = new MinimaxAgent(3, oppnumber);
							break;
						case "minimax2":
							opp = new MinimaxAgent(2, oppnumber);
							break;
						default:
							opp = new BruteForceAgent(oppnumber);
							break;
					}

					int winner = RunGame(virus, qnumber == 1 ? (Agent)ag : opp, qnumber == 2 ? (Agent)ag : opp);
					wins += winner == 1 ? 1 : 0;
					virus = new Virus(2, size);
				}
				Console.WriteLine("After 100000 memories");
				Console.WriteLine("Wins: " + wins);
				writer.WriteLine(((double)wins) / 10000.0);
			}
			writer.Close();
			agent.SaveLongTermMemory("m");
		}
	}
}
