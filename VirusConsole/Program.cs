using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirusNameSpace;
using VirusNameSpace.Agents;
using System.IO;

namespace VirusConsole
{
	class Program
	{
		static void Main(string[] args) {
			bool cntn = true;
			while (cntn) {
				Console.WriteLine("This is the main loop. Give a command:");
				String command = Console.ReadLine();
				int temp1, temp2, temp4;
				byte temp3 = 1;
				bool load = false;

				switch (command) {
					case "quit":
					case "end":
					case "exit":
						cntn = false;
						break;

					case "run minimax":
						Console.WriteLine("Minimax is player..?");
						temp3 = byte.Parse(Console.ReadLine());
						Console.WriteLine("Look how far ahead?");
						temp1 = int.Parse(Console.ReadLine());
						MinimaxAgent magent = new MinimaxAgent(temp1, temp3);
						Console.WriteLine("How many iterations?");
						temp1 = int.Parse(Console.ReadLine()); 
						Console.WriteLine("Which opponent?");
						command = Console.ReadLine();
						Console.WriteLine("Save how often?");
                        temp2 = int.Parse(Console.ReadLine());
						Console.WriteLine("Board size?");
						temp4 = int.Parse(Console.ReadLine());
						RunMinimax(temp4, temp3, magent, command, temp1, temp2);
						break;

					case "run minimaxmix":
						Console.WriteLine("MiniMaxMix is player..?");
						temp3 = byte.Parse(Console.ReadLine());
						Console.WriteLine("Look how far ahead?");
						temp1 = int.Parse(Console.ReadLine());
						MiniMaxMixAgent mmmagent = new MiniMaxMixAgent("brute1Q_L1persqrt_E1per1.sav7",temp1, temp3);
						Console.WriteLine("How many iterations?");
						temp1 = int.Parse(Console.ReadLine());
						Console.WriteLine("Which opponent?");
						command = Console.ReadLine();
						RunMiniMaxMix(temp3, mmmagent, command, temp1);
						break;

					case "run Q":
						Console.WriteLine("Q is player..?");
						temp3 = byte.Parse(Console.ReadLine());
						Console.WriteLine("How many games?");
						temp1 = int.Parse(Console.ReadLine());
						Console.WriteLine("Board size?");
						temp4 = int.Parse(Console.ReadLine());
						Console.WriteLine("Which opponent?");
						command = Console.ReadLine();
						StreamReader reader = new StreamReader("qdatafiles.txt");
						List<String> files = new List<string>();
						while (reader.Peek() != -1)
							files.Add(reader.ReadLine());
						reader.Close();
						foreach (String s in files) {
							Console.WriteLine("Using file " + s);
							QAgent qagentm = new QAgent(temp3);
							qagentm.Load(s);
							qagentm.TurnOffExploration();
							qagentm.TurnOffLearning();
							RunQ(temp3, qagentm, command, temp1, temp4);
						}
						break;

					case "train MemoryQ":
						Console.WriteLine("Q is player..?");
						temp3 = byte.Parse(Console.ReadLine());
						MemoryQAgent mqagent = new MemoryQAgent(temp3);
						if (load)
							mqagent.Load("TrainingData");
						Console.WriteLine("How many iterations?");
						temp1 = int.Parse(Console.ReadLine());
						Console.WriteLine("Save how often?");
						temp2 = int.Parse(Console.ReadLine());
						Console.WriteLine("Board size?");
						temp4 = int.Parse(Console.ReadLine());
						Console.WriteLine("Which opponent?");
						command = Console.ReadLine();
						TrainMemoryQ(temp4, temp3, mqagent, command, "log", "TrainingData", temp1, temp2);
						break;

					case "train mad Q":
						Console.WriteLine("Q is player..?");
						temp3 = byte.Parse(Console.ReadLine());
						QAgent qagent = new QAgent(temp3);
						qagent.Load("qmad");
						qagent.MinLearning = 0.05;
						Console.WriteLine("How many iterations?");
						temp1 = int.Parse(Console.ReadLine());
						TrainMadQ(temp3, qagent, "qmadlog", "qmad", temp1);
						break;

					case "train Q":
						Console.WriteLine("Q is player..?");
						temp3 = byte.Parse(Console.ReadLine());
						qagent = new QAgent(temp3);
						if (load)
							qagent.Load("TrainingData");
						Console.WriteLine("How many iterations?");
						temp1 = int.Parse(Console.ReadLine());
						Console.WriteLine("Save how often?");
						temp2 = int.Parse(Console.ReadLine());
						Console.WriteLine("Board size?");
						temp4 = int.Parse(Console.ReadLine());
						Console.WriteLine("Which opponent?");
						command = Console.ReadLine();
						TrainQ(temp4, temp3, qagent, command, "log", "TrainingData", temp1, temp2);
						break;

					case "continue training Q":
						load = true;
						goto case "train Q";

					case "gather Q data":
						Console.WriteLine("Q is player..?");
						temp3 = byte.Parse(Console.ReadLine());
						Console.WriteLine("How many iterations?");
						temp1 = int.Parse(Console.ReadLine());
						Console.WriteLine("Save how often?");
						temp2 = int.Parse(Console.ReadLine());
						Console.WriteLine("Which opponent?");
						command = Console.ReadLine();
						Console.WriteLine("Which part?");
						int part = int.Parse(Console.ReadLine());
						switch (part) {
							case 1:
								GatherQData(command, temp1, temp2, temp3, 0.9, 1, 1, true, true, 0, 0);
								GatherQData(command, temp1, temp2, temp3, 0.9, 1, 2, true, true, 0, 0);
								GatherQData(command, temp1, temp2, temp3, 0.9, 2, 1, true, true, 0, 0);
								GatherQData(command, temp1, temp2, temp3, 0.9, 2, 2, true, true, 0, 0);
								GatherQData(command, temp1, temp2, temp3, 0.9, 1, 1, true, false, 0, 0);
								GatherQData(command, temp1, temp2, temp3, 0.9, 1, 2, true, false, 0, 0);
								GatherQData(command, temp1, temp2, temp3, 0.9, 2, 1, true, false, 0, 0);
								GatherQData(command, temp1, temp2, temp3, 0.9, 2, 2, true, false, 0, 0);
								break;
							case 2:
								GatherQData(command, temp1, temp2, temp3, 0.9, 1, 1, false, true, 0, 0);
								GatherQData(command, temp1, temp2, temp3, 0.9, 1, 2, false, true, 0, 0);
								GatherQData(command, temp1, temp2, temp3, 0.9, 2, 1, false, true, 0, 0);
								GatherQData(command, temp1, temp2, temp3, 0.9, 2, 2, false, true, 0, 0);
								GatherQData(command, temp1, temp2, temp3, 0.9, 1, 1, false, false, 0, 0);
								GatherQData(command, temp1, temp2, temp3, 0.9, 1, 2, false, false, 0, 0);
								GatherQData(command, temp1, temp2, temp3, 0.9, 2, 1, false, false, 0, 0);
								GatherQData(command, temp1, temp2, temp3, 0.9, 2, 2, false, false, 0, 0);
								break;
							case 3:
								GatherQData(command, temp1, temp2, temp3, 1, 1, 1, true, true, 0, 0);
								GatherQData(command, temp1, temp2, temp3, 1, 1, 2, true, true, 0, 0);
								GatherQData(command, temp1, temp2, temp3, 1, 2, 1, true, true, 0, 0);
								GatherQData(command, temp1, temp2, temp3, 1, 2, 2, true, true, 0, 0);
								GatherQData(command, temp1, temp2, temp3, 1, 1, 1, true, false, 0, 0);
								GatherQData(command, temp1, temp2, temp3, 1, 1, 2, true, false, 0, 0);
								GatherQData(command, temp1, temp2, temp3, 1, 2, 1, true, false, 0, 0);
								GatherQData(command, temp1, temp2, temp3, 1, 2, 2, true, false, 0, 0);
								break;
							case 4:
								GatherQData(command, temp1, temp2, temp3, 1, 1, 1, false, true, 0, 0);
								GatherQData(command, temp1, temp2, temp3, 1, 1, 2, false, true, 0, 0);
								GatherQData(command, temp1, temp2, temp3, 1, 2, 1, false, true, 0, 0);
								GatherQData(command, temp1, temp2, temp3, 1, 2, 2, false, true, 0, 0);
								GatherQData(command, temp1, temp2, temp3, 1, 1, 1, false, false, 0, 0);
								GatherQData(command, temp1, temp2, temp3, 1, 1, 2, false, false, 0, 0);
								GatherQData(command, temp1, temp2, temp3, 1, 2, 1, false, false, 0, 0);
								GatherQData(command, temp1, temp2, temp3, 1, 2, 2, false, false, 0, 0);
								break;
							case 5:
								GatherQData(command, temp1, temp2, temp3, 0.9, 1, 1, true, false, 0, 0, 1);
								GatherQData(command, temp1, temp2, temp3, 0.9, 1, 1, true, true, 0, 0, 1);
								GatherQData(command, temp1, temp2, temp3, 1, 1, 1, true, false, 0, 0, 1);
								GatherQData(command, temp1, temp2, temp3, 1, 1, 1, true, true, 0, 0, 1);
								break;
							case 6:
								//GatherQData(command, temp1, temp2, temp3, 1, 1, 1, true, false, 0, 0);
								//GatherQData(command, temp1, temp2, temp3, 1, 1, 1, true, true, 0, 0);
								//GatherQData(command, temp1, temp2, temp3, 1, 1, 1, true, false, 0, 0.1);
								//GatherQData(command, temp1, temp2, temp3, 1, 1, 1, true, true, 0, 0.1);
								//GatherQData(command, temp1, temp2, temp3, 1, 1, 1, true, false, 0.2, 0);
								//GatherQData(command, temp1, temp2, temp3, 1, 1, 1, true, true, 0.2, 0);
								//GatherQData(command, temp1, temp2, temp3, 1, 1, 1, true, false, 0.2, 0.1);
								//GatherQData(command, temp1, temp2, temp3, 1, 1, 1, true, true, 0.2, 0.1);
								//GatherQData(command, temp1, temp2, temp3, 1, 1, 1, true, false, 0.5, 0);
								//GatherQData(command, temp1, temp2, temp3, 1, 1, 1, true, true, 0.5, 0);
								//GatherQData(command, temp1, temp2, temp3, 1, 1, 1, true, false, 0.5, 0.1);
								GatherQData(command, temp1, temp2, temp3, 1, 1, 1, true, true, 0.5, 0.1);
								break;
							case 7:
								GatherQData(command, temp1, temp2, temp3, 1, 1, 1, true, true, 0, 0);
								GatherQData(command, temp1, temp2, temp3, 1, 1, 1, false, true, 0, 0);
								GatherQData(command, temp1, temp2, temp3, 1, 1, 1, true, true, 0, 0.1);
								GatherQData(command, temp1, temp2, temp3, 1, 1, 1, false, true, 0, 0.1);
								break;
							case 8:
								GatherQData(command, temp1, temp2, temp3, 1, 1, 1, true, true, 0.2, 0);
								GatherQData(command, temp1, temp2, temp3, 1, 1, 1, false, true, 0.2, 0);
								GatherQData(command, temp1, temp2, temp3, 1, 1, 1, true, true, 0.2, 0.1);
								GatherQData(command, temp1, temp2, temp3, 1, 1, 1, false, true, 0.2, 0.1);
								GatherQData(command, temp1, temp2, temp3, 1, 1, 1, true, true, 0.5, 0);
								GatherQData(command, temp1, temp2, temp3, 1, 1, 1, false, true, 0.5, 0);
								GatherQData(command, temp1, temp2, temp3, 1, 1, 1, true, true, 0.5, 0.1);
								GatherQData(command, temp1, temp2, temp3, 1, 1, 1, false, true, 0.5, 0.1);
								break;
							case 9:
								GatherQDataShort(command, temp1, temp2, temp3, 1, 0, false, 0, 0);
								GatherQDataShort(command, temp1, temp2, temp3, 2, 0, false, 0, 0);
								GatherQDataShort(command, temp1, temp2, temp3, 2, 1, false, 0, 0);
								GatherQDataShort(command, temp1, temp2, temp3, 1, 0, true, 0, 0);
								GatherQDataShort(command, temp1, temp2, temp3, 2, 0, true, 0, 0);
								GatherQDataShort(command, temp1, temp2, temp3, 2, 1, true, 0, 0);
								break;
							case 10:
								GatherQData(command, temp1, temp2, temp3, 1, 1, 1, false, true, 0.2, 0);
								GatherQData(command, temp1, temp2, temp3, 1, 1, 1, false, true, 0.2, 0.1);
								GatherQData(command, temp1, temp2, temp3, 1, 1, 1, false, true, 0.5, 0);
								GatherQData(command, temp1, temp2, temp3, 1, 1, 1, false, true, 0.5, 0.1);
								break;
						}
						break;

					case "test game":
						TestGame();
						break;
				}
			}
		}

		static void RunMinimax(int size, byte qnumber, MinimaxAgent agent, String opponent, int iterations, int saveinterval = 360)
		{
			Virus virus = new Virus(2, size);
			int wins = 0;
			int wins2 = 0;
			byte oppnumber = qnumber == 1 ? (byte)2 : (byte)1;

			int n = 0;
			while(File.Exists("mmlog" + n)){
				n++;
			}

			StreamWriter writer = new StreamWriter("mmlog" + n);
			StreamWriter timeWriter = new StreamWriter("mmTimeLog", true);
			timeWriter.WriteLine("Player number: {0} Board size: {1} Opponent: {2}", qnumber, size, opponent);
			timeWriter.Close();

			for (int i = 1; i <= iterations; i++) {
				Agent opp;
				switch (opponent) {
					case "brute":
						opp = new BruteForceAgent(oppnumber);
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

				virus.SaveReplay = true;
				int winner = RunGame(virus, qnumber == 1 ? (Agent)agent : opp, qnumber == 2 ? (Agent)agent : opp);
				wins += winner == 1 ? 1 : 0;
				wins2 += winner == 1 ? 1 : 0;

				/*if (i % 100 == 0) {
					writer.WriteLine(wins2);
					//Console.WriteLine("Iteration: " + i);
					//Console.WriteLine("Wins: " + wins);
					wins2 = 0;
				}*/

				if (i % saveinterval == 0)
				{
					writer.WriteLine(wins);
					Console.WriteLine("Iteration: " + i);
					Console.WriteLine("Wins: " + wins);
					//Console.WriteLine(winners);
					//winners.Clear();
					wins = 0;
				}
				virus = new Virus(2, size);
			}
			writer.Close();
		}

		static void RunMiniMaxMix(byte qnumber, MiniMaxMixAgent agent, String opponent, int iterations) {
			Virus virus = new Virus(2, 5);
			int wins = 0;
			byte oppnumber = qnumber == 1 ? (byte)2 : (byte)1;

			int n = 0;
			while (File.Exists("mixlog" + n)) {
				n++;
			}
			StreamWriter writer = new StreamWriter("mixlog" + n);

			for (int i = 1; i <= iterations; i++) {
				Agent opp;
				switch (opponent) {
					case "brute":
						opp = new BruteForceAgent(oppnumber);
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

				if (i % 10 == 0) {
					writer.WriteLine(wins);
					Console.WriteLine("Iteration: " + i);
					Console.WriteLine("Wins: " + wins);
					wins = 0;
				}
				virus = new Virus(2, 5);
			}
			writer.Close();
		}

		static void RunQ(byte qnumber, QAgent agent, String opponent, int iterations, int size) {
			Virus virus = new Virus(2, size);
			int wins = 0;
			byte oppnumber = qnumber == 1 ? (byte)2 : (byte)1;

			Agent opp;
			switch (opponent) {
				case "brute":
					opp = new BruteForceAgent(oppnumber);
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

			int n = 0;
			while (File.Exists("qwinlog" + n)) {
				n++;
			}
			StreamWriter writer = new StreamWriter("qwinlog" + n);

			for (int i = 1; i <= iterations; i++) {
				int winner = RunGame(virus, qnumber == 1 ? (Agent)agent : opp, qnumber == 2 ? (Agent)agent : opp);
				wins += winner == 1 ? 1 : 0;
				virus = new Virus(2, size);
			}
			writer.WriteLine(wins);
			Console.WriteLine("Iteration: " + iterations);
			Console.WriteLine("Wins: " + wins);
			wins = 0;
			writer.Close();
		}
		
		static void GatherQData(String opponent, int iterations, int saveinterval, byte player, double disc, double lrmod, double exmod, bool lrsqrt, bool exsqrt, double randomness, double minlearn, double initvalue = 0) {
			GatherQDataFull(opponent, iterations, saveinterval, player, disc, lrmod, 0, exmod, 0, lrsqrt, exsqrt, randomness, minlearn, initvalue);
		}

		static void GatherQDataShort(String opponent, int iterations, int saveinterval, byte player, double exmod, double exstrt, bool exsqrt, double randomness, double minlearn) {
			GatherQDataFull(opponent, iterations, saveinterval, player, 1, 1, 0, exmod, exstrt, false, exsqrt, randomness, minlearn, 0);
		}

		static void GatherQDataFull(String opponent, int iterations, int saveinterval, byte player, double disc, double lrmod, double lrstrt, double exmod, double exstrt, bool lrsqrt, bool exsqrt, double randomness, double minlearn, double initvalue = 0) {
			QAgent agent;
			String name = opponent + disc + "Q_L" + lrmod + (lrsqrt ? "persqrt" : "per1") + "_E" + exmod + (exsqrt ? "persqrt" : "per1") + "r" + randomness + "m" + minlearn + "ls" + lrstrt + "xs" + exstrt + "i" + initvalue;
			Console.WriteLine("Gathering data for Q: " + name + ":");
			for (int i = 0; i < 10; i++) {
				if (!File.Exists(name + ".log" + i)) {
					agent = new QAgent(player, disc, lrmod, lrstrt, exmod, exstrt, lrsqrt ? 0.5 : 1, exsqrt ? 0.5 : 1, initvalue);
					agent.RandomRate = randomness;
					agent.MinLearning = minlearn;
					TrainQ(5, player, agent, opponent, name + ".log" + i, name + ".sav" + i, iterations, saveinterval);
				}
			}
		}

		static void TrainQ(int size, byte qnumber, QAgent agent, String opponent, String logname, String savename, int iterations, int saveinterval = 360) {
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

				if (i % 100 == 0) {
					if (agent.RandomRate == 0) {
						writer.WriteLine(wins2);
					}
					wins2 = 0;
				}
				if (i % saveinterval == 0) {
					agent.Save(savename);
					Console.WriteLine("Iteration: " + i);
					Console.WriteLine("Wins: " + wins);
					wins = 0;
					if (agent.RandomRate > 0) {
						agent.TurnOffExploration();
						agent.TurnOffLearning();
						for (int j = 1; j <= 1000; j++) {
							virus = new Virus(2, size);
							winner = RunGame(virus, qnumber == 1 ? (Agent)agent : opp, qnumber == 2 ? (Agent)agent : opp);
							wins += winner == 1 ? 1 : 0;
						}
						writer.WriteLine(wins);
						wins = 0;
						agent.TurnOnExploration();
						agent.TurnOnLearning();
					}
				}
				virus = new Virus(2, size);
			}
			writer.Close();
		}

		static void TrainMadQ(byte qnumber, QAgent agent, String logname, String savename, int iterations) {
			Virus virus = new Virus(2, 5);
			int wins = 0, wins2 = 0;
			byte oppnumber = qnumber == 1 ? (byte)2 : (byte)1;
			Agent opp = new MixedAgent(0.1, false, oppnumber);
			StreamWriter writer = new StreamWriter(logname);
			for (int i = 1; i <= iterations; i++) {

				int winner = RunGame(virus, qnumber == 1 ? (Agent)agent : opp, qnumber == 2 ? (Agent)agent : opp);
				wins += winner == 1 ? 1 : 0;
				wins2 += winner == 1 ? 1 : 0;

				if (i % 100 == 0) {
					writer.WriteLine(wins2);
					wins2 = 0;
				}
				if (i % 10000 == 0) {
					agent.Save(savename);
					Console.WriteLine("Iteration: " + i);
					Console.WriteLine("Wins: " + wins);
					wins = 0;
				}
				virus = new Virus(2, 5);
			}
			writer.Close();
		}

		static void TestGame() {
			bool testresult = false;
			Virus virus = new Virus();
			ShowBoard(virus);

			testresult = ShouldCrashTest(() => virus.Move(0, 4, 0, 4)); //moving starting player's piece to it's own position
			ShowResult("Test 1", testresult);
			testresult = ShouldCrashTest(() => virus.Move(0, 4, 0, 1)); //moving starting player's piece too far
			ShowResult("Test 2", testresult);
			testresult = ShouldCrashTest(() => virus.Move(0, 0, 0, 1)); //moving non-starting player's piece
			ShowResult("Test 3", testresult);
			testresult = ShouldCrashTest(() => virus.Move(0, 0, 0, 3)); //moving non-starting player's piece too far
			ShowResult("Test 4", testresult);

			Console.WriteLine("The game should be in the start state:");
			ShowBoard(virus);

			Console.WriteLine("Making a move:\nThis should move the top left piece one space SE");
			virus.Move(0, 4, 1, 3);
			ShowBoard(virus);

			testresult = virus.Winner == 0; //there shouldn't be a winner yet
			ShowResult("Test winner", testresult);
			testresult = ShouldCrashTest(() => virus.Move(1, 3, 0, 2)); //moving player 1's piece again
			ShowResult("Test own piece", testresult);
			testresult = ShouldCrashTest(() => virus.Move(0, 0, 0, 3)); //moving player 2's piece too far
			ShowResult("Test opponent's piece", testresult);

			Console.WriteLine("Making a move:\nThis should move the bottom left piece one space N");
			virus.Move(0, 0, 0, 1);
			ShowBoard(virus);

			testresult = virus.Winner == 0; //there shouldn't be a winner yet
			ShowResult("Test winner", testresult);
			testresult = ShouldCrashTest(() => virus.Move(0, 4, 3, 0)); //moving player 1's piece too far
			ShowResult("Test own piece", testresult);
			testresult = ShouldCrashTest(() => virus.Move(0, 0, 0, 2)); //moving player 2's piece again
			ShowResult("Test opponent's piece", testresult);

			Console.WriteLine("Making a move:\nThis should move the piece at (1,3) two spaces S\n and take the adjacent pieces");
			virus.Move(1, 3, 1, 1);
			ShowBoard(virus);

			testresult = virus.Winner == 0; //there shouldn't be a winner yet
			ShowResult("Test winner", testresult);
			testresult = ShouldCrashTest(() => virus.Move(1, 1, 1, 0)); //moving player 1's piece again
			ShowResult("Test own piece", testresult);
			testresult = ShouldCrashTest(() => virus.Move(4, 4, 1, 0)); //moving player 2's piece too far
			ShowResult("Test opponent's piece", testresult);
		}

		static bool ShouldCrashTest(System.Action function) {
			try {
				function();
			}
			catch {
				return true;
			}
			return false;
		}

		static void ShowResult(String test, bool result) {
			Console.WriteLine(test + ": " + (result ? "success" : "failure"));
		}

		static void ShowBoard(Virus virus) {
			for (int i = virus.Size - 1; i >= 0; i--) {
				for (int j = 0; j < virus.Size; j++) {
					Console.Write("{0} ", virus[j, i]);
				}
				Console.WriteLine();
			}
		}

		static byte RunGame(Virus virus, Agent p1, Agent p2) {
			Move move;
			byte winner;
			
			while (true) {
				move = p1.Move(virus);
				virus.Move(move.sx, move.sy, move.ex, move.ey);
				winner = virus.Winner;
				if (winner != 0)
					break;
				move = p2.Move(virus);
				virus.Move(move.sx, move.sy, move.ex, move.ey);
				winner = virus.Winner;
				if (winner != 0)
					break;
			}
			p1.EndGame(virus);
			p2.EndGame(virus);
			return winner;
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

				if (i % 100 == 0) {
					if (agent.RandomRate == 0) {
						writer.WriteLine(wins2);
					}
					wins2 = 0;
				}
				if (i % saveinterval == 0) {
					agent.Save(savename);
					Console.WriteLine("Iteration: " + i);
					Console.WriteLine("Wins: " + wins);
					wins = 0;
					if (agent.RandomRate > 0) {
						agent.TurnOffExploration();
						//agent.TurnOffLearning();
						for (int j = 1; j <= 1000; j++) {
							virus = new Virus(2, size);
							winner = RunGame(virus, qnumber == 1 ? (Agent)agent : opp, qnumber == 2 ? (Agent)agent : opp);
							wins += winner == 1 ? 1 : 0;
						}
						writer.WriteLine(wins);
						wins = 0;
						agent.TurnOnExploration();
						agent.ForgetShortTerm();
						//agent.TurnOnLearning();
					}
				}
				virus = new Virus(2, size);
			}
			writer.Close();
			agent.SaveLongTermMemory("m");
		}
	}
}
