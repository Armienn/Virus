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
			for (int i = 0; i < 5; i++) {
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
	}
}
