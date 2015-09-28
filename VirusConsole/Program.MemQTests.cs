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
		static void comTestMemQ() {

			//train a MemQ agent, and once every ten games have it teach an observer
			//do this with ten agents
			const int teachingagents = 10;
			const int gamesbetweenteaching = 10;
			const int learninggames = 10000;

			int wins = 0;
			int n = 0;
			while (File.Exists("observer" + n + ".log")) n++;
			StreamWriter writer = new StreamWriter("observer" + n + ".log");
			writer.WriteLine("sources");

			Virus virus;
			MemoryQAgent observer = new MemoryQAgent(1);
			Agent opponent = new BruteForceAgent(2);

			for (int i = 0; i < teachingagents; i++) {
				MemoryQAgent agent = new MemoryQAgent(1);

				wins = 0;
				for (int j = 0; j < learninggames; j++) {
					virus = new Virus(2, 5);
					wins += RunGame(virus, agent, opponent) == 1 ? 1 : 0;
					agent.ProcessShortTermMemory();

					if (j % gamesbetweenteaching == gamesbetweenteaching - 1) {
						agent.TellOfMemoryTo(observer, true);
					}
					if (j % 1000 == 999) {
						if (j > 9500) {
							writer.WriteLine(wins);
							Console.WriteLine("Wins: " + wins);
						}

						wins = 0;
					}
					
				}
			}

			writer.WriteLine("observer");
			for (int i = 0; i < 10; i++) {
				wins = 0;
				for (int j = 0; j < 1000; j++) {
					virus = new Virus();
					wins += RunGame(virus, observer, opponent) == 1 ? 1 : 0;
					observer.ForgetShortTerm();
				}
				writer.WriteLine(wins);
				Console.WriteLine("Observer wins: " + wins);
			}
			writer.Close();
		}

		static void comTestMemQ2() {

			//train a MemQ agent, and once every ten games have it teach an observer
			//do this with ten agents
			const int teachingagents = 10;
			const int gamesbetweenteaching = 10;
			const int learninggames = 10000;

			int wins = 0;
			int n = 0;
			while (File.Exists("observer" + n + ".log")) n++;
			StreamWriter writer = new StreamWriter("observer" + n + ".log");
			writer.WriteLine("sources");

			Virus virus;
			MemoryQAgent observer = new MemoryQAgent(1);
			Agent opponent = new BruteForceAgent(2);
			MemoryQAgent[] agents = new MemoryQAgent[teachingagents];

			for (int i = 0; i < teachingagents; i++) {
				agents[i] = new MemoryQAgent(1);

				wins = 0;
				for (int j = 0; j < learninggames; j++) {
					virus = new Virus(2, 5);
					wins += RunGame(virus, agents[i], opponent) == 1 ? 1 : 0;
					agents[i].ProcessShortTermMemory();
					if (j % 1000 == 999) {
						if (j > 9500) {
							writer.WriteLine(wins);
							Console.WriteLine("Wins: " + wins);
						}

						wins = 0;
					}

				}
			}

			writer.WriteLine("observer");
			int totalmems = 0;
			int memstoprocess = 0;
			while (totalmems < 100000) {
				//if (memstoprocess == 10000) memstoprocess = 100000;
				//if (memstoprocess == 1000) memstoprocess = 10000;
				//if (memstoprocess < 1000) memstoprocess = 1000;
				memstoprocess += 1000;
				while (totalmems < memstoprocess) {
					for (int i = 0; i < teachingagents; i++) {
						totalmems += agents[i].TellOfMemoryToExt(observer, true);
					}
				}

				//writer.WriteLine("observer");
				//for (int i = 0; i < 10; i++) {
					wins = 0;
					for (int j = 0; j < 10000; j++) {
						virus = new Virus();
						wins += RunGame(virus, observer, opponent) == 1 ? 1 : 0;
						observer.ForgetShortTerm();
					}
					writer.WriteLine(wins);
					Console.WriteLine("Observer wins after " + totalmems + "memories: " + wins);
				//}
			}
			writer.Close();
		}
	}
}
