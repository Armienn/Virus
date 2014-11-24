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
	partial class Program
	{
		static void Main(string[] args) {
			bool cntn = true;
			while (cntn) {
				Console.WriteLine("This is the main loop. Give a command:");
				String command = Console.ReadLine();
				bool load = false;

				switch (command) {
					case "quit":
					case "end":
					case "exit":
						cntn = false;
						break;

					case "run minimax":
						comRunMinimax(); break;

					case "run minimaxmix":
						comRunMinimaxmix(); break;

					case "run Q":
						comRunQ(); break;

					case "train MemoryQ":
						comTrainMemQ(load); break;

					case "continue training MemQ":
						load = true;
						goto case "train MemoryQ";

					case "gather MemQ data":
						comGatherMemQData(); break;

					case "train mad Q":
						comTrainMadQ(); break;

					case "train Q":
						comTrainQ(load); break;

					case "continue training Q":
						load = true;
						goto case "train Q";

					case "gather Q data":
						comGatherQData(); break;

					case "test game":
						TestGame(); break;

					case "testmemq":
						comTestMemQ(); comTestMemQ(); comTestMemQ(); comTestMemQ(); comTestMemQ(); break;
				}
			}
		}
	}
}
